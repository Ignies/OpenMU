// <copyright file="Season21LuckySets.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.Persistence.Initialization.Items;

/// <summary>
/// The set options of the lucky items of S21Emu <c>Item/SetItem - Settings</c>: <c>SetItemType.xml</c> gives each lucky
/// armor piece (groups 7 to 11, one number per set) its set, <c>SetItemOption.xml</c> the options. They always apply, from
/// two different pieces: the first n - 1 options, all of them on the full set.
/// </summary>
internal static class Season21LuckySets
{
    private const short FirstSetNumber = 2100;

    /// <summary>
    /// The options of the first lucky sets: health 30, skill damage 25, defense 20 and double damage 10 % by the count; the
    /// critical and excellent damage chances 10 % and their damage 10 on the full set.
    /// </summary>
    private static readonly (AttributeDefinition Attribute, float Value, AggregateType Aggregate)[] FirstSetOptions =
    [
        (Stats.MaximumHealth, 30, AggregateType.AddRaw),
        (Stats.SkillDamageBonus, 25, AggregateType.AddRaw),
        (Stats.DefenseBase, 20, AggregateType.AddFinal),
        (Stats.DoubleDamageChance, 0.1f, AggregateType.AddRaw),
        (Stats.CriticalDamageChance, 0.1f, AggregateType.AddRaw),
        (Stats.ExcellentDamageChance, 0.1f, AggregateType.AddRaw),
        (Stats.CriticalDamageBonus, 10, AggregateType.AddRaw),
        (Stats.ExcellentDamageBonus, 10, AggregateType.AddRaw),
    ];

    /// <summary>
    /// The options of the second lucky sets: health 50, defense 40, double damage 15 % and the critical damage chance 15 %
    /// by the count; the excellent damage chance 15 %, critical and excellent damage 20 and 5 % to ignore the defense on the
    /// full set. The four-piece sets (Amis, Trite) list the same options, the fourth as a full set option.
    /// </summary>
    private static readonly (AttributeDefinition Attribute, float Value, AggregateType Aggregate)[] SecondSetOptions =
    [
        (Stats.MaximumHealth, 50, AggregateType.AddRaw),
        (Stats.DefenseBase, 40, AggregateType.AddFinal),
        (Stats.DoubleDamageChance, 0.15f, AggregateType.AddRaw),
        (Stats.CriticalDamageChance, 0.15f, AggregateType.AddRaw),
        (Stats.ExcellentDamageChance, 0.15f, AggregateType.AddRaw),
        (Stats.CriticalDamageBonus, 20, AggregateType.AddRaw),
        (Stats.ExcellentDamageBonus, 20, AggregateType.AddRaw),
        (Stats.DefenseIgnoreChance, 0.05f, AggregateType.AddRaw),
    ];

    /// <summary>
    /// The sets: the index of <c>SetItemOption.xml</c>, the name, the item number of the pieces and the options.
    /// </summary>
    private static readonly (short Index, string Name, short ItemNumber, (AttributeDefinition Attribute, float Value, AggregateType Aggregate)[] Options)[] Sets =
    [
        (39, "Moros", 62, FirstSetOptions),
        (40, "Dione", 63, FirstSetOptions),
        (41, "Hades", 64, FirstSetOptions),
        (42, "Ophion", 65, FirstSetOptions),
        (43, "Meter", 66, SecondSetOptions),
        (44, "Hegaton", 67, SecondSetOptions),
        (45, "Castol", 68, SecondSetOptions),
        (46, "Taros", 69, SecondSetOptions),
        (47, "Nemesis", 70, SecondSetOptions),
        (48, "Amis", 71, SecondSetOptions),
        (49, "Trite", 72, SecondSetOptions),
        (76, "Dria", 96, FirstSetOptions),
        (77, "Deruff", 97, SecondSetOptions),
        (146, "Perth", 124, FirstSetOptions),
        (147, "Peihu", 125, SecondSetOptions),
        (165, "Metis", 186, FirstSetOptions),
        (166, "Ceres", 187, SecondSetOptions),
        (202, "Gunner Lucky 1st", 218, FirstSetOptions),
        (203, "Gunner Lucky 2nd", 219, SecondSetOptions),
        (222, "Heimdall", 249, FirstSetOptions),
        (223, "Minerva", 250, SecondSetOptions),
        (235, "Theia", 267, FirstSetOptions),
        (236, "Selene", 268, SecondSetOptions),
        (281, "Legion", 315, FirstSetOptions),
        (282, "Gluttony", 316, SecondSetOptions),
        (312, "Aether", 352, FirstSetOptions),
        (313, "Lachesis", 353, SecondSetOptions),
        (378, "Hardin", 408, FirstSetOptions),
        (379, "Elio", 409, SecondSetOptions),
    ];

    /// <summary>
    /// Adds the sets which aren't there yet, each with the pieces which the configuration has.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public static void Add(IContext context, GameConfiguration gameConfiguration)
    {
        var optionType = gameConfiguration.ItemOptionTypes.First(t => t == ItemOptionTypes.AncientOption);
        foreach (var (index, name, itemNumber, setOptions) in Sets)
        {
            var pieces = gameConfiguration.Items.Where(i => i.Number == itemNumber && i.Group is >= (byte)ItemGroups.Helm and <= (byte)ItemGroups.Boots).ToList();
            if (pieces.Count == 0 || gameConfiguration.ItemSetGroups.Any(g => g.Name.ValueInNeutralLanguage == name))
            {
                continue;
            }

            var set = context.CreateNew<ItemSetGroup>();
            set.SetGuid(FirstSetNumber, index);
            set.Name = new LocalizedString(name);
            set.AlwaysApplies = true;
            set.CountDistinct = true;
            set.MinimumItemCount = 2;
            var options = context.CreateNew<ItemOptionDefinition>();
            options.SetGuid(ItemOptionDefinitionNumbers.AncientOption, (short)(FirstSetNumber + index));
            options.Name = new LocalizedString($"{name} Lucky Set");
            set.Options = options;
            var number = 1;
            foreach (var (attribute, value, aggregate) in setOptions)
            {
                var option = context.CreateNew<IncreasableItemOption>();
                option.SetGuid(ItemOptionDefinitionNumbers.AncientOption, (short)(FirstSetNumber + index), (byte)number);
                option.Number = number++;
                option.OptionType = optionType;
                option.PowerUpDefinition = context.CreateNew<PowerUpDefinition>();
                option.PowerUpDefinition.TargetAttribute = attribute.GetPersistent(gameConfiguration);
                option.PowerUpDefinition.Boost = context.CreateNew<PowerUpDefinitionValue>();
                option.PowerUpDefinition.Boost.ConstantValue.AggregateType = aggregate;
                option.PowerUpDefinition.Boost.ConstantValue.Value = value;
                options.PossibleOptions.Add(option);
            }

            foreach (var piece in pieces)
            {
                var item = context.CreateNew<ItemOfItemSet>();
                item.SetGuid(FirstSetNumber, index, piece.Group);
                item.ItemDefinition = piece;
                item.ItemSetGroup = set;
                set.Items.Add(item);
                piece.PossibleItemSetGroups.Add(set);
            }

            gameConfiguration.ItemOptions.Add(options);
            gameConfiguration.ItemSetGroups.Add(set);
        }
    }
}
