// <copyright file="EvolutionQuestRules.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EvolutionQuests;

using MUnique.OpenMU.DataModel.Configuration.Quests;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.PlayerActions.Quests;

/// <summary>
/// The rules of the evolution quest chains. A chain is the quests of one giver, group and class which end in a
/// <see cref="QuestRewardType.CharacterEvolution"/> reward: its quests start one after the other, the first only when
/// the character meets the level rule of the evolution (<see cref="EvolutionRules"/>).
/// </summary>
public static class EvolutionQuestRules
{
    /// <summary>
    /// Gets the chain of a quest, ordered by number, or an empty list if the quest isn't part of an evolution chain.
    /// </summary>
    /// <param name="quest">The quest.</param>
    /// <returns>The chain.</returns>
    public static IReadOnlyList<QuestDefinition> GetEvolutionChain(this QuestDefinition quest)
    {
        if (quest.QualifiedCharacter is null || quest.QuestGiver is not { } giver)
        {
            return [];
        }

        var chain = giver.Quests
            .Where(q => q.Group == quest.Group && Equals(q.QualifiedCharacter, quest.QualifiedCharacter))
            .OrderBy(q => q.Number)
            .ToList();
        return chain.Any(q => q.Rewards.Any(r => r.RewardType == QuestRewardType.CharacterEvolution)) ? chain : [];
    }

    /// <summary>
    /// Gets the evolution which the chain of a quest leads to.
    /// </summary>
    /// <param name="quest">The quest.</param>
    /// <returns>The evolution, or <c>null</c> if the quest isn't part of an evolution chain.</returns>
    public static ClassEvolution? GetChainEvolution(this QuestDefinition quest)
        => quest.GetEvolutionChain().Count > 0 ? quest.QualifiedCharacter?.NextGenerationClass?.GetEvolution() : null;

    /// <summary>
    /// Determines whether the player can start the quest. A quest outside of an evolution chain is not restricted here.
    /// </summary>
    /// <param name="quest">The quest.</param>
    /// <param name="player">The player.</param>
    /// <returns><c>true</c>, if the chain allows to start the quest now.</returns>
    public static bool IsStartableInChain(this QuestDefinition quest, Player player)
    {
        var chain = quest.GetEvolutionChain();
        if (chain.Count == 0)
        {
            return true;
        }

        var lastFinished = player.GetQuestState(quest.Group)?.LastFinishedQuest;
        var finishedIndex = -1;
        for (var i = 0; i < chain.Count && lastFinished is not null; i++)
        {
            if (chain[i].Number == lastFinished.Number)
            {
                finishedIndex = i;
            }
        }

        var current = finishedIndex + 1 < chain.Count ? chain[finishedIndex + 1] : null;
        if (!Equals(current, quest))
        {
            return false;
        }

        return finishedIndex >= 0 || quest.GetChainEvolution() is not { } evolution || player.MeetsEvolutionRule(evolution);
    }

    /// <summary>
    /// Determines whether the class of a character has an evolution quest chain into its next class.
    /// </summary>
    /// <param name="characterClass">The character class.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    /// <returns><c>true</c>, if a quest of the configuration evolves this class.</returns>
    public static bool HasEvolutionQuest(this CharacterClass characterClass, GameConfiguration gameConfiguration)
    {
        return gameConfiguration.Monsters.Any(m => m.Quests.Any(q =>
            Equals(q.QualifiedCharacter, characterClass)
            && q.Rewards.Any(r => r.RewardType == QuestRewardType.CharacterEvolution)));
    }
}
