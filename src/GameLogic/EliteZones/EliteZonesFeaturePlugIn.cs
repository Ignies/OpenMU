// <copyright file="EliteZonesFeaturePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EliteZones;

using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Progression;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.Pathfinding;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The elite zones of the maps from Scorched Canyon on: five elemental areas per map, which hold its elites.
/// A walk into a zone is refused when the character misses its entry rule.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.EliteZonesFeaturePlugIn_Name), Description = nameof(PlugInResources.EliteZonesFeaturePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("5B1F7C3E-2A64-4E8D-9C71-0D3E6A2B8F45")]
public class EliteZonesFeaturePlugIn : IFeaturePlugIn, ISupportCustomConfiguration<EliteZoneConfiguration>, ISupportDefaultCustomConfiguration, IDisabledByDefault,
    IWalkRequestValidationPlugIn, IObjectAddedToMapPlugIn, IAttackableGotHitPlugIn, IMonsterItemDropPlugIn
{
    /// <summary>
    /// The magic effect numbers which show the debuffs: explosion (bleeding), poisoned, weakness, iced and innovation.
    /// </summary>
    private static readonly Dictionary<EliteDebuffKind, short> EffectNumbers = new()
    {
        [EliteDebuffKind.Bleeding] = 0x4B,
        [EliteDebuffKind.Poisoning] = 0x37,
        [EliteDebuffKind.DecreaseAttack] = 0x4C,
        [EliteDebuffKind.DecreaseAttackSpeed] = 0x38,
        [EliteDebuffKind.DecreaseDefense] = 0x4D,
    };

    private readonly object _dropLock = new();

    private EliteZoneConfiguration? _configuration;

    private WorldStateFile? _stateFile;

    private EliteDropCounts? _dropCounts;

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    public EliteZoneConfiguration? Configuration
    {
        get => this._configuration;
        set
        {
            this._configuration = value;
            this._stateFile = string.IsNullOrWhiteSpace(value?.StateFile) ? null : new WorldStateFile(value.StateFile);
            this._dropCounts = null;
        }
    }

    /// <summary>
    /// Gets or sets the clock; tests replace it.
    /// </summary>
    internal Func<DateTime> UtcNow { get; set; } = () => DateTime.UtcNow;

    /// <summary>
    /// Gets the active plugin of a game context.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The plugin, or <c>null</c> when the feature is off.</returns>
    public static EliteZonesFeaturePlugIn? Get(IGameContext gameContext) => gameContext.FeaturePlugIns.GetPlugIn<EliteZonesFeaturePlugIn>();

    /// <inheritdoc />
    public object CreateDefaultConfig() => new EliteZoneConfiguration();

    /// <summary>
    /// Gets the zones of a map.
    /// </summary>
    /// <param name="mapNumber">The map number.</param>
    /// <returns>The zones.</returns>
    public IEnumerable<EliteZoneDefinition> GetZones(short mapNumber)
        => this.Configuration?.Zones.Where(zone => zone.MapNumber == mapNumber) ?? [];

    /// <summary>
    /// Gets the zone which contains a point of a map.
    /// </summary>
    /// <param name="mapNumber">The map number.</param>
    /// <param name="point">The point.</param>
    /// <returns>The zone, or <c>null</c>.</returns>
    public EliteZoneDefinition? GetZoneAt(short mapNumber, Point point)
        => this.GetZones(mapNumber).FirstOrDefault(zone => zone.Contains(point));

    /// <summary>
    /// Determines whether a player may enter the zones of its current map.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns><c>true</c>, if the player meets the entry rule.</returns>
    public bool CanEnter(Player player)
    {
        var levelAboveMap = this.Configuration?.LevelAboveMap ?? 0;
        if (levelAboveMap <= 0)
        {
            return true;
        }

        var mapLevel = player.CurrentMap?.Definition is { } map ? ProgressionWorld.GetSeason21Requirement(map) ?? 0 : 0;
        return Season21LevelCheck.Meets(player, mapLevel + levelAboveMap);
    }

    /// <inheritdoc />
    public async ValueTask ValidateWalkRequestAsync(Player player, Memory<WalkingStep> steps, WalkRequestValidationArgs eventArgs)
    {
        if (!eventArgs.IsWalkAllowed
            || player.CurrentMap is not { } map
            || this.FindEnteredZone(map.Definition.Number, steps.Span) is null
            || this.CanEnter(player))
        {
            return;
        }

        eventArgs.IsWalkAllowed = false;
        await player.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.EliteZoneEntryDenied)).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask ObjectAddedToMapAsync(GameMap map, ILocateable addedObject)
    {
        if (addedObject is not Player player)
        {
            return;
        }

        var zones = this.GetZones(map.Definition.Number).ToList();
        if (zones.Count == 0)
        {
            return;
        }

        var canEnter = this.CanEnter(player);
        var shown = zones.Select(zone => (zone, canEnter)).ToList();
        await player.InvokeViewPlugInAsync<IEliteZonesViewPlugIn>(p => p.ShowEliteZonesAsync(map.Definition.Number, shown)).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void AttackableGotHit(IAttackable attackable, IAttacker attacker, HitInfo hitInfo)
    {
        if (attackable is Player player && attacker is Monster monster && this.Configuration?.Debuffs.Count > 0)
        {
            _ = this.ApplyDebuffsSafeAsync(player, monster, hitInfo);
        }
    }

    /// <inheritdoc />
    public void ItemDropping(AttackableNpcBase monster, Player killer, Item item, CancelEventArgs eventArgs)
    {
        if (eventArgs.Cancel
            || this.Configuration is not { DropLimits.Count: > 0 } configuration
            || item.Definition is not { } definition
            || configuration.DropLimits.FirstOrDefault(l => l.ItemGroup == definition.Group && l.ItemNumber == definition.Number) is not { } limit
            || killer.SelectedCharacter is not { } character)
        {
            return;
        }

        lock (this._dropLock)
        {
            var week = WeekStart(this.UtcNow(), configuration.WeekStartsOn);
            var counts = this._dropCounts ??= this._stateFile?.Load<EliteDropCounts>() ?? new EliteDropCounts();
            if (counts.WeekStart != week)
            {
                counts = this._dropCounts = new EliteDropCounts { WeekStart = week };
            }

            var key = $"{character.Id}:{definition.Group}:{definition.Number}";
            var count = counts.Counts.GetValueOrDefault(key);
            if (count >= limit.MaximumPerWeek)
            {
                eventArgs.Cancel = true;
                return;
            }

            counts.Counts[key] = count + 1;
            try
            {
                this._stateFile?.Save(counts);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                killer.Logger.LogWarning(ex, "The elite drop counts can't be written to {File}.", this._stateFile?.Path);
            }
        }
    }

    /// <summary>
    /// Puts the debuffs of the zone of an elite on the character it hit.
    /// </summary>
    /// <param name="player">The player who got hit.</param>
    /// <param name="monster">The monster.</param>
    /// <param name="hitInfo">The hit.</param>
    /// <returns>The task.</returns>
    internal async ValueTask ApplyDebuffsAsync(Player player, Monster monster, HitInfo hitInfo)
    {
        if (this.Configuration is not { } configuration
            || !monster.Definition.Designation.ValueInNeutralLanguage.Contains("Elite", StringComparison.OrdinalIgnoreCase)
            || this.GetZoneAt(monster.CurrentMap.Definition.Number, monster.Position) is not { } zone
            || player.Attributes is not { } attributes)
        {
            return;
        }

        foreach (var debuff in configuration.Debuffs.Where(d => d.Element == zone.Element))
        {
            if (!Rand.NextRandomBool(debuff.ChancePercent)
                || player.GameContext.Configuration.MagicEffects.FirstOrDefault(e => e.Number == EffectNumbers[debuff.Kind]) is not { } definition)
            {
                continue;
            }

            var duration = TimeSpan.FromSeconds(debuff.Seconds);
            MagicEffect effect = debuff.Kind switch
            {
                EliteDebuffKind.Bleeding => new BleedingMagicEffect(new ConstantElement(1), definition, duration, monster, player, BleedingDamage(debuff, hitInfo)),
                EliteDebuffKind.Poisoning => new BleedingMagicEffect(new ConstantElement(1), definition, duration, monster, player, attributes[Stats.MaximumHealth] * Rand.NextInt(debuff.Value, Math.Max(debuff.Value, debuff.MaximumValue) + 1) / 100f),
                EliteDebuffKind.DecreaseAttack => new MagicEffect(duration, definition, new MagicEffect.ElementWithTarget(new ConstantElement(debuff.Value / 100f), Stats.WeaknessPhysDmgDecrement)),
                EliteDebuffKind.DecreaseAttackSpeed => new MagicEffect(duration, definition, new MagicEffect.ElementWithTarget(new ConstantElement(-debuff.Value), Stats.AttackSpeed)),
                _ => new MagicEffect(duration, definition, new MagicEffect.ElementWithTarget(new ConstantElement(debuff.Value / 100f), Stats.InnovationDefDecrement)),
            };

            await player.MagicEffectList.AddEffectAsync(effect).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Finds the first zone which a walk enters from outside.
    /// </summary>
    /// <param name="mapNumber">The map number.</param>
    /// <param name="steps">The steps.</param>
    /// <returns>The zone, or <c>null</c>.</returns>
    internal EliteZoneDefinition? FindEnteredZone(short mapNumber, ReadOnlySpan<WalkingStep> steps)
    {
        foreach (var step in steps)
        {
            if (this.GetZoneAt(mapNumber, step.To) is { } zone && !zone.Contains(step.From))
            {
                return zone;
            }
        }

        return null;
    }

    private static float BleedingDamage(EliteDebuff debuff, HitInfo hitInfo)
    {
        var damage = hitInfo.TotalDamage * debuff.Value / 100f;
        return debuff.MaximumValue > 0 ? Math.Min(damage, debuff.MaximumValue) : damage;
    }

    private static DateTime WeekStart(DateTime now, DayOfWeek startsOn)
    {
        var days = ((int)now.DayOfWeek - (int)startsOn + 7) % 7;
        return now.Date.AddDays(-days);
    }

    private async Task ApplyDebuffsSafeAsync(Player player, Monster monster, HitInfo hitInfo)
    {
        try
        {
            await this.ApplyDebuffsAsync(player, monster, hitInfo).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            player.Logger.LogError(ex, "The debuffs of the elite {Monster} failed.", monster);
        }
    }
}
