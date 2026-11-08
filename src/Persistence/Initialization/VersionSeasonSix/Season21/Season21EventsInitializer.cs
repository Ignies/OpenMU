// <copyright file="Season21EventsInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using System.Text.Json;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.Crywolf;
using MUnique.OpenMU.GameLogic.Elements;
using MUnique.OpenMU.GameLogic.EvolutionQuests;
using MUnique.OpenMU.GameLogic.MiniGames.ImperialGuardian;
using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the events of Season 21 which run as mini games: their maps, definitions and configurations. Keeps what exists,
/// including an event configuration the operator changed.
/// </summary>
internal class Season21EventsInitializer
{
    private const short CrywolfBalgassNumber = 349;

    private static readonly TimeOnly CrywolfAltarRegistration = new(9, 30);

    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    private readonly IReadOnlyList<GameServerConfiguration> _gameServers;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21EventsInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    /// <param name="gameServers">The game servers.</param>
    public Season21EventsInitializer(IContext context, GameConfiguration gameConfiguration, IReadOnlyList<GameServerConfiguration> gameServers)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
        this._gameServers = gameServers;
    }

    /// <summary>
    /// Adds the Tormented Square: map 102, the event for up to 20 players and its rounds, and switches its start on.
    /// </summary>
    public void AddTormentedSquare()
    {
        new Season21MapsInitializer(this._context, this._gameConfiguration, this._gameServers).Initialize(Season21TormentedSquareData.Arena);
        if (this.FindSpawnGate(Season21TormentedSquareData.MapNumber) is not { } entrance)
        {
            return;
        }

        var definition = this.GetOrCreateDefinition(MiniGameType.TormentedSquare, 1, LocalizedString.FromResource(() => MiniGameNames.TormentedSquare), LocalizedString.FromResource(() => MiniGameDescriptions.TormentedSquare), entrance);
        if (definition.MaximumPlayerCount == 0)
        {
            definition.MapCreationPolicy = MiniGameMapCreationPolicy.Shared;
            definition.MaximumPlayerCount = 20;
            definition.MinimumPlayerCount = 1;
            definition.EnterDuration = TimeSpan.FromMinutes(2);
            definition.GameDuration = TimeSpan.FromMinutes(25);
            definition.ExitDuration = TimeSpan.FromMinutes(1);
            definition.AllowParty = true;
        }

        // The first application adds the rounds and switches the timetable on; later ones keep the operator's.
        if (this.UpdateEvents(events => events.TormentedSquare is null && (events.TormentedSquare = Season21TormentedSquareData.CreateDefinition()) is not null))
        {
            this.SwitchOn(typeof(TormentedSquareStartPlugIn).GUID);
        }
    }

    /// <summary>
    /// Adds the Instance Zone (map 114) with the entrances of the fourth and fifth class trials and the Labyrinth of
    /// Dimensions (map 115, a solo instance of 41 floors behind the Portal of Dimension). Sent and Eunice in the towns
    /// then take a character to the Instance Zone, and Cent and Eunice there into the party instance of the trial.
    /// </summary>
    public void AddInstanceZone()
    {
        new Season21MapsInitializer(this._context, this._gameConfiguration, this._gameServers).Initialize(Season21InstanceZoneData.Maps);
        this.RouteTrialsThroughTheInstanceZone();
        if (this.FindSpawnGate(Season21InstanceZoneData.LabyrinthMap) is not { } entrance)
        {
            return;
        }

        var definition = this.GetOrCreateDefinition(MiniGameType.LabyrinthOfDimensions, 1, LocalizedString.FromResource(() => MiniGameNames.LabyrinthOfDimensions), LocalizedString.FromResource(() => MiniGameDescriptions.LabyrinthOfDimensions), entrance);
        if (definition.MaximumPlayerCount == 0)
        {
            definition.MapCreationPolicy = MiniGameMapCreationPolicy.OnePerPlayer;
            definition.MaximumPlayerCount = 1;
            definition.MinimumPlayerCount = 1;
            definition.MinimumCharacterLevel = 400;
            definition.MinimumSpecialCharacterLevel = 400;
            definition.EnterDuration = TimeSpan.FromSeconds(10);
            definition.GameDuration = TimeSpan.FromMinutes(60);
            definition.ExitDuration = TimeSpan.FromMinutes(1);
        }

        this.UpdateEvents(events =>
        {
            var changed = false;
            if (events.Labyrinth is null)
            {
                events.Labyrinth = Season21InstanceZoneData.CreateLabyrinth();
                changed = true;
            }

            if (events.Entrances.All(e => e.NpcNumber != Season21InstanceZoneData.PortalOfDimension))
            {
                events.Entrances.Add(new EventEntrance { NpcNumber = Season21InstanceZoneData.PortalOfDimension, Type = MiniGameType.LabyrinthOfDimensions, GameLevel = 1 });
                changed = true;
            }

            return changed;
        });
    }

    /// <summary>
    /// Adds the Boss Battle Together: map 139, the event for up to 100 players from total level 800 for 10,000,000 zen,
    /// its five bosses and rewards, and switches its start on.
    /// </summary>
    public void AddBossBattle()
    {
        new Season21MapsInitializer(this._context, this._gameConfiguration, this._gameServers).Initialize(Season21BossBattleData.Zone);
        if (this.FindSpawnGate(Season21BossBattleData.MapNumber) is not { } entrance)
        {
            return;
        }

        var definition = this.GetOrCreateDefinition(MiniGameType.BossBattle, 1, LocalizedString.FromResource(() => MiniGameNames.BossBattle), LocalizedString.FromResource(() => MiniGameDescriptions.BossBattle), entrance);
        if (definition.MaximumPlayerCount == 0)
        {
            definition.MapCreationPolicy = MiniGameMapCreationPolicy.Shared;
            definition.MaximumPlayerCount = 100;
            definition.MinimumPlayerCount = 1;
            definition.MinimumCharacterLevel = 800;
            definition.MinimumSpecialCharacterLevel = 800;
            definition.EntranceFee = 10_000_000;
            definition.EnterDuration = TimeSpan.FromMinutes(5);
            definition.GameDuration = TimeSpan.FromMinutes(5);
            definition.ExitDuration = TimeSpan.FromMinutes(1);
            definition.AllowParty = true;
        }

        if (this.UpdateEvents(events => events.BossBattle is null && (events.BossBattle = Season21BossBattleData.CreateDefinition()) is not null))
        {
            this.SwitchOn(typeof(BossBattleStartPlugIn).GUID);
        }
    }

    /// <summary>
    /// Adds the Protector of Acheron: the event copies of Acheron (map 92) and Deventer (map 96), one game level each, 30
    /// waves of their area monsters, the reward bag for 100 points, and switches its start on.
    /// </summary>
    public void AddAcheronGuardian()
    {
        new Season21MapsInitializer(this._context, this._gameConfiguration, this._gameServers).Initialize(Season21AcheronGuardianData.Maps);
        var rewards = Season21ItemBags.GetOrCreate(this._context, this._gameConfiguration, Season21AcheronGuardianData.RewardBag, Season21AcheronGuardianData.RewardItems);
        LocalizedString[] names = [LocalizedString.FromResource(() => MiniGameNames.AcheronGuardian1), LocalizedString.FromResource(() => MiniGameNames.AcheronGuardian2)];
        for (var index = 0; index < Season21AcheronGuardianData.Levels.Count; index++)
        {
            if (this.FindSpawnGate(Season21AcheronGuardianData.Levels[index].Map) is not { } entrance)
            {
                return;
            }

            var definition = this.GetOrCreateDefinition(MiniGameType.AcheronGuardian, (byte)(index + 1), names[index], LocalizedString.FromResource(() => MiniGameDescriptions.AcheronGuardian), entrance);
            if (definition.MaximumPlayerCount != 0)
            {
                continue;
            }

            definition.MapCreationPolicy = MiniGameMapCreationPolicy.Shared;
            definition.MaximumPlayerCount = 50;
            definition.MinimumPlayerCount = 1;
            definition.MinimumCharacterLevel = 450;
            definition.MinimumSpecialCharacterLevel = 450;
            definition.EnterDuration = TimeSpan.FromMinutes(5);
            definition.GameDuration = TimeSpan.FromMinutes(30);
            definition.ExitDuration = TimeSpan.FromMinutes(1);
            definition.AllowParty = true;
            var reward = this._context.CreateNew<MiniGameReward>();
            reward.RewardType = MiniGameRewardType.Item;
            reward.RewardAmount = 1;
            reward.ItemReward = rewards;
            definition.Rewards.Add(reward);
        }

        if (this.UpdateEvents(events => events.AcheronGuardian.Count == 0 && AddWaves(events)))
        {
            this.SwitchOn(typeof(AcheronGuardianStartPlugIn).GUID);
        }
    }

    /// <summary>
    /// Applies the Season 18 rework of the Imperial Guardian (S21Emu <c>Event/ImperialGuardian/ImperialGuardianRenewal.xml</c>):
    /// every day from total level 1000, and at the completion one of the rewards of the data: four (20 %) or two (40 %)
    /// Artifact Upgrade Stones (20/94) or a Spider Artifact Fragment (20/95, 40 %).
    /// </summary>
    public void ApplyImperialGuardianRework()
    {
        foreach (var definition in this._gameConfiguration.MiniGameDefinitions.Where(d => d.Type == MiniGameType.ImperialGuardian))
        {
            definition.MinimumCharacterLevel = 1000;
            definition.MinimumSpecialCharacterLevel = 1000;
            definition.MaximumCharacterLevel = int.MaxValue;
            definition.MaximumSpecialCharacterLevel = int.MaxValue;
        }

        var plugIn = this.GetOrCreatePlugIn(typeof(ImperialGuardianFeaturePlugIn).GUID);
        var referenceHandler = new Updates.GameConfigurationReferenceHandler(this._gameConfiguration);
        ImperialGuardianEventDefinition configuration;
        try
        {
            configuration = plugIn.GetConfiguration<ImperialGuardianEventDefinition>(referenceHandler) ?? new ImperialGuardianEventDefinition();
        }
        catch (JsonException)
        {
            // A configuration which can't be read is the operator's; it stays.
            return;
        }

        if (configuration.CompletionRewards.Count > 0)
        {
            return;
        }

        configuration.CompletionRewards =
        [
            new() { Group = 20, Number = 94, Count = 4, Chance = 20 },
            new() { Group = 20, Number = 94, Count = 2, Chance = 40 },
            new() { Group = 20, Number = 95, Count = 1, Chance = 40 },
        ];
        plugIn.SetConfiguration(configuration, referenceHandler);
    }

    /// <summary>
    /// Adds the Chaos Castle Survival of the Fittest: map 97, a Chaos Castle for up to 70 players from level 280, its stages
    /// and the tournament, and switches its start and tournament on.
    /// </summary>
    public void AddChaosCastleSurvival()
    {
        new Season21MapsInitializer(this._context, this._gameConfiguration, this._gameServers).Initialize(Season21ChaosCastleSurvivalData.Castle);
        if (this._gameConfiguration.Maps.FirstOrDefault(m => m.Number == Season21ChaosCastleSurvivalData.MapNumber && m.Discriminator == 0) is not { } map
            || this.FindSpawnGate(Season21ChaosCastleSurvivalData.MapNumber) is not { } entrance)
        {
            return;
        }

        // The game spawns the monsters of the castle at its start, like in every chaos castle.
        foreach (var spawn in map.MonsterSpawns.Where(s => s.MonsterDefinition?.ObjectKind == NpcObjectKind.Monster))
        {
            spawn.SpawnTrigger = SpawnTrigger.ManuallyForEvent;
        }

        var definition = this.GetOrCreateDefinition(MiniGameType.ChaosCastleSurvival, 1, LocalizedString.FromResource(() => MiniGameNames.ChaosCastleSurvival), LocalizedString.FromResource(() => MiniGameDescriptions.ChaosCastleSurvival), entrance);
        if (definition.MaximumPlayerCount != 0)
        {
            return;
        }

        definition.MapCreationPolicy = MiniGameMapCreationPolicy.Shared;
        definition.MaximumPlayerCount = 70;
        definition.MinimumPlayerCount = 5;
        definition.MinimumCharacterLevel = 280;
        definition.MaximumCharacterLevel = 400;
        definition.MinimumSpecialCharacterLevel = 280;
        definition.MaximumSpecialCharacterLevel = 400;
        definition.EnterDuration = TimeSpan.FromMinutes(5);
        definition.GameDuration = TimeSpan.FromMinutes(10);
        definition.ExitDuration = TimeSpan.FromMinutes(4);
        definition.SaveRankingStatistics = true;
        definition.AllowParty = false;
        new VersionSeasonSix.Events.ChaosCastleInitializer(this._context, this._gameConfiguration).CreateEvents(definition);
        this.SwitchOn(typeof(ChaosCastleSurvivalPlugIn).GUID);
        this.SwitchOn(typeof(ChaosCastleSurvivalStartPlugIn).GUID);
    }

    /// <summary>
    /// Adds the Illusion Temple League of Season 21 (S21Emu <c>Event/IlusionTempleLeague</c>): the statues of the renewed
    /// temple on maps 98 and 99 (they spawn with the match), a league temple on each for two guild parties of up to five
    /// from total level 400, entered from the waiting area (gates 444/445), and switches the league and its timetable on.
    /// Keeps a temple and statues the operator changed.
    /// </summary>
    public void AddIllusionTempleLeague()
    {
        for (byte level = 1; level <= Season21IllusionTempleLeagueData.MapNumbers.Length; level++)
        {
            var mapNumber = Season21IllusionTempleLeagueData.MapNumbers[level - 1];
            if (this._gameConfiguration.Maps.FirstOrDefault(m => m.Number == mapNumber && m.Discriminator == 0) is not { } map
                || map.ExitGates.Where(g => g.IsSpawnGate).OrderBy(g => g.X1).ThenBy(g => g.Y1).FirstOrDefault() is not { } waitingArea)
            {
                continue;
            }

            this.AddIllusionTempleLeagueStatues(map);
            var name = MiniGameNames.ResourceManager.GetLocalizedString($"IllusionTempleLeague{level}");
            var definition = this.GetOrCreateDefinition(MiniGameType.IllusionTempleLeague, level, name, LocalizedString.FromResource(() => MiniGameDescriptions.IllusionTempleLeague), waitingArea);
            if (definition.MaximumPlayerCount != 0)
            {
                continue;
            }

            definition.MapCreationPolicy = MiniGameMapCreationPolicy.Shared;
            definition.MaximumPlayerCount = 10;
            definition.MinimumPlayerCount = 2;
            definition.MinimumCharacterLevel = 400;
            definition.MinimumSpecialCharacterLevel = 400;
            definition.EnterDuration = TimeSpan.FromMinutes(5);
            definition.GameDuration = TimeSpan.FromMinutes(15);
            definition.ExitDuration = TimeSpan.FromMinutes(1);
            definition.AllowParty = true;
            definition.ArePlayerKillersAllowedToEnter = false;
        }

        this.SwitchOn(typeof(IllusionTempleLeaguePlugIn).GUID);
        this.SwitchOn(typeof(IllusionTempleLeagueStartPlugIn).GUID);
    }

    /// <summary>
    /// Applies the Crywolf renewal of Season 21 Part 1 Update 3 (webzen guide 425): the army takes the element of the week
    /// (fire, water, earth, wind, darkness, from Thursday 00:00), Balgass appears 6:40 into the battle, is immune to debuffs
    /// and the rewards go into the gremory case; the altars open on Monday 9:30. The state durations are the ones of S21Emu
    /// <c>Event/Crywolf.dat</c>. Keeps a renewal and a schedule the operator already set.
    /// </summary>
    public void ApplyCrywolfRenewal()
    {
        this.MakeImmuneToDebuffs(CrywolfBalgassNumber);
        var plugIn = this.GetOrCreatePlugIn(typeof(CrywolfPlugIn).GUID);
        var referenceHandler = new Updates.GameConfigurationReferenceHandler(this._gameConfiguration);
        CrywolfEventDefinition configuration;
        try
        {
            configuration = plugIn.GetConfiguration<CrywolfEventDefinition>(referenceHandler) ?? new CrywolfEventDefinition();
        }
        catch (JsonException)
        {
            // A configuration which can't be read is the operator's; it stays.
            return;
        }

        if (configuration.WeeklyElements.Count == 0)
        {
            configuration.WeeklyElements = [Element.Fire, Element.Water, Element.Earth, Element.Wind, Element.Darkness];
            configuration.BalgassAppearanceDelay = TimeSpan.FromMinutes(6) + TimeSpan.FromSeconds(40);
            configuration.RewardsIntoGremoryCase = true;
            configuration.Notify1Duration = TimeSpan.FromSeconds(300);
            configuration.Notify2Duration = TimeSpan.FromSeconds(60);
            configuration.ReadyDuration = TimeSpan.FromSeconds(120);
            configuration.BattleDuration = TimeSpan.FromSeconds(900);
            configuration.EndDuration = TimeSpan.FromSeconds(15);
            configuration.EndCycleDuration = TimeSpan.FromSeconds(10);
        }

        var season6 = new CrywolfEventDefinition();
        if (configuration.StartDays.SequenceEqual(season6.StartDays) && configuration.StartTimes.SequenceEqual(season6.StartTimes))
        {
            // The altars can be contracted in the ready state, which follows the two notifications.
            configuration.StartDays = [DayOfWeek.Monday];
            configuration.StartTimes = [CrywolfAltarRegistration.Add(-(configuration.Notify1Duration + configuration.Notify2Duration))];
        }

        plugIn.SetConfiguration(configuration, referenceHandler);
    }

    private static bool AddWaves(Season21EventsConfiguration events)
    {
        events.AcheronGuardian.AddRange(Season21AcheronGuardianData.Levels.Select(level => Season21AcheronGuardianData.CreateDefinition(level.Areas)));
        return true;
    }

    private void MakeImmuneToDebuffs(short monsterNumber)
    {
        if (this._gameConfiguration.Monsters.FirstOrDefault(m => m.Number == monsterNumber) is not { } monster)
        {
            return;
        }

        // A database from before the immunity existed lacks its definition.
        if (this._gameConfiguration.Attributes.All(a => a.Id != Stats.IsImmuneToDebuffs.Id))
        {
            this._gameConfiguration.Attributes.Add(this._context.CreateNew<AttributeDefinition>(Stats.IsImmuneToDebuffs.Id, Stats.IsImmuneToDebuffs.Designation, Stats.IsImmuneToDebuffs.Description));
        }

        var immunity = Stats.IsImmuneToDebuffs.GetPersistent(this._gameConfiguration);
        if (monster.Attributes.Any(a => a.AttributeDefinition == immunity))
        {
            return;
        }

        monster.AddAttributes(new Dictionary<AttributeDefinition, float> { [Stats.IsImmuneToDebuffs] = 1 }, this._context, this._gameConfiguration);
    }

    private void RouteTrialsThroughTheInstanceZone()
    {
        var typeId = typeof(EvolutionQuestFeaturePlugIn).GUID;
        if (this._gameConfiguration.PlugInConfigurations.FirstOrDefault(c => c.TypeId == typeId) is not { } plugIn)
        {
            plugIn = this.GetOrCreatePlugIn(typeId);
            plugIn.IsActive = true;
        }

        EvolutionQuestConfiguration configuration;
        try
        {
            configuration = plugIn.GetConfiguration<EvolutionQuestConfiguration>(new Updates.IdIgnoringReferenceHandler()) ?? new EvolutionQuestConfiguration();
        }
        catch (JsonException)
        {
            // A configuration which can't be read is the operator's; it stays.
            return;
        }

        if (configuration.Entrances.Any(e => e.NpcNumber == Season21InstanceZoneData.CentEntrance))
        {
            return;
        }

        foreach (var (town, evolution, gate) in new[] { ((short)764, ClassEvolution.Fourth, (238, 237, 239, 238)), ((short)939, ClassEvolution.Fifth, (116, 236, 120, 238)) })
        {
            if (configuration.Entrances.FirstOrDefault(e => e.NpcNumber == town) is { } row)
            {
                row.Map = Season21InstanceZoneData.InstanceZoneMap;
                (row.X1, row.Y1, row.X2, row.Y2) = ((byte)gate.Item1, (byte)gate.Item2, (byte)gate.Item3, (byte)gate.Item4);
            }
        }

        configuration.Entrances.Add(new EvolutionTrialEntrance { NpcNumber = Season21InstanceZoneData.CentEntrance, Evolution = ClassEvolution.Fourth, Map = EvolutionQuestConfiguration.Season21TrialMap, X1 = 147, Y1 = 27, X2 = 148, Y2 = 28 });
        configuration.Entrances.Add(new EvolutionTrialEntrance { NpcNumber = Season21InstanceZoneData.EuniceEntrance, Evolution = ClassEvolution.Fifth, Map = EvolutionQuestConfiguration.Season21TrialMap, X1 = 101, Y1 = 27, X2 = 105, Y2 = 28 });
        plugIn.SetConfiguration(configuration, null);
    }

    private void AddIllusionTempleLeagueStatues(GameMapDefinition map)
    {
        short number = 1;
        foreach (var (monsterNumber, x, y) in Season21IllusionTempleLeagueData.Statues)
        {
            if (this._gameConfiguration.Monsters.FirstOrDefault(m => m.Number == monsterNumber) is not { } statue
                || map.MonsterSpawns.Any(s => s.MonsterDefinition == statue))
            {
                number++;
                continue;
            }

            var area = this._context.CreateNew<MonsterSpawnArea>();
            area.SetGuid(map.Number, number++);
            area.GameMap = map;
            area.MonsterDefinition = statue;
            area.Quantity = 1;
            area.Direction = monsterNumber == Season21IllusionTempleLeagueData.Statues[0].Number ? Direction.South : Direction.Undefined;
            area.SpawnTrigger = SpawnTrigger.ManuallyForEvent;
            (area.X1, area.X2, area.Y1, area.Y2) = (x, x, y, y);
            map.MonsterSpawns.Add(area);
        }
    }

    private ExitGate? FindSpawnGate(byte mapNumber)
        => this._gameConfiguration.Maps.FirstOrDefault(m => m.Number == mapNumber && m.Discriminator == 0)?.ExitGates.FirstOrDefault(g => g.IsSpawnGate);

    private MiniGameDefinition GetOrCreateDefinition(MiniGameType type, byte level, LocalizedString name, LocalizedString description, ExitGate entrance)
    {
        var id = GuidHelper.CreateGuid<MiniGameDefinition>((short)type, level);
        if (this._gameConfiguration.MiniGameDefinitions.FirstOrDefault(d => d.GetId() == id) is { } existing)
        {
            return existing;
        }

        var definition = this._context.CreateNew<MiniGameDefinition>();
        definition.SetGuid((short)type, level);
        definition.Type = type;
        definition.GameLevel = level;
        definition.Name = name;
        definition.Description = description;
        definition.Entrance = entrance;
        definition.ArePlayerKillersAllowedToEnter = true;
        definition.MinimumCharacterLevel = 1;
        definition.MaximumCharacterLevel = int.MaxValue;
        definition.MinimumSpecialCharacterLevel = 1;
        definition.MaximumSpecialCharacterLevel = int.MaxValue;
        this._gameConfiguration.MiniGameDefinitions.Add(definition);
        return definition;
    }

    /// <summary>
    /// Changes the configuration of the events, when the change says it changed something.
    /// </summary>
    /// <returns><c>true</c>, if the configuration changed.</returns>
    private bool UpdateEvents(Func<Season21EventsConfiguration, bool> update)
    {
        var plugIn = this.GetOrCreatePlugIn(typeof(Season21EventsFeaturePlugIn).GUID);
        Season21EventsConfiguration configuration;
        try
        {
            configuration = plugIn.GetConfiguration<Season21EventsConfiguration>(new Updates.IdIgnoringReferenceHandler()) ?? new Season21EventsConfiguration();
        }
        catch (JsonException)
        {
            // A configuration which can't be read is the operator's; it stays.
            return false;
        }

        if (!update(configuration))
        {
            return false;
        }

        plugIn.SetConfiguration(configuration, null);
        plugIn.IsActive = true;
        return true;
    }

    private void SwitchOn(Guid typeId) => this.GetOrCreatePlugIn(typeId).IsActive = true;

    private PlugInConfiguration GetOrCreatePlugIn(Guid typeId)
    {
        if (this._gameConfiguration.PlugInConfigurations.FirstOrDefault(c => c.TypeId == typeId) is { } existing)
        {
            return existing;
        }

        var plugIn = this._context.CreateNew<PlugInConfiguration>();
        plugIn.SetGuid(typeId);
        plugIn.TypeId = typeId;
        this._gameConfiguration.PlugInConfigurations.Add(plugIn);
        return plugIn;
    }
}
