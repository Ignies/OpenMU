// <copyright file="StarSmithForgeSealHandlerPlugIn.cs" company="MUnique">
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
/// Handles the Constellation Forge seal (EE 9B); a no-op while Star Smithing is off.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.StarSmithForgeSealHandlerPlugIn_Name), Description = nameof(PlugInResources.StarSmithForgeSealHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("B69F5CBA-27A5-4532-AAA6-D32345F563E5")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class StarSmithForgeSealHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => StarSmithForgeSeal.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < StarSmithForgeSeal.Length || StarSmithingFeaturePlugIn.Get(player.GameContext) is not { } smithing)
        {
            return;
        }

        await smithing.SealForgeAsync(player).ConfigureAwait(false);
    }
}
