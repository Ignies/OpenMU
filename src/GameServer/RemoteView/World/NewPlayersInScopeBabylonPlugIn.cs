// <copyright file="NewPlayersInScopeBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.World;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.Network;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Shows new players in view to the Babylon client: the Season 6 packet, followed by the class code and the wide gear
/// of the player (<see cref="PlayerAppearanceWide"/>), which the 5 bit class and the Season 6 appearance can't carry.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.NewPlayersInScopeBabylonPlugIn_Name), Description = nameof(PlugInResources.NewPlayersInScopeBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("5AB47FC6-14FB-47FA-9C25-4C1928CE0182")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class NewPlayersInScopeBabylonPlugIn : NewPlayersInScopePlugIn
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NewPlayersInScopeBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public NewPlayersInScopeBabylonPlugIn(RemotePlayer player)
        : base(player)
    {
    }

    /// <inheritdoc/>
    protected override async ValueTask SendCharacterAsync(Player newPlayer, bool isSpawned)
    {
        await base.SendCharacterAsync(newPlayer, isSpawned).ConfigureAwait(false);
        await this.SendAppearanceWideAsync(newPlayer).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async ValueTask SendTransformedCharacterAsync(Player newPlayer, bool isSpawned)
    {
        await base.SendTransformedCharacterAsync(newPlayer, isSpawned).ConfigureAwait(false);
        await this.SendAppearanceWideAsync(newPlayer).ConfigureAwait(false);
    }

    private async ValueTask SendAppearanceWideAsync(Player newPlayer)
    {
        if (this.Player.Connection is not { } connection
            || newPlayer.SelectedCharacter?.CharacterClass?.GetClassCode() is not { } classCode)
        {
            return;
        }

        var items = newPlayer.Inventory?.EquippedItems.ToList() ?? [];

        int Write()
        {
            var itemSerializer = this.Player.ItemSerializer;
            var size = PlayerAppearanceWideRef.GetRequiredSize(items.Count);
            var span = connection.Output.GetSpan(size)[..size];
            var packet = new PlayerAppearanceWideRef(span)
            {
                PlayerId = newPlayer.GetId(this.Player),
                ClassCode = classCode,
                ItemCount = (byte)items.Count,
            };

            for (var i = 0; i < items.Count; i++)
            {
                var entry = packet[i];
                entry.Slot = items[i].ItemSlot;
                entry.ItemData.Clear();
                itemSerializer.SerializeItem(entry.ItemData, items[i]);
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }
}
