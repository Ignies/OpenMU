// <copyright file="ItemCustomizationBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.ItemCustomization;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.ItemCustomization;
using MUnique.OpenMU.GameLogic.Views.ItemCustomization;
using MUnique.OpenMU.Network;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;
using ReportResult = MUnique.OpenMU.GameLogic.ItemCustomization.ItemCustomizationReportResult;

/// <summary>
/// Sends the item customisations to the Babylon client (EE 34, EE 36).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.ItemCustomizationBabylonPlugIn_Name), Description = nameof(PlugInResources.ItemCustomizationBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("01D56570-167C-411C-BF18-EAD28048A0DD")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class ItemCustomizationBabylonPlugIn : IItemCustomizationViewPlugIn
{
    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemCustomizationBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public ItemCustomizationBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc />
    public async ValueTask SendCustomizationsAsync(IReadOnlyList<ItemCustomizationEntryData> entries)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        var count = Math.Min(entries.Count, (ushort.MaxValue - 7) / ItemCustomizationEntryRef.Length);
        int Write()
        {
            var size = ItemCustomizationsRef.GetRequiredSize(count);
            var packet = new ItemCustomizationsRef(connection.Output.GetSpan(size)[..size]) { Count = (ushort)count };
            for (var i = 0; i < count; i++)
            {
                Fill(packet[i], entries[i]);
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask SendDetailAsync(ItemCustomizationEntryData entry)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        int Write()
        {
            var size = ItemCustomizationDetailRef.Length;
            var packet = new ItemCustomizationDetailRef(connection.Output.GetSpan(size)[..size])
            {
                Storage = (byte)entry.Storage,
                ObjectId = entry.ObjectId,
                Slot = entry.Slot,
                Version = (uint)entry.Customization.Version,
            };
            ItemCustomizationText.WriteLore(entry.Customization.Lore, packet.Lore);
            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask SendReportResultAsync(ReportResult result)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        int Write()
        {
            var size = ItemCustomizationReportResultRef.Length;
            _ = new ItemCustomizationReportResultRef(connection.Output.GetSpan(size)[..size]) { Result = (byte)result };
            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask SendChangedAsync(IReadOnlyList<ItemCustomizationEntryData> entries)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        var count = Math.Min(entries.Count, (ushort.MaxValue - 7) / ItemCustomizationEntryRef.Length);
        int Write()
        {
            var size = ItemCustomizationsChangedRef.GetRequiredSize(count);
            var packet = new ItemCustomizationsChangedRef(connection.Output.GetSpan(size)[..size]) { Count = (ushort)count };
            for (var i = 0; i < count; i++)
            {
                Fill(packet[i], entries[i]);
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    private static void Fill(ItemCustomizationEntryRef entry, ItemCustomizationEntryData data)
    {
        var c = data.Customization;
        entry.Storage = (byte)data.Storage;
        entry.ObjectId = data.ObjectId;
        entry.Slot = data.Slot;
        entry.Version = (uint)c.Version;
        entry.Rarity = c.Rarity;
        var hasLook = ItemLookBytes.TryWrite(c.Look, entry.Look);
        ItemCustomizationText.WriteName(c.Name, entry.Name);
        entry.Flags = ItemCustomizationText.Flags(hasLook, c.Name, c.Lore);
    }
}
