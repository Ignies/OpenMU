// <copyright file="Season21PlaceholderRows.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// The placeholder rows of the Season 21 server data which the data of this folder replaced (see Readme.md, "Placeholder
/// rows"), and the repair of a database which still has them.
/// </summary>
internal static class Season21PlaceholderRows
{
    /// <summary>
    /// Gets the rows as the server data has them: number, level, health, damage, defense, attack rate and defense rate.
    /// </summary>
    public static IReadOnlyList<(short Number, int Level, int Health, int MinimumDamage, int MaximumDamage, int Defense, int AttackRate, int DefenseRate)> Rows { get; } =
    [
        (744, 772, 125000, 3100, 3200, 1500, 1950, 700),
        (745, 776, 120000, 1500, 1600, 1400, 1850, 2500),
        (834, 970, 1515000, 4900, 5040, 5000, 4400, 2050),
        (836, 1085, 1930000, 5300, 5400, 6150, 4670, 1700),
        (839, 1160, 800000, 5600, 5780, 3800, 5000, 2450),
        (840, 1195, 850000, 5800, 6000, 3950, 5200, 2600),
        (859, 1230, 650000, 4900, 5050, 3600, 4800, 2050),
        (860, 1238, 650000, 5050, 5100, 3800, 4850, 2080),
        (861, 1243, 650000, 5080, 5135, 3850, 4890, 2150),
        (862, 1248, 650000, 5090, 5150, 3870, 4920, 2350),
        (863, 1290, 650000, 5095, 5180, 3895, 4980, 2400),
        (864, 1298, 700000, 5235, 5600, 4000, 5050, 2475),
        (865, 1300, 850000, 5750, 5800, 4300, 5130, 2550),
        (905, 1520, 106000, 5410, 5600, 4920, 5100, 2550),
        (945, 2028, 4943000, 20800, 21500, 9650, 5300, 5790),
        (946, 2030, 5025000, 21800, 22500, 10010, 5300, 5660),
        (947, 2030, 5050000, 22200, 22900, 10020, 5100, 5710),
        (972, 2235, 8570000, 36500, 37000, 14600, 15750, 8450),
        (973, 2240, 8620000, 35000, 35500, 14800, 16750, 8244),
        (974, 2240, 8595000, 35750, 36250, 14700, 16500, 8624),
    ];

    /// <summary>
    /// Gets the values of Season 6 monsters which S21Emu <c>Monster.xml</c> changed: number, attribute, the Season 6 value,
    /// the Season 21 value (Phoenix of Darkness 75,000 health, Death Spirit level 118).
    /// </summary>
    public static IReadOnlyList<(short Number, AttributeDefinition Attribute, float Season6, float Season21)> ClassicRows { get; } =
    [
        (77, Stats.MaximumHealth, 95000, 75000),
        (410, Stats.Level, 119, 118),
    ];

    /// <summary>
    /// Replaces the numbers of each monster which still has its placeholder row by the ones of the data, and the Season 6
    /// values of <see cref="ClassicRows"/>; a monster with any other number, e.g. one the operator changed, stays.
    /// </summary>
    /// <param name="gameConfiguration">The game configuration.</param>
    public static void Repair(GameConfiguration gameConfiguration)
    {
        foreach (var (number, attribute, season6, season21) in ClassicRows)
        {
            if (gameConfiguration.Monsters.FirstOrDefault(m => m.Number == number)?.Attributes.FirstOrDefault(a => a.AttributeDefinition == attribute) is { } stored
                && stored.Value == season6)
            {
                stored.Value = season21;
            }
        }

        var current = Season21MapData.All.SelectMany(set => set.Monsters)
            .Concat(Season21EliteZoneData.Monsters.Select(m => m.Row))
            .GroupBy(row => row.Number)
            .ToDictionary(g => g.Key, g => g.First());
        foreach (var placeholder in Rows)
        {
            if (gameConfiguration.Monsters.FirstOrDefault(m => m.Number == placeholder.Number) is not { } monster
                || !current.TryGetValue(placeholder.Number, out var row))
            {
                continue;
            }

            var values = new (AttributeDefinition Attribute, int Placeholder, int Value)[]
            {
                (Stats.Level, placeholder.Level, row.Level),
                (Stats.MaximumHealth, placeholder.Health, row.Health),
                (Stats.MinimumPhysBaseDmg, placeholder.MinimumDamage, row.MinimumDamage),
                (Stats.MaximumPhysBaseDmg, placeholder.MaximumDamage, row.MaximumDamage),
                (Stats.DefenseBase, placeholder.Defense, row.Defense),
                (Stats.AttackRatePvm, placeholder.AttackRate, row.AttackRate),
                (Stats.DefenseRatePvm, placeholder.DefenseRate, row.DefenseRate),
            };
            var attributes = values.Select(v => (Stored: monster.Attributes.FirstOrDefault(a => a.AttributeDefinition == v.Attribute), v.Placeholder, v.Value)).ToList();
            if (attributes.Any(a => a.Stored is null || a.Stored.Value != a.Placeholder))
            {
                continue;
            }

            foreach (var (stored, _, value) in attributes)
            {
                stored!.Value = value;
            }
        }
    }
}
