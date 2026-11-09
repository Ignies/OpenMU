// <copyright file="CastleSiegeCrownSwitchStatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.CastleSiege;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Views.CastleSiege;
using MUnique.OpenMU.Network.Packets.ServerToClient;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The default implementation of the <see cref="ICastleSiegeCrownSwitchStatePlugIn"/>
/// which answers a Crown switch push to the game client (0xB2 0x14).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.CastleSiegeCrownSwitchStatePlugIn_Name), Description = nameof(PlugInResources.CastleSiegeCrownSwitchStatePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("6B0E7E3C-2F43-4C4B-9A43-8D7C1E5B2A61")]
public class CastleSiegeCrownSwitchStatePlugIn : ICastleSiegeCrownSwitchStatePlugIn
{
    private const ushort NoPlayer = 0xFFFF;

    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="CastleSiegeCrownSwitchStatePlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public CastleSiegeCrownSwitchStatePlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc />
    public ValueTask ShowCrownSwitchStateAsync(ushort switchId, Player? occupant, CastleSiegeCrownSwitchAccess access)
        => this._player.Connection?.SendCastleSiegeCrownSwitchStateAsync(
            switchId,
            occupant?.Id ?? NoPlayer,
            (CastleSiegeCrownSwitchStateType)access) ?? default;
}
