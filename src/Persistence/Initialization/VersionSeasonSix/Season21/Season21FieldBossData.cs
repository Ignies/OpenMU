// <copyright file="Season21FieldBossData.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.GameLogic.FieldBosses;
using MUnique.OpenMU.Interfaces;

/// <summary>
/// The field bosses of Season 21, from the Season 21 server data: the monsters (<c>Monster/Monster.xml</c>), the boss
/// rules (<c>Event/BossFightSystem.xml</c>, <c>Event/WorldBoss</c>, <c>Event/InvasionManager.xml</c>) and the item bags
/// (<c>EventItemBag</c>). See the readme.
/// </summary>
internal static class Season21FieldBossData
{
    /// <summary>
    /// Gets the monsters which no map update defines yet, with the map they belong to and the version which added them.
    /// Medusa and the Sapi Queens it calls (560) keep the level of Season 6 (175, 141) on their Season 6 map; the others the
    /// experience level of the data.
    /// </summary>
    public static IReadOnlyList<(byte Map, GameVersion IntroducedIn, Season21Monster Row)> Monsters { get; } =
    [
        (56, GameVersion.Season6Episode1, new(560, LocalizedString.FromResource(() => MonsterNames.SapiQueen), Level: 141, Health: 358000, MinimumDamage: 1917, MaximumDamage: 2301, Defense: 740, AttackRate: 1140, DefenseRate: 1135, MoveRange: 4, AttackRange: 3, ViewRange: 9, MoveDelay: 700, AttackDelay: 2000, RespawnDelay: 8, Attribute: 2, Resistances: [150, 20, 20, 20, 0, 0, 0])),
        (56, GameVersion.Season6Episode1, new(561, LocalizedString.FromResource(() => MonsterNames.Medusa), Level: 175, Health: 7800000, MinimumDamage: 7040, MaximumDamage: 7800, Defense: 1450, AttackRate: 5900, DefenseRate: 3800, MoveRange: 3, AttackRange: 7, ViewRange: 10, MoveDelay: 800, AttackDelay: 4900, RespawnDelay: 10, Attribute: 2, Resistances: [254, 254, 150, 150, 0, 0, 0])),
        (112, GameVersion.Season11Episode1, new(734, LocalizedString.FromResource(() => MonsterNames.LordOfFerea), Level: 900, Health: 25000000, MinimumDamage: 4500, MaximumDamage: 5000, Defense: 3050, AttackRate: 7500, DefenseRate: 5300, MoveRange: 5, AttackRange: 8, ViewRange: 10, MoveDelay: 300, AttackDelay: 900, RespawnDelay: 86400, Attribute: 2, Resistances: [0, 0, 0, 0, 0, 0, 0])),
        (112, GameVersion.Season11Episode1, new(735, LocalizedString.FromResource(() => MonsterNames.FereaCrystalOrb), Level: 500, Health: 5000000, MinimumDamage: 0, MaximumDamage: 0, Defense: 0, AttackRate: 0, DefenseRate: 1, MoveRange: 0, AttackRange: 0, ViewRange: 0, MoveDelay: 0, AttackDelay: 0, RespawnDelay: 0, Attribute: 0, Resistances: [0, 0, 0, 0, 0, 0, 0])),
        (122, GameVersion.Season13Episode1, new(790, LocalizedString.FromResource(() => MonsterNames.MonsterOfTheSwamp), Level: 840, Health: 10000000, MinimumDamage: 5000, MaximumDamage: 6000, Defense: 1600, AttackRate: 5000, DefenseRate: 4000, MoveRange: 3, AttackRange: 2, ViewRange: 7, MoveDelay: 800, AttackDelay: 800, RespawnDelay: 10, Attribute: 2, Resistances: [0, 0, 0, 0, 0, 0, 0])),
        (122, GameVersion.Season13Episode1, new(791, LocalizedString.FromResource(() => MonsterNames.PetOfTheSwamp), Level: 810, Health: 2000000, MinimumDamage: 3500, MaximumDamage: 3800, Defense: 1200, AttackRate: 3500, DefenseRate: 2700, MoveRange: 3, AttackRange: 2, ViewRange: 7, MoveDelay: 800, AttackDelay: 2000, RespawnDelay: 10, Attribute: 2, Resistances: [0, 0, 0, 0, 0, 0, 0])),
        (122, GameVersion.Season13Episode1, new(792, LocalizedString.FromResource(() => MonsterNames.PetOfTheSwamp), Level: 810, Health: 2000000, MinimumDamage: 3500, MaximumDamage: 3800, Defense: 1200, AttackRate: 3500, DefenseRate: 2700, MoveRange: 3, AttackRange: 2, ViewRange: 7, MoveDelay: 800, AttackDelay: 2000, RespawnDelay: 10, Attribute: 2, Resistances: [0, 0, 0, 0, 0, 0, 0])),
        (122, GameVersion.Season13Episode1, new(793, LocalizedString.FromResource(() => MonsterNames.MonsterOfTheWater), Level: 860, Health: 20000000, MinimumDamage: 7000, MaximumDamage: 8500, Defense: 2000, AttackRate: 7000, DefenseRate: 5300, MoveRange: 3, AttackRange: 2, ViewRange: 7, MoveDelay: 500, AttackDelay: 1500, RespawnDelay: 10, Attribute: 2, Resistances: [0, 0, 0, 0, 0, 0, 0])),
        (122, GameVersion.Season13Episode1, new(794, LocalizedString.FromResource(() => MonsterNames.GodOfDarkness), Level: 920, Health: 30000000, MinimumDamage: 10000, MaximumDamage: 12000, Defense: 3000, AttackRate: 10000, DefenseRate: 6950, MoveRange: 9, AttackRange: 4, ViewRange: 9, MoveDelay: 500, AttackDelay: 1000, RespawnDelay: 86400, Attribute: 2, Resistances: [0, 0, 0, 0, 0, 0, 0])),
    ];

    /// <summary>
    /// Gets the item bags of the bosses: every section drops one item.
    /// </summary>
    public static IReadOnlyList<(short Monster, IReadOnlyList<IReadOnlyList<short>> Sections)> Drops { get; } =
    [
        (561, [
            [
                3628, 3629, 3630, 3633, 3634, 3635, 3636, 3704, 3730, 3732, 3762, 3794, 4140, 4141, 4142, 4143, 4144, 4145, 4146, 4147,
                4148, 4216, 4242, 4244, 4274, 4306, 4652, 4653, 4654, 4655, 4656, 4657, 4658, 4659,
            ],
            [
                52, 4660, 4728, 4754, 4756, 4786, 4818, 5164, 5165, 5166, 5167, 5168, 5169, 5170, 5171, 5172, 5240, 5268, 5298, 5330,
                5676, 5677, 5678, 5679, 5680, 5681, 5682, 5683, 5684, 5752, 5778, 5780, 5810, 5842,
            ],
            [6352, 6353, 6354, 6355, 6356, 6357, 6358, 6405, 7477, 7478, 7479, 7480],
            [
                18, 19, 20, 21, 24, 25, 26, 27, 28, 31, 52, 60, 69, 76, 1031, 1035, 1036, 1037, 1038, 1039,
                1040, 1041, 1054, 1558, 1562, 2066, 2067, 2068, 2070, 2071, 2092, 2094, 2569, 2570, 2571, 2573, 2578, 2580, 2590, 2591,
                2596, 3088, 3089, 3090, 3091, 3092, 3112,
            ],
            [
                14, 3585, 3586, 3587, 3588, 3589, 3590, 4096, 4099, 4100, 4101, 4102, 4608, 4610, 4611, 4612, 4613, 4614, 5121, 5123,
                5124, 5125, 5126, 5632, 5633, 5634, 5635, 5636, 5637, 5638, 6668,
            ],
            [
                2057, 3078, 3591, 3592, 3593, 3594, 3595, 3596, 4103, 4104, 4105, 4106, 4107, 4108, 4615, 4616, 4617, 4618, 4619, 4620,
                5127, 5128, 5129, 5130, 5131, 5132, 5639, 5640, 5641, 5642, 5643, 6669, 6679,
            ],
            [
                32, 3598, 3624, 3643, 3676, 4109, 4110, 4111, 4136, 4155, 4188, 4215, 4273, 4621, 4622, 4623, 4648, 4667, 4700, 4727,
                4785, 5133, 5134, 5160, 5239, 5297, 5644, 5646, 5647, 5672, 5751, 5809, 6679, 6683,
            ],
            [7181, 7182, 7184, 7190, 7210],
            [7520, 7615, 7616],
            [6432, 6732, 6833, 6834, 7581],
        ]),
        (673, [
            [
                19, 52, 60, 76, 1037, 1562, 2066, 2094, 2570, 2596, 3628, 3629, 3630, 3633, 3634, 3635, 3636, 3637, 3638, 3639,
                3640, 3642, 3658, 3659, 3704, 3730, 3732, 3762, 3794, 4140, 4141, 4142, 4143, 4144, 4145, 4146, 4147, 4148, 4149, 4150,
                4151, 4152, 4154, 4170, 4171, 4216, 4242, 4244, 4274, 4306, 4652, 4653, 4654, 4655, 4656, 4657, 4658, 4659, 4660, 4661,
                4662, 4663, 4664, 4666, 4682, 4683, 4728, 4754, 4756, 4786, 4818, 5164, 5165, 5166, 5167, 5168, 5169, 5170, 5171, 5172,
                5173, 5174, 5175, 5176, 5178, 5194, 5195, 5240, 5268, 5298, 5330, 5676, 5677, 5678, 5679, 5680, 5681, 5682, 5683, 5684,
                5685, 5686, 5687, 5688, 5690, 5706, 5707, 5752, 5778, 5780, 5810, 5842,
            ],
            [6352, 6353, 6354, 6355, 6356, 6357, 6358, 7477, 7478, 7479, 7480],
            [6405, 7124, 7181, 7182, 7184, 7190, 7210, 7634, 7635],
            [6593, 6594, 6595, 6601, 6602, 6603, 7181],
            [7520, 7615, 7616],
        ]),
        (716, [
            [8518, 8520, 8522],
            [6346, 6347, 6350, 6351, 6352, 6353, 6354, 6355, 6356, 6357, 6358, 6361, 6405, 6450, 6451, 6452, 7477, 7478, 7479, 7480],
            [6709, 6733, 7124, 7181, 7513, 7520, 7615, 7616],
            [
                7007, 7008, 7009, 7010, 7011, 7012, 7013, 7014, 7015, 7016, 7017, 7018, 7019, 7020, 7021, 7022, 7023, 7024, 7025, 7026,
                7027, 7028, 7029, 7030, 7031, 7032, 7033, 7034, 7035, 7036, 7037, 7038, 7039, 7040, 7041, 7042, 7043, 7044, 7045, 7046,
                7047, 7048, 7049, 7050, 7051, 7052, 7053, 7054, 7055, 7056, 7057, 7058, 7059, 7060, 7061, 7062,
            ],
        ]),
        (734, [
            [6346, 6347, 6350, 6351, 6352, 6353, 6354, 6355, 6356, 6357, 6358, 6361, 6450, 6451, 6452],
            [6268, 6269, 6270, 6271, 6272, 6273, 6405, 6709, 6834, 7122, 7210, 7513, 7554, 7615, 7616],
            [
                7007, 7008, 7009, 7010, 7011, 7012, 7013, 7014, 7015, 7016, 7017, 7018, 7019, 7020, 7021, 7022, 7023, 7024, 7025, 7026,
                7027, 7028, 7029, 7030, 7031, 7032, 7033, 7034, 7035, 7036, 7037, 7038, 7039, 7040, 7041, 7042, 7043, 7044, 7045, 7046,
                7047, 7048, 7049, 7050, 7051, 7052, 7053, 7054, 7055, 7056, 7057, 7058, 7059, 7060, 7061, 7062,
            ],
        ]),
        (746, [
            [6346, 6347, 6350, 6351, 6352, 6353, 6354, 6355, 6356, 6357, 6358, 6361, 6450, 6451, 6452],
            [6268, 6269, 6270, 6271, 6272, 6273, 6405, 6709, 6834, 7122, 7210, 7513, 7554, 7615, 7616],
            [
                7007, 7008, 7009, 7010, 7011, 7012, 7013, 7014, 7015, 7016, 7017, 7018, 7019, 7020, 7021, 7022, 7023, 7024, 7025, 7026,
                7027, 7028, 7029, 7030, 7031, 7032, 7033, 7034, 7035, 7036, 7037, 7038, 7039, 7040, 7041, 7042, 7043, 7044, 7045, 7046,
                7047, 7048, 7049, 7050, 7051, 7052, 7053, 7054, 7055, 7056, 7057, 7058, 7059, 7060, 7061, 7062,
            ],
        ]),
        (794, [
            [6346, 6347, 6350, 6351, 6352, 6353, 6354, 6355, 6356, 6357, 6358, 6361, 6450, 6451, 6452],
            [6268, 6269, 6270, 6271, 6272, 6273, 6365, 6375, 6385, 6395, 6405],
            [
                3682, 3683, 3684, 3685, 3687, 3688, 3689, 3706, 3764, 3796, 4194, 4195, 4196, 4197, 4198, 4199, 4200, 4201, 4218, 4276,
                4308, 4706, 4707, 4708, 4709, 4710, 4711, 4712, 4713, 4730, 4788, 4820, 5218, 5219, 5220, 5221, 5222, 5223, 5225, 5242,
                5300, 5332, 5730, 5731, 5732, 5733, 5734, 5735, 5736, 5737, 5754, 5812, 5844,
            ],
        ]),
    ];

    /// <summary>
    /// Gets Priest James (725), who hands out the Spirit Nixie's Shard, at his places of the spawn files of the data
    /// (Elbeland, Loren Market).
    /// </summary>
    public static IReadOnlyList<(byte Map, short Npc, byte X, byte Y, Direction Direction)> Npcs { get; } =
    [
        (51, 725, 32, 239, Direction.West),
        (79, 725, 215, 56, Direction.South),
    ];

    /// <summary>
    /// Gets the spawn areas of the boss monsters. They spawn manually by the field bosses plugin; the areas place them
    /// in their map's set, so the progression normalizes them with it.
    /// </summary>
    public static IReadOnlyList<(byte Map, short Monster, byte X, byte Y)> Spawns { get; } =
    [
        (56, 561, 53, 108),
        (100, 673, 130, 114),
        (110, 716, 149, 144),
        (112, 735, 24, 123),
        (112, 734, 31, 120),
        (113, 746, 30, 218),
        (122, 790, 184, 18),
        (122, 791, 184, 18),
        (122, 792, 184, 18),
        (122, 793, 184, 18),
        (122, 794, 184, 18),
    ];

    /// <summary>
    /// Creates the default bosses.
    /// </summary>
    /// <returns>The bosses.</returns>
    /// <remarks>
    /// Medusa: InvasionManager ID 8, group 0 (ground 3) with its 7 Sapi Queens. Lord of Ferea: the crystal orb first,
    /// then the lord with the four health phases of <c>FereaBoss.xml</c> (90, 70, 50, 30 %). Nix and Core Magriffy fight
    /// in the closed areas of their terrain (Nix entered at the Spirit of Nixie, Magriffy by the gate at 179,148); the God
    /// of Darkness in the boss zone the two portals lead to, 10 players, 10 minutes, the swamp monster with its pets,
    /// then the water monster, then the god. Lord Silvester stands in open terrain.
    /// </remarks>
    public static List<FieldBossDefinition> CreateBosses() =>
    [
        new()
        {
            Name = "Medusa",
            MapNumber = 56,
            Stages = [Stage(561, 53, 108, [Summon(560, 7, 50, 103, 57, 114)])],
            Grounds =
            [
                new FieldBossGround { Name = "Ground 1", X = 181, Y = 119 },
                new FieldBossGround { Name = "Ground 2", X = 130, Y = 167 },
                new FieldBossGround { Name = "Ground 3", X = 53, Y = 108 },
                new FieldBossGround { Name = "Ground 4", X = 152, Y = 15 },
            ],
        },
        new()
        {
            Name = "Silvester",
            MapNumber = 100,
            Stages = [Stage(673, 130, 114)],
        },
        new()
        {
            Name = "Magriffy",
            MapNumber = 110,
            BattleMinutes = 30,
            Arena = new FieldBossArena { X1 = 127, Y1 = 122, X2 = 161, Y2 = 172, EntryX = 133, EntryY = 166, ClosesWhenEngaged = true },
            Stages = [Stage(716, 149, 144)],
        },
        new()
        {
            Name = "Ferea",
            MapNumber = 112,
            Stages =
            [
                Stage(735, 24, 123),
                new FieldBossStage
                {
                    MonsterNumber = 734,
                    X = 31,
                    Y = 120,
                    Abilities =
                    [
                        Strike("Whirlwind", 3, 80, 30),
                        Strike("Land mine", 2, 95, 20),
                    ],
                    Phases =
                    [
                        FereaPhase(90, 1, 2, 2),
                        FereaPhase(70, 2, 2, 3),
                        FereaPhase(50, 3, 3, 4),
                        FereaPhase(30, 3, 3, 4),
                    ],
                },
            ],
        },
        new()
        {
            Name = "Nix",
            MapNumber = 113,
            EntryNpcNumber = 748,
            EntryItemGroup = 14,
            EntryItemNumber = 390,
            EntryItemNpcNumber = 725,
            EntryItemPrice = 1_000_000,
            BattleMinutes = 30,
            Arena = new FieldBossArena { X1 = 14, Y1 = 202, X2 = 47, Y2 = 236, EntryX = 24, EntryY = 228, ClosesWhenEngaged = true },
            Stages = [Stage(746, 30, 218)],
        },
        new()
        {
            Name = "GodOfDarkness",
            MapNumber = 122,
            BattleMinutes = 10,
            Arena = new FieldBossArena { X1 = 174, Y1 = 8, X2 = 194, Y2 = 28, EntryX = 192, EntryY = 9, MaximumPlayers = 10 },
            Stages =
            [
                Stage(790, 184, 18, [Summon(791, 4, 178, 12, 190, 24), Summon(792, 4, 178, 12, 190, 24)], [new FieldBossAbility { Name = "Call the pets", Kind = FieldBossAbilityKind.Summon, IntervalSeconds = 20, MaximumAlive = 8, Summons = [Summon(791, 4, 178, 12, 190, 24), Summon(792, 4, 178, 12, 190, 24)] }]),
                Stage(793, 184, 18, abilities: [Strike("Wave", 4, 100, 15), Strike("Water course", 4, 100, 15)]),
                Stage(794, 184, 18, abilities: [Strike("Flame", 5, 90, 10), Strike("Darkness", 5, 100, 20)]),
            ],
        },
    ];

    private static FieldBossStage Stage(short monster, byte x, byte y, List<FieldBossSummon>? summons = null, List<FieldBossAbility>? abilities = null)
        => new() { MonsterNumber = monster, X = x, Y = y, Summons = summons ?? [], Abilities = abilities ?? [] };

    private static FieldBossAbility Strike(string name, byte distance, int damagePercent, int chancePercent)
        => new() { Name = name, Kind = FieldBossAbilityKind.AreaStrike, IntervalSeconds = 2, Distance = distance, DamagePercent = damagePercent, ChancePercent = chancePercent };

    private static FieldBossSummon Summon(short monster, int count, byte x1, byte y1, byte x2, byte y2)
        => new() { MonsterNumber = monster, Count = count, X1 = x1, Y1 = y1, X2 = x2, Y2 = y2 };

    private static FieldBossHealthPhase FereaPhase(int healthPercent, int knights, int archers, int fighters) => new()
    {
        HealthPercent = healthPercent,
        Summons =
        [
            Summon(730, knights, 27, 115, 35, 125),
            Summon(731, archers, 27, 115, 35, 125),
            Summon(732, fighters, 27, 115, 35, 125),
        ],
    };
}
