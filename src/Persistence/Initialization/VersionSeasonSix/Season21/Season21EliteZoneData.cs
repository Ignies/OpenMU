// <copyright file="Season21EliteZoneData.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.GameLogic.EliteZones;
using MUnique.OpenMU.Interfaces;

/// <summary>
/// The elite zones of the later maps: five per map at the entrances Webzen publishes, with the elites which spawn
/// inside, from the spawn files of the Season 21 server data (<c>Monster/MonsterSetBase</c>). See the readme.
/// </summary>
internal static class Season21EliteZoneData
{
    /// <summary>
    /// Gets the zones.
    /// </summary>
    public static IReadOnlyList<Season21EliteZone> Zones { get; } =
    [
        new(130, EliteZoneElement.Fire, 185, 210, 179, 204, 191, 216, 179, 204, 191, 216, [new(839, 1), new(840, 1)]),
        new(130, EliteZoneElement.Water, 132, 27, 126, 21, 138, 33, 126, 21, 138, 33, [new(839, 1), new(840, 1)]),
        new(130, EliteZoneElement.Earth, 79, 217, 73, 211, 85, 223, 73, 211, 85, 223, [new(839, 1), new(840, 1)]),
        new(130, EliteZoneElement.Wind, 220, 101, 214, 95, 226, 107, 214, 95, 226, 107, [new(839, 1), new(840, 1)]),
        new(130, EliteZoneElement.Darkness, 39, 93, 33, 87, 45, 99, 33, 87, 45, 99, [new(839, 1), new(840, 1)]),
        new(131, EliteZoneElement.Fire, 214, 229, 208, 223, 220, 235, 208, 223, 220, 235, [new(848, 1), new(849, 1), new(850, 1)]),
        new(131, EliteZoneElement.Water, 50, 47, 44, 41, 56, 53, 44, 41, 56, 53, [new(848, 1), new(849, 1), new(850, 1)]),
        new(131, EliteZoneElement.Earth, 227, 133, 221, 127, 233, 139, 221, 127, 233, 139, [new(848, 1), new(849, 1), new(850, 1)]),
        new(131, EliteZoneElement.Wind, 22, 96, 16, 90, 28, 102, 16, 90, 28, 102, [new(848, 1), new(849, 1), new(850, 1)]),
        new(131, EliteZoneElement.Darkness, 29, 178, 23, 172, 35, 184, 23, 172, 35, 184, [new(848, 1), new(849, 1), new(850, 1)]),
        new(132, EliteZoneElement.Fire, 55, 66, 49, 60, 61, 72, 49, 60, 61, 72, [new(864, 3), new(865, 3)]),
        new(132, EliteZoneElement.Water, 79, 203, 73, 197, 85, 209, 73, 197, 85, 209, [new(864, 3), new(865, 3)]),
        new(132, EliteZoneElement.Earth, 179, 49, 173, 43, 185, 55, 173, 43, 185, 55, [new(864, 3), new(865, 3)]),
        new(132, EliteZoneElement.Wind, 178, 205, 172, 199, 184, 211, 172, 199, 184, 211, [new(864, 3), new(865, 3)]),
        new(132, EliteZoneElement.Darkness, 215, 117, 209, 111, 221, 123, 209, 111, 221, 123, [new(864, 3), new(865, 3)]),
        new(134, EliteZoneElement.Fire, 20, 59, 16, 58, 24, 61, 16, 58, 24, 61, [new(879, 2), new(880, 1)]),
        new(134, EliteZoneElement.Water, 47, 108, 42, 105, 50, 111, 42, 105, 50, 111, [new(879, 2), new(880, 1)]),
        new(134, EliteZoneElement.Earth, 168, 169, 165, 166, 172, 172, 165, 166, 172, 172, [new(879, 2), new(880, 1)]),
        new(134, EliteZoneElement.Wind, 97, 161, 97, 156, 99, 165, 97, 156, 99, 165, [new(879, 2), new(880, 1)]),
        new(134, EliteZoneElement.Darkness, 226, 130, 224, 125, 231, 130, 224, 125, 231, 130, [new(879, 2), new(880, 1)]),
        new(136, EliteZoneElement.Fire, 163, 127, 157, 121, 169, 133, 157, 121, 169, 133, [new(898, 2), new(899, 1), new(900, 1)]),
        new(136, EliteZoneElement.Water, 40, 81, 34, 75, 46, 87, 34, 75, 46, 87, [new(898, 2), new(899, 1), new(900, 1)]),
        new(136, EliteZoneElement.Earth, 211, 68, 205, 62, 217, 74, 205, 62, 217, 74, [new(898, 2), new(899, 1), new(900, 1)]),
        new(136, EliteZoneElement.Wind, 79, 105, 73, 99, 85, 111, 73, 99, 85, 111, [new(898, 2), new(899, 1), new(900, 1)]),
        new(136, EliteZoneElement.Darkness, 148, 183, 142, 177, 154, 189, 142, 177, 154, 189, [new(898, 2), new(899, 1), new(900, 1)]),
        new(137, EliteZoneElement.Fire, 40, 38, 17, 34, 40, 45, 17, 34, 22, 45, [new(908, 3), new(909, 3), new(910, 3)]),
        new(137, EliteZoneElement.Water, 18, 215, 18, 215, 26, 239, 18, 234, 26, 239, [new(908, 3), new(909, 3), new(910, 3)]),
        new(137, EliteZoneElement.Earth, 124, 10, 98, 8, 124, 17, 98, 8, 102, 17, [new(908, 3), new(909, 3), new(910, 3)]),
        new(137, EliteZoneElement.Wind, 225, 212, 220, 212, 236, 240, 220, 240, 236, 240, [new(908, 3), new(909, 3), new(910, 3)]),
        new(137, EliteZoneElement.Darkness, 235, 11, 208, 9, 235, 18, 208, 9, 213, 18, [new(908, 3), new(909, 3), new(910, 3)]),
        new(138, EliteZoneElement.Fire, 171, 40, 165, 34, 177, 46, 165, 34, 177, 46, [new(914, 2), new(915, 2), new(916, 2)]),
        new(138, EliteZoneElement.Water, 30, 135, 24, 129, 36, 141, 24, 129, 36, 141, [new(914, 2), new(915, 2), new(916, 2)]),
        new(138, EliteZoneElement.Earth, 105, 37, 99, 31, 111, 43, 99, 31, 111, 43, [new(914, 2), new(915, 2), new(916, 2)]),
        new(138, EliteZoneElement.Wind, 47, 223, 41, 217, 53, 229, 41, 217, 53, 229, [new(914, 2), new(915, 2), new(916, 2)]),
        new(138, EliteZoneElement.Darkness, 214, 196, 208, 190, 220, 202, 208, 190, 220, 202, [new(914, 2), new(915, 2), new(916, 2)]),
        new(140, EliteZoneElement.Fire, 29, 48, 16, 48, 29, 55, 16, 50, 23, 55, [new(930, 2), new(931, 2), new(932, 2)]),
        new(140, EliteZoneElement.Water, 234, 221, 234, 210, 244, 221, 237, 210, 244, 219, [new(930, 3), new(931, 3), new(932, 3)]),
        new(140, EliteZoneElement.Earth, 86, 109, 71, 105, 86, 113, 71, 105, 81, 113, [new(930, 3), new(931, 3), new(932, 3)]),
        new(140, EliteZoneElement.Wind, 30, 217, 15, 217, 30, 224, 15, 217, 25, 224, [new(930, 3), new(931, 3), new(932, 3)]),
        new(140, EliteZoneElement.Darkness, 228, 27, 228, 24, 243, 31, 235, 24, 243, 31, [new(930, 3), new(931, 3), new(932, 3)]),
        new(141, EliteZoneElement.Fire, 148, 233, 138, 233, 152, 243, 138, 236, 152, 243, [new(945, 2), new(946, 2), new(947, 2)]),
        new(141, EliteZoneElement.Water, 25, 198, 19, 170, 26, 198, 19, 170, 26, 184, [new(945, 2), new(946, 2), new(947, 2)]),
        new(141, EliteZoneElement.Earth, 188, 11, 165, 8, 188, 14, 165, 8, 178, 14, [new(945, 2), new(946, 2), new(947, 2)]),
        new(141, EliteZoneElement.Wind, 242, 158, 239, 158, 247, 183, 239, 173, 247, 183, [new(945, 6)]),
        new(141, EliteZoneElement.Darkness, 55, 15, 30, 12, 55, 18, 30, 12, 42, 18, [new(945, 2), new(946, 2), new(947, 2)]),
        new(142, EliteZoneElement.Fire, 226, 54, 211, 45, 237, 64, 223, 49, 231, 62, [new(958, 3), new(959, 3), new(960, 3)]),
        new(142, EliteZoneElement.Water, 224, 197, 207, 187, 233, 206, 220, 189, 230, 203, [new(958, 3), new(959, 3), new(960, 3)]),
        new(142, EliteZoneElement.Earth, 122, 23, 116, 12, 131, 42, 117, 19, 130, 27, [new(958, 3), new(959, 3), new(960, 3)]),
        new(142, EliteZoneElement.Wind, 25, 116, 16, 106, 41, 125, 20, 110, 30, 122, [new(958, 3), new(959, 3), new(960, 3)]),
        new(142, EliteZoneElement.Darkness, 88, 231, 77, 221, 103, 239, 82, 222, 93, 238, [new(958, 3), new(959, 3), new(960, 3)]),
        new(143, EliteZoneElement.Fire, 24, 48, 18, 42, 30, 54, 18, 42, 30, 54, [new(966, 2), new(967, 2), new(968, 2)]),
        new(143, EliteZoneElement.Water, 216, 60, 210, 54, 222, 66, 210, 54, 222, 66, [new(966, 2), new(967, 2), new(968, 2)]),
        new(143, EliteZoneElement.Earth, 46, 126, 40, 120, 52, 132, 40, 120, 52, 132, [new(966, 2), new(967, 2), new(968, 2)]),
        new(143, EliteZoneElement.Wind, 236, 132, 230, 126, 242, 138, 230, 126, 242, 138, [new(966, 2), new(967, 2), new(968, 2)]),
        new(143, EliteZoneElement.Darkness, 237, 198, 231, 192, 243, 204, 231, 192, 243, 204, [new(966, 2), new(967, 2), new(968, 2)]),
        new(144, EliteZoneElement.Fire, 42, 17, 36, 11, 48, 23, 36, 11, 48, 23, [new(972, 2), new(973, 2), new(974, 2)]),
        new(144, EliteZoneElement.Water, 227, 121, 221, 115, 233, 127, 221, 115, 233, 127, [new(972, 2), new(973, 2), new(974, 2)]),
        new(144, EliteZoneElement.Earth, 24, 211, 18, 205, 30, 217, 18, 205, 30, 217, [new(972, 2), new(973, 2), new(974, 2)]),
        new(144, EliteZoneElement.Wind, 206, 16, 200, 10, 212, 22, 200, 10, 212, 22, [new(972, 2), new(973, 2), new(974, 2)]),
        new(144, EliteZoneElement.Darkness, 201, 218, 195, 212, 207, 224, 195, 212, 207, 224, [new(972, 2), new(973, 2), new(974, 2)]),
        new(145, EliteZoneElement.Fire, 33, 46, 27, 40, 39, 52, 27, 40, 39, 52, [new(978, 2), new(979, 2), new(980, 2)]),
        new(145, EliteZoneElement.Water, 43, 190, 37, 184, 49, 196, 37, 184, 49, 196, [new(978, 2), new(979, 2), new(980, 2)]),
        new(145, EliteZoneElement.Earth, 206, 217, 200, 211, 212, 223, 200, 211, 212, 223, [new(978, 2), new(979, 2), new(980, 2)]),
        new(145, EliteZoneElement.Wind, 215, 105, 209, 99, 221, 111, 209, 99, 221, 111, [new(978, 2), new(979, 2), new(980, 2)]),
        new(145, EliteZoneElement.Darkness, 220, 25, 214, 19, 226, 31, 214, 19, 226, 31, [new(978, 2), new(979, 2), new(980, 2)]),
    ];

    /// <summary>
    /// Gets the elites which the maps update doesn't define, because the data never spawns them outside a zone.
    /// </summary>
    public static IReadOnlyList<(byte Map, Season21Monster Row)> Monsters { get; } =
    [
        (138, new(914, LocalizedString.FromResource(() => MonsterNames.EliteMagmaGladiator), Level: 1605, Health: 4701000, MinimumDamage: 21100, MaximumDamage: 21700, Defense: 9600, AttackRate: 5100, DefenseRate: 5400, MoveRange: 4, AttackRange: 3, ViewRange: 7, MoveDelay: 400, AttackDelay: 1700, RespawnDelay: 15, Attribute: 2, Resistances: [29, 29, 29, 29, 0, 0, 0])),
        (138, new(915, LocalizedString.FromResource(() => MonsterNames.EliteMagmaHook), Level: 1615, Health: 4843000, MinimumDamage: 21000, MaximumDamage: 22250, Defense: 9800, AttackRate: 5300, DefenseRate: 5560, MoveRange: 4, AttackRange: 3, ViewRange: 7, MoveDelay: 400, AttackDelay: 1700, RespawnDelay: 15, Attribute: 2, Resistances: [28, 28, 28, 28, 0, 0, 0])),
        (138, new(916, LocalizedString.FromResource(() => MonsterNames.EliteMagmaShaman), Level: 1620, Health: 4925000, MinimumDamage: 21300, MaximumDamage: 21900, Defense: 10160, AttackRate: 5300, DefenseRate: 5600, MoveRange: 4, AttackRange: 3, ViewRange: 7, MoveDelay: 400, AttackDelay: 1700, RespawnDelay: 15, Attribute: 2, Resistances: [27, 27, 27, 27, 0, 0, 0])),
        (143, new(966, LocalizedString.FromResource(() => MonsterNames.EliteDevilSword), Level: 2225, Health: 8345636, MinimumDamage: 36057, MaximumDamage: 36422, Defense: 14286, AttackRate: 15750, DefenseRate: 9585, MoveRange: 4, AttackRange: 1, ViewRange: 7, MoveDelay: 400, AttackDelay: 1770, RespawnDelay: 15, Attribute: 2, Resistances: [29, 29, 29, 29, 0, 0, 0])),
        (143, new(967, LocalizedString.FromResource(() => MonsterNames.EliteDevilScythe), Level: 2230, Health: 8307542, MinimumDamage: 37830, MaximumDamage: 38213, Defense: 14448, AttackRate: 16750, DefenseRate: 8214, MoveRange: 4, AttackRange: 3, ViewRange: 7, MoveDelay: 400, AttackDelay: 1770, RespawnDelay: 15, Attribute: 2, Resistances: [27, 27, 27, 27, 0, 0, 0])),
        (143, new(968, LocalizedString.FromResource(() => MonsterNames.EliteDevilSorcerer), Level: 2230, Health: 8201815, MinimumDamage: 34370, MaximumDamage: 34718, Defense: 13507, AttackRate: 16500, DefenseRate: 8624, MoveRange: 4, AttackRange: 3, ViewRange: 7, MoveDelay: 400, AttackDelay: 1770, RespawnDelay: 15, Attribute: 2, Resistances: [28, 28, 28, 28, 0, 0, 0])),
        (144, new(972, LocalizedString.FromResource(() => MonsterNames.EliteShadowMasterOfDoom), Level: 2235, Health: 10855000, MinimumDamage: 40080, MaximumDamage: 40755, Defense: 15470, AttackRate: 15750, DefenseRate: 8763, MoveRange: 4, AttackRange: 1, ViewRange: 7, MoveDelay: 400, AttackDelay: 1770, RespawnDelay: 15, Attribute: 2, Resistances: [29, 29, 29, 29, 0, 0, 0])),
        (144, new(973, LocalizedString.FromResource(() => MonsterNames.EliteNapinOfDoom), Level: 2240, Health: 10919000, MinimumDamage: 38430, MaximumDamage: 39105, Defense: 15680, AttackRate: 16750, DefenseRate: 8549, MoveRange: 4, AttackRange: 3, ViewRange: 7, MoveDelay: 400, AttackDelay: 1770, RespawnDelay: 15, Attribute: 2, Resistances: [27, 27, 27, 27, 0, 0, 0])),
        (144, new(974, LocalizedString.FromResource(() => MonsterNames.EliteSapiQueenOfDoom), Level: 2240, Health: 10887000, MinimumDamage: 39255, MaximumDamage: 39930, Defense: 15575, AttackRate: 16500, DefenseRate: 8943, MoveRange: 4, AttackRange: 3, ViewRange: 7, MoveDelay: 400, AttackDelay: 1770, RespawnDelay: 15, Attribute: 2, Resistances: [28, 28, 28, 28, 0, 0, 0])),
        (145, new(978, LocalizedString.FromResource(() => MonsterNames.EliteAquilasWarrior), Level: 2325, Health: 10887000, MinimumDamage: 40080, MaximumDamage: 40755, Defense: 15470, AttackRate: 17460, DefenseRate: 10370, MoveRange: 4, AttackRange: 2, ViewRange: 9, MoveDelay: 700, AttackDelay: 2500, RespawnDelay: 8, Attribute: 2, Resistances: [150, 20, 20, 20, 0, 0, 0])),
        (145, new(979, LocalizedString.FromResource(() => MonsterNames.EliteAquilasGunner), Level: 2320, Health: 10855000, MinimumDamage: 38430, MaximumDamage: 39105, Defense: 15680, AttackRate: 16440, DefenseRate: 10577, MoveRange: 4, AttackRange: 2, ViewRange: 9, MoveDelay: 700, AttackDelay: 2500, RespawnDelay: 8, Attribute: 2, Resistances: [150, 20, 20, 20, 0, 0, 0])),
        (145, new(980, LocalizedString.FromResource(() => MonsterNames.EliteAquilasMagician), Level: 2325, Health: 10887000, MinimumDamage: 39255, MaximumDamage: 39930, Defense: 15575, AttackRate: 13400, DefenseRate: 10474, MoveRange: 4, AttackRange: 3, ViewRange: 9, MoveDelay: 700, AttackDelay: 2000, RespawnDelay: 8, Attribute: 2, Resistances: [150, 20, 20, 20, 0, 0, 0])),
    ];

    /// <summary>
    /// Gets the tiles (row, first and last column) of Twisted Karutan which the server terrain of the data blocks and
    /// the terrain of the Season 21 client lets walk on: its five elite zones.
    /// </summary>
    public static IReadOnlyList<(byte Y, byte X1, byte X2)> TwistedKarutanOpenings { get; } =
    [
        (12, 118, 119), (13, 117, 119), (14, 117, 120), (14, 127, 129), (15, 117, 121), (15, 127, 130),
        (16, 118, 122), (16, 125, 130), (17, 118, 130), (18, 117, 130), (19, 116, 129), (20, 116, 129),
        (21, 116, 130), (22, 116, 131), (23, 117, 131), (24, 117, 131), (25, 117, 130), (26, 117, 130),
        (27, 117, 130), (28, 120, 129), (29, 120, 127), (30, 119, 126), (31, 119, 125), (32, 119, 124),
        (33, 119, 122), (34, 119, 122), (35, 119, 122), (36, 119, 122), (37, 119, 122), (38, 119, 122),
        (39, 119, 122), (40, 121, 123), (41, 122, 123), (42, 122, 122), (45, 226, 229), (46, 225, 234),
        (47, 224, 235), (48, 224, 236), (49, 223, 236), (50, 223, 237), (51, 213, 213), (51, 223, 237),
        (52, 212, 213), (52, 222, 233), (52, 236, 237), (53, 212, 232), (54, 211, 231), (55, 211, 231),
        (56, 212, 231), (57, 212, 232), (58, 212, 217), (58, 221, 233), (58, 236, 237), (59, 223, 237),
        (60, 223, 237), (61, 224, 237), (62, 224, 237), (63, 228, 236), (64, 228, 229), (106, 17, 20),
        (107, 16, 21), (108, 16, 22), (108, 26, 27), (109, 16, 22), (109, 25, 28), (110, 16, 29),
        (111, 18, 29), (112, 18, 29), (113, 18, 30), (114, 21, 29), (114, 40, 41), (115, 21, 32),
        (115, 36, 41), (116, 21, 41), (117, 21, 41), (118, 20, 41), (119, 19, 41), (120, 18, 30),
        (120, 39, 41), (121, 18, 28), (122, 18, 26), (123, 18, 27), (124, 20, 27), (125, 22, 26),
        (187, 221, 223), (188, 220, 229), (189, 220, 230), (190, 221, 231), (191, 221, 232), (192, 220, 232),
        (193, 220, 232), (194, 217, 218), (194, 221, 228), (194, 230, 231), (195, 210, 228), (196, 210, 227),
        (197, 207, 226), (198, 208, 226), (199, 208, 212), (199, 215, 227), (200, 208, 212), (200, 217, 228),
        (200, 231, 232), (201, 222, 233), (202, 222, 233), (203, 221, 233), (204, 221, 231), (205, 225, 231),
        (206, 228, 231), (221, 78, 79), (221, 86, 90), (222, 77, 79), (222, 81, 83), (222, 86, 93),
        (223, 77, 83), (223, 87, 93), (224, 77, 83), (224, 87, 93), (225, 77, 83), (225, 86, 102),
        (226, 78, 103), (227, 80, 103), (228, 83, 103), (229, 83, 93), (229, 102, 102), (230, 83, 93),
        (231, 83, 93), (232, 82, 93), (233, 81, 92), (234, 78, 92), (235, 78, 92), (236, 78, 91),
        (237, 78, 90), (238, 78, 90), (239, 85, 89),
    ];

    /// <summary>
    /// Gets the Elite Zone Flags (838, 866, 925) at the entrances, from the spawn files of the data. Ignis is left out:
    /// its file copies the flags of Kanturu Underground.
    /// </summary>
    public static IReadOnlyList<(byte Map, short Npc, byte X, byte Y)> Flags { get; } =
    [
        (130, 838, 62, 219),
        (130, 838, 87, 235),
        (130, 838, 89, 210),
        (130, 838, 77, 197),
        (130, 838, 170, 195),
        (130, 838, 154, 225),
        (130, 838, 214, 214),
        (130, 838, 155, 28),
        (130, 838, 135, 42),
        (130, 838, 113, 24),
        (130, 838, 226, 120),
        (130, 838, 207, 122),
        (130, 838, 230, 90),
        (130, 838, 48, 86),
        (130, 838, 29, 111),
        (130, 838, 31, 80),
        (131, 838, 205, 141),
        (131, 838, 205, 219),
        (131, 838, 38, 198),
        (131, 838, 42, 117),
        (131, 838, 62, 30),
        (132, 866, 55, 66),
        (132, 866, 79, 203),
        (132, 866, 179, 49),
        (132, 866, 178, 205),
        (132, 866, 215, 117),
        (137, 838, 40, 38),
        (137, 838, 18, 215),
        (137, 838, 124, 10),
        (137, 838, 225, 212),
        (137, 838, 235, 11),
        (140, 925, 50, 42),
        (140, 925, 214, 218),
        (140, 925, 92, 113),
        (140, 925, 50, 215),
        (140, 925, 225, 29),
        (141, 925, 154, 219),
        (141, 925, 25, 198),
        (141, 925, 191, 14),
        (141, 925, 242, 158),
        (141, 925, 60, 18),
        (142, 925, 213, 58),
        (142, 925, 213, 51),
        (142, 925, 209, 200),
        (142, 925, 208, 193),
        (142, 925, 118, 40),
        (142, 925, 124, 40),
        (142, 925, 40, 114),
        (142, 925, 40, 120),
        (142, 925, 102, 229),
        (142, 925, 102, 223),
    ];

    /// <summary>
    /// Creates the debuffs of the elites from <c>Monster/EliteMonster.txt</c>: its five blocks are the elements fire to darkness;
    /// bleeding 17 % of the hit a second up to 12,000, poisoning 2-3 % of the maximum health a second, 13 seconds;
    /// attack and attack speed down 10, defense down 20 %, 15 seconds; every hit (rate 100).
    /// </summary>
    /// <returns>The debuffs.</returns>
    public static List<EliteDebuff> CreateDebuffs() =>
    [
        Bleeding(EliteZoneElement.Fire), Decrease(EliteZoneElement.Fire, EliteDebuffKind.DecreaseAttack, 10),
        Poisoning(EliteZoneElement.Water), Decrease(EliteZoneElement.Water, EliteDebuffKind.DecreaseAttack, 10),
        Bleeding(EliteZoneElement.Earth), Decrease(EliteZoneElement.Earth, EliteDebuffKind.DecreaseAttackSpeed, 10),
        Poisoning(EliteZoneElement.Wind), Decrease(EliteZoneElement.Wind, EliteDebuffKind.DecreaseDefense, 20),
        Bleeding(EliteZoneElement.Darkness), Decrease(EliteZoneElement.Darkness, EliteDebuffKind.DecreaseAttackSpeed, 10),
    ];

    /// <summary>
    /// Creates the weekly drop limits from <c>Item/ItemDropLimit.xml</c> (item index = group * 512 + number): Garuda's
    /// Flame and Sealed Armor Fragment 5, Guardian Elite Option Conversion Fragment, Golden Crest, Elite Guardian Upgrade
    /// Stone and Jewel of Harmony 10.
    /// </summary>
    /// <returns>The limits.</returns>
    public static List<EliteDropLimit> CreateDropLimits() =>
    [
        Limit(7122, 5), Limit(7639, 5), Limit(7637, 10), Limit(7124, 10), Limit(7636, 10), Limit(7210, 10),
    ];

    private static EliteDebuff Bleeding(EliteZoneElement element)
        => new() { Element = element, Kind = EliteDebuffKind.Bleeding, Value = 17, MaximumValue = 12000, Seconds = 13 };

    private static EliteDebuff Poisoning(EliteZoneElement element)
        => new() { Element = element, Kind = EliteDebuffKind.Poisoning, Value = 2, MaximumValue = 3, Seconds = 13 };

    private static EliteDebuff Decrease(EliteZoneElement element, EliteDebuffKind kind, int value)
        => new() { Element = element, Kind = kind, Value = value, Seconds = 15 };

    private static EliteDropLimit Limit(int index, int maximum)
        => new() { ItemGroup = (byte)(index / 512), ItemNumber = (short)(index % 512), MaximumPerWeek = maximum };
}
