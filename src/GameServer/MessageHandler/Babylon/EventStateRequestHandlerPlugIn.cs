// <copyright file="EventStateRequestHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.PlayerActions.MiniGames;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;
using MiniGameType = MUnique.OpenMU.DataModel.Configuration.MiniGameType;

/// <summary>
/// Handler for the <see cref="EventStateRequest"/> of the Babylon client: answers the state of a Season 21 event with
/// <see cref="EventState"/>, for its entry window.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.EventStateRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.EventStateRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("6A0C2F4B-8D31-4E7A-9B25-C3E1F7D05A68")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class EventStateRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    private readonly EventStateRequestAction _action = new();

    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => EventStateRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < EventStateRequest.Length)
        {
            return;
        }

        EventStateRequest message = packet;
        var type = (MiniGameType)message.EventType;
        if (type < MiniGameType.PartyInstance || !Enum.IsDefined(type))
        {
            return;
        }

        await this._action.ShowStateAsync(player, type, message.GameLevel).ConfigureAwait(false);
    }
}
