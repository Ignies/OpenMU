// <copyright file="Season21NpcSpawns.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.Interfaces;

/// <summary>
/// Places passive NPCs of the Season 21 updates on existing maps.
/// </summary>
internal static class Season21NpcSpawns
{
    /// <summary>
    /// Defines a passive NPC when the game has none of the number, and places it at a point unless it stands there.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    /// <param name="spawnNumber">The number of the spawn area's id.</param>
    /// <param name="mapNumber">The map number.</param>
    /// <param name="npcNumber">The NPC number.</param>
    /// <param name="name">The name of the NPC.</param>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="direction">The direction.</param>
    /// <param name="introducedIn">The version which introduced a new NPC; the map's version when not given.</param>
    public static void Place(IContext context, GameConfiguration gameConfiguration, short spawnNumber, byte mapNumber, short npcNumber, LocalizedString name, byte x, byte y, Direction direction, GameVersion? introducedIn = null)
    {
        if (gameConfiguration.Maps.FirstOrDefault(m => m.Number == mapNumber && m.Discriminator == 0) is not { } map)
        {
            return;
        }

        if (gameConfiguration.Monsters.FirstOrDefault(m => m.Number == npcNumber) is not { } npc)
        {
            npc = Season21MapsInitializer.CreateNpc(context, npcNumber, name, introducedIn ?? map.IntroducedIn);
            gameConfiguration.Monsters.Add(npc);
        }

        if (map.MonsterSpawns.Any(s => s.MonsterDefinition == npc && s.X1 == x && s.Y1 == y))
        {
            return;
        }

        var area = context.CreateNew<MonsterSpawnArea>();
        area.SetGuid(map.Number, spawnNumber);
        area.GameMap = map;
        area.MonsterDefinition = npc;
        area.Quantity = 1;
        area.Direction = direction;
        area.SpawnTrigger = SpawnTrigger.Automatic;
        (area.X1, area.Y1, area.X2, area.Y2) = (x, y, x, y);
        map.MonsterSpawns.Add(area);
    }
}
