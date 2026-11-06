// <copyright file="SoulConverterBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.World;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.SoulConverter;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.Network;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Shows the monster soul converter (<see cref="SoulConverterState"/>, <see cref="SoulConverterResult"/>) to the Babylon client.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.SoulConverterBabylonPlugIn_Name), Description = nameof(PlugInResources.SoulConverterBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("9D4A6E13-2B85-4C7F-A0E6-3F1B8D5C2A79")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class SoulConverterBabylonPlugIn : ISoulConverterViewPlugIn
{
    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="SoulConverterBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public SoulConverterBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc/>
    public async ValueTask ShowSoulConverterStateAsync(byte tier, int activationsLeft, DateTime? expiresAt, IReadOnlyList<SoulMissionProgress> missions)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        var shown = missions.Take(byte.MaxValue).ToList();
        int Write()
        {
            var size = SoulConverterStateRef.GetRequiredSize(shown.Count);
            var packet = new SoulConverterStateRef(connection.Output.GetSpan(size)[..size])
            {
                Tier = tier,
                PurchasesLeft = (byte)Math.Clamp(activationsLeft, 0, byte.MaxValue),
                ExpiresAt = expiresAt is { } at ? (uint)Math.Clamp(new DateTimeOffset(DateTime.SpecifyKind(at, DateTimeKind.Utc)).ToUnixTimeSeconds(), 0, uint.MaxValue) : 0,
                MissionCount = (byte)shown.Count,
            };
            for (var i = 0; i < shown.Count; i++)
            {
                var mission = shown[i];
                var row = packet[i];
                row.MapNumber = (ushort)mission.MapNumber;
                row.MonsterNumber = (ushort)mission.MonsterNumber;
                row.Souls = (ushort)Math.Clamp(mission.Souls, 0, ushort.MaxValue);
                row.RequiredSouls = (ushort)Math.Clamp(mission.RequiredSouls, 0, ushort.MaxValue);
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask ShowSoulConverterResultAsync(SoulConverterClaimResult result, SoulRewardKind kind)
    {
        await this._player.Connection.SendSoulConverterResultAsync((byte)result, (byte)kind).ConfigureAwait(false);
    }
}
