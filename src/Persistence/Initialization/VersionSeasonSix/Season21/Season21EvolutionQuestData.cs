// <copyright file="Season21EvolutionQuestData.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.EvolutionQuests;
using MUnique.OpenMU.Interfaces;

/// <summary>
/// The data of the fourth and fifth class quests, from S21Emu <c>Quest/Quest.xml</c> (indexes 7-12),
/// <c>QuestObjective.txt</c>, <c>QuestReward.xml</c>, <c>QuestInstanceManager.xml</c> and <c>MonsterSetBase</c>.
/// </summary>
internal static class Season21EvolutionQuestData
{
    /// <summary>
    /// The town NPC of the fourth class quest (Sent, Lorencia 131/147).
    /// </summary>
    internal const short SentTown = 764;

    /// <summary>
    /// The quest NPC of the fourth class quest in the Trial Area.
    /// </summary>
    internal const short SentQuest = 766;

    /// <summary>
    /// The town NPC of the fifth class quest (Eunice, Devias 193/11).
    /// </summary>
    internal const short EuniceVillage = 939;

    /// <summary>
    /// The quest NPC of the fifth class quest in the Trial Area.
    /// </summary>
    internal const short EuniceQuest = 941;

    /// <summary>
    /// Gets the Trial Area, its monsters and its NPCs. The town NPCs are in the set too, for their definitions; they're
    /// placed on their town maps by <see cref="TownNpcs"/>.
    /// </summary>
    /// <remarks>
    /// The data gives Sent (763) no experience level; the level of the fourth evolution (800) is used. Eunice 937 and
    /// 938 are placeholders of 10 health in the data and kept as they are.
    /// </remarks>
    public static Season21MapSet TrialArea { get; } = new(
        GameVersion.Season15Episode1,
        750,
        0,
        [
            new(EvolutionQuestConfiguration.Season21TrialMap, LocalizedString.FromResource(() => MapNames.TrialArea), 0, 122),
        ],
        [
            new(763, LocalizedString.FromResource(() => MonsterNames.Sent), Level: 800, Health: 1000000, MinimumDamage: 2600, MaximumDamage: 2800, Defense: 1750, AttackRate: 2900, DefenseRate: 1020, MoveRange: 3, AttackRange: 3, ViewRange: 5, MoveDelay: 400, AttackDelay: 2200, RespawnDelay: 10, Attribute: 2, Resistances: [2, 2, 0, 2, 2, 0, 0]),
            new(937, LocalizedString.FromResource(() => MonsterNames.Eunice), Level: 2, Health: 10, MinimumDamage: 10, MaximumDamage: 10, Defense: 10, AttackRate: 10, DefenseRate: 10, MoveRange: 0, AttackRange: 0, ViewRange: 0, MoveDelay: 0, AttackDelay: 0, RespawnDelay: 10, Attribute: 0, Resistances: [0, 0, 0, 0, 0, 0, 0]),
            new(938, LocalizedString.FromResource(() => MonsterNames.EuniceAndGiant), Level: 2, Health: 10, MinimumDamage: 10, MaximumDamage: 10, Defense: 10, AttackRate: 10, DefenseRate: 10, MoveRange: 0, AttackRange: 0, ViewRange: 0, MoveDelay: 0, AttackDelay: 0, RespawnDelay: 10, Attribute: 0, Resistances: [0, 0, 0, 0, 0, 0, 0]),
        ],
        [],
        [],
        [])
    {
        Npcs =
        [
            new(SentTown, LocalizedString.FromResource(() => MonsterNames.SentTown), 0, 131, 147, Direction.South),
            new(SentQuest, LocalizedString.FromResource(() => MonsterNames.SentQuest), EvolutionQuestConfiguration.Season21TrialMap, 146, 30, Direction.South),
            new(EuniceVillage, LocalizedString.FromResource(() => MonsterNames.EuniceVillage), 2, 193, 11, Direction.South),
            new(EuniceQuest, LocalizedString.FromResource(() => MonsterNames.EuniceQuest), EvolutionQuestConfiguration.Season21TrialMap, 103, 29, Direction.South),
        ],
    };

    /// <summary>
    /// Gets the NPCs which stand in the towns of Season 6.
    /// </summary>
    public static IReadOnlyList<Season21Npc> TownNpcs { get; } = TrialArea.Npcs.Where(npc => npc.Map != EvolutionQuestConfiguration.Season21TrialMap).ToList();

    /// <summary>
    /// Gets the quests: three for each evolution, the last one evolves.
    /// </summary>
    public static IReadOnlyList<Season21EvolutionQuest> Quests { get; } =
    [
        new(7, ClassEvolution.Fourth, SentQuest, LocalizedString.FromResource(() => QuestNames.FourthClassEligibilityTest), [(763, 1)], 20, null, false),
        new(8, ClassEvolution.Fourth, SentQuest, LocalizedString.FromResource(() => QuestNames.FourthClassNewBattle1), [(767, 5), (770, 5), (771, 5)], 30, null, false),
        new(9, ClassEvolution.Fourth, SentQuest, LocalizedString.FromResource(() => QuestNames.FourthClassNewBattle2), [(763, 1)], 50, (13, 155), true),
        new(10, ClassEvolution.Fifth, EuniceQuest, LocalizedString.FromResource(() => QuestNames.FifthClassNewPowerNuke), [(937, 1)], 0, (20, 235), false),
        new(11, ClassEvolution.Fifth, EuniceQuest, LocalizedString.FromResource(() => QuestNames.FifthClassNewPowerBolt), [(937, 1)], 0, (20, 236), false),
        new(12, ClassEvolution.Fifth, EuniceQuest, LocalizedString.FromResource(() => QuestNames.FifthClassNewPowerAoE), [(937, 1)], 200, (20, 237), true),
    ];
}
