// <copyright file="PentagramBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.Inventory;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic.Elements;
using MUnique.OpenMU.GameLogic.PlayerActions.Items;
using MUnique.OpenMU.GameLogic.Views.Inventory;
using MUnique.OpenMU.Network;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Shows the errtels of a pentagram and the results of errtel socket requests to the Babylon client
/// (<see cref="PentagramErrtels"/>, <see cref="PentagramResult"/>).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.PentagramBabylonPlugIn_Name), Description = nameof(PlugInResources.PentagramBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("FBBC05E4-F578-436B-B661-C4E843B508F5")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class PentagramBabylonPlugIn : IPentagramViewPlugIn
{
    /// <summary>
    /// The option number of a rank which an errtel doesn't have.
    /// </summary>
    private const byte NoRankOption = 0xFF;

    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="PentagramBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public PentagramBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc/>
    public async ValueTask ShowErrtelSocketResultAsync(ErrtelSocketMode mode, ErrtelSocketResult result, byte pentagramSlot, byte socket)
    {
        await this._player.Connection.SendPentagramResultAsync((byte)mode, (byte)result, pentagramSlot, socket).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask ShowPentagramErrtelsAsync(Item pentagram)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        var errtels = Enumerable.Range(0, Pentagrams.Sockets)
            .Select(socket => (Socket: socket, Options: pentagram.GetRankOptions(socket).Take(Pentagrams.MaximumRank).ToList()))
            .Where(e => e.Options.Count > 0)
            .Select(e => (e.Socket, e.Options, Definition: this._player.GameContext.Configuration.FindErrtelDefinition(e.Options[0].ItemOption)))
            .ToList();

        int Write()
        {
            var size = PentagramErrtelsRef.GetRequiredSize(errtels.Count);
            var packet = new PentagramErrtelsRef(connection.Output.GetSpan(size)[..size])
            {
                PentagramSlot = pentagram.ItemSlot,
                ErrtelCount = (byte)errtels.Count,
            };

            for (var i = 0; i < errtels.Count; i++)
            {
                var (socket, options, definition) = errtels[i];
                var entry = packet[i];
                entry.SocketIndex = (byte)socket;
                entry.ErrtelNumber = (ushort)(definition?.Number ?? 0);
                entry.RankCount = (byte)options.Count;
                WriteRankOptions(entry.RankOptions, options);
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <summary>
    /// Writes the rank options of an errtel: pairs of option number and level, by rank.
    /// </summary>
    /// <param name="target">The target, two bytes per rank.</param>
    /// <param name="rankOptions">The rank options, ordered by rank.</param>
    private static void WriteRankOptions(Span<byte> target, IReadOnlyList<ItemOptionLink> rankOptions)
    {
        for (var rank = 0; rank * 2 < target.Length; rank++)
        {
            var hasRank = rank < rankOptions.Count;
            target[rank * 2] = hasRank ? (byte)rankOptions[rank].ItemOption!.Number : NoRankOption;
            target[(rank * 2) + 1] = hasRank ? (byte)rankOptions[rank].Level : (byte)0;
        }
    }
}
