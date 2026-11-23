// <copyright file="StarSmithItemPowerUpFactory.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

using System.Runtime.CompilerServices;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.Persistence;

/// <summary>
/// The power-up factory decorator of smithed boosts (Readme.md "PR 6 contract", "Boosts in play"). A worn item's
/// boosts, each clamped to its per-item cap, go into a pool attribute per stat and context; one capped element per pool
/// feeds the real attribute, so the sum over all worn items never passes the total cap, whichever item came first, and
/// is recounted when one comes off. Caps are clamped to the code ceilings. While Star Smithing is off nothing is added.
/// </summary>
/// <param name="inner">The factory it decorates.</param>
/// <param name="balance">The balance tables, or null while Star Smithing is off.</param>
public sealed class StarSmithItemPowerUpFactory(IItemPowerUpFactory inner, Func<StarSmithBalance?> balance) : IItemPowerUpFactory
{
    private static readonly StarSmithBoostStat[] AllStats = Enum.GetValues<StarSmithBoostStat>();

    /// <summary>The pools: per stat, against monsters (index 0) and against players (index 1).</summary>
    private static readonly AttributeDefinition[,] Pools = CreatePools();

    private static readonly ConditionalWeakTable<AttributeSystem, object> Bridged = new();

    /// <inheritdoc />
    public IEnumerable<PowerUpWrapper> GetPowerUps(Item item, AttributeSystem attributeSystem)
    {
        foreach (var powerUp in inner.GetPowerUps(item, attributeSystem))
        {
            yield return powerUp;
        }

        if (balance() is not { } table
            || item.Durability <= 0
            || !InventoryConstants.IsEquippedSlot(item.ItemSlot)
            || item is not IIdentifiable { Id: var id }
            || StarSmithBoostBook.Get(id) is not { Count: > 0 } boosts)
        {
            yield break;
        }

        this.Bridge(attributeSystem);
        foreach (var boost in boosts)
        {
            var value = boost.Value / 100.0;
            yield return new PowerUpWrapper(new ConstantElement((float)Math.Clamp(value, 0, table.PerItem(boost.Stat, pvp: false))), Pools[(int)boost.Stat, 0], attributeSystem);
            if (StarSmithBalance.HasPvpValue(boost.Stat))
            {
                yield return new PowerUpWrapper(new ConstantElement((float)Math.Clamp(value, 0, table.PerItem(boost.Stat, pvp: true))), Pools[(int)boost.Stat, 1], attributeSystem);
            }
        }
    }

    /// <inheritdoc />
    public IEnumerable<PowerUpWrapper> GetSetPowerUps(IEnumerable<Item> equippedItems, AttributeSystem attributeSystem, GameConfiguration gameConfiguration)
        => inner.GetSetPowerUps(equippedItems, attributeSystem, gameConfiguration);

    /// <summary>
    /// The attribute a pool feeds, how its value maps, and how it aggregates.
    /// </summary>
    private static (AttributeDefinition Target, Func<double, float> Map, AggregateType Type) TargetOf(StarSmithBoostStat stat, bool pvp) => stat switch
    {
        StarSmithBoostStat.Damage => (pvp ? Stats.StarSmithDamageIncreasePvp : Stats.FinalDamageIncreasePvm, v => (float)(v / 100), AggregateType.AddRaw),
        StarSmithBoostStat.Defense => (pvp ? Stats.DefensePvp : Stats.DefensePvm, v => (float)(1 + (v / 100)), AggregateType.Multiplicate),
        StarSmithBoostStat.MaxHealth => (Stats.MaximumHealth, v => (float)v, AggregateType.AddRaw),
        StarSmithBoostStat.MaxMana => (Stats.MaximumMana, v => (float)v, AggregateType.AddRaw),
        StarSmithBoostStat.AttackSpeed => (Stats.AttackSpeed, v => (float)v, AggregateType.AddRaw),
        StarSmithBoostStat.CriticalRate => (Stats.CriticalDamageChance, v => (float)(v / 100), AggregateType.AddRaw),
        StarSmithBoostStat.ExcellentRate => (Stats.ExcellentDamageChance, v => (float)(v / 100), AggregateType.AddRaw),
        _ => (pvp ? Stats.StarSmithDamageReceiveDecrementPvp : Stats.FinalDamageReceiveDecrementPvm, v => (float)(v / 100), AggregateType.AddRaw),
    };

    private static AttributeDefinition[,] CreatePools()
    {
        var pools = new AttributeDefinition[AllStats.Length, 2];
        foreach (var stat in AllStats)
        {
            for (var pvp = 0; pvp < 2; pvp++)
            {
                var index = ((int)stat * 2) + pvp;
                pools[(int)stat, pvp] = new AttributeDefinition(new Guid($"5E1A2C3D-7B6F-4E21-9C4A-2D8F1B3E7{index:X3}"), $"Star Smith {stat} pool ({(pvp == 0 ? "PvE" : "PvP")})", "The smithed boosts of the worn items before the total cap.");
            }
        }

        return pools;
    }

    /// <summary>
    /// Feeds each pool into its attribute through the total cap, once per attribute system.
    /// </summary>
    private void Bridge(AttributeSystem attributeSystem)
    {
        lock (Bridged)
        {
            if (Bridged.TryGetValue(attributeSystem, out _))
            {
                return;
            }

            Bridged.Add(attributeSystem, new object());
        }

        foreach (var stat in AllStats)
        {
            foreach (var pvp in StarSmithBalance.HasPvpValue(stat) ? new[] { false, true } : [false])
            {
                var (target, map, type) = TargetOf(stat, pvp);
                var pool = attributeSystem.GetOrCreateAttribute(Pools[(int)stat, pvp ? 1 : 0]);
                attributeSystem.AddElement(new CappedElement(pool, () => balance()?.Total(stat, pvp) ?? 0, map, type), target);
            }
        }
    }

    /// <summary>
    /// A pool's value up to the total cap, mapped onto its attribute; it changes when the pool does.
    /// </summary>
    private sealed class CappedElement : IElement
    {
        private readonly IElement _pool;
        private readonly Func<double> _cap;
        private readonly Func<double, float> _map;

        public CappedElement(IElement pool, Func<double> cap, Func<double, float> map, AggregateType aggregateType)
        {
            this._pool = pool;
            this._cap = cap;
            this._map = map;
            this.AggregateType = aggregateType;
            pool.ValueChanged += (_, args) => this.ValueChanged?.Invoke(this, args);
        }

        public event EventHandler? ValueChanged;

        public float Value => this._map(Math.Clamp(this._pool.Value, 0, Math.Max(0, this._cap())));

        public AggregateType AggregateType { get; }
    }
}
