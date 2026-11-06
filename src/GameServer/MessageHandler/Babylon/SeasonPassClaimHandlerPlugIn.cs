// <copyright file="SeasonPassClaimHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.SeasonPass;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles the <see cref="SeasonPassClaim"/> of the Babylon client: the reward of a level of a pass.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.SeasonPassClaimHandlerPlugIn_Name), Description = nameof(PlugInResources.SeasonPassClaimHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("E95A2C38-7D4B-4F06-9C13-0B8F4E6A2D79")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class SeasonPassClaimHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => SeasonPassClaim.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < SeasonPassClaim.Length)
        {
            return;
        }

        SeasonPassClaim message = packet;
        var pass = (SeasonPassKind)message.Pass;
        var result = SeasonPassPlugIn.Get(player.GameContext) is { } plugIn
            ? await plugIn.ClaimAsync(player, pass, message.Level).ConfigureAwait(false)
            : GameLogic.SeasonPass.SeasonPassClaimResult.Off;
        await player.InvokeViewPlugInAsync<ISeasonPassViewPlugIn>(p => p.ShowSeasonPassClaimResultAsync(pass, message.Level, result)).ConfigureAwait(false);
    }
}
