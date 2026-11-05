// <copyright file="AppearanceWideTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.GameServer;
using MUnique.OpenMU.GameServer.RemoteView;
using MUnique.OpenMU.GameServer.RemoteView.World;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Tests the wide appearance of the Babylon protocol: the class code and the W32 gear of players in view.
/// </summary>
[TestFixture]
public class AppearanceWideTest
{
    private IGameContext _gameContext = null!;

    /// <summary>
    /// Sets up the Season 6 configuration.
    /// </summary>
    [OneTimeSetUp]
    public async ValueTask SetupAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        await new DataInitialization(contextProvider, new NullLoggerFactory()).CreateInitialDataAsync(0, false).ConfigureAwait(false);
        var gameConfiguration = (await contextProvider.CreateNewConfigurationContext().GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        var mapInitializer = new MapInitializer(gameConfiguration, new NullLogger<MapInitializer>(), NullDropGenerator.Instance, null);
        var gameContext = new GameContext(gameConfiguration, contextProvider, mapInitializer, new NullLoggerFactory(), new PlugInManager(null, new NullLoggerFactory(), null, null), NullDropGenerator.Instance, new ConfigurationChangeMediator());
        mapInitializer.PlugInManager = gameContext.PlugInManager;
        mapInitializer.PathFinderPool = gameContext.PathFinderPool;
        this._gameContext = gameContext;
    }

    /// <summary>
    /// A Babylon client gets the Season 6 character packet and then the class code and the W32 gear of the player.
    /// </summary>
    [Test]
    public async ValueTask PlayerInViewHasClassCodeAndW32GearAsync()
    {
        var (viewer, output) = CreateBabylonViewer();
        var (other, sword) = await this.CreateEquippedPlayerAsync(128 + 56).ConfigureAwait(false); // Grow Lancer

        await new NewPlayersInScopeBabylonPlugIn(viewer).NewPlayersInScopeAsync([other]).ConfigureAwait(false);

        var data = output.ToArray();
        Assert.That(data[0], Is.EqualTo(0xC2));
        Assert.That(data[3], Is.EqualTo(0x12), "the Season 6 packet comes first");
        var offset = (data[1] << 8) | data[2];
        var wide = new PlayerAppearanceWide(data.AsMemory(offset, (data[offset + 1] << 8) | data[offset + 2]));
        Assert.That(wide.Header.Code, Is.EqualTo(0xEE));
        Assert.That(wide.PlayerId, Is.EqualTo(other.GetId(viewer)));
        Assert.That(wide.ClassCode, Is.EqualTo(56));
        Assert.That(wide.ItemCount, Is.EqualTo(1));
        var expected = new byte[ItemSerializerW32.Length];
        new ItemSerializerW32().SerializeItem(expected, sword);
        Assert.That(wide[0].Slot, Is.EqualTo(sword.ItemSlot));
        Assert.That(wide[0].ItemData.ToArray(), Is.EqualTo(expected));
    }

    /// <summary>
    /// A changed piece of gear goes to a Babylon client with its slot and W32 data; a removed one as 0xFF.
    /// </summary>
    [Test]
    public async ValueTask AppearanceChangeHasSlotAndW32DataAsync()
    {
        var (viewer, output) = CreateBabylonViewer();
        var (other, sword) = await this.CreateEquippedPlayerAsync(128 + 11).ConfigureAwait(false); // Dragon Knight

        await new AppearanceChangedBabylonPlugIn(viewer).AppearanceChangedAsync(other, sword, true).ConfigureAwait(false);
        var equipped = new AppearanceChangedWide(output.ToArray());
        var expected = new byte[ItemSerializerW32.Length];
        new ItemSerializerW32().SerializeItem(expected, sword);
        Assert.That(equipped.PlayerId, Is.EqualTo(other.GetId(viewer)));
        Assert.That(equipped.Slot, Is.EqualTo(sword.ItemSlot));
        Assert.That(equipped.ClassCode, Is.EqualTo(11));
        Assert.That(equipped.ItemData.ToArray(), Is.EqualTo(expected));

        output.SetLength(0);
        await other.Inventory!.RemoveItemAsync(sword).ConfigureAwait(false);
        await new AppearanceChangedBabylonPlugIn(viewer).AppearanceChangedAsync(other, sword, false).ConfigureAwait(false);
        var removed = new AppearanceChangedWide(output.ToArray());
        Assert.That(removed.ItemData.ToArray(), Has.All.EqualTo(0xFF));
    }

    /// <summary>
    /// The Babylon client takes the wide plugins; a Season 6 client keeps its own.
    /// </summary>
    [Test]
    public void OnlyTheBabylonClientGetsTheWidePlugIns()
    {
        var (player, _) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        var manager = player.GameContext.PlugInManager;
        manager.RegisterPlugIn<IViewPlugIn, NewPlayersInScopePlugIn>();
        manager.RegisterPlugIn<IViewPlugIn, NewPlayersInScopeBabylonPlugIn>();
        manager.RegisterPlugIn<IViewPlugIn, AppearanceChangedPlugIn>();
        manager.RegisterPlugIn<IViewPlugIn, AppearanceChangedBabylonPlugIn>();

        player.ClientVersion = BabylonClient.Version;
        Assert.That(player.ViewPlugIns.GetPlugIn<INewPlayersInScopePlugIn>(), Is.InstanceOf<NewPlayersInScopeBabylonPlugIn>());
        Assert.That(player.ViewPlugIns.GetPlugIn<IAppearanceChangedPlugIn>(), Is.InstanceOf<AppearanceChangedBabylonPlugIn>());

        player.ClientVersion = new(6, 3, Network.PlugIns.ClientLanguage.English);
        Assert.That(player.ViewPlugIns.GetPlugIn<INewPlayersInScopePlugIn>()?.GetType(), Is.EqualTo(typeof(NewPlayersInScopePlugIn)));
        Assert.That(player.ViewPlugIns.GetPlugIn<IAppearanceChangedPlugIn>()?.GetType(), Is.EqualTo(typeof(AppearanceChangedPlugIn)));
    }

    private static (RemotePlayer Player, System.IO.MemoryStream Output) CreateBabylonViewer()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        var manager = player.GameContext.PlugInManager;
        manager.RegisterPlugIn<IViewPlugIn, AppearanceSerializer>();
        manager.RegisterPlugIn<IViewPlugIn, ItemSerializer>();
        manager.RegisterPlugIn<IViewPlugIn, ItemSerializerW32>();
        player.ClientVersion = BabylonClient.Version;
        return (player, output);
    }

    private async ValueTask<(Player Player, Item Sword)> CreateEquippedPlayerAsync(byte classNumber)
    {
        var player = await PlayerTestHelper.CreatePlayerAsync(this._gameContext).ConfigureAwait(false);
        await player.PlayerState.TryAdvanceToAsync(PlayerState.EnteredWorld).ConfigureAwait(false);
        player.SelectedCharacter!.CharacterClass!.Number = classNumber;
        var sword = player.PersistenceContext.CreateNew<Item>();
        sword.Definition = this._gameContext.Configuration.Items.First(i => i.Name.ValueInNeutralLanguage == "Blade");
        sword.Level = 11;
        sword.Durability = 40;
        await player.Inventory!.AddItemAsync(InventoryConstants.LeftHandSlot, sword).ConfigureAwait(false);
        return (player, sword);
    }
}
