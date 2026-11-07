// <copyright file="ArcaWarBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.Events;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.ArcaWar;
using MUnique.OpenMU.Network;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The implementation of the <see cref="IArcaWarViewPlugIn"/> for the Babylon client: <c>EE BB</c>, <c>EE BC</c> and
/// <c>EE BD</c>.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.ArcaWarBabylonPlugIn_Name), Description = nameof(PlugInResources.ArcaWarBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("A3C95E27-6B14-4F80-9D3A-E8257B1C40D6")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class ArcaWarBabylonPlugIn : IArcaWarViewPlugIn
{
    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArcaWarBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public ArcaWarBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc/>
    public async ValueTask ShowRequestResultAsync(ArcaWarRequestKind kind, ArcaWarRequestResult result)
    {
        await this._player.Connection.SendArcaWarRequestResponseAsync((byte)kind, (byte)result).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask ShowStateAsync(ArcaWarStage stage, uint secondsLeft, byte guilds, bool registered, uint points)
    {
        await this._player.Connection.SendArcaWarStageStateAsync((byte)stage, guilds, registered, secondsLeft, points).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask ShowObelisksAsync(IReadOnlyList<ArcaObeliskView> obelisks)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        int Write()
        {
            var size = ArcaWarObelisksRef.GetRequiredSize(obelisks.Count);
            var span = connection.Output.GetSpan(size)[..size];
            var packet = new ArcaWarObelisksRef(span) { ObeliskCount = (byte)obelisks.Count };
            for (var i = 0; i < obelisks.Count; i++)
            {
                var obelisk = obelisks[i];
                var state = packet[i];
                state.Index = obelisk.Index;
                state.Element = (byte)obelisk.Element;
                state.MapNumber = (ushort)obelisk.MapNumber;
                state.X = obelisk.X;
                state.Y = obelisk.Y;
                state.HealthPercent = obelisk.HealthPercent;
                state.AuraMask = obelisk.AuraMask;
                state.IsOpen = obelisk.IsOpen;
                state.OwnerGuildId = obelisk.OwnerGuildId;
                state.OwnerGuildName = obelisk.OwnerGuildName;
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }
}
