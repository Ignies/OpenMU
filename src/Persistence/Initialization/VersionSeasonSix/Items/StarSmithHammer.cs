// <copyright file="StarSmithHammer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Items;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.Persistence.Initialization.Items;
using MUnique.OpenMU.Persistence.Initialization.Properties;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

/// <summary>
/// Initializer for the Star Smith's Hammer (2/1000), the tool of the star smithing, which Hanzo the Blacksmith sells.
/// It's a light one-handed mace like the Mace (2/0), which every character class can wear: no options, no level up, no drop.
/// </summary>
internal class StarSmithHammer : InitializerBase
{
    /// <summary>
    /// The item group of the hammer, the maces and scepters.
    /// </summary>
    internal const byte ItemGroup = 2;

    /// <summary>
    /// The item number of the hammer, the first of the fork's own numbers of the group.
    /// </summary>
    internal const short ItemNumber = 1000;

    /// <summary>
    /// The monster number of Hanzo the Blacksmith in Lorencia.
    /// </summary>
    internal const short HanzoNumber = 251;

    /// <summary>
    /// The value of the hammer. The value based price, floor(value^2 * 10 / 12), gives 1,001,013, which the merchant rounds to 1,001,000 Zen.
    /// </summary>
    internal const int HammerValue = 1096;

    private const byte HammerDurability = 255;
    private const int MinimumDamage = 4;
    private const int MaximumDamage = 8;

    /// <summary>
    /// The attack speed of the Mace (2/0), if the configuration doesn't have it.
    /// </summary>
    private const int MaceAttackSpeed = 15;

    /// <summary>
    /// The drop level of the Mace (2/0), if the configuration doesn't have it.
    /// </summary>
    private const byte MaceDropLevel = 7;

    /// <summary>
    /// Initializes a new instance of the <see cref="StarSmithHammer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public StarSmithHammer(IContext context, GameConfiguration gameConfiguration)
        : base(context, gameConfiguration)
    {
    }

    /// <inheritdoc />
    /// <remarks>It runs after the NPCs are created, because it puts the hammer on sale at Hanzo.</remarks>
    public override void Initialize()
    {
        Apply(this.Context, this.GameConfiguration);
    }

    /// <summary>
    /// Creates the hammer and Hanzo's offer, each if it doesn't exist yet.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    internal static void Apply(IContext context, GameConfiguration gameConfiguration)
    {
        var hammer = gameConfiguration.Items.FirstOrDefault(IsHammer) ?? CreateHammer(context, gameConfiguration);
        AddToHanzo(context, gameConfiguration, hammer);
    }

    /// <summary>
    /// Determines whether the definition is the one of the hammer.
    /// </summary>
    /// <param name="definition">The item definition.</param>
    /// <returns><c>true</c> if it's the hammer.</returns>
    internal static bool IsHammer(ItemDefinition definition) => definition.Group == ItemGroup && definition.Number == ItemNumber;

    private static ItemDefinition CreateHammer(IContext context, GameConfiguration gameConfiguration)
    {
        var mace = gameConfiguration.Items.FirstOrDefault(item => item is { Group: ItemGroup, Number: 0 });
        var hammer = context.CreateNew<ItemDefinition>();
        hammer.Group = ItemGroup;
        hammer.Number = ItemNumber;
        hammer.Name = LocalizedString.FromResource(() => ItemNames.StarSmithsHammer);
        hammer.Width = mace?.Width ?? 1;
        hammer.Height = mace?.Height ?? 3;

        // It doesn't drop, but the excellent and ancient bonuses divide by the drop level.
        hammer.DropLevel = mace?.DropLevel ?? MaceDropLevel;
        hammer.ItemSlot = mace?.ItemSlot ?? gameConfiguration.ItemSlotTypes.First(type => type.ItemSlots.Contains(0) && type.ItemSlots.Contains(1));
        hammer.Durability = HammerDurability;
        hammer.MaximumItemLevel = 0;
        hammer.MaximumSockets = 0;
        hammer.DropsFromMonsters = false;
        hammer.Value = HammerValue;
        hammer.SetGuid(hammer.Group, hammer.Number);
        foreach (var characterClass in gameConfiguration.CharacterClasses)
        {
            hammer.QualifiedCharacters.Add(characterClass);
        }

        var attackSpeed = mace?.BasePowerUpAttributes.FirstOrDefault(p => p.TargetAttribute == Stats.AttackSpeedByWeapon)?.BaseValue ?? MaceAttackSpeed;
        hammer.BasePowerUpAttributes.Add(CreatePowerUp(context, gameConfiguration, Stats.MinimumPhysBaseDmgByWeapon, MinimumDamage));
        hammer.BasePowerUpAttributes.Add(CreatePowerUp(context, gameConfiguration, Stats.MaximumPhysBaseDmgByWeapon, MaximumDamage));
        hammer.BasePowerUpAttributes.Add(CreatePowerUp(context, gameConfiguration, Stats.AttackSpeedByWeapon, attackSpeed));
        hammer.BasePowerUpAttributes.Add(CreatePowerUp(context, gameConfiguration, Stats.EquippedWeaponCount, 1));
        hammer.BasePowerUpAttributes.Add(CreatePowerUp(context, gameConfiguration, Stats.DoubleWieldWeaponCount, 1));
        gameConfiguration.Items.Add(hammer);

        new ItemPriceDefinitions(context, gameConfiguration).AssignPrice(hammer);
        return hammer;
    }

    private static ItemBasePowerUpDefinition CreatePowerUp(IContext context, GameConfiguration gameConfiguration, AttributeDefinition attribute, float value)
    {
        var powerUp = context.CreateNew<ItemBasePowerUpDefinition>();
        powerUp.TargetAttribute = attribute.GetPersistent(gameConfiguration);
        powerUp.BaseValue = value;
        powerUp.AggregateType = AggregateType.AddRaw;
        return powerUp;
    }

    private static void AddToHanzo(IContext context, GameConfiguration gameConfiguration, ItemDefinition hammer)
    {
        if (gameConfiguration.Monsters.FirstOrDefault(monster => monster.Number == HanzoNumber) is not { } hanzo)
        {
            return;
        }

        var store = hanzo.MerchantStore ??= context.CreateNew<ItemStorage>();
        if (store.Items.Any(item => item.Definition is { } definition && IsHammer(definition)))
        {
            return;
        }

        var grid = new bool[Season21ShopsInitializer.Columns, Season21ShopsInitializer.Rows];
        foreach (var item in store.Items.Where(item => item.Definition is not null))
        {
            Occupy(grid, item);
        }

        if (Season21ShopsInitializer.FindPlace(grid, hammer) is not { } slot)
        {
            return;
        }

        var offer = context.CreateNew<Item>();
        offer.Definition = hammer;
        offer.ItemSlot = slot;
        offer.Durability = hammer.Durability;
        store.Items.Add(offer);
    }

    private static void Occupy(bool[,] grid, Item item)
    {
        var columns = grid.GetLength(0);
        var rows = grid.GetLength(1);
        var x = item.ItemSlot % columns;
        var y = item.ItemSlot / columns;
        for (var dy = 0; dy < Math.Max((int)item.Definition!.Height, 1) && y + dy < rows; dy++)
        {
            for (var dx = 0; dx < Math.Max((int)item.Definition.Width, 1) && x + dx < columns; dx++)
            {
                grid[x + dx, y + dy] = true;
            }
        }
    }
}
