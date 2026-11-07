// <copyright file="AbilityCoreBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.AbilityCore;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic.AbilityCore;
using MUnique.OpenMU.GameLogic.Views.AbilityCore;
using MUnique.OpenMU.Network;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Shows the Ability Enhancement Core to the Babylon client (EE 43, 45).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.AbilityCoreBabylonPlugIn_Name), Description = nameof(PlugInResources.AbilityCoreBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("5E8B1D47-3C92-4A6F-B0E5-7D2A9C4F1B83")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class AbilityCoreBabylonPlugIn : IAbilityCoreViewPlugIn
{
    private const byte NoItem = 0xFF;

    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="AbilityCoreBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public AbilityCoreBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc />
    public async ValueTask ShowAbilityCoreAsync(uint openSlots, uint activeSlots, IReadOnlyCollection<Item> cards)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        var list = cards.Where(c => c.ItemSlot < AbilityCoreSlots.Count).Take(AbilityCoreSlots.Count).ToList();

        int Write()
        {
            var size = AbilityCoreStateRef.GetRequiredSize(list.Count);
            var packet = new AbilityCoreStateRef(connection.Output.GetSpan(size)[..size])
            {
                OpenSlots = openSlots,
                ActiveSlots = activeSlots,
                CardCount = (byte)list.Count,
            };
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
    public async ValueTask AbilityCoreResultAsync(AbilityCoreAction action, AbilityCoreResult result, byte coreSlot, byte inventorySlot, Item? item)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        int Write()
        {
            var size = AbilityCoreResponseRef.Length;
            var packet = new AbilityCoreResponseRef(connection.Output.GetSpan(size)[..size])
            {
                Result = (byte)result,
                Action = (byte)action,
                CoreSlot = coreSlot,
                InventorySlot = inventorySlot,
            };

            this.WriteItem(packet.ItemData, item);
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
