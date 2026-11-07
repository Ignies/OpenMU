// <copyright file="Season21EvolutionQuestsInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Quests;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.EvolutionQuests;

/// <summary>
/// Adds the fourth and fifth class quests (<see cref="Season21EvolutionQuestData"/>): the Trial Area with its
/// monsters and NPCs, Sent in Lorencia and Eunice in Devias, and the quests of every class. Keeps what exists.
/// </summary>
internal class Season21EvolutionQuestsInitializer
{
    /// <summary>
    /// The number of the first spawn of the town NPCs on their maps, far above the spawn numbers of the towns.
    /// </summary>
    private const short FirstTownNpcSpawnNumber = 900;

    /// <summary>
    /// The number of the quest "Infiltration of Barracks" (third evolution, second step) in the Season 6 quest group.
    /// </summary>
    private const short BarracksQuestNumber = 5;

    /// <summary>
    /// The level up points of the Barracks quest in Season 6.
    /// </summary>
    private const int Season6BarracksPoints = 10;

    /// <summary>
    /// The level up points of the Barracks quest in Season 21.
    /// </summary>
    private const int Season21BarracksPoints = 20;

    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    private readonly IReadOnlyList<GameServerConfiguration> _gameServers;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21EvolutionQuestsInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    /// <param name="gameServers">The game servers; the Trial Area is hosted by the ones which host Lorencia.</param>
    public Season21EvolutionQuestsInitializer(IContext context, GameConfiguration gameConfiguration, IReadOnlyList<GameServerConfiguration> gameServers)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
        this._gameServers = gameServers;
    }

    /// <summary>
    /// Adds what the configuration doesn't contain yet.
    /// </summary>
    public void Initialize()
    {
        new Season21MapsInitializer(this._context, this._gameConfiguration, this._gameServers).Initialize(Season21EvolutionQuestData.TrialArea);
        this.SetNpcWindows();
        this.PlaceTownNpcs();
        this.RaiseBarracksQuestPoints();
        foreach (var quest in Season21EvolutionQuestData.Quests)
        {
            foreach (var characterClass in this._gameConfiguration.CharacterClasses.Where(c => c.NextGenerationClass?.GetEvolution() == quest.Evolution).ToList())
            {
                this.CreateQuest(quest, characterClass);
            }
        }
    }

    /// <summary>
    /// The second quest of the third evolution (Infiltration of Barracks) gives 20 points (S21Emu QuestReward.xml,
    /// RequireIndex 5), not the 10 of Season 6; a value of the operator stays.
    /// </summary>
    private void RaiseBarracksQuestPoints()
    {
        var rewards = this._gameConfiguration.Monsters.SelectMany(m => m.Quests)
            .Where(q => q is { Group: 0, Number: BarracksQuestNumber })
            .SelectMany(q => q.Rewards)
            .Where(r => r is { RewardType: QuestRewardType.LevelUpPoints, Value: Season6BarracksPoints });
        foreach (var reward in rewards)
        {
            reward.Value = Season21BarracksPoints;
        }
    }

    private void SetNpcWindows()
    {
        foreach (var npc in this._gameConfiguration.Monsters.Where(m => m.Number is Season21EvolutionQuestData.SentQuest or Season21EvolutionQuestData.EuniceQuest))
        {
            npc.NpcWindow = NpcWindow.NpcDialog;
        }
    }

    private void PlaceTownNpcs()
    {
        var number = FirstTownNpcSpawnNumber;
        foreach (var npc in Season21EvolutionQuestData.TownNpcs)
        {
            number++;
            if (this._gameConfiguration.Maps.FirstOrDefault(m => m.Number == npc.Map && m.Discriminator == 0) is not { } map
                || this._gameConfiguration.Monsters.FirstOrDefault(m => m.Number == npc.Number) is not { } definition
                || map.MonsterSpawns.Any(s => Equals(s.MonsterDefinition, definition)))
            {
                continue;
            }

            var area = this._context.CreateNew<MonsterSpawnArea>();
            area.SetGuid(map.Number, number);
            area.GameMap = map;
            area.MonsterDefinition = definition;
            area.Quantity = 1;
            area.Direction = npc.Direction;
            area.SpawnTrigger = SpawnTrigger.Automatic;
            (area.X1, area.X2, area.Y1, area.Y2) = (npc.X, npc.X, npc.Y, npc.Y);
            map.MonsterSpawns.Add(area);
        }
    }

    private void CreateQuest(Season21EvolutionQuest data, CharacterClass characterClass)
    {
        if (this._gameConfiguration.Monsters.FirstOrDefault(m => m.Number == data.Giver) is not { } giver
            || giver.Quests.Any(q => q.Group == EvolutionQuestConfiguration.Season21QuestGroup && q.Number == data.Number && Equals(q.QualifiedCharacter, characterClass)))
        {
            return;
        }

        var quest = this._context.CreateNew<QuestDefinition>();
        quest.SetGuid(EvolutionQuestConfiguration.Season21QuestGroup, data.Number, characterClass.Number);
        quest.Name = data.Name;
        quest.Group = EvolutionQuestConfiguration.Season21QuestGroup;
        quest.Number = data.Number;
        quest.StartingNumber = data.Number;
        quest.QualifiedCharacter = characterClass;
        quest.QuestGiver = giver;
        giver.Quests.Add(quest);

        foreach (var (monster, count) in data.Kills)
        {
            quest.WithMonsterKillRequirement(count, monster, this._context, this._gameConfiguration);
        }

        if (data.LevelUpPoints > 0)
        {
            this.AddReward(quest, QuestRewardType.LevelUpPoints, data.LevelUpPoints);
        }

        if (data.ItemReward is var (group, number)
            && this._gameConfiguration.Items.Any(i => i.Group == group && i.Number == number))
        {
            quest.WithItemReward(group, number, this._context, this._gameConfiguration);
        }

        if (data.Evolves)
        {
            this.AddReward(quest, QuestRewardType.CharacterEvolution, 0);
        }
    }

    private void AddReward(QuestDefinition quest, QuestRewardType type, int value)
    {
        var reward = this._context.CreateNew<QuestReward>();
        reward.RewardType = type;
        reward.Value = value;
        quest.Rewards.Add(reward);
    }
}
