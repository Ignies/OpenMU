// <copyright file="SeasonPassStateRequestHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.SeasonPass;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles the <see cref="SeasonPassStateRequest"/> of the Babylon client: the state of the season pass window.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.SeasonPassStateRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.SeasonPassStateRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("F06B3D49-8E5C-4A17-AD24-1C9F5A7B3E80")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class SeasonPassStateRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => SeasonPassStateRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (SeasonPassPlugIn.Get(player.GameContext) is { } plugIn)
        {
            await plugIn.ShowStateAsync(player).ConfigureAwait(false);
        }
    }
}
