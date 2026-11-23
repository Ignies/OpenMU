// <copyright file="StarSmithForgeAbandonHandlerPlugIn.cs" company="MUnique">
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
/// Handles the Constellation Forge abandon (EE 9C); a no-op while Star Smithing is off.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.StarSmithForgeAbandonHandlerPlugIn_Name), Description = nameof(PlugInResources.StarSmithForgeAbandonHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("3C56451D-9476-4398-A091-9BA3A3EEBA25")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class StarSmithForgeAbandonHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => StarSmithForgeAbandon.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < StarSmithForgeAbandon.Length || StarSmithingFeaturePlugIn.Get(player.GameContext) is not { } smithing)
        {
            return;
        }

        await smithing.AbandonForgeAsync(player).ConfigureAwait(false);
    }
}
