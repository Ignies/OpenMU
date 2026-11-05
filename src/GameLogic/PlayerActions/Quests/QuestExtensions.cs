// <copyright file="QuestExtensions.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Quests;

using MUnique.OpenMU.DataModel.Configuration.Quests;
using MUnique.OpenMU.Persistence;

/// <summary>
/// Extensions regarding quests.
/// </summary>
public static class QuestExtensions
{
    /// <summary>
    /// Clears the quest state for the specified player.
    /// </summary>
    /// <param name="questState">State of the quest.</param>
    /// <param name="persistenceContext">The persistence context of the player.</param>
    public static async ValueTask ClearAsync(this CharacterQuestState questState, IContext persistenceContext)
    {
        questState.ActiveQuest = null;
        questState.ClientActionPerformed = false;
        foreach (var requirementState in questState.RequirementStates)
        {
            await persistenceContext.DeleteAsync(requirementState).ConfigureAwait(false);
        }

        questState.RequirementStates.Clear();
    }

    /// <summary>
    /// Gets the level-up points which the character got from its finished quests.
    /// </summary>
    /// <param name="character">The character.</param>
    /// <param name="quests">All quest definitions.</param>
    /// <returns>The level-up points of the finished quests.</returns>
    /// <remarks>
    /// The quests of a group form a chain: every quest up to the group's last finished quest is finished. A quest number
    /// may exist once per character class, so each number counts once. Repeatable quests are left out.
    /// </remarks>
    public static int GetEarnedLevelUpPoints(this DataModel.Entities.Character character, IEnumerable<QuestDefinition> quests)
    {
        var lastFinished = new Dictionary<short, short>();
        foreach (var state in character.QuestStates)
        {
            if (state.LastFinishedQuest is { } last && (!lastFinished.TryGetValue(state.Group, out var known) || last.Number > known))
            {
                lastFinished[state.Group] = last.Number;
            }
        }

        if (lastFinished.Count == 0)
        {
            return 0;
        }

        return quests
            .Where(q => !q.Repeatable && lastFinished.TryGetValue(q.Group, out var last) && q.Number <= last)
            .GroupBy(q => (q.Group, q.Number))
            .Sum(g => g.Max(q => q.Rewards.Where(r => r.RewardType == QuestRewardType.LevelUpPoints).Sum(r => r.Value)));
    }
}
