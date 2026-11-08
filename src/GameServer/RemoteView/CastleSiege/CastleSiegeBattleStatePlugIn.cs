// <copyright file="CastleSiegeBattleStatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.CastleSiege;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.Views.CastleSiege;
using MUnique.OpenMU.Network.Packets.ServerToClient;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The default implementation of the <see cref="ICastleSiegeBattleStatePlugIn"/> which sends the battle start or end
/// (0xB2 0x17) and the remaining battle time (0xB2 0x1E) to the game client.
/// </summary>
[PlugIn]
[Display(Name = PlugInName, Description = PlugInDescription)]
[Guid("6C1E4B57-0D93-4F2A-8B71-3A9E5D2C8F04")]
public class CastleSiegeBattleStatePlugIn : ICastleSiegeBattleStatePlugIn
{
    private const string PlugInName = "Castle Siege Battle State";

    private const string PlugInDescription = "View plugin which tells the client that the castle siege battle started or ended, and its remaining time.";

    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="CastleSiegeBattleStatePlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public CastleSiegeBattleStatePlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc />
    public ValueTask ShowBattleStateAsync(bool isStarted)
        => this._player.Connection?.SendCastleSiegeBattleStartEndAsync(isStarted) ?? default;

    /// <inheritdoc />
    public ValueTask ShowRemainingTimeAsync(byte hours, byte minutes)
        => this._player.Connection?.SendCastleSiegeRemainingTimeAsync(hours, minutes) ?? default;
}
