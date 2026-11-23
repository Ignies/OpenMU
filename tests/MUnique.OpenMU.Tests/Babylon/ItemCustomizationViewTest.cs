// <copyright file="ItemCustomizationViewTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using System.Text;
using MUnique.OpenMU.GameLogic.ItemCustomization;
using MUnique.OpenMU.GameLogic.Views.ItemCustomization;
using MUnique.OpenMU.GameServer.RemoteView.ItemCustomization;
using MUnique.OpenMU.Network.Packets.Babylon;

/// <summary>
/// The bytes the Babylon client gets: EE 34 with each customised item, EE 36 with a lore.
/// </summary>
[TestFixture]
public class ItemCustomizationViewTest
{
    private static readonly DataModel.Entities.ItemCustomization Frost = new()
    {
        Name = "Frost Edge",
        Lore = "Forged in ice.\nNever thawed.",
        Rarity = 4,
        Look = """{"mode":"tone","color":"#1e90ff","strength":90,"glow":"#3366ff"}""",
        Version = 7,
    };

    /// <summary>
    /// EE 34 lists each item with its place, slot, version, flags, rarity, look and name.
    /// </summary>
    [Test]
    public async ValueTask TheCustomizationsCarryEachItemAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(GameServer.BabylonClient.Version);
        var plain = new DataModel.Entities.ItemCustomization { Rarity = 0, Version = 1 };
        await new ItemCustomizationBabylonPlugIn(player).SendCustomizationsAsync(
        [
            new ItemCustomizationEntryData(ItemCustomizationStorage.Worn, 0x1234, 3, Frost),
            new ItemCustomizationEntryData(ItemCustomizationStorage.Inventory, 0x0200, 40, plain),
        ]).ConfigureAwait(false);

        var bytes = output.ToArray();
        Assert.That(bytes.Take(5), Is.EqualTo(new byte[] { 0xC2, 0x00, 7 + (2 * 123), 0xEE, 0x34 }));
        var packet = new ItemCustomizationsRef(bytes);
        Assert.That(packet.Count, Is.EqualTo(2));
        var first = packet[0];
        Assert.That((first.Storage, first.ObjectId, first.Slot, first.Version, first.Flags, first.Rarity), Is.EqualTo(((byte)0, (ushort)0x1234, (byte)3, 7u, (byte)(1 | 2 | 4), (byte)4)));
        Assert.That(first.Look[..6].ToArray(), Is.EqualTo(new byte[] { 1, 2, 0x1E, 0x90, 0xFF, 90 }));
        Assert.That(Encoding.UTF8.GetString(first.Name).TrimEnd('\0'), Is.EqualTo("Frost Edge"));
        var second = packet[1];
        Assert.That((second.Storage, second.Slot, second.Flags), Is.EqualTo(((byte)1, (byte)40, (byte)0)));
        Assert.That(second.Look.ToArray(), Is.All.EqualTo(0));
    }

    /// <summary>
    /// EE 39 pushes a changed place unasked, laid out as EE 34.
    /// </summary>
    [Test]
    public async ValueTask AChangeIsPushedAsEe39Async()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(GameServer.BabylonClient.Version);
        var send = typeof(ItemCustomizationBabylonPlugIn).GetMethod("SendChangedAsync");
        Assert.That(send, Is.Not.Null);
        IReadOnlyList<ItemCustomizationEntryData> entries = [new ItemCustomizationEntryData(ItemCustomizationStorage.Inventory, 0x0200, 47, new DataModel.Entities.ItemCustomization { Lore = "Kept.", Version = 2 })];
        await ((ValueTask)send!.Invoke(new ItemCustomizationBabylonPlugIn(player), [entries])!).ConfigureAwait(false);
        var bytes = output.ToArray();
        Assert.That(bytes.Take(7), Is.EqualTo(new byte[] { 0xC2, 0x00, 7 + 123, 0xEE, 0x39, 1, 0 }));
        Assert.That(bytes.Skip(7).Take(10), Is.EqualTo(new byte[] { 1, 0x02, 0x00, 47, 2, 0, 0, 0, 4, 0 }));
    }

    /// <summary>
    /// EE 38 answers a report with one byte.
    /// </summary>
    [Test]
    public async ValueTask TheReportResultIsOneByteAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(GameServer.BabylonClient.Version);
        var send = typeof(ItemCustomizationBabylonPlugIn).GetMethod("SendReportResultAsync");
        Assert.That(send, Is.Not.Null);
        await ((ValueTask)send!.Invoke(new ItemCustomizationBabylonPlugIn(player), [GameLogic.ItemCustomization.ItemCustomizationReportResult.TooFast])!).ConfigureAwait(false);
        Assert.That(output.ToArray(), Is.EqualTo(new byte[] { 0xC1, 5, 0xEE, 0x38, 3 }));
    }

    /// <summary>
    /// EE 36 carries the lore of one item at its version.
    /// </summary>
    [Test]
    public async ValueTask TheDetailCarriesTheLoreAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(GameServer.BabylonClient.Version);
        await new ItemCustomizationBabylonPlugIn(player).SendDetailAsync(new ItemCustomizationEntryData(ItemCustomizationStorage.Vault, 0x0200, 12, Frost)).ConfigureAwait(false);

        var packet = new ItemCustomizationDetailRef(output.ToArray());
        Assert.That((packet.Storage, packet.ObjectId, packet.Slot, packet.Version), Is.EqualTo(((byte)2, (ushort)0x0200, (byte)12, 7u)));
        Assert.That(Encoding.UTF8.GetString(packet.Lore).TrimEnd('\0'), Is.EqualTo("Forged in ice.\nNever thawed."));
    }

    /// <summary>
    /// Star Smithing PR 6: EE 36 grows to 451 bytes with the smith level (433) and up to three boosts (435 count, then
    /// stat and value times 100); EE 34 sets flag 32 when an item has boosts, so the client asks for them.
    /// </summary>
    [Test]
    public async ValueTask TheDetailCarriesTheBoostsAsync()
    {
        var smithed = new DataModel.Entities.ItemCustomization { Rarity = 3, Version = 9, Boosts = "0:625;2:30000", SmithLevel = 40 };
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(GameServer.BabylonClient.Version);
        await new ItemCustomizationBabylonPlugIn(player).SendDetailAsync(new ItemCustomizationEntryData(ItemCustomizationStorage.Inventory, 0x0200, 12, smithed)).ConfigureAwait(false);

        var bytes = output.ToArray();
        Assert.That(bytes, Has.Length.EqualTo(451));
        Assert.That(bytes.Take(3), Is.EqualTo(new byte[] { 0xC2, 0x01, 0xC3 }));
        Assert.That(bytes.Skip(433).ToArray(), Is.EqualTo(new byte[] { 40, 0, 2, 0, 0x71, 0x02, 0, 0, 2, 0x30, 0x75, 0, 0, 0, 0, 0, 0, 0 }));

        var (other, list) = CastleSiegeRemoteViewTestHelper.CreatePlayer(GameServer.BabylonClient.Version);
        await new ItemCustomizationBabylonPlugIn(other).SendCustomizationsAsync([new ItemCustomizationEntryData(ItemCustomizationStorage.Inventory, 0x0200, 12, smithed)]).ConfigureAwait(false);
        Assert.That(new ItemCustomizationsRef(list.ToArray())[0].Flags, Is.EqualTo((byte)32));
    }
}
