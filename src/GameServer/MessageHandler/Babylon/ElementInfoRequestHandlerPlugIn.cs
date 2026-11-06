// <copyright file="ElementInfoRequestHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Views.Character;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles the <see cref="ElementInfoRequest"/> of the Babylon client: it answers with the elemental stats.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.ElementInfoRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.ElementInfoRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("72CDFA5C-66FF-4545-8EF4-31478D6A6733")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class ElementInfoRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => ElementInfoRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        await player.InvokeViewPlugInAsync<IElementInfoViewPlugIn>(p => p.ShowElementInfoAsync()).ConfigureAwait(false);
    }
}
