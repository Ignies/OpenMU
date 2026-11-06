// <copyright file="MiningBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.World;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.Mining;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Shows the mining window to the Babylon client (<see cref="Network.Packets.Babylon.MiningResult"/>).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.MiningBabylonPlugIn_Name), Description = nameof(PlugInResources.MiningBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("8F2C5A17-3D6E-4B91-A0C8-7E4D1B9F6A23")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class MiningBabylonPlugIn : IMiningViewPlugIn
{
    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="MiningBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public MiningBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc/>
    public async ValueTask ShowMiningResultAsync(NonPlayerCharacter spot, GameLogic.Mining.MiningResult result, byte stage, int itemCount, int bonusCount)
    {
        await this._player.Connection.SendMiningResultAsync(spot.Id, (byte)result, stage, (ushort)Math.Min(itemCount, ushort.MaxValue), (ushort)Math.Min(bonusCount, ushort.MaxValue)).ConfigureAwait(false);
    }
}
