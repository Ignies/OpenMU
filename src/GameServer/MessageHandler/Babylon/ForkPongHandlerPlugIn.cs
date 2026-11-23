// <copyright file="ForkPongHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.StarSmithing;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles the answer to the beat ping (EE F4): only its token is read, the time is the server's own; a no-op while
/// Star Smithing is off.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.ForkPongHandlerPlugIn_Name), Description = nameof(PlugInResources.ForkPongHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("3F7A1C52-9B4E-4D8A-A6C1-5E2B7D9F0A14")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class ForkPongHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => ForkPong.SubCode;

    /// <inheritdoc/>
    public ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length >= ForkPong.Length && StarSmithingFeaturePlugIn.Get(player.GameContext) is { } smithing)
        {
            ForkPong message = packet;
            smithing.Pong(player, message.Token);
        }

        return ValueTask.CompletedTask;
    }
}
