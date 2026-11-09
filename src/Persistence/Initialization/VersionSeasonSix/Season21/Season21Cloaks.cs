// <copyright file="Season21Cloaks.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// The wing values of the cloaks of the Crusader (item group 21), which the item data of the server leaves out.
/// </summary>
/// <remarks>
/// Level, defense and durability are of the item table of the Season 21 client (<c>item.bmd</c>), the required level
/// is the one of the wings of the same kind in <c>Item.xml</c>, the damage rates are of <c>ItemOptionWing.txt</c>
/// (index 10752 + number). The options and level tables are the ones of a Season 6 wing with the same options.
/// </remarks>
internal static class Season21Cloaks
{
    /// <summary>
    /// The item group of the cloaks.
    /// </summary>
    public const byte Group = 21;

    private static readonly CloakRow[] Rows =
    [
        new(0, 100, 20, 200, 150, DefensePerLevel: 3, DamageIncrease: 12, DamageIncreasePerLevel: 2, DamageAbsorb: 12, Template: 0, Excellent: false), // Cloak of Protection
        new(1, 150, 45, 220, 400, DefensePerLevel: 4, DamageIncrease: 39, DamageIncreasePerLevel: 2, DamageAbsorb: 39, Template: 40, Excellent: true), // Cloak of Desire
        new(2, 150, 45, 220, 400, DefensePerLevel: 4, DamageIncrease: 39, DamageIncreasePerLevel: 2, DamageAbsorb: 39, Template: 40, Excellent: true), // [Bound] Cloak of Desire
        new(3, 153, 155, 220, 800, DefensePerLevel: 0, DamageIncrease: 55, DamageIncreasePerLevel: 1, DamageAbsorb: 43, Template: 40, Excellent: false), // Cloak of Sanctuary
        new(5, 156, 175, 220, 1200, DefensePerLevel: 0, DamageIncrease: 55, DamageIncreasePerLevel: 1, DamageAbsorb: 43, Template: 40, Excellent: false), // Cloak of Light
    ];

    /// <summary>
    /// Gives the defined cloaks the values they miss. A value which is set already, e.g. by an operator, stays.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public static void Apply(IContext context, GameConfiguration gameConfiguration)
    {
        ItemDefinition? Find(byte group, short number) => gameConfiguration.Items.FirstOrDefault(i => i.Group == group && i.Number == number);
        if (Find(12, 0) is not { } firstWing || Find(12, 3) is not { } secondWing || Find(12, 36) is not { } thirdWing)
        {
            return;
        }

        ItemLevelBonusTable? TableOf(ItemDefinition wing, AttributeDefinition attribute)
            => wing.BasePowerUpAttributes.FirstOrDefault(p => p.TargetAttribute == attribute)?.BonusPerLevelTable;

        foreach (var row in Rows)
        {
            if (Find(Group, row.Number) is not { } cloak || Find(12, row.Template) is not { } template)
            {
                continue;
            }

            cloak.ItemSlot ??= firstWing.ItemSlot;
            cloak.DropLevel = cloak.DropLevel == 0 ? row.DropLevel : cloak.DropLevel;
            cloak.Durability = cloak.Durability == 0 ? row.Durability : cloak.Durability;
            if (cloak.Requirements.All(r => r.Attribute != Stats.Level))
            {
                var requirement = context.CreateNew<AttributeRequirement>();
                requirement.Attribute = Stats.Level.GetPersistent(gameConfiguration);
                requirement.MinimumValue = row.Level;
                cloak.Requirements.Add(requirement);
            }

            if (cloak.BasePowerUpAttributes.Count == 0)
            {
                var defenseTable = row.DefensePerLevel switch
                {
                    3 => TableOf(firstWing, Stats.DefenseBase),
                    4 => TableOf(thirdWing, Stats.DefenseBase),
                    _ => null,
                };
                var increaseTable = TableOf(row.DamageIncreasePerLevel == 1 ? secondWing : firstWing, Stats.AttackDamageIncrease);
                Add(context, gameConfiguration, cloak, Stats.DefenseBase, row.DefenseValue, AggregateType.AddRaw, defenseTable);
                Add(context, gameConfiguration, cloak, Stats.AttackDamageIncrease, 1f + (row.DamageIncrease / 100f), AggregateType.Multiplicate, increaseTable);
                Add(context, gameConfiguration, cloak, Stats.DamageReceiveDecrement, 1f - (row.DamageAbsorb / 100f), AggregateType.Multiplicate, TableOf(firstWing, Stats.DamageReceiveDecrement));
                foreach (var source in firstWing.BasePowerUpAttributes.Where(p => p.TargetAttribute == Stats.CanFly || p.TargetAttribute == Stats.MovementSpeed || p.TargetAttribute == Stats.MovementSpeedUnderwater))
                {
                    Add(context, gameConfiguration, cloak, source.TargetAttribute!, source.BaseValue, source.AggregateType, null);
                }
            }

            if (!cloak.PossibleItemOptions.SelectMany(o => o.PossibleOptions).Any(p => p.OptionType == ItemOptionTypes.Luck || p.OptionType == ItemOptionTypes.Option || p.OptionType == ItemOptionTypes.Wing))
            {
                foreach (var option in template.PossibleItemOptions.Where(o => row.Excellent || o.PossibleOptions.All(p => p.OptionType != ItemOptionTypes.Wing)))
                {
                    cloak.PossibleItemOptions.Add(option);
                }
            }
        }
    }

    private static void Add(IContext context, GameConfiguration gameConfiguration, ItemDefinition item, AttributeDefinition attribute, float value, AggregateType aggregateType, ItemLevelBonusTable? table)
    {
        var powerUp = context.CreateNew<ItemBasePowerUpDefinition>();
        powerUp.TargetAttribute = attribute.GetPersistent(gameConfiguration);
        powerUp.BaseValue = value;
        powerUp.AggregateType = aggregateType;
        powerUp.BonusPerLevelTable = table;
        item.BasePowerUpAttributes.Add(powerUp);
    }

    /// <summary>
    /// The values of a cloak.
    /// </summary>
    /// <param name="Number">The item number.</param>
    /// <param name="DropLevel">The item level.</param>
    /// <param name="DefenseValue">The defense.</param>
    /// <param name="Durability">The durability.</param>
    /// <param name="Level">The required character level.</param>
    /// <param name="DefensePerLevel">The defense per item level of <c>ItemOptionWing.txt</c>: 3 as the first and second wings, 4 as the third, 0 none.</param>
    /// <param name="DamageIncrease">The damage increase in percent.</param>
    /// <param name="DamageIncreasePerLevel">The damage increase per item level in percent.</param>
    /// <param name="DamageAbsorb">The damage absorb in percent; 2 more per item level for every cloak.</param>
    /// <param name="Template">The Season 6 wing (group 12) whose options the cloak takes.</param>
    /// <param name="Excellent">Whether the cloak takes the excellent options of the third wings.</param>
    private sealed record CloakRow(short Number, byte DropLevel, int DefenseValue, byte Durability, int Level, int DefensePerLevel, int DamageIncrease, int DamageIncreasePerLevel, int DamageAbsorb, short Template, bool Excellent);
}
