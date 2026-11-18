// <copyright file="StarSmithingViewTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The bytes the Babylon client gets from Star Smithing: EE 91 (the smith's state), EE 92 (the hammering) and EE 97 (an action's answer).
/// </summary>
[TestFixture]
public class StarSmithingViewTest
{
    private const string PlugIn = "MUnique.OpenMU.GameServer.RemoteView.StarSmithing.StarSmithingBabylonPlugIn, MUnique.OpenMU.GameServer";

    /// <summary>
    /// EE 91: the hammer flags, the level, the experience, the experience to the next level, the hammer's durability.
    /// </summary>
    [Test]
    public async ValueTask TheStateAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(GameServer.BabylonClient.Version);
        await CallAsync(player, "SendStateAsync", new StarSmithState(HammerHeld: true, HammerBroken: false, Level: 3, Experience: 47, NextLevelExperience: 579, HammerDurability: 250)).ConfigureAwait(false);
        Assert.That(output.ToArray(), Is.EqualTo(new byte[] { 0xC1, 16, 0xEE, 0x91, 1, 3, 0, 47, 0, 0, 0, 0x43, 0x02, 0, 0, 250 }));
    }

    /// <summary>
    /// EE 97: the action, the result, the text refusal, the Zen it cost.
    /// </summary>
    [Test]
    public async ValueTask TheResultAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(GameServer.BabylonClient.Version);
        await CallAsync(player, "SendResultAsync", StarSmithAction.Recolor, StarSmithResultCode.NotEnoughZen, (byte)0, 500_000u).ConfigureAwait(false);
        Assert.That(output.ToArray(), Is.EqualTo(new byte[] { 0xC1, 12, 0xEE, 0x97, 1, 7, 0, 0x20, 0xA1, 0x07, 0x00, 0 }));
    }

    /// <summary>
    /// EE 92: the smith as the receiver knows it (itself: the constant id), the action, the strikes.
    /// </summary>
    [Test]
    public async ValueTask TheAnimationAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(GameServer.BabylonClient.Version);
        await CallAsync(player, "SendAnimationAsync", player, StarSmithAction.Rename, (byte)3).ConfigureAwait(false);
        Assert.That(output.ToArray(), Is.EqualTo(new byte[] { 0xC1, 8, 0xEE, 0x92, 0x02, 0x00, 2, 3 }));
    }

    private static async ValueTask CallAsync(object player, string method, params object[] arguments)
    {
        var type = Type.GetType(PlugIn);
        Assert.That(type, Is.Not.Null);
        var send = type!.GetMethod(method);
        Assert.That(send, Is.Not.Null);
        await ((ValueTask)send!.Invoke(Activator.CreateInstance(type, player), arguments)!).ConfigureAwait(false);
    }
}
