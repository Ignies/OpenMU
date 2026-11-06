// <copyright file="SeasonPassBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.World;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.SeasonPass;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.Network;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Shows the season pass (<see cref="SeasonPassState"/>, <see cref="Network.Packets.Babylon.SeasonPassClaimResult"/>) to the Babylon client.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.SeasonPassBabylonPlugIn_Name), Description = nameof(PlugInResources.SeasonPassBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("D84F1B27-6C3A-4E95-8B02-9A7E3F5D1C68")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class SeasonPassBabylonPlugIn : ISeasonPassViewPlugIn
{
    /// <summary>
    /// The bit of the index of a special mission.
    /// </summary>
    private const byte SpecialMissionBit = 0x80;

    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeasonPassBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public SeasonPassBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc/>
    public async ValueTask ShowSeasonPassStateAsync(SeasonPassView state)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        var missions = state.Missions.Take(byte.MaxValue).ToList();
        int Write()
        {
            var size = SeasonPassStateRef.GetRequiredSize(missions.Count);
            var packet = new SeasonPassStateRef(connection.Output.GetSpan(size)[..size])
            {
                Season = state.Season,
                EndsAt = state.EndsAt is { } end ? (uint)Math.Clamp(new DateTimeOffset(DateTime.SpecifyKind(end, DateTimeKind.Utc)).ToUnixTimeSeconds(), 0, uint.MaxValue) : 0,
                Level = (byte)Math.Clamp(state.Level, 0, byte.MaxValue),
                Experience = (uint)Math.Max(0, state.Experience),
                Passes = state.Passes,
                ClaimedFree = state.ClaimedFree,
                ClaimedGold = state.ClaimedGold,
                ClaimedPremium = state.ClaimedPremium,
                MissionCount = (byte)missions.Count,
            };
            for (var i = 0; i < missions.Count; i++)
            {
                var mission = missions[i];
                var row = packet[i];
                row.Index = mission.Index >= 128 ? (byte)(SpecialMissionBit | (mission.Index - 128)) : (byte)mission.Index;
                row.Completions = (byte)Math.Clamp(mission.Completions, 0, byte.MaxValue);
                row.Progress = (uint)Math.Clamp(mission.Progress, 0, uint.MaxValue);
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask ShowSeasonPassClaimResultAsync(SeasonPassKind pass, byte level, GameLogic.SeasonPass.SeasonPassClaimResult result)
    {
        await this._player.Connection.SendSeasonPassClaimResultAsync((byte)pass, level, (byte)result).ConfigureAwait(false);
    }
}
