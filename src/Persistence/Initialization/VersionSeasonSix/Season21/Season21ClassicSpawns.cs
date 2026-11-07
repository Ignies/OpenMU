// <copyright file="Season21ClassicSpawns.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.Persistence.Initialization.Properties;

/// <summary>
/// The monster rosters of Season 6 maps which the Season 21 server data changed, and the repair of a database which still
/// has the Season 6 roster.
/// </summary>
internal static class Season21ClassicSpawns
{
    private const byte DeviasNumber = 2;

    private const short FirstSpawnNumber = 3800;

    private const short FirstTownNpcSpawnNumber = 3900;

    /// <summary>
    /// The town NPCs of S21Emu <c>MonsterSetBase</c> which the Season 6 towns lack: map, NPC, name, x, y, direction and the
    /// version of an NPC which Season 6 doesn't have. Captain Slough (the Acheron guide) and the maple NPCs stand in Noria,
    /// and Lorencia got the entrance NPCs of the events (Apostle Devin, Moss, Lugard, Jerint). Sent (764) is placed by the
    /// evolution quests, Wizard Gillard (891) by the shops.
    /// </summary>
    private static readonly (byte Map, short Npc, LocalizedString Name, byte X, byte Y, byte Direction, GameVersion? IntroducedIn)[] TownNpcs =
    [
        (3, 580, LocalizedString.FromResource(() => MonsterNames.CaptainSlough), 196, 113, 2, GameVersion.Season7),
        (3, 873, LocalizedString.FromResource(() => MonsterNames.MapleSpirit), 179, 126, 1, GameVersion.Season21Episode1),
        (3, 874, LocalizedString.FromResource(() => MonsterNames.MapleTree), 179, 129, 3, GameVersion.Season21Episode1),
        (0, 406, LocalizedString.FromResource(() => MonsterNames.PriestDevin), 142, 135, 2, null),
        (0, 492, LocalizedString.FromResource(() => MonsterNames.MossTheMerchant), 141, 134, 3, null),
        (0, 540, LocalizedString.FromResource(() => MonsterNames.Lugard), 130, 128, 3, null),
        (0, 522, LocalizedString.FromResource(() => MonsterNames.AdviserJerinteu), 130, 126, 3, null),
    ];

    /// <summary>
    /// The monster spawns of Devias of the Season 6 initialization: monster, x1, x2, y1, y2 and quantity (560 monsters).
    /// </summary>
    private static readonly (short Monster, byte X1, byte X2, byte Y1, byte Y2, short Quantity)[] Season6Devias =
    [
        (20, 194, 194, 165, 165, 10),
        (20, 36, 36, 25, 25, 10),
        (20, 210, 242, 210, 220, 15),
        (20, 0, 251, 128, 245, 200),
        (25, 0, 128, 128, 245, 75),
        (23, 0, 128, 0, 128, 75),
        (22, 0, 128, 0, 128, 75),
        (24, 128, 251, 0, 128, 65),
        (21, 128, 251, 0, 128, 35),
    ];

    /// <summary>
    /// The monster spawns of Devias of S21Emu <c>MonsterSetBase/002 - Devias.xml</c>: the spots, then the single monsters
    /// (280 monsters: 40 Yeti, 82 Elite Yeti, 11 Ice Queens).
    /// </summary>
    private static readonly (short Monster, byte X1, byte X2, byte Y1, byte Y2, short Quantity)[] Season21Devias =
    [
        (24, 170, 245, 80, 200, 30),
        (23, 170, 245, 80, 200, 10),
        (22, 50, 166, 6, 244, 40),
        (23, 50, 166, 6, 244, 30),
        (19, 50, 166, 6, 244, 40),
        (21, 50, 166, 6, 244, 30),
        (20, 50, 166, 6, 244, 20),
        (20, 5, 50, 50, 155, 20),
        (20, 6, 80, 184, 244, 10),
        (25, 6, 80, 184, 244, 3),
        (25, 74, 74, 239, 239, 1),
        (25, 70, 70, 212, 212, 1),
        (25, 45, 45, 189, 189, 1),
        (25, 113, 113, 230, 230, 1),
        (25, 94, 94, 203, 203, 1),
        (25, 63, 63, 160, 160, 1),
        (20, 203, 203, 244, 244, 1),
        (20, 194, 194, 241, 241, 1),
        (20, 203, 203, 232, 232, 1),
        (20, 209, 209, 224, 224, 1),
        (20, 186, 186, 224, 224, 1),
        (20, 200, 200, 171, 171, 1),
        (20, 207, 207, 158, 158, 1),
        (20, 221, 221, 177, 177, 1),
        (20, 213, 213, 193, 193, 1),
        (20, 196, 196, 218, 218, 1),
        (20, 230, 230, 200, 200, 1),
        (20, 221, 221, 160, 160, 1),
        (20, 205, 205, 149, 149, 1),
        (20, 184, 184, 166, 166, 1),
        (20, 212, 212, 170, 170, 1),
        (25, 116, 116, 210, 210, 1),
        (20, 203, 203, 201, 201, 1),
        (24, 170, 170, 6, 6, 1),
        (24, 161, 161, 58, 58, 1),
        (24, 180, 180, 78, 78, 1),
        (20, 190, 190, 146, 146, 1),
        (25, 51, 51, 227, 227, 1),
        (20, 189, 189, 160, 160, 1),
        (20, 207, 207, 183, 183, 1),
        (20, 218, 218, 201, 201, 1),
        (20, 240, 240, 207, 207, 1),
        (20, 238, 238, 184, 184, 1),
        (20, 227, 227, 167, 167, 1),
        (20, 202, 202, 209, 209, 1),
        (20, 113, 113, 105, 105, 1),
        (20, 96, 96, 103, 103, 1),
        (20, 102, 102, 117, 117, 1),
        (20, 241, 241, 198, 198, 1),
        (20, 228, 228, 184, 184, 1),
        (20, 193, 193, 231, 231, 1),
        (20, 214, 214, 152, 152, 1),
        (20, 197, 197, 183, 183, 1),
        (24, 162, 162, 11, 11, 1),
        (24, 244, 244, 80, 80, 1),
        (24, 238, 238, 88, 88, 1),
        (24, 229, 229, 83, 83, 1),
    ];

    /// <summary>
    /// Places the town NPCs of the Season 21 data (<see cref="TownNpcs"/>); an NPC which already stands at its point stays
    /// as it is.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public static void PlaceTownNpcs(IContext context, GameConfiguration gameConfiguration)
    {
        var number = FirstTownNpcSpawnNumber;
        foreach (var (map, npc, name, x, y, direction, introducedIn) in TownNpcs)
        {
            Season21NpcSpawns.Place(context, gameConfiguration, number++, map, npc, name, x, y, (Direction)direction, introducedIn);
        }
    }

    /// <summary>
    /// Replaces the monster spawns of Devias by the ones of the Season 21 data when they are still the Season 6 ones; a
    /// roster the operator changed stays, and so do the NPCs.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public static void Repair(IContext context, GameConfiguration gameConfiguration)
    {
        if (gameConfiguration.Maps.FirstOrDefault(m => m.Number == DeviasNumber && m.Discriminator == 0) is not { } map)
        {
            return;
        }

        var spawns = map.MonsterSpawns.Where(s => s.MonsterDefinition?.ObjectKind == NpcObjectKind.Monster).ToList();
        var roster = spawns.Select(s => ((short)s.MonsterDefinition!.Number, s.X1, s.X2, s.Y1, s.Y2, s.Quantity)).OrderBy(r => r).ToList();
        if (!roster.SequenceEqual(Season6Devias.OrderBy(r => r)))
        {
            return;
        }

        var monsters = Season21Devias.Select(r => r.Monster).Distinct()
            .ToDictionary(n => n, n => gameConfiguration.Monsters.FirstOrDefault(m => m.Number == n));
        if (monsters.Values.Any(m => m is null))
        {
            return;
        }

        spawns.ForEach(s => map.MonsterSpawns.Remove(s));
        var terrain = new GameMapTerrain(map);
        var number = FirstSpawnNumber;
        foreach (var (monster, dataX1, dataX2, dataY1, dataY2, quantity) in Season21Devias)
        {
            var (x1, x2, y1, y2) = dataX1 == dataX2 && dataY1 == dataY2 && NearestSpawnTile(terrain, dataX1, dataY1) is { } tile
                ? (tile.X, tile.X, tile.Y, tile.Y)
                : (dataX1, dataX2, dataY1, dataY2);
            var area = context.CreateNew<MonsterSpawnArea>();
            area.SetGuid(map.Number, number++);
            area.GameMap = map;
            area.MonsterDefinition = monsters[monster];
            (area.X1, area.X2, area.Y1, area.Y2) = (x1, x2, y1, y2);
            area.Quantity = quantity;
            area.Direction = Direction.Undefined;
            area.SpawnTrigger = SpawnTrigger.Automatic;
            map.MonsterSpawns.Add(area);
        }
    }

    /// <summary>
    /// Gets the nearest tile to a point of the data on which a monster can spawn: the point itself when it is walkable and
    /// no safe zone. A few points of the data lie on blocked tiles, where the game server can't place their monster.
    /// </summary>
    private static (byte X, byte Y)? NearestSpawnTile(GameMapTerrain terrain, byte x, byte y)
    {
        const int maximumDistance = 5;
        for (var distance = 0; distance <= maximumDistance; distance++)
        {
            for (var dx = -distance; dx <= distance; dx++)
            {
                for (var dy = -distance; dy <= distance; dy++)
                {
                    if (Math.Max(Math.Abs(dx), Math.Abs(dy)) == distance
                        && x + dx is >= 0 and <= byte.MaxValue && y + dy is >= 0 and <= byte.MaxValue
                        && terrain.WalkMap[x + dx, y + dy] && !terrain.SafezoneMap[x + dx, y + dy])
                    {
                        return ((byte)(x + dx), (byte)(y + dy));
                    }
                }
            }
        }

        return null;
    }
}
