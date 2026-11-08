// <copyright file="BabylonClientTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using System.Reflection;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameServer;
using MUnique.OpenMU.GameServer.MessageHandler;
using MUnique.OpenMU.Network.Packets.ClientToServer;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Tests for the <see cref="BabylonClient"/>: its version, and that it gets exactly the plugins of the Season 6
/// client plus the ones of the fork protocol.
/// </summary>
[TestFixture]
public class BabylonClientTest
{
    private static readonly ClientVersion Season6 = new(6, 3, ClientLanguage.English);

    /// <summary>
    /// Tests that the version bytes of the login packet resolve to the Babylon client, and back.
    /// </summary>
    [Test]
    public void VersionBytesResolveToTheBabylonClient()
    {
        Span<byte> versionBytes = [(byte)'2', (byte)'1', (byte)'B', (byte)'0', (byte)'1'];

        Assert.That(ClientVersionResolver.Resolve(versionBytes), Is.EqualTo(BabylonClient.Version));
        Assert.That(ClientVersionResolver.Resolve(BabylonClient.Version), Is.EqualTo(versionBytes.ToArray()));
    }

    /// <summary>
    /// Tests that the Season 6 version bytes still resolve to the default version.
    /// </summary>
    [Test]
    public void Season6VersionBytesStayTheDefault()
    {
        Span<byte> versionBytes = [(byte)'1', (byte)'0', (byte)'4', (byte)'0', (byte)'4'];

        Assert.That(ClientVersionResolver.Resolve(versionBytes), Is.EqualTo(ClientVersionResolver.DefaultVersion));
    }

    /// <summary>
    /// The login with the short password (50 bytes with the serial, as the Babylon client sends it) is read as such:
    /// read as the long one, its version bytes came from the wrong place and the server always took Season 6.
    /// </summary>
    [Test]
    public void ReadsTheVersionOfTheShortPasswordLogin()
    {
        var packet = new byte[LoginShortPassword.Length];
        _ = new LoginShortPassword(packet);
        BabylonClient.VersionBytes.CopyTo(new LoginShortPassword(packet).ClientVersion);

        Assert.That(MUnique.OpenMU.GameServer.MessageHandler.Login.LogInHandlerPlugIn.VersionOf(packet), Is.EqualTo(BabylonClient.Version));
    }

    /// <summary>
    /// The login with the long password keeps its version.
    /// </summary>
    [Test]
    public void ReadsTheVersionOfTheLongPasswordLogin()
    {
        var packet = new byte[LoginLongPassword.Length];
        _ = new LoginLongPassword(packet);
        BabylonClient.VersionBytes.CopyTo(new LoginLongPassword(packet).ClientVersion);

        Assert.That(MUnique.OpenMU.GameServer.MessageHandler.Login.LogInHandlerPlugIn.VersionOf(packet), Is.EqualTo(BabylonClient.Version));
    }

    /// <summary>
    /// Tests that every view plugin and packet handler which suits the Season 6 client suits the Babylon client,
    /// and that the only additional ones are those of the fork protocol, which the Season 6 client never gets.
    /// </summary>
    [Test]
    public void GetsTheSeason6PlugInsAndTheForkPlugIns()
    {
        var plugInTypes = typeof(BabylonClient).Assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<PlugInAttribute>() is not null)
            .Where(t => typeof(IViewPlugIn).IsAssignableFrom(t) || typeof(IPacketHandlerPlugInBase).IsAssignableFrom(t))
            .ToList();

        var forSeason6 = plugInTypes.Where(t => Season6.IsPlugInSuitable(t)).ToHashSet();
        var forBabylon = plugInTypes.Where(t => BabylonClient.Version.IsPlugInSuitable(t)).ToHashSet();
        var forkOnly = forBabylon.Except(forSeason6).ToList();

        Assert.That(forSeason6, Is.SubsetOf(forBabylon));
        Assert.That(forkOnly, Is.Not.Empty);
        Assert.That(forkOnly.Select(t => t.GetCustomAttribute<MinimumClientAttribute>()?.Client), Has.All.EqualTo(BabylonClient.MinimumVersion));
    }
}
