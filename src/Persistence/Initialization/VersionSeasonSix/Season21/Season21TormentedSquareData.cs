// <copyright file="Season21TormentedSquareData.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.Interfaces;

/// <summary>
/// The Tormented Square of Season 21, from S21Emu <c>Event/TormentedSquare/TormentedSquare.xml</c> and
/// <c>TormentedSquare_Monsters.xml</c>, on map 102 (<c>Terrain103.att</c>, gate 474).
/// </summary>
internal static class Season21TormentedSquareData
{
    /// <summary>
    /// The number of the map.
    /// </summary>
    internal const byte MapNumber = 102;

    /// <summary>
    /// Gets the map with its entrance gate.
    /// </summary>
    public static Season21MapSet Arena { get; } = new(
        GameVersion.Season9Episode1,
        0,
        0,
        [new(MapNumber, LocalizedString.FromResource(() => MapNames.TormentedSquare), 0, 103)],
        [
            new(654, LocalizedString.FromResource(() => MonsterNames.GoldenTantalos), Level: 115, Health: 800000, MinimumDamage: 400, MaximumDamage: 500, Defense: 400, AttackRate: 3000, DefenseRate: 600, MoveRange: 3, AttackRange: 2, ViewRange: 7, MoveDelay: 400, AttackDelay: 1400, RespawnDelay: 0, Attribute: 2, Resistances: [0, 0, 0, 0, 0, 0, 0]),
            new(655, LocalizedString.FromResource(() => MonsterNames.GoldenErohim), Level: 120, Health: 1200000, MinimumDamage: 500, MaximumDamage: 600, Defense: 600, AttackRate: 3000, DefenseRate: 800, MoveRange: 3, AttackRange: 6, ViewRange: 8, MoveDelay: 650, AttackDelay: 1600, RespawnDelay: 0, Attribute: 2, Resistances: [0, 0, 0, 0, 0, 0, 0]),
            new(656, LocalizedString.FromResource(() => MonsterNames.GoldenHellMaine), Level: 125, Health: 3000000, MinimumDamage: 600, MaximumDamage: 700, Defense: 700, AttackRate: 3000, DefenseRate: 1300, MoveRange: 6, AttackRange: 5, ViewRange: 10, MoveDelay: 600, AttackDelay: 2000, RespawnDelay: 0, Attribute: 2, Resistances: [0, 0, 0, 0, 0, 0, 0]),
        ],
        [],
        [new(474, Season21GateKind.Spawn, MapNumber, 125, 126, 130, 130)],
        []);

    /// <summary>
    /// Gets the rounds: monster, count, kind (0 normal, 1 boss), points of a kill and of the cleared round.
    /// </summary>
    public static IReadOnlyList<(short Monster, int Count, byte Kind, int Points, int ClearPoints)> Rounds { get; } =
    [
        (572, 130, 0, 300, 3000), (344, 130, 0, 600, 6000), (549, 130, 0, 900, 9000), (575, 130, 0, 1200, 12000),
        (315, 50, 1, 3000, 15000), (550, 130, 0, 1500, 18000), (552, 130, 0, 1800, 21000), (314, 130, 0, 2100, 24000),
        (335, 130, 0, 2400, 27000), (557, 50, 1, 10000, 30000), (554, 130, 0, 2700, 33000), (336, 130, 0, 3000, 36000),
        (337, 130, 0, 3300, 39000), (556, 130, 0, 5000, 42000), (340, 50, 1, 200000, 45000),
    ];

    /// <summary>
    /// Gets the golden monsters of the rounds 1-5, 6-10 and 11-15: ten of them with their points, spawn rate 100 %.
    /// </summary>
    public static IReadOnlyList<(short Monster, int Points)> GoldenMonsters { get; } = [(654, 10000), (655, 30000), (656, 90000)];

    /// <summary>
    /// Gets the spawn points: kind (0 normal, 1 boss, 2 golden), x and y.
    /// </summary>
    public static IReadOnlyList<(byte Kind, byte X, byte Y)> SpawnPoints { get; } =
    [
        (2, 99, 105), (2, 99, 127), (2, 100, 150), (2, 118, 106), (2, 118, 120), (0, 118, 119), (2, 118, 136), (2, 118, 150),
        (2, 137, 105), (2, 138, 129), (2, 137, 151), (1, 107, 116), (0, 107, 115), (1, 107, 140), (1, 131, 116), (0, 131, 115),
        (1, 132, 140), (1, 118, 127), (1, 128, 128), (1, 109, 128), (0, 109, 127), (1, 109, 106), (1, 110, 149), (1, 128, 149),
        (0, 128, 148), (1, 127, 106), (1, 142, 117), (1, 143, 142), (0, 143, 141), (1, 97, 140), (1, 96, 116), (1, 147, 105),
        (1, 147, 129), (0, 147, 128), (1, 148, 150), (1, 123, 115), (1, 124, 142), (0, 124, 141), (1, 114, 142), (1, 113, 114),
        (1, 150, 117), (1, 150, 141), (0, 150, 140), (1, 94, 152), (1, 93, 101), (1, 114, 100), (1, 130, 100), (0, 130, 99),
        (1, 143, 100), (1, 143, 153), (1, 123, 154), (1, 106, 154), (0, 106, 153), (1, 92, 128), (1, 102, 134), (1, 112, 134),
        (0, 112, 133), (1, 101, 120), (0, 101, 119), (1, 111, 121), (1, 133, 122), (0, 133, 121), (1, 139, 136), (1, 126, 134),
        (0, 126, 133), (0, 102, 112), (0, 102, 111), (0, 108, 112), (0, 105, 103), (0, 113, 109), (0, 113, 108), (0, 118, 115),
        (0, 118, 114), (0, 124, 111), (0, 130, 111), (0, 130, 110), (0, 136, 111), (0, 143, 111), (0, 146, 124), (0, 146, 123),
        (0, 141, 124), (0, 127, 121), (0, 123, 121), (0, 123, 120), (0, 123, 126), (0, 105, 124), (0, 105, 123), (0, 105, 131),
        (0, 107, 134), (0, 107, 133), (0, 119, 141), (0, 121, 131), (0, 132, 133), (0, 133, 129), (0, 133, 128), (0, 137, 142),
        (0, 130, 144), (0, 140, 148), (0, 144, 147), (0, 144, 148), (0, 145, 137), (0, 145, 131), (0, 145, 130), (0, 150, 135),
        (0, 123, 148), (0, 105, 148), (0, 103, 142), (0, 103, 141), (0, 101, 140), (0, 97, 134), (0, 97, 133), (0, 97, 148),
        (0, 100, 145), (0, 99, 155), (0, 112, 154), (0, 117, 154), (0, 131, 154), (0, 131, 153), (0, 137, 154), (0, 137, 117),
        (0, 137, 100), (0, 122, 106), (0, 122, 105), (0, 120, 102), (0, 124, 101), (0, 110, 100), (0, 113, 105), (0, 98, 100),
        (0, 103, 101), (0, 95, 110),
    ];

    /// <summary>
    /// Creates the definition of the rounds.
    /// </summary>
    /// <returns>The definition.</returns>
    public static RoundsEventDefinition CreateDefinition() => new()
    {
        RoundDuration = TimeSpan.FromMinutes(1),
        PauseAfterRound = TimeSpan.FromSeconds(10),
        PauseAfterBossRound = TimeSpan.FromSeconds(20),
        PartyIsATeam = true,
        SpawnPoints = SpawnPoints.Select(p => new EventSpawnPoint { Kind = p.Kind, X = p.X, Y = p.Y, Range = 8 }).ToList(),
        Rounds = Rounds.Select((r, index) => new EventRound
        {
            MonsterNumber = r.Monster,
            Count = r.Count,
            SpawnKind = r.Kind,
            Points = r.Points,
            ClearPoints = r.ClearPoints,
            IsBossRound = r.Kind == 1,
            BonusMonsters = [new EventBonusMonster { MonsterNumber = GoldenMonsters[index / 5].Monster, Count = 10, SpawnKind = 2, Points = GoldenMonsters[index / 5].Points }],
        }).ToList(),
    };
}
