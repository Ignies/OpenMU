// <copyright file="Season21EliteZonesInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.EliteZones;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Places the elites of the later maps in their elite zones and configures the <see cref="EliteZonesFeaturePlugIn"/>.
/// </summary>
internal class Season21EliteZonesInitializer
{
    /// <summary>
    /// The number of Twisted Karutan, whose elite zones the server terrain of the data walls in.
    /// </summary>
    internal const byte TwistedKarutanNumber = 142;

    /// <summary>
    /// The first number of the spawn areas of the zones; the maps update numbers its spawns from 10 up.
    /// </summary>
    private const short FirstSpawnNumber = 2000;

    /// <summary>
    /// The offset of the tile data in a terrain file, behind its header.
    /// </summary>
    private const int TerrainHeaderSize = 3;

    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21EliteZonesInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Season21EliteZonesInitializer(IContext context, GameConfiguration gameConfiguration)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
    }

    /// <summary>
    /// Creates the zones' configuration of the plugin for the maps a game has.
    /// </summary>
    /// <param name="maps">The numbers of the maps.</param>
    /// <returns>The zones.</returns>
    public static List<EliteZoneDefinition> CreateZones(IReadOnlySet<short> maps)
        => Season21EliteZoneData.Zones.Where(z => maps.Contains(z.Map)).Select(z => new EliteZoneDefinition
        {
            MapNumber = z.Map,
            Element = z.Element,
            EntranceX = z.EntranceX,
            EntranceY = z.EntranceY,
            X1 = z.X1,
            Y1 = z.Y1,
            X2 = z.X2,
            Y2 = z.Y2,
        }).ToList();

    /// <summary>
    /// Applies the zones.
    /// </summary>
    public void Initialize()
    {
        this.CreateMonsters();
        this.OpenTwistedKarutan();
        foreach (var group in Season21EliteZoneData.Zones.GroupBy(z => z.Map))
        {
            if (this.FindMap(group.Key) is { } map)
            {
                this.PlaceElites(map, group.ToList());
            }
        }

        this.Configure();
    }

    private static bool HasSpawnTile(GameMapTerrain terrain, Season21EliteZone zone)
    {
        for (var x = zone.SpawnX1; x <= zone.SpawnX2; x++)
        {
            for (var y = zone.SpawnY1; y <= zone.SpawnY2; y++)
            {
                if (terrain.WalkMap[x, y] && !terrain.SafezoneMap[x, y])
                {
                    return true;
                }
            }
        }

        return false;
    }

    private GameMapDefinition? FindMap(byte number)
        => this._gameConfiguration.Maps.FirstOrDefault(map => map.Number == number && map.Discriminator == 0);

    private void CreateMonsters()
    {
        foreach (var (mapNumber, row) in Season21EliteZoneData.Monsters.Where(m => this._gameConfiguration.Monsters.All(d => d.Number != m.Row.Number)))
        {
            if (this.FindMap(mapNumber) is { } map)
            {
                this._gameConfiguration.Monsters.Add(Season21MapsInitializer.CreateMonster(this._context, this._gameConfiguration, row, map.IntroducedIn));
            }
        }
    }

    private void OpenTwistedKarutan()
    {
        if (this.FindMap(TwistedKarutanNumber) is not { TerrainData: { Length: > TerrainHeaderSize } terrain } map)
        {
            return;
        }

        var opened = terrain.ToArray();
        foreach (var (y, x1, x2) in Season21EliteZoneData.TwistedKarutanOpenings)
        {
            for (var x = x1; x <= x2; x++)
            {
                var index = TerrainHeaderSize + (y * 256) + x;
                if (index < opened.Length && opened[index] == (byte)TerrainAttributeType.Blocked)
                {
                    opened[index] = 0;
                }
            }
        }

        if (!opened.AsSpan().SequenceEqual(terrain))
        {
            map.TerrainData = opened;
        }
    }

    private void PlaceElites(GameMapDefinition map, IReadOnlyList<Season21EliteZone> zones)
    {
        var elites = zones.SelectMany(z => z.Elites).Select(e => e.Monster).ToHashSet();
        var terrain = new GameMapTerrain(map);
        var wanted = new List<(Season21EliteZone Zone, Season21EliteSpawn Elite, MonsterDefinition Monster)>();
        foreach (var zone in zones.Where(z => HasSpawnTile(terrain, z)))
        {
            foreach (var elite in zone.Elites)
            {
                if (this._gameConfiguration.Monsters.FirstOrDefault(m => m.Number == elite.Monster) is { } monster)
                {
                    wanted.Add((zone, elite, monster));
                }
            }
        }

        var existing = map.MonsterSpawns.Where(s => s.MonsterDefinition is { } m && elites.Contains(m.Number)).ToList();
        var number = FirstSpawnNumber;
        foreach (var (zone, elite, monster) in wanted)
        {
            number++;
            var kept = existing.FirstOrDefault(s => s.MonsterDefinition == monster && s.X1 == zone.SpawnX1 && s.Y1 == zone.SpawnY1 && s.X2 == zone.SpawnX2 && s.Y2 == zone.SpawnY2);
            if (kept is not null)
            {
                existing.Remove(kept);
                kept.Quantity = elite.Quantity;
                continue;
            }

            var area = this._context.CreateNew<MonsterSpawnArea>();
            area.SetGuid(map.Number, number);
            area.GameMap = map;
            area.MonsterDefinition = monster;
            area.Quantity = elite.Quantity;
            area.SpawnTrigger = SpawnTrigger.Automatic;
            (area.X1, area.Y1, area.X2, area.Y2) = (zone.SpawnX1, zone.SpawnY1, zone.SpawnX2, zone.SpawnY2);
            map.MonsterSpawns.Add(area);
        }

        foreach (var spawn in existing)
        {
            map.MonsterSpawns.Remove(spawn);
        }
    }

    private void Configure()
    {
        var configuration = Season21PlugInConfigurations.GetOrCreate<EliteZoneConfiguration>(this._context, this._gameConfiguration, typeof(EliteZonesFeaturePlugIn).GUID, out var plugIn);
        if (configuration is null)
        {
            return;
        }

        var maps = this._gameConfiguration.Maps.Where(m => m.Discriminator == 0).Select(m => m.Number).ToHashSet();
        var missing = CreateZones(maps).Where(z => !configuration.Zones.Any(c => c.MapNumber == z.MapNumber && c.Element == z.Element)).ToList();
        if (missing.Count > 0)
        {
            // The first application turns the feature on; later ones leave the operator's switch alone.
            plugIn.IsActive |= configuration.Zones.Count == 0;
            configuration.Zones.AddRange(missing);
            plugIn.SetConfiguration(configuration, null);
        }
    }
}
