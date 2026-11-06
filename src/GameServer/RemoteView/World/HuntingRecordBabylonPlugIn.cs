// <copyright file="HuntingRecordBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.World;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.HuntingRecord;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.Network;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Shows the hunting record (<see cref="HuntingRecordList"/>) and the ranking (<see cref="RankingPage"/>) to the Babylon client.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.HuntingRecordBabylonPlugIn_Name), Description = nameof(PlugInResources.HuntingRecordBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("3C9E7B21-84D5-4A6F-B0E2-5F1A9D7C3B48")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class HuntingRecordBabylonPlugIn : IHuntingRecordViewPlugIn
{
    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="HuntingRecordBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public HuntingRecordBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc/>
    public async ValueTask ShowHuntingRecordAsync(short mapNumber, IReadOnlyList<HuntingDay> days)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        var shown = days.Take(byte.MaxValue).ToList();
        int Write()
        {
            var size = HuntingRecordListRef.GetRequiredSize(shown.Count);
            var packet = new HuntingRecordListRef(connection.Output.GetSpan(size)[..size])
            {
                MapNumber = (ushort)mapNumber,
                DayCount = (byte)shown.Count,
            };
            for (var i = 0; i < shown.Count; i++)
            {
                var day = shown[i];
                var row = packet[i];
                row.Day = (uint)new DateTimeOffset(DateTime.SpecifyKind(day.Day, DateTimeKind.Utc)).ToUnixTimeSeconds();
                row.Seconds = (uint)Math.Clamp(day.Seconds, 0, uint.MaxValue);
                row.Kills = (uint)Math.Clamp(day.Kills, 0, uint.MaxValue);
                row.Experience = (ulong)Math.Max(0, day.Experience);
                row.Damage = (ulong)Math.Max(0, day.Damage);
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask ShowRankingPageAsync(RankingKind kind, int page, int pageCount, int ownRank, IReadOnlyList<GameLogic.HuntingRecord.RankingEntry> entries)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        var shown = entries.Take(byte.MaxValue).ToList();
        int Write()
        {
            var size = RankingPageRef.GetRequiredSize(shown.Count);
            var packet = new RankingPageRef(connection.Output.GetSpan(size)[..size])
            {
                Kind = (byte)kind,
                Page = (byte)Math.Clamp(page, 0, byte.MaxValue),
                PageCount = (byte)Math.Clamp(pageCount, 0, byte.MaxValue),
                OwnRank = (ushort)Math.Clamp(ownRank, 0, ushort.MaxValue),
                EntryCount = (byte)shown.Count,
            };
            for (var i = 0; i < shown.Count; i++)
            {
                var entry = shown[i];
                var row = packet[i];
                row.Rank = (ushort)Math.Clamp(entry.Rank, 0, ushort.MaxValue);
                row.Name = entry.Name;
                row.ClassCode = ClassCodes.ToCode(entry.ClassNumber) ?? 0;
                row.Value = (uint)Math.Clamp(entry.Value, 0, uint.MaxValue);
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }
}
