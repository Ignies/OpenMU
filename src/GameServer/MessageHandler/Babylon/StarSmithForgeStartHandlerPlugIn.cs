// <copyright file="StarSmithForgeStartHandlerPlugIn.cs" company="MUnique">
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
/// Handles the Constellation Forge start (EE 99); a no-op while Star Smithing is off.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.StarSmithForgeStartHandlerPlugIn_Name), Description = nameof(PlugInResources.StarSmithForgeStartHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("117A4002-19E1-4C95-8614-2D872AA283D5")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class StarSmithForgeStartHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => StarSmithForgeStart.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < StarSmithForgeStart.Length || StarSmithingFeaturePlugIn.Get(player.GameContext) is not { } smithing)
        {
            return;
        }

        StarSmithForgeStart message = packet;
        var ingredients = message.IngredientSlots[..Math.Min((int)message.IngredientCount, message.IngredientSlots.Length)].ToArray();
        var catalysts = message.CatalystSlots[..Math.Min((int)message.CatalystCount, message.CatalystSlots.Length)].ToArray();
        await smithing.StartForgeAsync(player, message.TargetSlot, ingredients, catalysts).ConfigureAwait(false);
    }
}
