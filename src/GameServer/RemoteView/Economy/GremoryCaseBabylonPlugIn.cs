// <copyright file="GremoryCaseBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.Economy;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic.GremoryCase;
using MUnique.OpenMU.GameLogic.ItemExpiry;
using MUnique.OpenMU.GameLogic.Views.Economy;
using MUnique.OpenMU.Network;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Shows the gremory cases to the Babylon client (C2 EE 60, C1 EE 61, 63 and 64).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.GremoryCaseBabylonPlugIn_Name), Description = nameof(PlugInResources.GremoryCaseBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("F2C84E19-7A3D-4B60-9E25-1D6B8C3F4A70")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class GremoryCaseBabylonPlugIn : IGremoryCaseViewPlugIn
{
    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="GremoryCaseBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public GremoryCaseBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc />
    public async ValueTask ShowGremoryCaseAsync(GremoryCaseKind kind, int capacity, IReadOnlyCollection<Item> items)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        var list = items.Take(byte.MaxValue).ToList();

        int Write()
        {
            var size = GremoryCaseListRef.GetRequiredSize(list.Count);
            var packet = new GremoryCaseListRef(connection.Output.GetSpan(size)[..size])
            {
                Kind = (byte)kind,
                Capacity = (byte)Math.Clamp(capacity, 0, byte.MaxValue),
                ItemCount = (byte)list.Count,
            };

            for (var i = 0; i < list.Count; i++)
            {
                var entry = packet[i];
                entry.Slot = list[i].ItemSlot;
                entry.ExpiresAt = list[i].GetExpiryUnixSeconds();
                entry.ItemData.Clear();
                this._player.ItemSerializer.SerializeItem(entry.ItemData, list[i]);
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask GremoryCaseItemAddedAsync(GremoryCaseKind kind, Item item)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        int Write()
        {
            var size = GremoryCaseItemAddedRef.Length;
            var packet = new GremoryCaseItemAddedRef(connection.Output.GetSpan(size)[..size])
            {
                Kind = (byte)kind,
                Slot = item.ItemSlot,
                ExpiresAt = item.GetExpiryUnixSeconds(),
            };

            packet.ItemData.Clear();
            this._player.ItemSerializer.SerializeItem(packet.ItemData, item);
            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask GremoryCaseClaimResultAsync(GremoryCaseKind kind, byte slot, GremoryCaseClaimResult result)
    {
        if (this._player.Connection is { } connection)
        {
            await connection.SendGremoryCaseClaimResponseAsync((byte)kind, slot, (byte)result).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async ValueTask GremoryCaseItemExpiredAsync(GremoryCaseKind kind, byte slot)
    {
        if (this._player.Connection is { } connection)
        {
            await connection.SendGremoryCaseItemExpiredAsync((byte)kind, slot).ConfigureAwait(false);
        }
    }
}
