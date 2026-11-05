// <copyright file="AppearanceChangedBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.World;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.Network;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Shows a changed piece of gear of a player in view to the Babylon client (<see cref="AppearanceChangedWide"/>).
/// </summary>
/// <remarks>
/// It replaces the Season 6 packet for this client: that one writes the item slot and glow level over byte 1 of the item
/// data, which is a part of the item number in the W32 format.
/// </remarks>
[PlugIn]
[Display(Name = nameof(PlugInResources.AppearanceChangedBabylonPlugIn_Name), Description = nameof(PlugInResources.AppearanceChangedBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("E6F84A57-0F5F-4AC8-9C57-FB8BF0053CF4")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class AppearanceChangedBabylonPlugIn : IAppearanceChangedPlugIn
{
    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppearanceChangedBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public AppearanceChangedBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc/>
    public async ValueTask AppearanceChangedAsync(Player changedPlayer, Item item, bool isEquipped)
    {
        if (this._player.Connection is not { } connection || changedPlayer.Inventory is not { } inventory)
        {
            return;
        }

        var classCode = changedPlayer.SelectedCharacter?.CharacterClass?.GetClassCode() ?? 0;

        int Write()
        {
            var size = AppearanceChangedWideRef.Length;
            var span = connection.Output.GetSpan(size)[..size];
            var packet = new AppearanceChangedWideRef(span)
            {
                PlayerId = changedPlayer.GetId(this._player),
                Slot = item.ItemSlot,
                ClassCode = classCode,
            };

            if (inventory.EquippedItems.Contains(item))
            {
                packet.ItemData.Clear();
                this._player.ItemSerializer.SerializeItem(packet.ItemData, item);
            }
            else
            {
                packet.ItemData.Fill(0xFF);
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }
}
