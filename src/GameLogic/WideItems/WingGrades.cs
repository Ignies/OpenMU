// <copyright file="WingGrades.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.WideItems;

using MUnique.OpenMU.DataModel.Configuration.Items;

/// <summary>
/// Rolls the grade options of the fourth and fifth wings.
/// </summary>
public static class WingGrades
{
    /// <summary>
    /// Rolls the grade options of the item: per option definition of grade options, the first option always, each
    /// further one by its add chance up to the maximum count, with a grade by the rates (option level = grade + 1).
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="rates">The weights of the grades, or <c>null</c> for equal weights.</param>
    /// <param name="randomizer">The randomizer.</param>
    /// <param name="createLink">Creates an option link, e.g. in the persistence context of a persistent item.</param>
    public static void Roll(Item item, IReadOnlyList<int>? rates, IRandomizer randomizer, Func<ItemOptionLink>? createLink = null)
    {
        foreach (var definition in item.Definition!.PossibleItemOptions.Where(IsGradeDefinition))
        {
            var index = item.ItemOptions.Count(o => o.ItemOption?.OptionType == Season21ItemOptionTypes.WingGrade);
            for (var i = index; i < definition.MaximumOptionsPerItem; i++)
            {
                if (i > 0 && !randomizer.NextRandomBool(definition.AddChance))
                {
                    continue;
                }

                var option = definition.PossibleOptions.Where(p => item.ItemOptions.All(o => o.ItemOption != p)).SelectRandom(randomizer);
                if (option is null)
                {
                    break;
                }

                var link = createLink?.Invoke() ?? new ItemOptionLink();
                link.ItemOption = option;
                link.Level = RollLevel(option, rates, randomizer);
                link.Index = index++;
                item.ItemOptions.Add(link);
            }
        }
    }

    /// <summary>
    /// Determines whether the option definition holds grade options.
    /// </summary>
    /// <param name="definition">The definition.</param>
    /// <returns><c>true</c>, if it holds grade options.</returns>
    public static bool IsGradeDefinition(ItemOptionDefinition definition)
        => definition.PossibleOptions.Any(p => p.OptionType == Season21ItemOptionTypes.WingGrade);

    /// <summary>
    /// Rolls the level of a grade option by the rates.
    /// </summary>
    /// <param name="option">The option.</param>
    /// <param name="rates">The weights of the grades, or <c>null</c> for equal weights.</param>
    /// <param name="randomizer">The randomizer.</param>
    /// <returns>The level, the grade + 1.</returns>
    public static int RollLevel(IncreasableItemOption option, IReadOnlyList<int>? rates, IRandomizer randomizer)
    {
        var levels = option.LevelDependentOptions.Select(l => l.Level).OrderBy(l => l).ToList();
        if (levels.Count <= 1)
        {
            return levels.Count == 0 ? 1 : levels[0];
        }

        var weights = levels.Select((_, i) => rates is null ? 1 : (i < rates.Count ? rates[i] : 0)).ToList();
        return weights.Sum() <= 0 ? levels[0] : levels.SelectWeightedRandom(weights, randomizer);
    }
}
