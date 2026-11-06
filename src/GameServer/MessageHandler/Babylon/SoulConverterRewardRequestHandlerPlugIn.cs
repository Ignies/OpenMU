// <copyright file="SoulConverterRewardRequestHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.SoulConverter;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles the <see cref="SoulConverterRewardRequest"/> of the Babylon client: the reward of a completed converter.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.SoulConverterRewardRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.SoulConverterRewardRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("2F6C9A84-3D1E-4B57-8E0A-5C7D2B9F4E61")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class SoulConverterRewardRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => SoulConverterRewardRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < SoulConverterRewardRequest.Length)
        {
            return;
        }

        SoulConverterRewardRequest message = packet;
        var kind = (SoulRewardKind)message.RewardKind;
        var result = SoulConverterPlugIn.Get(player.GameContext) is { } plugIn
            ? await plugIn.ClaimAsync(player, kind).ConfigureAwait(false)
            : SoulConverterClaimResult.Off;
        await player.InvokeViewPlugInAsync<ISoulConverterViewPlugIn>(p => p.ShowSoulConverterResultAsync(result, kind)).ConfigureAwait(false);
    }
}
