// <copyright file="MuunPacketsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using System.Buffers.Binary;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Muuns;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.Muuns;
using MUnique.OpenMU.GameServer;
using MUnique.OpenMU.GameServer.RemoteView;
using MUnique.OpenMU.GameServer.RemoteView.Muuns;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Persistence.InMemory;
using BasicModel = MUnique.OpenMU.Persistence.BasicModel;

/// <summary>
/// Tests the Muun packets (EE 50-59) and the Muun fields of the W32 item format; a Season 6 client gets none of them.
/// </summary>
[TestFixture]
public class MuunPacketsTest
{
    private const uint TimedOptionSeconds = 1_794_528_000;

    private static readonly DateTime TimedOptionEnd = new(2026, 11, 13, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// The fork marker carries the Muun feature, protocol bit 11.
    /// </summary>
    [Test]
    public void ForkCarriesTheMuunFeature()
    {
        Assert.That(GameLogic.ForkInfo.Has(MuunFeaturePlugIn.FeatureId), Is.True);
        Assert.That(GameLogic.ForkFeatures.BitOf(MuunFeaturePlugIn.FeatureId), Is.EqualTo(1u << 11));
        Assert.That(GameLogic.ForkFeatures.Available & (1u << 11), Is.Not.Zero);
    }

    /// <summary>
    /// A Season 6 client has no Muun view, so it has no Muun system and keeps every Season 6 packet.
    /// </summary>
    [Test]
    public void OnlyTheBabylonClientGetsTheMuunView()
    {
        var (player, _) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        player.GameContext.PlugInManager.RegisterPlugIn<IViewPlugIn, MuunBabylonPlugIn>();
        player.GameContext.FeaturePlugIns.AddPlugIn(new MuunFeaturePlugIn(), true);

        player.ClientVersion = BabylonClient.Version;
        Assert.That(player.ViewPlugIns.GetPlugIn<IMuunViewPlugIn>(), Is.InstanceOf<MuunBabylonPlugIn>());
        Assert.That(MuunService.IsAvailable(player), Is.True);

        player.ClientVersion = new(6, 3, Network.PlugIns.ClientLanguage.English);
        Assert.That(player.ViewPlugIns.GetPlugIn<IMuunViewPlugIn>(), Is.Null);
        Assert.That(MuunService.IsAvailable(player), Is.False);
    }

    /// <summary>
    /// The Muun inventory lists the slot and the W32 item of every item.
    /// </summary>
    [Test]
    public async ValueTask MuunInventoryListHasItsFieldsAsync()
    {
        var (player, output) = CreateBabylonPlayer();
        var muun = CreateMuun();

        await new MuunBabylonPlugIn(player).ShowMuunInventoryAsync([muun]).ConfigureAwait(false);

        var packet = new MuunInventoryList(output.ToArray());
        Assert.That((packet.Header.Code, packet.Header.SubCode, packet.ItemCount), Is.EqualTo(((byte)0xEE, (byte)0x50, (byte)1)));
        Assert.That(packet[0].Slot, Is.EqualTo(MuunSlots.Main));
        Assert.That((packet[0].ItemData[0], BinaryPrimitives.ReadUInt16LittleEndian(packet[0].ItemData[1..]), packet[0].ItemData[3]), Is.EqualTo(((byte)16, (ushort)4, (byte)3)));
    }

    /// <summary>
    /// The appearance names the player, the Muun and whether it is ridden; the options carry their flags.
    /// </summary>
    [Test]
    public async ValueTask AppearanceAndOptionsHaveTheirFieldsAsync()
    {
        var (player, output) = CreateBabylonPlayer();
        var view = new MuunBabylonPlugIn(player);

        await view.ShowMuunAppearanceAsync(player, CreateMuun(), true).ConfigureAwait(false);
        var appearance = new MuunAppearance(output.ToArray());
        Assert.That(output.Length, Is.EqualTo(MuunAppearance.Length));
        Assert.That((appearance.Header.SubCode, appearance.ItemGroup, appearance.ItemNumber, appearance.ItemLevel, appearance.IsRiding), Is.EqualTo(((byte)0x56, (byte)16, (ushort)4, (byte)3, true)));

        output.SetLength(0);
        await view.ShowMuunAppearanceAsync(player, null, false).ConfigureAwait(false);
        Assert.That(new MuunAppearance(output.ToArray()).ItemGroup, Is.EqualTo(0xFF), "no Muun");

        output.SetLength(0);
        await view.ShowMuunOptionsAsync([new MuunActiveOption(MuunSlots.Sub, MuunOptionType.Defense, 760, true, true)]).ConfigureAwait(false);
        var options = new MuunOptions(output.ToArray());
        Assert.That((options.Header.SubCode, options.OptionCount), Is.EqualTo(((byte)0x57, (byte)1)));
        Assert.That((options[0].Slot, options[0].OptionType, options[0].Value, options[0].Flags), Is.EqualTo(((byte)1, (byte)2, (ushort)760, (byte)3)));
    }

    /// <summary>
    /// The result of an action carries the changed item, or 0xFF for none; the exchange list carries its offers.
    /// </summary>
    [Test]
    public async ValueTask ActionResultAndExchangeHaveTheirFieldsAsync()
    {
        var (player, output) = CreateBabylonPlayer();
        var view = new MuunBabylonPlugIn(player);

        await view.MuunActionResultAsync(MuunAction.Evolve, MuunActionResult.LevelTooLow, MuunStorageKind.MuunInventory, 7, null).ConfigureAwait(false);
        var response = new MuunActionResponse(output.ToArray());
        Assert.That((response.Header.SubCode, response.Action, response.Result, response.Storage, response.Slot), Is.EqualTo(((byte)0x55, (byte)2, (byte)5, (byte)1, (byte)7)));
        Assert.That(response.ItemData.ToArray(), Is.All.EqualTo(0xFF));

        output.SetLength(0);
        await view.ShowMuunExchangeAsync([new MuunExchangeOffer { CostGroup = 16, CostNumber = 240, CostCount = 2, ResultGroup = 16, ResultNumber = 211, TargetNumber = 411 }]).ConfigureAwait(false);
        var list = new MuunExchangeList(output.ToArray());
        Assert.That((list.Header.SubCode, list.OfferCount), Is.EqualTo(((byte)0x58, (byte)1)));
        Assert.That((list[0].CostNumber, list[0].CostCount, list[0].ResultNumber, list[0].TargetNumber), Is.EqualTo(((ushort)240, (byte)2, (ushort)211, (ushort)411)));
    }

    /// <summary>
    /// W32 carries the end of the additional option of a Muun and the Muun of an evolution stone in the kind block
    /// (bytes 22-25 and 22-23), both ways.
    /// </summary>
    [Test]
    public void W32CarriesTheTimedOptionAndTheEvolutionTarget()
    {
        var w32 = new ItemSerializerW32();
        var muun = CreateMuun();
        var stoneDefinition = CreateStoneDefinition();
        var configuration = new BasicModel.GameConfiguration();
        configuration.Items.Add(muun.Definition!);
        configuration.Items.Add(stoneDefinition);
        var context = new InMemoryPersistenceContextProvider().CreateNewContext();
        var stone = new BasicModel.Item { Definition = stoneDefinition, Durability = 1 };
        Assert.That(MuunService.SetEvolutionTarget(stone, 411, context), Is.True);

        var data = new byte[ItemSerializerW32.Length];
        w32.SerializeItem(data, muun);
        Assert.That(BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(22)), Is.EqualTo(TimedOptionSeconds));
        Assert.That(BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(28)), Is.Zero, "the Muun itself doesn't expire");
        Assert.That(w32.DeserializeItem(data, configuration, context).TimedOptionExpiresAt, Is.EqualTo(TimedOptionEnd));

        Array.Clear(data);
        w32.SerializeItem(data, stone);
        Assert.That(BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(22)), Is.EqualTo(411));
        Assert.That(MuunService.GetEvolutionTarget(w32.DeserializeItem(data, configuration, context)), Is.EqualTo((short)411));
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

    private static BasicModel.Item CreateMuun()
    {
        return new BasicModel.Item
        {
            Definition = new BasicModel.ItemDefinition { Group = 16, Number = 4, Width = 1, Height = 1, Durability = 255 },
            Durability = 200,
            Level = 3,
            ItemSlot = MuunSlots.Main,
            TimedOptionExpiresAt = TimedOptionEnd,
        };
    }

    private static BasicModel.ItemDefinition CreateStoneDefinition()
    {
        var target = new BasicModel.ItemOptionDefinition { Name = "Muun Evolution Target", MaximumOptionsPerItem = 1 };
        target.PossibleOptions.Add(new BasicModel.IncreasableItemOption { OptionType = Season21ItemOptionTypes.MuunEvolutionTarget });
        var stone = new BasicModel.ItemDefinition { Group = 16, Number = 211, Width = 1, Height = 1, Durability = 1 };
        stone.PossibleItemOptions.Add(target);
        return stone;
    }
}
