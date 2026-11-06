// <copyright file="EventEnterRequestHandlerPlugIn.cs" company="MUnique">
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
/// Handler for the <see cref="EventEnterRequest"/> of the Babylon client: enters a Season 21 event or party instance
/// with the usual entry rules of a mini game.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.EventEnterRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.EventEnterRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("36C5CB9A-BFD0-43AF-B011-3C3F2DAEAB56")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class EventEnterRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <summary>
    /// The ticket slot which says "no ticket"; an event with a ticket finds it in the inventory itself.
    /// </summary>
    private const byte NoTicket = 0xFF;

    private readonly EnterMiniGameAction _enterAction = new();

    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => EventEnterRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < EventEnterRequest.Length)
        {
            return;
        }

        EventEnterRequest message = packet;
        var type = (MiniGameType)message.EventType;
        if (type < MiniGameType.PartyInstance || !Enum.IsDefined(type))
        {
            // The Season 6 events keep their own entry packets.
            return;
        }

        await this._enterAction.TryEnterMiniGameAsync(player, type, message.GameLevel, NoTicket).ConfigureAwait(false);
    }
}
