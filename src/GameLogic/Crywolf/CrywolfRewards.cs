// <copyright file="CrywolfRewards.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Crywolf;

using MUnique.OpenMU.GameLogic.Rewards;

/// <summary>
/// Gives the reward of a contractor or hero of the crywolf event to the player, like in Season 21.
/// </summary>
public static class CrywolfRewards
{
    /// <summary>
    /// Delivers the reward item of the definition into the gremory case of the player, or its inventory when the case
    /// isn't available.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="definition">The definition of the event.</param>
    /// <returns><c>true</c>, if the item was delivered.</returns>
    public static ValueTask<bool> GiveAsync(Player player, CrywolfEventDefinition definition)
        => RewardDelivery.DeliverAsync(player, new RewardItem { Group = definition.RewardItemGroup, Number = definition.RewardItemNumber, Count = 1 });
}
