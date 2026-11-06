// <copyright file="Season21ChaosCastleSurvivalData.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.Interfaces;

/// <summary>
/// The castle of the Chaos Castle Survival of the Fittest of Season 21: map 97 (<c>Terrain98.att</c>, gate 443) with the two
/// monsters of S21Emu <c>Monster/MonsterSetBase/097 - Chaos Castle Final.xml</c> at their places.
/// </summary>
internal static class Season21ChaosCastleSurvivalData
{
    /// <summary>
    /// The number of the map.
    /// </summary>
    internal const byte MapNumber = 97;

    /// <summary>
    /// The places of the monsters (<c>EVENT</c> section of the data).
    /// </summary>
    private static readonly (short Monster, byte X, byte Y)[] Spawns =
    [
        (644, 26, 105), (645, 26, 98), (644, 28, 90), (645, 27, 86), (644, 28, 82), (645, 26, 79), (644, 34, 78),
        (645, 33, 81), (644, 39, 78), (645, 26, 77), (644, 38, 80), (645, 43, 77), (644, 38, 86), (645, 43, 84),
        (644, 41, 82), (645, 27, 94), (644, 32, 91), (645, 29, 84), (644, 42, 90), (645, 44, 87), (644, 30, 78),
        (645, 38, 92), (644, 40, 98), (645, 29, 101), (644, 33, 103), (645, 38, 89), (644, 42, 105), (645, 42, 99),
        (644, 35, 105), (645, 29, 93), (644, 28, 97), (645, 26, 84), (644, 28, 79), (644, 25, 82), (645, 29, 76),
        (644, 33, 76), (645, 35, 79), (644, 31, 80), (645, 41, 79), (644, 38, 76), (645, 36, 81), (644, 39, 82),
        (645, 41, 86), (644, 42, 94), (645, 35, 92), (644, 34, 90), (645, 38, 101), (644, 30, 105), (645, 26, 102),
        (644, 28, 100), (645, 38, 106), (644, 35, 102), (645, 42, 102), (644, 43, 96), (645, 30, 88), (644, 25, 91),
        (645, 29, 91), (644, 30, 98), (645, 41, 93), (644, 41, 89), (645, 43, 91), (644, 39, 95), (645, 32, 101),
        (644, 29, 103), (645, 33, 105), (644, 25, 97), (645, 24, 94), (644, 27, 88), (645, 32, 94), (644, 32, 89),
        (645, 36, 92), (644, 37, 89), (645, 40, 91), (644, 38, 99), (645, 40, 101), (644, 43, 81), (645, 36, 76),
        (644, 42, 75), (645, 26, 75), (644, 24, 80), (645, 30, 82), (644, 24, 89), (645, 24, 86), (644, 24, 101),
        (645, 37, 104), (644, 41, 96), (645, 39, 87), (644, 41, 76), (645, 38, 84), (644, 38, 96), (645, 30, 95),
        (644, 32, 78), (645, 29, 86), (644, 27, 104), (645, 40, 104), (644, 44, 102), (645, 43, 98), (644, 39, 93),
        (645, 41, 84),
    ];

    /// <summary>
    /// Gets the map with its monsters and entrance gate. The monsters are spawned by the game at its start.
    /// </summary>
    public static Season21MapSet Castle { get; } = new(
        GameVersion.Season9Episode1,
        0,
        0,
        [new(MapNumber, LocalizedString.FromResource(() => MapNames.ChaosCastleSurvival), 2, 98)],
        [
            new(644, LocalizedString.FromResource(() => MonsterNames.ChaosCastle15), Level: 111, Health: 20000, MinimumDamage: 300, MaximumDamage: 345, Defense: 245, AttackRate: 480, DefenseRate: 190, MoveRange: 50, AttackRange: 1, ViewRange: 7, MoveDelay: 400, AttackDelay: 2000, RespawnDelay: 10, Attribute: 2, Resistances: [7, 7, 7, 7, 0, 0, 0]),
            new(645, LocalizedString.FromResource(() => MonsterNames.ChaosCastle16), Level: 114, Health: 23000, MinimumDamage: 330, MaximumDamage: 380, Defense: 280, AttackRate: 520, DefenseRate: 200, MoveRange: 50, AttackRange: 6, ViewRange: 7, MoveDelay: 400, AttackDelay: 1600, RespawnDelay: 10, Attribute: 2, Resistances: [7, 7, 7, 7, 0, 0, 0]),
        ],
        [.. Spawns.Select(s => new Season21Spawn(s.Monster, s.X, s.Y, s.X, s.Y, 1, MapNumber))],
        [new(443, Season21GateKind.Spawn, MapNumber, 31, 88, 36, 95)],
        []);
}
