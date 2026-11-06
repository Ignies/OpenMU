// <copyright file="FieldBossStateBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.World;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.FieldBosses;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Shows the state of a field boss to the Babylon client (<see cref="Network.Packets.Babylon.FieldBossState"/>).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.FieldBossStateBabylonPlugIn_Name), Description = nameof(PlugInResources.FieldBossStateBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("6A3E1C85-D74B-4F29-9E60-1B8D2C5A7F03")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class FieldBossStateBabylonPlugIn : IFieldBossStateViewPlugIn
{
    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldBossStateBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public FieldBossStateBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc/>
    public async ValueTask ShowFieldBossStateAsync(short mapNumber, short bossNumber, FieldBossStatus status, byte healthPercent, uint secondsLeft)
    {
        await this._player.Connection.SendFieldBossStateAsync((ushort)mapNumber, (ushort)bossNumber, (byte)status, healthPercent, secondsLeft).ConfigureAwait(false);
    }
}
