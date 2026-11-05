// <copyright file="ForkHelloHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.PlayerActions.Babylon;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handler for the hello of the Babylon client, which negotiates the features of the fork.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.ForkHelloHandlerPlugIn_Name), Description = nameof(PlugInResources.ForkHelloHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("E2B5A0C7-91D4-4F63-8B2A-5C7D1E9F3A64")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class ForkHelloHandlerPlugIn : ISubPacketHandlerPlugIn
{
    private readonly ForkHelloAction _action = new();

    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => ForkHello.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < ForkHello.Length)
        {
            return;
        }

        ForkHello message = packet;
        await this._action.HelloAsync(player, message.ClientWire, message.WantedFeatures).ConfigureAwait(false);
    }
}
