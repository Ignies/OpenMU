// <copyright file="EnhanceTreeAddPointHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.PlayerActions.Character;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles the <see cref="EnhanceTreeAddPoint"/> of the Babylon client: a point for a node of the skill enhancement tree.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.EnhanceTreeAddPointHandlerPlugIn_Name), Description = nameof(PlugInResources.EnhanceTreeAddPointHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("C1B3D9F4-2E6A-4B85-A7D0-5F9C3E1A8B62")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class EnhanceTreeAddPointHandlerPlugIn : ISubPacketHandlerPlugIn
{
    private readonly AddEnhanceTreePointAction _action = new();

    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => EnhanceTreeAddPoint.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < EnhanceTreeAddPoint.Length)
        {
            return;
        }

        EnhanceTreeAddPoint message = packet;
        await this._action.AddPointAsync(player, (short)message.SkillNumber).ConfigureAwait(false);
    }
}
