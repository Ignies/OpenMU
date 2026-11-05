// <copyright file="LuckyItemExtensions.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic;

using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.PlayerActions.Craftings;

/// <summary>
/// Extensions to determine the lucky items.
/// </summary>
public static class LuckyItemExtensions
{
    /// <summary>
    /// Determines whether the item definition is a lucky item, which can be refined, wears down while it's worn and can
    /// be repaired and elevated by the jewels of extension and elevation.
    /// </summary>
    /// <remarks>
    /// The lucky items are the items for which a lucky item ticket is exchanged: the result items of the craftings with
    /// the <see cref="LuckyItemTicketExchangeCrafting"/> handler of the NPCs with the lucky item window. So they are
    /// defined by the configuration, not by item numbers.
    /// </remarks>
    /// <param name="definition">The item definition.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    /// <returns><c>true</c>, if the item definition is a lucky item.</returns>
    public static bool IsLuckyItem(this ItemDefinition definition, GameConfiguration gameConfiguration)
    {
        var handlerName = typeof(LuckyItemTicketExchangeCrafting).FullName;
        return gameConfiguration.Monsters
            .Where(monster => monster.NpcWindow == NpcWindow.CombineLuckyItem)
            .SelectMany(monster => monster.ItemCraftings)
            .Where(crafting => crafting.ItemCraftingHandlerClassName == handlerName)
            .SelectMany(crafting => crafting.SimpleCraftingSettings?.ResultItems ?? [])
            .Any(result => result.ItemDefinition == definition);
    }
}
