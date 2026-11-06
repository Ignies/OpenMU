// <copyright file="MuunBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.Muuns;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Muuns;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.Muuns;
using MUnique.OpenMU.Network;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Shows the Muun system to the Babylon client (EE 50, 52, 55-58).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.MuunBabylonPlugIn_Name), Description = nameof(PlugInResources.MuunBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("8B4D2E61-7C3A-4F95-B0D8-2E6F1A9C3B74")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class MuunBabylonPlugIn : IMuunViewPlugIn
{
    private const byte NoItem = 0xFF;

    private const ushort NoTarget = 0xFFFF;

    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="MuunBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public MuunBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc />
    public async ValueTask ShowMuunInventoryAsync(IReadOnlyCollection<Item> items)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        var list = items.Where(i => i.ItemSlot < MuunSlots.Count).Take(MuunSlots.Count).ToList();

        int Write()
        {
            var size = MuunInventoryListRef.GetRequiredSize(list.Count);
            var packet = new MuunInventoryListRef(connection.Output.GetSpan(size)[..size]) { ItemCount = (byte)list.Count };
            for (var i = 0; i < list.Count; i++)
            {
                var entry = packet[i];
                entry.Slot = list[i].ItemSlot;
                this.WriteItem(entry.ItemData, list[i]);
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask MuunItemMovedAsync(MuunActionResult result, MuunStorageKind fromStorage, byte fromSlot, MuunStorageKind toStorage, byte toSlot, Item? item)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        int Write()
        {
            var size = MuunItemMoveResponseRef.Length;
            var packet = new MuunItemMoveResponseRef(connection.Output.GetSpan(size)[..size])
            {
                Result = (byte)result,
                FromStorage = (byte)fromStorage,
                FromSlot = fromSlot,
                ToStorage = (byte)toStorage,
                ToSlot = toSlot,
            };

            this.WriteItem(packet.ItemData, item);
            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask MuunActionResultAsync(MuunAction action, MuunActionResult result, MuunStorageKind storage, byte slot, Item? item)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        int Write()
        {
            var size = MuunActionResponseRef.Length;
            var packet = new MuunActionResponseRef(connection.Output.GetSpan(size)[..size])
            {
                Action = (byte)action,
                Result = (byte)result,
                Storage = (byte)storage,
                Slot = slot,
            };

            this.WriteItem(packet.ItemData, item);
            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask ShowMuunAppearanceAsync(Player owner, Item? mainMuun, bool isRiding)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        await connection.SendMuunAppearanceAsync(
            owner.GetId(this._player),
            mainMuun?.Definition?.Group ?? NoItem,
            (ushort)(mainMuun?.Definition?.Number ?? 0),
            mainMuun?.Level ?? 0,
            isRiding).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask ShowMuunSkillAsync(Player owner, IAttackable target, int level, uint damage)
    {
        if (this._player.Connection is { } connection)
        {
            await connection.SendMuunSkillUsedAsync(owner.GetId(this._player), target.GetId(this._player), (byte)Math.Clamp(level, 0, byte.MaxValue), damage).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async ValueTask ShowMuunOptionsAsync(IReadOnlyList<MuunActiveOption> options)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        int Write()
        {
            var size = MuunOptionsRef.GetRequiredSize(options.Count);
            var packet = new MuunOptionsRef(connection.Output.GetSpan(size)[..size]) { OptionCount = (byte)options.Count };
            for (var i = 0; i < options.Count; i++)
            {
                var option = options[i];
                var entry = packet[i];
                entry.Slot = option.Slot;
                entry.OptionType = (byte)option.Type;
                entry.Value = (ushort)Math.Clamp(option.Value, 0, ushort.MaxValue);
                entry.Flags = (byte)((option.IsActive ? 1 : 0) | (option.IsBoosted ? 2 : 0));
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask ShowMuunExchangeAsync(IReadOnlyList<MuunExchangeOffer> offers)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        var list = offers.Take(byte.MaxValue).ToList();

        int Write()
        {
            var size = MuunExchangeListRef.GetRequiredSize(list.Count);
            var packet = new MuunExchangeListRef(connection.Output.GetSpan(size)[..size]) { OfferCount = (byte)list.Count };
            for (var i = 0; i < list.Count; i++)
            {
                var offer = list[i];
                var entry = packet[i];
                entry.Index = (byte)i;
                entry.CostGroup = offer.CostGroup;
                entry.CostNumber = (ushort)offer.CostNumber;
                entry.CostCount = offer.CostCount;
                entry.ResultGroup = offer.ResultGroup;
                entry.ResultNumber = (ushort)offer.ResultNumber;
                entry.TargetNumber = offer.TargetNumber is { } target ? (ushort)target : NoTarget;
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    private void WriteItem(Span<byte> target, Item? item)
    {
        if (item is null)
        {
            target.Fill(NoItem);
            return;
        }

        target.Clear();
        this._player.ItemSerializer.SerializeItem(target, item);
    }
}
