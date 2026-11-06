// <copyright file="EvolutionQuestConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EvolutionQuests;

using MUnique.OpenMU.GameLogic.Classes;

/// <summary>
/// The configuration of the <see cref="EvolutionQuestFeaturePlugIn"/>. The defaults are the trial of the Season 21
/// server data (S21Emu <c>Quest/QuestInstanceManager.xml</c>, <c>Move/Gate.xml</c> gates 540 and 692).
/// </summary>
public class EvolutionQuestConfiguration
{
    /// <summary>
    /// The quest group of the Season 21 evolution quests; their numbers are the indexes of S21Emu <c>Quest/Quest.xml</c>.
    /// </summary>
    public const short Season21QuestGroup = 21;

    /// <summary>
    /// The number of the trial map of Season 21 (Trial Area).
    /// </summary>
    public const byte Season21TrialMap = 121;

    /// <summary>
    /// Gets or sets a value indicating whether a third class character stops gaining experience at the level of the
    /// fourth evolution until it did the fourth class quest.
    /// </summary>
    public bool HoldExperienceUntilFourthEvolution { get; set; } = true;

    /// <summary>
    /// Gets or sets the town NPCs which take a character to the trial.
    /// </summary>
    public List<EvolutionTrialEntrance> Entrances { get; set; } =
    [
        new() { NpcNumber = 764, Evolution = ClassEvolution.Fourth, Map = Season21TrialMap, X1 = 147, Y1 = 27, X2 = 148, Y2 = 28 },
        new() { NpcNumber = 939, Evolution = ClassEvolution.Fifth, Map = Season21TrialMap, X1 = 101, Y1 = 27, X2 = 105, Y2 = 28 },
    ];

    /// <summary>
    /// Gets or sets the monsters of the trial steps.
    /// </summary>
    public List<EvolutionTrialStep> Steps { get; set; } =
    [
        Step(7, TimeSpan.FromMinutes(10), (763, 147, 29)),
        Step(8, TimeSpan.FromMinutes(5), (767, 148, 28), (767, 147, 28), (770, 146, 29), (770, 145, 30), (771, 146, 31), (771, 148, 39)),
        Step(9, TimeSpan.FromMinutes(10), (763, 147, 29), (767, 148, 28), (770, 146, 28), (771, 147, 31)),
        FifthStep(10),
        FifthStep(11),
        FifthStep(12),
    ];

    /// <summary>
    /// Gets the trial step of a quest.
    /// </summary>
    /// <param name="group">The quest group.</param>
    /// <param name="number">The quest number.</param>
    /// <returns>The step, or <c>null</c>.</returns>
    public EvolutionTrialStep? GetStep(short group, short number) => this.Steps.FirstOrDefault(s => s.QuestGroup == group && s.QuestNumber == number);

    private static EvolutionTrialStep FifthStep(short number)
        => Step(number, TimeSpan.FromMinutes(10), (937, 103, 29), (895, 104, 28), (895, 103, 28), (896, 102, 29), (896, 101, 30), (897, 102, 31), (897, 104, 39));

    private static EvolutionTrialStep Step(short number, TimeSpan timeLimit, params (short Monster, byte X, byte Y)[] spawns) => new()
    {
        QuestGroup = Season21QuestGroup,
        QuestNumber = number,
        Map = Season21TrialMap,
        TimeLimit = timeLimit,
        Spawns = spawns.Select(s => new EvolutionTrialSpawn { MonsterNumber = s.Monster, X = s.X, Y = s.Y }).ToList(),
    };
}
