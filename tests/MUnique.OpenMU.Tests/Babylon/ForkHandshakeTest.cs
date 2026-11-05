// <copyright file="ForkHandshakeTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using System.IO;
using System.Text.Json;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.PlayerActions.Babylon;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.Inventory;
using MUnique.OpenMU.GameLogic.Views.PlayerShop;
using MUnique.OpenMU.GameServer;
using MUnique.OpenMU.GameServer.RemoteView;
using MUnique.OpenMU.GameServer.RemoteView.Babylon;
using MUnique.OpenMU.GameServer.RemoteView.Inventory;
using MUnique.OpenMU.GameServer.RemoteView.PlayerShop;
using MUnique.OpenMU.Network.Packets.Babylon;

/// <summary>
/// Tests the hello and welcome of the Babylon protocol (docs/babylon.md, "Protocol").
/// </summary>
[TestFixture]
public class ForkHandshakeTest
{
    private static readonly uint WireW32 = ForkFeatures.BitOf(ItemSerializerW32.FeatureId);
    private static readonly uint ClassCodeBit = ForkFeatures.BitOf(ClassCodes.FeatureId);

    /// <summary>
    /// Tests that the fork carries the protocol, the W32 items and the class codes.
    /// </summary>
    [Test]
    public void ForkCarriesTheProtocolFeatures()
    {
        Assert.That(ForkInfo.Has(ForkFeatures.ProtocolFeatureId), Is.True);
        Assert.That(ForkFeatures.Available & (WireW32 | ClassCodeBit), Is.EqualTo(WireW32 | ClassCodeBit));
    }

    /// <summary>
    /// Tests that every bit id is a feature id once, and that the bits keep their numbers.
    /// </summary>
    [Test]
    public void BitIdsAreUniqueAndStable()
    {
        Assert.That(ForkFeatures.BitIds, Is.Unique);
        Assert.That(ForkFeatures.BitIds, Has.All.Matches("^[a-z][a-z0-9_]{0,39}$"));
        Assert.That(WireW32, Is.EqualTo(1u));
        Assert.That(ClassCodeBit, Is.EqualTo(2u));
        Assert.That(ForkFeatures.BitOf("not_a_feature"), Is.Zero);
    }

    /// <summary>
    /// Tests that the negotiation keeps only the wanted features which the fork carries.
    /// </summary>
    [Test]
    public void NegotiationKeepsWantedAndCarried()
    {
        Assert.That(ForkFeatures.Negotiate(uint.MaxValue), Is.EqualTo(ForkFeatures.Available));
        Assert.That(ForkFeatures.Negotiate(ClassCodeBit), Is.EqualTo(ClassCodeBit));
        Assert.That(ForkFeatures.Negotiate(0), Is.Zero);
    }

    /// <summary>
    /// Tests the welcome of a Babylon client which wants W32 and the class codes.
    /// </summary>
    [Test]
    public async ValueTask WelcomeAnnouncesW32AndClassCodesAsync()
    {
        var (player, output) = CreateBabylonPlayer();

        await new ForkHelloAction().HelloAsync(player, ForkFeatures.WireVersion, WireW32 | ClassCodeBit).ConfigureAwait(false);

        var data = output.ToArray();
        var welcome = new ForkWelcome(data);
        Assert.Multiple(() =>
        {
            Assert.That(data, Has.Length.EqualTo(ForkWelcome.Length));
            Assert.That(data[..4], Is.EqualTo(new byte[] { 0xC1, (byte)ForkWelcome.Length, 0xEE, 0xF1 }));
            Assert.That(welcome.ServerWire, Is.EqualTo(ForkFeatures.WireVersion));
            Assert.That(welcome.ActiveFeatures, Is.EqualTo(WireW32 | ClassCodeBit));
            Assert.That(welcome.ItemFormat, Is.EqualTo((byte)ItemFormat.W32));
            Assert.That(welcome.ClassEncoding, Is.EqualTo((byte)ClassEncoding.ClassCode));
            Assert.That(welcome.MaximumLevel, Is.EqualTo(400));
            Assert.That(welcome.MaximumStars, Is.Zero);
            Assert.That(player.ForkFeatures, Is.EqualTo(WireW32 | ClassCodeBit));
        });
    }

    /// <summary>
    /// Tests that a client of an unknown wire version gets no feature, so it stays on Season 6.
    /// </summary>
    [Test]
    public async ValueTask OldWireGetsNoFeatureAsync()
    {
        var (player, output) = CreateBabylonPlayer();

        await new ForkHelloAction().HelloAsync(player, 0, uint.MaxValue).ConfigureAwait(false);

        Assert.That(new ForkWelcome(output.ToArray()).ActiveFeatures, Is.Zero);
    }

    /// <summary>
    /// Tests that the welcome announces Season 6 items and drops the W32 bit when the W32 serializer is not active.
    /// </summary>
    [Test]
    public async ValueTask WithoutTheW32SerializerTheItemsStaySeason6Async()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        player.GameContext.PlugInManager.RegisterPlugIn<IViewPlugIn, ItemSerializer>();
        player.GameContext.PlugInManager.RegisterPlugIn<IViewPlugIn, ForkWelcomePlugIn>();
        player.ClientVersion = BabylonClient.Version;

        await new ForkHelloAction().HelloAsync(player, ForkFeatures.WireVersion, WireW32).ConfigureAwait(false);

        var welcome = new ForkWelcome(output.ToArray());
        Assert.That(welcome.ItemFormat, Is.EqualTo((byte)ItemFormat.Season6));
        Assert.That(welcome.ActiveFeatures & WireW32, Is.Zero);
    }

    /// <summary>
    /// Tests that the Season 6 client never gets the welcome plugin or the W32 serializer.
    /// </summary>
    [Test]
    public void Season6ClientGetsNoForkPlugIn()
    {
        var (player, _) = CreateBabylonPlayer();
        player.ClientVersion = new(6, 3, Network.PlugIns.ClientLanguage.English);

        Assert.That(player.ViewPlugIns.GetPlugIn<GameLogic.Views.Babylon.IForkWelcomePlugIn>(), Is.Null);
        Assert.That(player.ViewPlugIns.GetPlugIn<IItemSerializer>(), Is.InstanceOf<ItemSerializer>());
    }

    /// <summary>
    /// Tests that the Babylon client gets the player shop packets whose items take the W32 size, because the
    /// Season 6 ones have fixed 12 byte items.
    /// </summary>
    [Test]
    public void BabylonClientGetsTheW32ShopPackets()
    {
        var (player, _) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        var manager = player.GameContext.PlugInManager;
        manager.RegisterPlugIn<IViewPlugIn, ShowShopItemListPlugIn>();
        manager.RegisterPlugIn<IViewPlugIn, ShowShopItemListExtendedPlugIn>();
        manager.RegisterPlugIn<IViewPlugIn, ShowShopItemListW32PlugIn>();
        manager.RegisterPlugIn<IViewPlugIn, PlayerShopBuyRequestResultPlugIn>();
        manager.RegisterPlugIn<IViewPlugIn, PlayerShopBuyRequestResultExtendedPlugIn>();
        manager.RegisterPlugIn<IViewPlugIn, PlayerShopBuyRequestResultW32PlugIn>();

        player.ClientVersion = BabylonClient.Version;
        Assert.That(player.ViewPlugIns.GetPlugIn<IShowShopItemListPlugIn>(), Is.InstanceOf<ShowShopItemListW32PlugIn>());
        Assert.That(player.ViewPlugIns.GetPlugIn<IPlayerShopBuyRequestResultPlugIn>(), Is.InstanceOf<PlayerShopBuyRequestResultW32PlugIn>());

        player.ClientVersion = new(6, 3, Network.PlugIns.ClientLanguage.English);
        Assert.That(player.ViewPlugIns.GetPlugIn<IShowShopItemListPlugIn>()?.GetType(), Is.EqualTo(typeof(ShowShopItemListPlugIn)));
    }

    /// <summary>
    /// Tests that fork.json lists the protocol features, as <see cref="ForkInfo"/> does.
    /// </summary>
    [Test]
    public void MarkerFileListsTheProtocolFeatures()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "fork.json")))
        {
            directory = directory.Parent;
        }

        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory!.FullName, "fork.json")));
        var features = document.RootElement.GetProperty("features").EnumerateArray().Select(f => f.GetString()).ToList();
        Assert.That(features, Is.SupersetOf(new[] { ForkFeatures.ProtocolFeatureId, ItemSerializerW32.FeatureId, ClassCodes.FeatureId }));
    }

    private static (RemotePlayer Player, MemoryStream Output) CreateBabylonPlayer()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        player.GameContext.Configuration.MaximumLevel = 400;
        player.GameContext.PlugInManager.RegisterPlugIn<IViewPlugIn, ItemSerializer>();
        player.GameContext.PlugInManager.RegisterPlugIn<IViewPlugIn, ItemSerializerW32>();
        player.GameContext.PlugInManager.RegisterPlugIn<IViewPlugIn, ForkWelcomePlugIn>();
        player.ClientVersion = BabylonClient.Version;
        return (player, output);
    }
}
