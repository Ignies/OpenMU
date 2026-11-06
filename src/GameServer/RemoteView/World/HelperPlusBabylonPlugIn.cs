// <copyright file="HelperPlusBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.World;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.HelperPlus;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Shows the helper plus rules (<see cref="HelperPlusRules"/>) and move results (<see cref="Network.Packets.Babylon.HelperPlusMoveResult"/>)
/// to the Babylon client.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.HelperPlusBabylonPlugIn_Name), Description = nameof(PlugInResources.HelperPlusBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("7B3E9C25-1A64-4F08-9D2B-5E8C1F6A3D97")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class HelperPlusBabylonPlugIn : IHelperPlusViewPlugIn
{
    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="HelperPlusBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public HelperPlusBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc/>
    public async ValueTask ShowHelperPlusRulesAsync(HelperPlusFlags flags, HelperPlusConfiguration configuration)
    {
        await this._player.Connection.SendHelperPlusRulesAsync(
            (byte)flags,
            (ushort)Math.Clamp(configuration.PlusMinimumTotalLevel, 0, ushort.MaxValue),
            configuration.HuntRange,
            configuration.PickupRange,
            configuration.PlusHuntRange,
            configuration.PlusPickupRange,
            (ushort)Math.Max((short)0, configuration.PlusCostPercent)).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask ShowHelperPlusMoveResultAsync(short zoneIndex, GameLogic.HelperPlus.HelperPlusMoveResult result)
    {
        await this._player.Connection.SendHelperPlusMoveResultAsync((ushort)zoneIndex, (byte)result).ConfigureAwait(false);
    }
}
