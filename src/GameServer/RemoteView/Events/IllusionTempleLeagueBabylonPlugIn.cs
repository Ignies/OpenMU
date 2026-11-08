// <copyright file="IllusionTempleLeagueBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.Events;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.MiniGames;
using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.Network;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The implementation of the <see cref="IIllusionTempleLeagueViewPlugIn"/> for the Babylon client: <c>EE BE</c> and
/// <c>EE BF</c>.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.IllusionTempleLeagueBabylonPlugIn_Name), Description = nameof(PlugInResources.IllusionTempleLeagueBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("5D8E3A16-B427-4C91-9F0A-6E13C7B28D45")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class IllusionTempleLeagueBabylonPlugIn : IIllusionTempleLeagueViewPlugIn
{
    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="IllusionTempleLeagueBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public IllusionTempleLeagueBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc/>
    public async ValueTask ShowLeagueStateAsync(IllusionTempleLeagueStanding standing)
    {
        await this._player.Connection.SendIllusionTempleLeagueStateAsync(
            (byte)standing.Stage,
            ToByte(standing.Rank),
            ToByte(standing.GuildEntries),
            ToByte(standing.MaximumGuildEntries),
            (ushort)Math.Clamp(standing.Stones, 0, ushort.MaxValue),
            (ushort)Math.Clamp(standing.Kills, 0, ushort.MaxValue),
            (uint)Math.Max(0, standing.LevelSum),
            (byte)standing.Qualification,
            ToByte(standing.PlayerEntries),
            ToByte(standing.MaximumPlayerEntries)).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask ShowStonesAsync(IllusionTempleTeam ownTeam, IllusionTempleHollowState hollow, IReadOnlyList<IllusionTempleStoneView> stones)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        int Write()
        {
            var size = IllusionTempleStonesRef.GetRequiredSize(stones.Count);
            var span = connection.Output.GetSpan(size)[..size];
            var packet = new IllusionTempleStonesRef(span)
            {
                StoneCount = (byte)stones.Count,
                OwnTeam = ToTeamCode(ownTeam),
                HollowState = (byte)hollow,
            };
            for (var i = 0; i < stones.Count; i++)
            {
                var stone = stones[i];
                var state = packet[i];
                state.Index = stone.Index;
                state.Holder = stone.Holder is { } holder ? ToTeamCode(holder) : (byte)0;
                state.X = stone.X;
                state.Y = stone.Y;
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    private static byte ToTeamCode(IllusionTempleTeam team) => team == IllusionTempleTeam.AlliedForces ? (byte)1 : (byte)2;

    private static byte ToByte(int value) => (byte)Math.Clamp(value, 0, byte.MaxValue);
}
