// <copyright file="Season21InstanceZoneData.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.GameLogic.Rewards;
using MUnique.OpenMU.Interfaces;

/// <summary>
/// The Instance Zone (map 114) and the Labyrinth of Dimensions (map 115) of Season 21, from S21Emu
/// <c>MonsterSetBase/114 - Instance Zone.xml</c>, <c>Move/Gate.xml</c>, <c>Quest/QuestInstanceManager.xml</c> and
/// <c>Event/LabyrinthOfDimension</c>.
/// </summary>
internal static class Season21InstanceZoneData
{
    /// <summary>
    /// The number of the Instance Zone.
    /// </summary>
    internal const byte InstanceZoneMap = 114;

    /// <summary>
    /// The number of the Labyrinth of Dimensions.
    /// </summary>
    internal const byte LabyrinthMap = 115;

    /// <summary>
    /// The number of Loren Market, which the Instance Zone is linked to on foot.
    /// </summary>
    internal const byte LorenMarketMap = 79;

    /// <summary>
    /// The Portal of Dimension, the entrance of the labyrinth.
    /// </summary>
    internal const short PortalOfDimension = 758;

    /// <summary>
    /// Cent at the entrance of the fourth class trial.
    /// </summary>
    internal const short CentEntrance = 765;

    /// <summary>
    /// Eunice at the entrance of the fifth class trial.
    /// </summary>
    internal const short EuniceEntrance = 940;

    /// <summary>
    /// Gets the two maps, their NPCs, monsters and gates. The labyrinth is played in its first room (the data's rooms
    /// open by the server through the path of each floor, which isn't ported).
    /// </summary>
    public static Season21MapSet Maps { get; } = new(
        GameVersion.Season13Episode1,
        0,
        0,
        [
            new(InstanceZoneMap, LocalizedString.FromResource(() => MapNames.InstanceZone), 0, 115),
            new(LabyrinthMap, LocalizedString.FromResource(() => MapNames.LabyrinthOfDimensions), InstanceZoneMap, 116),
        ],
        [
            new(754, LocalizedString.FromResource(() => MonsterNames.GreatSwordOfKundun), Level: 150, Health: 80, MinimumDamage: 4, MaximumDamage: 5, Defense: 7, AttackRate: 9999, DefenseRate: 900, MoveRange: 2, AttackRange: 3, ViewRange: 10, MoveDelay: 300, AttackDelay: 1500, RespawnDelay: 7, Attribute: 1, Resistances: [254, 254, 254, 254, 0, 0, 0]),
            new(755, LocalizedString.FromResource(() => MonsterNames.SoldierOfKundun), Level: 150, Health: 65, MinimumDamage: 4, MaximumDamage: 5, Defense: 7, AttackRate: 9999, DefenseRate: 900, MoveRange: 2, AttackRange: 4, ViewRange: 10, MoveDelay: 300, AttackDelay: 1100, RespawnDelay: 7, Attribute: 1, Resistances: [254, 254, 254, 254, 0, 0, 0]),
            new(756, LocalizedString.FromResource(() => MonsterNames.LancerOfKundun), Level: 150, Health: 70, MinimumDamage: 4, MaximumDamage: 5, Defense: 7, AttackRate: 9999, DefenseRate: 900, MoveRange: 2, AttackRange: 4, ViewRange: 10, MoveDelay: 300, AttackDelay: 1300, RespawnDelay: 7, Attribute: 1, Resistances: [254, 254, 254, 254, 0, 0, 0]),
            new(757, LocalizedString.FromResource(() => MonsterNames.RulerOfKundun), Level: 105, Health: 600, MinimumDamage: 80, MaximumDamage: 85, Defense: 45, AttackRate: 1000, DefenseRate: 900, MoveRange: 3, AttackRange: 1, ViewRange: 3, MoveDelay: 400, AttackDelay: 1600, RespawnDelay: 120, Attribute: 1, Resistances: [254, 254, 254, 254, 0, 0, 0]),
        ],
        [],
        [
            new(537, Season21GateKind.Spawn, InstanceZoneMap, 238, 237, 239, 238),
            new(691, Season21GateKind.Target, InstanceZoneMap, 116, 236, 120, 238),
            new(680, Season21GateKind.Target, InstanceZoneMap, 169, 236, 174, 237),
            new(533, Season21GateKind.Target, InstanceZoneMap, 232, 56, 234, 59),
            new(532, Season21GateKind.Entrance, LorenMarketMap, 227, 55, 227, 60, Target: 533),
            new(535, Season21GateKind.Target, LorenMarketMap, 223, 56, 224, 59),
            new(534, Season21GateKind.Entrance, InstanceZoneMap, 228, 56, 229, 59, Target: 535),
            new(5000, Season21GateKind.Spawn, LabyrinthMap, 20, 11, 22, 13),
        ],
        [])
    {
        Npcs =
        [
            new(PortalOfDimension, LocalizedString.FromResource(() => MonsterNames.PortalOfDimension), InstanceZoneMap, 236, 59, Direction.SouthWest),
            new(CentEntrance, LocalizedString.FromResource(() => MonsterNames.CentEntrance), InstanceZoneMap, 240, 239, Direction.SouthWest),
            new(522, LocalizedString.FromResource(() => MonsterNames.AdviserJerinteu), InstanceZoneMap, 171, 239, Direction.SouthWest),
            new(EuniceEntrance, LocalizedString.FromResource(() => MonsterNames.EuniceEntrance), InstanceZoneMap, 118, 239, Direction.SouthWest),
        ],
    };

    /// <summary>
    /// Gets the multipliers of health, damage and defense of the 41 floors.
    /// </summary>
    public static IReadOnlyList<(float Health, float Damage, float Defense)> Floors { get; } =
    [
        (1.0f, 1.1f, 1.0f), (1.1f, 1.2f, 1.3f), (1.2f, 1.4f, 1.6f), (1.3f, 1.6f, 1.9f), (1.4f, 1.8f, 2.2f), (1.5f, 2.2f, 2.5f),
        (1.6f, 2.7f, 2.8f), (1.7f, 2.9f, 3.1f), (1.8f, 3.1f, 3.4f), (1.9f, 3.3f, 3.7f), (2.0f, 3.5f, 4.0f), (2.1f, 3.7f, 4.3f),
        (2.2f, 3.9f, 4.6f), (2.3f, 4.1f, 4.9f), (2.4f, 4.3f, 5.2f), (2.5f, 4.5f, 5.5f), (2.6f, 4.7f, 5.8f), (2.7f, 4.9f, 6.1f),
        (2.8f, 5.1f, 6.4f), (2.9f, 5.3f, 6.7f), (2.0f, 5.5f, 7.0f), (3.1f, 5.7f, 7.2f), (3.2f, 5.9f, 7.4f), (3.3f, 6.1f, 7.6f),
        (3.4f, 6.3f, 7.8f), (3.5f, 6.5f, 8.0f), (3.6f, 6.7f, 8.2f), (3.7f, 6.9f, 8.4f), (3.8f, 7.1f, 8.6f), (3.9f, 7.3f, 8.8f),
        (3.0f, 7.5f, 9.0f), (4.1f, 7.7f, 9.1f), (4.2f, 7.9f, 9.2f), (4.3f, 8.1f, 9.3f), (4.4f, 8.3f, 9.4f), (4.5f, 8.5f, 9.5f),
        (4.6f, 8.7f, 9.6f), (4.7f, 8.9f, 9.7f), (4.8f, 9.1f, 9.8f), (4.9f, 9.3f, 9.9f), (5.0f, 9.5f, 10.0f),
    ];

    /// <summary>
    /// Gets the Ruud of the first floors (<c>EventReward</c>).
    /// </summary>
    public static IReadOnlyList<int> FloorRuud { get; } = [30, 70, 130, 170, 250, 350, 400];

    /// <summary>
    /// Gets the boxes of the labyrinth by floor: lower, medium and higher Maze of Dimensions box (bags 240-242) at the
    /// floors 10, 20 and 30; a box gives one random item of its bag.
    /// </summary>
    public static IReadOnlyList<(int Floor, short Bag)> Boxes { get; } = [(10, 240), (20, 241), (30, 242)];

    /// <summary>
    /// Gets the items of the box bags, as <c>group * 512 + number</c>.
    /// </summary>
    public static IReadOnlyDictionary<short, int[]> BoxBags { get; } = new Dictionary<short, int[]>
    {
        [240] = [7181, 7540, 7541, 7182],
        [241] = [7181, 7539, 7540, 7541, 7182],
        [242] = [7181, 7539, 7540, 7541, 7063, 7182],
    };

    /// <summary>
    /// Creates the floors of the labyrinth: the mission monsters of the data (Great Sword 754 x2, Soldier 755 x3 and
    /// Lancer 756 x3 of Kundun) within five minutes, the goblin (757) by its rate of 20 % for 2000 points; the points of
    /// a first clear (50 per floor), the Ruud of the floor and a box every ten floors.
    /// </summary>
    /// <returns>The definition.</returns>
    public static RoundsEventDefinition CreateLabyrinth() => new()
    {
        RoundDuration = TimeSpan.FromMinutes(5),
        PauseAfterRound = TimeSpan.FromSeconds(5),
        PauseAfterBossRound = TimeSpan.FromSeconds(5),
        EndsWhenARoundFails = true,
        PartyIsATeam = false,
        SpawnPoints = [new EventSpawnPoint { Kind = 0, X = 21, Y = 12, Range = 6 }],
        Rounds = Floors.Select((floor, index) => new EventRound
        {
            MonsterNumber = 754,
            Count = 2,
            ClearPoints = 50 * (index + 1),
            ClearRuud = index < FloorRuud.Count ? FloorRuud[index] : 0,
            HealthMultiplier = floor.Health,
            DamageMultiplier = floor.Damage,
            DefenseMultiplier = floor.Defense,
            ClearRandomItems = Boxes.Where(b => b.Floor == index + 1).SelectMany(b => BoxBags[b.Bag]).Select(code => new RewardItem { Group = (byte)(code / 512), Number = (short)(code % 512) }).ToList(),
            BonusMonsters =
            [
                new EventBonusMonster { MonsterNumber = 755, Count = 3 },
                new EventBonusMonster { MonsterNumber = 756, Count = 3 },
                new EventBonusMonster { MonsterNumber = 757, Count = 1, Points = 2000, ChancePercent = 20 },
            ],
        }).ToList(),
    };
}
