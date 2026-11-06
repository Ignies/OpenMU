// <copyright file="GremoryCaseListRequestHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.GremoryCase;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles the <see cref="GremoryCaseListRequest"/> of the Babylon client: a request for the content of the gremory cases.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.GremoryCaseListRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.GremoryCaseListRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("9C1E5B73-2D8A-4F06-B3E9-7A4D1C6F8E20")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class GremoryCaseListRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => GremoryCaseListRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < GremoryCaseListRequest.Length)
        {
            return;
        }

        await GremoryCaseService.ShowAsync(player).ConfigureAwait(false);
    }
}
