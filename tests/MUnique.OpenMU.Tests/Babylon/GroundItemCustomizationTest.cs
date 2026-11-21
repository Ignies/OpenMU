// <copyright file="GroundItemCustomizationTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.ItemCustomization;
using MUnique.OpenMU.Pathfinding;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;
using BasicModel = MUnique.OpenMU.Persistence.BasicModel;

/// <summary>
/// Tests the ground place of the item customisation channel: an item lying on the asking player's map, named by its
/// drop id, is told as slot 0 while it is within the player's view, so a smithed item keeps its name, look and lore
/// when it is dropped.
/// </summary>
[TestFixture]
public class GroundItemCustomizationTest
{
    private const ItemCustomizationStorage Ground = ItemCustomizationStorage.GroundItem;

    /// <summary>
    /// A dropped item in view is collected by its drop id, as slot 0, whatever bag slot it last had.
    /// </summary>
    [Test]
    public async ValueTask DropInViewIsCollectedAsSlotZeroAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var item = CreateItem(slot: 23);
        await using var drop = await DropAsync(player, item, new Point(3, 4)).ConfigureAwait(false);

        var found = await ItemCustomizationCollector.CollectAsync(player, [(Ground, drop.Id)]).ConfigureAwait(false);

        Assert.That(found, Is.EqualTo(new[] { (Ground, drop.Id, (byte)0, item.Id) }));
        Assert.That(await ItemCustomizationCollector.ItemIdAsync(player, Ground, drop.Id, 0).ConfigureAwait(false), Is.EqualTo(item.Id));
    }

    /// <summary>
    /// A drop out of the player's view, or a drop id that names nothing, gives nothing.
    /// </summary>
    [Test]
    public async ValueTask DropOutOfViewGivesNothingAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        await using var far = await DropAsync(player, CreateItem(slot: 12), new Point(200, 200)).ConfigureAwait(false);

        var found = await ItemCustomizationCollector.CollectAsync(player, [(Ground, far.Id), (Ground, (ushort)(far.Id + 1))]).ConfigureAwait(false);

        Assert.That(found, Is.Empty);
        Assert.That(await ItemCustomizationCollector.ItemIdAsync(player, Ground, far.Id, 0).ConfigureAwait(false), Is.Null);
    }

    /// <summary>
    /// A test player whose map keeps drops for a minute (the test configuration's zero would remove them at once)
    /// and who sees 15 tiles around.
    /// </summary>
    private static async ValueTask<Player> CreatePlayerAsync()
    {
        var configuration = PlayerTestHelper.CreateGameConfiguration();
        configuration.ItemDropDuration = TimeSpan.FromMinutes(1);
        configuration.InfoRange = 15;
        var mapInitializer = new MapInitializer(configuration, new NullLogger<MapInitializer>(), NullDropGenerator.Instance, null);
        var gameContext = new GameContext(configuration, new InMemoryPersistenceContextProvider(), mapInitializer, new NullLoggerFactory(), new PlugInManager(null, new NullLoggerFactory(), null, null), NullDropGenerator.Instance, new ConfigurationChangeMediator());
        mapInitializer.PlugInManager = gameContext.PlugInManager;
        mapInitializer.PathFinderPool = gameContext.PathFinderPool;
        return await PlayerTestHelper.CreatePlayerAsync(gameContext).ConfigureAwait(false);
    }

    private static async ValueTask<DroppedItem> DropAsync(Player player, BasicModel.Item item, Point position)
    {
        var map = player.CurrentMap!;
        var drop = new DroppedItem(item, position, map, null, null);
        await map.AddAsync(drop).ConfigureAwait(false);
        return drop;
    }

    private static BasicModel.Item CreateItem(byte slot)
        => new()
        {
            Id = Guid.NewGuid(),
            ItemSlot = slot,
            Durability = 1,
            Definition = new BasicModel.ItemDefinition { Group = 0, Number = 1, Width = 1, Height = 3, Value = 100, Durability = 1 },
        };
}
