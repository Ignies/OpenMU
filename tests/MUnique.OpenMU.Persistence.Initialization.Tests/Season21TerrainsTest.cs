// <copyright file="Season21TerrainsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;

/// <summary>
/// Tests that the terrains of the later maps are the walk grids of the Season 21 client where the server files of the
/// data differ from it, and that a database which still has the data's files takes them.
/// </summary>
[TestFixture]
internal class Season21TerrainsTest
{
    private const byte FereaNumber = 112;
    private const byte OldKethotumNumber = 135;
    private const byte IgnisVulcanusNumber = 138;
    private const byte BloodTarkanNumber = 140;
    private const byte TwistedKarutanNumber = 142;

    private static readonly byte[] SyncedMaps = [FereaNumber, OldKethotumNumber, IgnisVulcanusNumber, BloodTarkanNumber, TwistedKarutanNumber];

    /// <summary>
    /// A tile of each synced map has the attribute of the client: Ferea's NPC mark moved from 24/124 to 230/146, the
    /// mark of Old Kethotum at 106/176 is gone, Twisted Karutan opens 148/99 and closes 40/112, and Ignis Vulcanus and
    /// Blood Tarkan have holes (no ground) where the data had walls.
    /// </summary>
    /// <param name="mapNumber">The map number.</param>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="attribute">The attribute of the client's terrain.</param>
    [TestCase(FereaNumber, 24, 124, 0)]
    [TestCase(FereaNumber, 230, 146, 3)]
    [TestCase(OldKethotumNumber, 106, 176, 0)]
    [TestCase(TwistedKarutanNumber, 148, 99, 0)]
    [TestCase(TwistedKarutanNumber, 40, 112, 4)]
    [TestCase(IgnisVulcanusNumber, 94, 63, 8)]
    [TestCase(BloodTarkanNumber, 74, 6, 8)]
    public async Task TerrainIsTheClientsAsync(byte mapNumber, byte x, byte y, byte attribute)
    {
        var (_, gameConfiguration) = await CreateWithSeason21MapsAsync().ConfigureAwait(false);

        var terrain = new GameMapTerrain(MapOf(gameConfiguration, mapNumber));

        Assert.That(terrain.AttributeMap[x, y], Is.EqualTo(attribute));
    }

    /// <summary>
    /// A database which got Ferea with the data's terrain (the NPC mark at 24/124) takes the client's.
    /// </summary>
    [Test]
    public async Task UpdateReplacesTheDataTerrainAsync()
    {
        var (context, gameConfiguration) = await CreateWithSeason21MapsAsync().ConfigureAwait(false);
        var ferea = MapOf(gameConfiguration, FereaNumber);
        var expected = ferea.TerrainData!.ToArray();
        ferea.TerrainData = WithTile(WithTile(expected.ToArray(), 24, 124, 2), 230, 146, 1);

        await new AddSeason21MapsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(ferea.TerrainData, Is.EqualTo(expected));
    }

    /// <summary>
    /// A tile whose value the operator chose stays; the other tiles of the list still take the client's.
    /// </summary>
    [Test]
    public async Task UpdateKeepsATileTheOperatorChoseAsync()
    {
        var (context, gameConfiguration) = await CreateWithSeason21MapsAsync().ConfigureAwait(false);
        var ferea = MapOf(gameConfiguration, FereaNumber);
        ferea.TerrainData = WithTile(WithTile(ferea.TerrainData!.ToArray(), 24, 124, 4), 230, 146, 1);

        await new AddSeason21MapsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var terrain = new GameMapTerrain(ferea);
        Assert.That(terrain.AttributeMap[24, 124], Is.EqualTo(4));
        Assert.That(terrain.AttributeMap[230, 146], Is.EqualTo(3));
    }

    /// <summary>
    /// Every gate and every monster spawn area of the synced maps keeps a tile to stand on.
    /// </summary>
    /// <param name="mapNumber">The map number.</param>
    [TestCaseSource(nameof(SyncedMaps))]
    public async Task GatesAndSpawnsKeepAWalkableTileAsync(byte mapNumber)
    {
        var (_, gameConfiguration) = await CreateWithSeason21MapsAsync().ConfigureAwait(false);
        var map = MapOf(gameConfiguration, mapNumber);
        var terrain = new GameMapTerrain(map);

        var gates = map.ExitGates.Where(g => !HasWalkableTile(terrain, g.X1, g.Y1, g.X2, g.Y2, false)).Select(g => $"gate {g.X1}/{g.Y1}");
        var spawns = map.MonsterSpawns
            .Where(s => s.MonsterDefinition?.ObjectKind == NpcObjectKind.Monster)
            .Where(s => !HasWalkableTile(terrain, s.X1, s.Y1, s.X2, s.Y2, true))
            .Select(s => $"monster {s.MonsterDefinition!.Number} @ {s.X1}/{s.Y1}");

        Assert.That(gates.Concat(spawns), Is.Empty);
    }

    private static bool HasWalkableTile(GameMapTerrain terrain, byte x1, byte y1, byte x2, byte y2, bool outsideSafezone)
        => Enumerable.Range(x1, x2 - x1 + 1).Any(x => Enumerable.Range(y1, y2 - y1 + 1)
            .Any(y => terrain.WalkMap[x, y] && !(outsideSafezone && terrain.SafezoneMap[x, y])));

    private static byte[] WithTile(byte[] terrain, byte x, byte y, byte attribute)
    {
        terrain[3 + (y * 256) + x] = attribute;
        return terrain;
    }

    private static GameMapDefinition MapOf(GameConfiguration gameConfiguration, byte mapNumber)
        => gameConfiguration.Maps.Single(m => m.Number == mapNumber && m.Discriminator == 0);

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateWithSeason21MapsAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        var dataInitialization = new VersionSeasonSix.DataInitialization(contextProvider, new NullLoggerFactory());
        await dataInitialization.CreateInitialDataAsync(1, true).ConfigureAwait(false);
        var context = contextProvider.CreateNewContext();
        var gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        await new AddSeason21LevelCapsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21MapsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        return (context, gameConfiguration);
    }
}
