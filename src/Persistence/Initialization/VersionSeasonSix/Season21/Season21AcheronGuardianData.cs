// <copyright file="Season21AcheronGuardianData.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.Interfaces;

/// <summary>
/// The Protector of Acheron of Season 21, from S21Emu <c>Event/AcheronGuardian.dat</c> (30 minutes, 10 monsters an area
/// coming back every 60 seconds, 4:29, 12:00 and 23:06), <c>EventItemBag/164 - Protection Of Acheron.xml</c> and
/// <c>Move/Gate.xml</c>, on the event copies of Acheron (map 92) and Deventer (map 96).
/// </summary>
/// <remarks>
/// The data names the areas but not their monsters; the monsters of the area on the hunting map (the Devil Fairies,
/// Elemental Beasts and Knights of Alkmaar, Ubaid and Deventer) defend them, and a kill is worth 10 points.
/// </remarks>
internal static class Season21AcheronGuardianData
{
    /// <summary>
    /// The points of a kill (no data).
    /// </summary>
    internal const int KillPoints = 10;

    /// <summary>
    /// The points a player needs for the reward (no data: ten kills).
    /// </summary>
    internal const int PointsForReward = 100;

    /// <summary>
    /// The bag of the reward.
    /// </summary>
    internal const short RewardBag = 164;

    /// <summary>
    /// Gets the maps with their entrance gates.
    /// </summary>
    public static Season21MapSet Maps { get; } = new(
        GameVersion.Season9Episode1,
        0,
        0,
        [
            new(92, LocalizedString.FromResource(() => MapNames.AcheronArca), 91, 93),
            new(96, LocalizedString.FromResource(() => MapNames.DeventerArca), 95, 97),
        ],
        [],
        [],
        [
            new(426, Season21GateKind.Spawn, 92, 51, 190, 60, 197),
            new(440, Season21GateKind.Spawn, 96, 19, 104, 20, 105),
        ],
        []);

    /// <summary>
    /// Gets the items of the reward bag, as <c>group * 512 + number</c>.
    /// </summary>
    public static IReadOnlyList<int> RewardItems { get; } = [6352, 6353, 6354, 6355, 6356, 6357, 6358, 7210, 7181, 7182, 7184, 7190, 7615, 6833, 7513];

    /// <summary>
    /// Gets the areas by game level: 1 Acheron (Alkmaar 89/41, Ubaid 188/101), 2 Deventer (162/54), each with its three
    /// monsters.
    /// </summary>
    public static IReadOnlyList<(byte Map, IReadOnlyList<(byte X, byte Y, short[] Monsters)> Areas)> Levels { get; } =
    [
        (92, [(89, 41, [586, 587, 588]), (188, 101, [589, 590, 591])]),
        (96, [(162, 54, [608, 609, 610])]),
    ];

    /// <summary>
    /// Creates the waves of a game level: 30 waves of one minute; each brings 10 monsters to every area.
    /// </summary>
    /// <param name="areas">The areas.</param>
    /// <returns>The definition.</returns>
    public static RoundsEventDefinition CreateDefinition(IReadOnlyList<(byte X, byte Y, short[] Monsters)> areas)
    {
        var spawnPoints = areas.Select((area, index) => new EventSpawnPoint { Kind = (byte)index, X = area.X, Y = area.Y, Range = 6 }).ToList();
        return new RoundsEventDefinition
        {
            RoundDuration = TimeSpan.FromSeconds(60),
            PauseAfterRound = TimeSpan.Zero,
            PauseAfterBossRound = TimeSpan.Zero,
            PartyIsATeam = false,
            MinimumPointsForRewards = PointsForReward,
            SpawnPoints = spawnPoints,
            Rounds = Enumerable.Range(0, 30).Select(_ => CreateWave(areas)).ToList(),
        };
    }

    private static EventRound CreateWave(IReadOnlyList<(byte X, byte Y, short[] Monsters)> areas)
    {
        // Ten monsters an area: four of the first kind, three of the others.
        var first = areas[0].Monsters;
        var wave = new EventRound { MonsterNumber = first[0], Count = 4, SpawnKind = 0, Points = KillPoints };
        for (var index = 0; index < areas.Count; index++)
        {
            var monsters = areas[index].Monsters;
            foreach (var (monster, count) in monsters.Select((m, i) => (m, i == 0 ? 4 : 3)).Skip(index == 0 ? 1 : 0))
            {
                wave.BonusMonsters.Add(new EventBonusMonster { MonsterNumber = monster, Count = count, SpawnKind = (byte)index, Points = KillPoints });
            }
        }

        return wave;
    }
}
