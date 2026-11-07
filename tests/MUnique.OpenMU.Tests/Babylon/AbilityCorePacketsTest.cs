// <copyright file="AbilityCorePacketsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using System.Buffers.Binary;
using MUnique.OpenMU.GameLogic.AbilityCore;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.AbilityCore;
using MUnique.OpenMU.GameServer;
using MUnique.OpenMU.GameServer.RemoteView;
using MUnique.OpenMU.GameServer.RemoteView.AbilityCore;
using MUnique.OpenMU.Network.Packets.Babylon;
using BasicModel = MUnique.OpenMU.Persistence.BasicModel;

/// <summary>
/// Tests the Ability Enhancement Core packets of the Babylon protocol (EE 43-45).
/// </summary>
[TestFixture]
public class AbilityCorePacketsTest
{
    /// <summary>
    /// The fork carries the core as the feature bit 5 of the handshake.
    /// </summary>
    [Test]
    public void ForkCarriesTheCore()
    {
        Assert.That(GameLogic.ForkInfo.Has(AbilityCoreFeaturePlugIn.FeatureId), Is.True);
        Assert.That(GameLogic.ForkFeatures.BitOf(AbilityCoreFeaturePlugIn.FeatureId), Is.EqualTo(1u << 5));
        Assert.That(GameLogic.ForkFeatures.Available & (1u << 5), Is.Not.Zero);
    }

    /// <summary>
    /// Only the Babylon client gets the core view; a Season 6 client has no core window.
    /// </summary>
    [Test]
    public void OnlyTheBabylonClientGetsTheCoreView()
    {
        var (player, _) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        player.GameContext.PlugInManager.RegisterPlugIn<IViewPlugIn, AbilityCoreBabylonPlugIn>();

        player.ClientVersion = BabylonClient.Version;
        Assert.That(player.ViewPlugIns.GetPlugIn<IAbilityCoreViewPlugIn>(), Is.InstanceOf<AbilityCoreBabylonPlugIn>());

        player.ClientVersion = new(6, 3, Network.PlugIns.ClientLanguage.English);
        Assert.That(player.ViewPlugIns.GetPlugIn<IAbilityCoreViewPlugIn>(), Is.Null);
    }

    /// <summary>
    /// The state carries the open and active slots and every card with its slot and W32 item.
    /// </summary>
    [Test]
    public async ValueTask StateHasItsFieldsAsync()
    {
        var (player, output) = CreateBabylonPlayer();

        await new AbilityCoreBabylonPlugIn(player).ShowAbilityCoreAsync(0x1F, 0x11, [CreateCard()]).ConfigureAwait(false);

        var packet = new AbilityCoreState(output.ToArray());
        Assert.That((packet.Header.Code, packet.Header.SubCode, packet.OpenSlots, packet.ActiveSlots, packet.CardCount), Is.EqualTo(((byte)0xEE, (byte)0x43, 0x1Fu, 0x11u, (byte)1)));
        Assert.That(packet[0].Slot, Is.EqualTo(AbilityCoreSlots.FirstOption));
        Assert.That((packet[0].ItemData[0], BinaryPrimitives.ReadUInt16LittleEndian(packet[0].ItemData[1..]), packet[0].ItemData[3]), Is.EqualTo(((byte)20, (ushort)216, (byte)2)));
    }

    /// <summary>
    /// The response carries the result, the action, both slots and the card, or 0xFF for none.
    /// </summary>
    [Test]
    public async ValueTask ResponseHasItsFieldsAsync()
    {
        var (player, output) = CreateBabylonPlayer();

        await new AbilityCoreBabylonPlugIn(player).AbilityCoreResultAsync(AbilityCoreAction.LevelUp, AbilityCoreResult.NotEnoughStones, 4, 20, null).ConfigureAwait(false);

        var data = output.ToArray();
        Assert.That(data, Has.Length.EqualTo(AbilityCoreResponse.Length));
        var packet = new AbilityCoreResponse(data);
        Assert.That((packet.Header.SubCode, packet.Result, packet.Action, packet.CoreSlot, packet.InventorySlot), Is.EqualTo(((byte)0x45, (byte)13, (byte)2, (byte)4, (byte)20)));
        Assert.That(packet.ItemData.ToArray(), Is.All.EqualTo(0xFF));
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

    private static BasicModel.Item CreateCard()
    {
        return new BasicModel.Item
        {
            Definition = new BasicModel.ItemDefinition { Group = 20, Number = 216, Width = 1, Height = 1 },
            Level = 2,
            ItemSlot = AbilityCoreSlots.FirstOption,
        };
    }
}
