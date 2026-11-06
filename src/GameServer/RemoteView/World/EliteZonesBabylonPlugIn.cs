// <copyright file="EliteZonesBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.World;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.EliteZones;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.Network;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Shows the elite zones of the map to the Babylon client (<see cref="EliteZones"/>).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.EliteZonesBabylonPlugIn_Name), Description = nameof(PlugInResources.EliteZonesBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("2F6D9B41-8C3A-4E75-A0B2-5E1C7D4F9A36")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class EliteZonesBabylonPlugIn : IEliteZonesViewPlugIn
{
    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="EliteZonesBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public EliteZonesBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc/>
    public async ValueTask ShowEliteZonesAsync(short mapNumber, IReadOnlyList<(EliteZoneDefinition Zone, bool CanEnter)> zones)
    {
        if (this._player.Connection is not { } connection || zones.Count == 0)
        {
            return;
        }

        var count = Math.Min(zones.Count, byte.MaxValue);

        int Write()
        {
            var size = EliteZonesRef.GetRequiredSize(count);
            var packet = new EliteZonesRef(connection.Output.GetSpan(size)[..size])
            {
                MapNumber = (ushort)mapNumber,
                ZoneCount = (byte)count,
            };

            for (var i = 0; i < count; i++)
            {
                var (zone, canEnter) = zones[i];
                var entry = packet[i];
                entry.Element = (byte)zone.Element;
                entry.EntranceX = zone.EntranceX;
                entry.EntranceY = zone.EntranceY;
                entry.X1 = zone.X1;
                entry.Y1 = zone.Y1;
                entry.X2 = zone.X2;
                entry.Y2 = zone.Y2;
                entry.CanEnter = canEnter ? (byte)1 : (byte)0;
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }
}
