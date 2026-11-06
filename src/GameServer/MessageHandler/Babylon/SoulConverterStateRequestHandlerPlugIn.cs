// <copyright file="SoulConverterStateRequestHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.SoulConverter;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles the <see cref="SoulConverterStateRequest"/> of the Babylon client: the state of the converter window.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.SoulConverterStateRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.SoulConverterStateRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("6A1E3C97-4F28-4D05-B7C3-8E2A9D6F1B34")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class SoulConverterStateRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => SoulConverterStateRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (SoulConverterPlugIn.Get(player.GameContext) is { } plugIn)
        {
            await plugIn.ShowStateAsync(player).ConfigureAwait(false);
        }
    }
}
