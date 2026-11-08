// <copyright file="LogInHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Login;

using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.PlayerActions;
using MUnique.OpenMU.GameServer.RemoteView;
using MUnique.OpenMU.Network;
using MUnique.OpenMU.Network.Packets;
using MUnique.OpenMU.Network.Packets.ClientToServer;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.Network.Xor;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Packet handler for login packets.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.LogInHandlerPlugIn_Name), Description = nameof(PlugInResources.LogInHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("4A816FE5-809B-4D42-AF9F-1929FABD3295")]
[BelongsToGroup(LogInOutGroup.GroupKey)]
public class LogInHandlerPlugIn : ISubPacketHandlerPlugIn
{
    private readonly ISpanDecryptor _decryptor = new Xor3Decryptor(0);

    private readonly LoginAction _loginAction = new();

    private enum LoginFormat
    {
        Long,

        Short,

        Version075,
    }

    /// <inheritdoc/>
    public bool IsEncryptionExpected => true;

    /// <inheritdoc/>
    public byte Key => LoginLongPassword.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        switch (FormatOf(packet.Length))
        {
            case LoginFormat.Short:
                LoginShortPassword shortLogin = packet;
                await this.HandleLoginAsync(player, this.Decrypt(shortLogin.Username), this.Decrypt(shortLogin.Password), shortLogin.TickCount, VersionOf(packet)).ConfigureAwait(false);
                break;
            case LoginFormat.Version075:
                // we have some version like 0.75 which just uses three bytes as version identifier
                Login075 login075 = packet;
                await this.HandleLoginAsync(player, this.Decrypt(login075.Username), this.Decrypt(login075.Password), login075.TickCount, VersionOf(packet)).ConfigureAwait(false);
                break;
            default:
                LoginLongPassword longLogin = packet;
                await this.HandleLoginAsync(player, this.Decrypt(longLogin.Username), this.Decrypt(longLogin.Password), longLogin.TickCount, VersionOf(packet)).ConfigureAwait(false);
                break;
        }
    }

    /// <summary>
    /// Gets the client version of a login packet.
    /// </summary>
    /// <param name="packet">The login packet.</param>
    /// <returns>The client version.</returns>
    internal static ClientVersion VersionOf(Memory<byte> packet)
    {
        return FormatOf(packet.Length) switch
        {
            LoginFormat.Short => ClientVersionResolver.Resolve(((LoginShortPassword)packet).ClientVersion),
            LoginFormat.Version075 => ClientVersionResolver.Resolve(((Login075)packet).ClientVersion),
            _ => ClientVersionResolver.Resolve(((LoginLongPassword)packet).ClientVersion),
        };
    }

    // The short password with its serial is 50 bytes, inside the range of the long password without one.
    private static LoginFormat FormatOf(int length) => length switch
    {
        _ when length == LoginShortPassword.Length => LoginFormat.Short,
        < 42 and > 28 + 3 => LoginFormat.Short,
        < 42 => LoginFormat.Version075,
        _ => LoginFormat.Long,
    };

    private string Decrypt(Span<byte> stringSpan)
    {
        this._decryptor.Decrypt(stringSpan);
        return stringSpan.ExtractString(0, stringSpan.Length, Encoding.UTF8);
    }

    private async ValueTask HandleLoginAsync(Player player, string username, string password, uint tickCount, ClientVersion version)
    {
        if (player.Logger.IsEnabled(LogLevel.Debug))
        {
            player.Logger.LogDebug($"User tries to log in. username:{username}, version:{version}, tickCount:{tickCount} ");
        }

        await this._loginAction.LoginAsync(player, username, password).ConfigureAwait(false);
        if (player is RemotePlayer remotePlayer)
        {
            // Set Version in RemotePlayer so that the right plugins will be selected
            remotePlayer.ClientVersion = version;
        }
    }
}
