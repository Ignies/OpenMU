// <copyright file="EvolutionTrialStep.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EvolutionQuests;

/// <summary>
/// The monsters which a quest of an evolution chain spawns on the trial map while it's active.
/// </summary>
public class EvolutionTrialStep
{
    /// <summary>
    /// Gets or sets the group of the quest.
    /// </summary>
    public short QuestGroup { get; set; }

    /// <summary>
    /// Gets or sets the number of the quest.
    /// </summary>
    public short QuestNumber { get; set; }

    /// <summary>
    /// Gets or sets the number of the map where the monsters spawn.
    /// </summary>
    public byte Map { get; set; }

    /// <summary>
    /// Gets or sets how long the monsters stay; after that, the player starts the trial again at the quest NPC.
    /// </summary>
    public TimeSpan TimeLimit { get; set; }

    /// <summary>
    /// Gets or sets the monsters. A killed monster comes back after its respawn delay while the trial runs.
    /// </summary>
    public List<EvolutionTrialSpawn> Spawns { get; set; } = [];
}
