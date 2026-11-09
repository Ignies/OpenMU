// <copyright file="BlessingOfEuniceConsumeHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Applies the Blessing of Eunice (19, 17): its effect, more experience from monsters, is only for the fifth evolution.
/// </summary>
[Guid("8C0F5E1A-7D42-4B39-A6E3-2F91C4D7B850")]
[PlugIn]
[Display(Name = nameof(PlugInResources.BlessingOfEuniceConsumeHandlerPlugIn_Name), Description = nameof(PlugInResources.BlessingOfEuniceConsumeHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
public class BlessingOfEuniceConsumeHandlerPlugIn : ApplyMagicEffectConsumeHandlerPlugIn
{
    /// <inheritdoc />
    public override ItemIdentifier Key => new(17, 19);

    /// <inheritdoc />
    public override ValueTask<bool> ConsumeItemAsync(Player player, Item item, Item? targetItem, FruitUsage fruitUsage)
    {
        return player.SelectedCharacter?.CharacterClass?.GetEvolution() == ClassEvolution.Fifth
            ? base.ConsumeItemAsync(player, item, targetItem, fruitUsage)
            : ValueTask.FromResult(false);
    }
}
