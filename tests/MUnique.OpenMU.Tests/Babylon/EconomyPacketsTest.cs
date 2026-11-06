// <copyright file="EconomyPacketsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using System.Buffers.Binary;
using MUnique.OpenMU.GameLogic.GremoryCase;
using MUnique.OpenMU.GameLogic.Ruud;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.Economy;
using MUnique.OpenMU.GameServer;
using MUnique.OpenMU.GameServer.RemoteView;
using MUnique.OpenMU.GameServer.RemoteView.Economy;
using MUnique.OpenMU.Network.Packets.Babylon;
using BasicModel = MUnique.OpenMU.Persistence.BasicModel;

/// <summary>
/// Tests the packets of the gremory case, Ruud, the merchant store tabs and the item expiry (EE 31, 60-75).
/// </summary>
[TestFixture]
public class EconomyPacketsTest
{
    private const uint ExpirySeconds = 1_794_528_000;

    private static readonly DateTime Expiry = new(2026, 11, 13, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// A Season 6 client gets none of the views, so it keeps every Season 6 packet and way.
    /// </summary>
    [Test]
    public void OnlyTheBabylonClientGetsTheEconomyViews()
    {
        var (player, _) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        var manager = player.GameContext.PlugInManager;
        manager.RegisterPlugIn<IViewPlugIn, GremoryCaseBabylonPlugIn>();
        manager.RegisterPlugIn<IViewPlugIn, RuudBabylonPlugIn>();
        manager.RegisterPlugIn<IViewPlugIn, MerchantStoreTabsBabylonPlugIn>();
        manager.RegisterPlugIn<IViewPlugIn, ItemExpiredBabylonPlugIn>();

        player.ClientVersion = BabylonClient.Version;
        Assert.That(player.ViewPlugIns.GetPlugIn<IGremoryCaseViewPlugIn>(), Is.InstanceOf<GremoryCaseBabylonPlugIn>());
        Assert.That(player.ViewPlugIns.GetPlugIn<IRuudViewPlugIn>(), Is.InstanceOf<RuudBabylonPlugIn>());
        Assert.That(player.ViewPlugIns.GetPlugIn<IMerchantStoreTabsPlugIn>(), Is.InstanceOf<MerchantStoreTabsBabylonPlugIn>());
        Assert.That(player.ViewPlugIns.GetPlugIn<IItemExpiredPlugIn>(), Is.InstanceOf<ItemExpiredBabylonPlugIn>());

        player.ClientVersion = new(6, 3, Network.PlugIns.ClientLanguage.English);
        Assert.That(player.ViewPlugIns.GetPlugIn<IGremoryCaseViewPlugIn>(), Is.Null);
        Assert.That(player.ViewPlugIns.GetPlugIn<IRuudViewPlugIn>(), Is.Null);
        Assert.That(player.ViewPlugIns.GetPlugIn<IMerchantStoreTabsPlugIn>(), Is.Null);
        Assert.That(player.ViewPlugIns.GetPlugIn<IItemExpiredPlugIn>(), Is.Null);
        Assert.That(GremoryCaseService.IsAvailable(player), Is.False, "Rewards keep their Season 6 way.");
    }

    /// <summary>
    /// The list of a case has the slot, the expiry and the item of each reward; the W32 item carries the expiry too.
    /// </summary>
    [Test]
    public async ValueTask GremoryCaseListHasItsFieldsAsync()
    {
        var (player, output) = CreateBabylonPlayer();
        var item = CreateItem();

        await new GremoryCaseBabylonPlugIn(player).ShowGremoryCaseAsync(GremoryCaseKind.Account, 50, [item]).ConfigureAwait(false);

        var packet = new GremoryCaseList(output.ToArray());
        Assert.That((packet.Header.Code, packet.Header.SubCode), Is.EqualTo(((byte)0xEE, (byte)0x60)));
        Assert.That((packet.Kind, packet.Capacity, packet.ItemCount), Is.EqualTo(((byte)1, (byte)50, (byte)1)));
        var entry = packet[0];
        Assert.That((entry.Slot, entry.ExpiresAt), Is.EqualTo(((byte)3, ExpirySeconds)));
        var data = entry.ItemData;
        Assert.That((data[0], BinaryPrimitives.ReadUInt16LittleEndian(data[1..])), Is.EqualTo(((byte)14, (ushort)282)));
        Assert.That(BinaryPrimitives.ReadUInt32LittleEndian(data[28..]), Is.EqualTo(ExpirySeconds), "W32 bytes 28-31.");
        Assert.That(data[5] & 0x80, Is.EqualTo(0x80), "W32 flag 7: timed.");
    }

    /// <summary>
    /// The added reward and the claim result have their fields.
    /// </summary>
    [Test]
    public async ValueTask GremoryCaseAddedAndClaimHaveTheirFieldsAsync()
    {
        var (player, output) = CreateBabylonPlayer();
        var view = new GremoryCaseBabylonPlugIn(player);

        await view.GremoryCaseItemAddedAsync(GremoryCaseKind.Character, CreateItem()).ConfigureAwait(false);
        var added = new GremoryCaseItemAdded(output.ToArray());
        Assert.That(output.Length, Is.EqualTo(GremoryCaseItemAdded.Length));
        Assert.That((added.Kind, added.Slot, added.ExpiresAt), Is.EqualTo(((byte)0, (byte)3, ExpirySeconds)));

        output.SetLength(0);
        await view.GremoryCaseClaimResultAsync(GremoryCaseKind.Character, 3, GremoryCaseClaimResult.InventoryFull).ConfigureAwait(false);
        var claimed = new GremoryCaseClaimResponse(output.ToArray());
        Assert.That((claimed.Header.SubCode, claimed.Kind, claimed.Slot, claimed.Result), Is.EqualTo(((byte)0x63, (byte)0, (byte)3, (byte)1)));
    }

    /// <summary>
    /// The Ruud and the items of the Ruud shop have their fields.
    /// </summary>
    [Test]
    public async ValueTask RuudPacketsHaveTheirFieldsAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);
        var view = new RuudBabylonPlugIn(player);

        await view.ShowRuudAsync(150_000, -50_000).ConfigureAwait(false);
        var changed = new RuudChanged(output.ToArray());
        Assert.That((changed.Header.SubCode, changed.Ruud, unchecked((int)changed.Change)), Is.EqualTo(((byte)0x70, 150_000u, -50_000)));

        output.SetLength(0);
        var entry = new RuudShopEntry { Group = 5, Number = 41, Level = 0, LuckChance = 35, OptionChance = 30, OptionLevel = 2, Price = 100_000 };
        await view.ShowRuudShopAsync([(7, entry)]).ConfigureAwait(false);
        var list = new RuudShopList(output.ToArray());
        Assert.That((list.Header.SubCode, list.ItemCount), Is.EqualTo(((byte)0x71, (byte)1)));
        var shown = list[0];
        Assert.That((shown.Index, shown.ItemGroup, shown.ItemNumber, shown.Flags, shown.Price), Is.EqualTo(((ushort)7, (byte)5, (ushort)41, (byte)3, 100_000u)));
    }

    /// <summary>
    /// The tabs packet has the count and the active tab.
    /// </summary>
    [Test]
    public async ValueTask MerchantStoreTabsHaveTheirFieldsAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);

        await new MerchantStoreTabsBabylonPlugIn(player).ShowMerchantStoreTabsAsync(5, 2).ConfigureAwait(false);

        var packet = new MerchantStoreTabs(output.ToArray());
        Assert.That(output.Length, Is.EqualTo(MerchantStoreTabs.Length));
        Assert.That((packet.Header.SubCode, packet.TabCount, packet.ActiveTab), Is.EqualTo(((byte)0x74, (byte)5, (byte)2)));
    }

    private static (RemotePlayer Player, System.IO.MemoryStream Output) CreateBabylonPlayer()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        var manager = player.GameContext.PlugInManager;
        manager.RegisterPlugIn<IViewPlugIn, ItemSerializer>();
        manager.RegisterPlugIn<IViewPlugIn, ItemSerializerW32>();
        player.ClientVersion = BabylonClient.Version;
        return (player, output);
    }

    private static BasicModel.Item CreateItem()
    {
        return new BasicModel.Item
        {
            Definition = new BasicModel.ItemDefinition { Group = 14, Number = 282, Width = 1, Height = 1, Durability = 1 },
            Durability = 1,
            ItemSlot = 3,
            ExpiresAt = Expiry,
        };
    }
}
