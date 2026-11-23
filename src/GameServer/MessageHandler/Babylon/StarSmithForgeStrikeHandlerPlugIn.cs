// <copyright file="StarSmithForgeStrikeHandlerPlugIn.cs" company="MUnique">
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
/// Handles a Constellation Forge strike (EE 9A); the server stamps the time on arrival. A no-op while Star Smithing is off.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.StarSmithForgeStrikeHandlerPlugIn_Name), Description = nameof(PlugInResources.StarSmithForgeStrikeHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("06E10C89-73B7-4BB7-B09F-86FACD22E766")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class StarSmithForgeStrikeHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => StarSmithForgeStrike.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < StarSmithForgeStrike.Length || StarSmithingFeaturePlugIn.Get(player.GameContext) is not { } smithing)
        {
            return;
        }

        StarSmithForgeStrike message = packet;
        await smithing.StrikeForgeAsync(player, message.Star).ConfigureAwait(false);
    }
}
