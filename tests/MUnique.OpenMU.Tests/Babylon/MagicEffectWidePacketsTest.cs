// <copyright file="MagicEffectWidePacketsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.GameServer;
using MUnique.OpenMU.GameServer.RemoteView;
using MUnique.OpenMU.GameServer.RemoteView.World;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.Packets.ServerToClient;
using BasicModel = MUnique.OpenMU.Persistence.BasicModel;

/// <summary>
/// Tests the magic effects numbered past a byte (EE 32), such as the Blessing of Eunice (447), which the Season 6
/// packet (07) can't carry.
/// </summary>
[TestFixture]
public class MagicEffectWidePacketsTest
{
    private const short Eunice = 447;

    /// <summary>
    /// The Babylon client gets the wide effect view; a Season 6 client keeps the Season 6 one.
    /// </summary>
    [Test]
    public void OnlyTheBabylonClientGetsTheWideEffectView()
    {
        var (player, _) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        var manager = player.GameContext.PlugInManager;
        manager.RegisterPlugIn<IViewPlugIn, DeActivateMagicEffectPlugIn>();
        manager.RegisterPlugIn<IViewPlugIn, DeActivateMagicEffectBabylonPlugIn>();

        player.ClientVersion = BabylonClient.Version;
        Assert.That(player.ViewPlugIns.GetPlugIn<IActivateMagicEffectPlugIn>(), Is.InstanceOf<DeActivateMagicEffectBabylonPlugIn>());
        Assert.That(player.ViewPlugIns.GetPlugIn<IDeactivateMagicEffectPlugIn>(), Is.InstanceOf<DeActivateMagicEffectBabylonPlugIn>());

        player.ClientVersion = new(6, 3, Network.PlugIns.ClientLanguage.English);
        Assert.That(player.ViewPlugIns.GetPlugIn<IActivateMagicEffectPlugIn>(), Is.InstanceOf<DeActivateMagicEffectPlugIn>());
    }

    /// <summary>
    /// An effect past a byte goes in the wide packet with its number and remaining time, and its end too.
    /// </summary>
    [Test]
    public async ValueTask AnEffectPastAByteGoesInTheWidePacketAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);
        var view = new DeActivateMagicEffectBabylonPlugIn(player);
        var effect = CreateEffect(Eunice, TimeSpan.FromMinutes(30));

        await view.ActivateMagicEffectAsync(effect, player).ConfigureAwait(false);
        var data = output.ToArray();
        Assert.That(data, Has.Length.EqualTo(MagicEffectStatusWide.Length));
        var added = new MagicEffectStatusWide(data);
        Assert.That((added.Header.Code, added.Header.SubCode), Is.EqualTo(((byte)0xEE, (byte)0x32)));
        Assert.That((added.IsActive, added.ObjectId, added.EffectNumber, added.SecondsLeft), Is.EqualTo((true, player.GetId(player), (ushort)Eunice, 1800u)));

        output.SetLength(0);
        await view.DeactivateMagicEffectAsync(effect, player).ConfigureAwait(false);
        var removed = new MagicEffectStatusWide(output.ToArray());
        Assert.That((removed.IsActive, removed.EffectNumber, removed.SecondsLeft), Is.EqualTo((false, (ushort)Eunice, 0u)));
    }

    /// <summary>
    /// An effect which fits a byte keeps the Season 6 packet on the Babylon client.
    /// </summary>
    [Test]
    public async ValueTask AnEffectOfAByteKeepsTheSeasonSixPacketAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);

        await new DeActivateMagicEffectBabylonPlugIn(player).ActivateMagicEffectAsync(CreateEffect(187, TimeSpan.FromMinutes(1)), player).ConfigureAwait(false);

        var packet = new MagicEffectStatus(output.ToArray());
        Assert.That((packet.Header.Code, packet.IsActive, packet.EffectId), Is.EqualTo(((byte)0x07, true, (byte)187)));
    }

    /// <summary>
    /// The Season 6 packet never carries an effect past a byte: its number would be cut to another effect (447 to 191).
    /// </summary>
    [Test]
    public async ValueTask TheSeasonSixViewSendsNoEffectPastAByteAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(new(6, 3, Network.PlugIns.ClientLanguage.English));

        await new DeActivateMagicEffectPlugIn(player).ActivateMagicEffectAsync(CreateEffect(Eunice, TimeSpan.FromMinutes(30)), player).ConfigureAwait(false);

        Assert.That(output.Length, Is.Zero);
    }

    private static MagicEffect CreateEffect(short number, TimeSpan duration)
    {
        var definition = new BasicModel.MagicEffectDefinition { Number = number, SendDuration = true };
        return new MagicEffect(duration, definition);
    }
}
