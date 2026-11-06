// <copyright file="ImperialGuardianItemReward.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.ImperialGuardian;

using MUnique.OpenMU.GameLogic.Rewards;

/// <summary>
/// An item which a player may get when the event has been completed (Season 18 rework): one reward of the list is chosen
/// by its chance, and its count of items is given.
/// </summary>
public class ImperialGuardianItemReward
{
    /// <summary>
    /// Gets or sets the group of the item.
    /// </summary>
    public byte Group { get; set; }

    /// <summary>
    /// Gets or sets the number of the item.
    /// </summary>
    public short Number { get; set; }

    /// <summary>
    /// Gets or sets how many items are given.
    /// </summary>
    public int Count { get; set; } = 1;

    /// <summary>
    /// Gets or sets the chance of this reward, relative to the others of the list.
    /// </summary>
    public int Chance { get; set; } = 1;

    /// <summary>
    /// Gives the player one reward of the list, chosen by the chances, into the gremory case when it's on.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="rewards">The rewards.</param>
    public static async ValueTask GiveOneAsync(Player player, IList<ImperialGuardianItemReward> rewards)
    {
        var total = rewards.Sum(r => Math.Max(r.Chance, 0));
        if (total <= 0 || Pick(rewards, Rand.NextInt(0, total)) is not { } reward)
        {
            return;
        }

        for (var i = 0; i < reward.Count; i++)
        {
            await RewardDelivery.DeliverAsync(player, new RewardItem { Group = reward.Group, Number = reward.Number }).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Chooses one reward of the list by the chances.
    /// </summary>
    /// <param name="rewards">The rewards.</param>
    /// <param name="roll">A random number from 0 to the sum of the chances, excluding it.</param>
    /// <returns>The reward, or <c>null</c> when the list has none.</returns>
    public static ImperialGuardianItemReward? Pick(IList<ImperialGuardianItemReward> rewards, int roll)
    {
        foreach (var reward in rewards.Where(r => r.Chance > 0))
        {
            if (roll < reward.Chance)
            {
                return reward;
            }

            roll -= reward.Chance;
        }

        return null;
    }
}
