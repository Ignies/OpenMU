// <copyright file="Season21EventBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.Events;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.MiniGames;
using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.GameLogic.PlayerActions.MiniGames;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;
using MiniGameType = MUnique.OpenMU.DataModel.Configuration.MiniGameType;

/// <summary>
/// The implementation of the <see cref="ISeason21EventViewPlugIn"/> for the Babylon client: the event packets
/// <c>EE B0</c>, <c>EE B2</c>, <c>EE B3</c>, <c>EE B4</c> and the league of the labyrinth, <c>EE B5</c>.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.Season21EventBabylonPlugIn_Name), Description = nameof(PlugInResources.Season21EventBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("1F45F6D9-7B13-42E1-89E0-18AE133352F5")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class Season21EventBabylonPlugIn : ISeason21EventViewPlugIn
{
    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21EventBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public Season21EventBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc/>
    public async ValueTask ShowStateAsync(MiniGameType type, byte gameLevel, MiniGameState state, uint secondsLeft)
    {
        await this._player.Connection.SendEventStateAsync((ushort)type, gameLevel, ToWire(state), secondsLeft).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask ShowScoreAsync(MiniGameType type, byte round, int score, int rank, int playerCount, uint secondsLeft)
    {
        await this._player.Connection.SendEventScoreAsync((ushort)type, round, (uint)Math.Max(score, 0), (ushort)Math.Clamp(rank, 0, ushort.MaxValue), (ushort)Math.Clamp(playerCount, 0, ushort.MaxValue), secondsLeft).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask ShowResultAsync(MiniGameType type, bool success, int score, int rank, long experience)
    {
        await this._player.Connection.SendEventResultAsync((ushort)type, success, (uint)Math.Max(score, 0), (ushort)Math.Clamp(rank, 0, ushort.MaxValue), (uint)Math.Clamp(experience, 0, uint.MaxValue)).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask ShowEnterResultAsync(MiniGameType type, EnterResult result)
    {
        await this._player.Connection.SendEventEnterResultAsync((ushort)type, (byte)result).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask ShowLabyrinthLeagueAsync(LabyrinthStanding standing, LabyrinthDayKind day)
    {
        await this._player.Connection.SendLabyrinthLeagueAsync(
            (byte)standing.Tier,
            (byte)Math.Clamp(standing.Rank, 0, byte.MaxValue),
            (byte)Math.Clamp(standing.GroupSize, 0, byte.MaxValue),
            (byte)day,
            (uint)Math.Max(standing.Points, 0),
            (ushort)Math.Clamp(standing.FloorsToday, 0, ushort.MaxValue)).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the state byte of the packet: 0 open, 1 closed, 2 playing, 3 ended (a disposed game is ended).
    /// </summary>
    private static byte ToWire(MiniGameState state) => state switch
    {
        MiniGameState.Open => 0,
        MiniGameState.Closed => 1,
        MiniGameState.Playing => 2,
        _ => 3,
    };
}
