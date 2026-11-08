// <copyright file="EvolutionQuestInGameTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.EvolutionQuests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Quests;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.EvolutionQuests;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlayerActions;
using MUnique.OpenMU.GameLogic.PlayerActions.Character;
using MUnique.OpenMU.GameLogic.PlayerActions.Quests;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Progression;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.Character;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix;
using MUnique.OpenMU.PlugIns;
using Moq;

/// <summary>
/// Runs characters through the fourth class quest in an in-memory game with the Season 21 data: Sent in Lorencia, the
/// Trial Area, the three steps and the evolution, plus the experience hold at the level of the fourth evolution.
/// </summary>
[TestFixture]
public class EvolutionQuestInGameTest
{
    private const short Group = EvolutionQuestConfiguration.Season21QuestGroup;

    private readonly QuestMonsterKillCountPlugIn _killCounter = new();

    private GameConfiguration _gameConfiguration = null!;

    private GameContext _gameContext = null!;

    private EvolutionQuestFeaturePlugIn _plugIn = null!;

    /// <summary>
    /// Initializes the Season 6 data with the Season 21 maps, classes, items and quests.
    /// </summary>
    [OneTimeSetUp]
    public async ValueTask SetupAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        await new DataInitialization(contextProvider, new NullLoggerFactory()).CreateInitialDataAsync(0, false).ConfigureAwait(false);
        using var context = contextProvider.CreateNewConfigurationContext();
        this._gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        await new AddSeason21LevelCapsUpdatePlugIn().ApplyUpdateAsync(context, this._gameConfiguration).ConfigureAwait(false);
        await new AddSeason21MapsUpdatePlugIn().ApplyUpdateAsync(context, this._gameConfiguration).ConfigureAwait(false);
        await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, this._gameConfiguration).ConfigureAwait(false);
        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, this._gameConfiguration).ConfigureAwait(false);
        await new AddSeason21EvolutionQuestsUpdatePlugIn().ApplyUpdateAsync(context, this._gameConfiguration).ConfigureAwait(false);

        var mapInitializer = new MapInitializer(this._gameConfiguration, new NullLogger<MapInitializer>(), NullDropGenerator.Instance, null);
        var plugInManager = new PlugInManager(null, new NullLoggerFactory(), null, null);
        this._gameContext = new GameContext(this._gameConfiguration, contextProvider, mapInitializer, new NullLoggerFactory(), plugInManager, NullDropGenerator.Instance, new ConfigurationChangeMediator());
        mapInitializer.PlugInManager = this._gameContext.PlugInManager;
        mapInitializer.PathFinderPool = this._gameContext.PathFinderPool;

        this._plugIn = new EvolutionQuestFeaturePlugIn();
        this._gameContext.FeaturePlugIns.AddPlugIn(this._plugIn, true);
        plugInManager.RegisterPlugInAtPlugInPoint<IExperienceCalculationPlugIn>(this._plugIn);
        plugInManager.RegisterPlugInAtPlugInPoint<IPlayerTalkToNpcPlugIn>(this._plugIn);
        plugInManager.RegisterPlugInAtPlugInPoint<IAttackableGotKilledPlugIn>(this._plugIn);
        plugInManager.RegisterPlugInAtPlugInPoint<IObjectAddedToMapPlugIn>(this._plugIn);
        plugInManager.RegisterPlugInAtPlugInPoint<IObjectRemovedFromMapPlugIn>(this._plugIn);
    }

    /// <summary>
    /// Resets the configuration of the plugin.
    /// </summary>
    [SetUp]
    public void ResetConfiguration() => this._plugIn.Configuration = null;

    /// <summary>
    /// A blade master goes from Sent in Lorencia through the three trial steps and becomes a dragon knight.
    /// </summary>
    [Test]
    public async ValueTask BladeMasterBecomesDragonKnightAsync()
    {
        var player = await this.CreateThirdClassAsync(CharacterClassNumber.BladeMaster, 400, 400).ConfigureAwait(false);
        var character = player.SelectedCharacter!;
        var points = character.LevelUpPoints;

        await this.TalkAsync(player, 764).ConfigureAwait(false);
        Assert.That(player.CurrentMap?.Definition.Number, Is.EqualTo(EvolutionQuestConfiguration.Season21TrialMap));

        await this.TalkAsync(player, 766).ConfigureAwait(false);
        Assert.That(player.GetAvailableQuestsOfOpenedNpc().Select(q => q.Number), Is.EqualTo(new short[] { 7 }));

        foreach (var step in new short[] { 7, 8, 9 })
        {
            await new QuestStartAction().StartQuestAsync(player, Group, step).ConfigureAwait(false);
            var trial = this._plugIn.GetTrial(player);
            Assert.That(trial?.Step.QuestNumber, Is.EqualTo(step));
            Assert.That(trial!.Monsters.Select(m => m.CurrentMap), Has.All.EqualTo(player.CurrentMap));

            await new QuestCompletionAction().CompleteQuestAsync(player, Group, step).ConfigureAwait(false);
            Assert.That(player.GetQuestState(Group)?.ActiveQuest?.Number, Is.EqualTo(step), "not completed without the kills");

            foreach (var requirement in player.GetQuestState(Group)!.ActiveQuest!.RequiredMonsterKills)
            {
                var monster = trial.Monsters.First(m => Equals(m.Definition, requirement.Monster));
                for (var i = 0; i < requirement.MinimumNumber; i++)
                {
                    await this._killCounter.AttackableGotKilledAsync(monster, player).ConfigureAwait(false);
                }
            }

            await new QuestCompletionAction().CompleteQuestAsync(player, Group, step).ConfigureAwait(false);
            Assert.That(player.GetQuestState(Group)?.LastFinishedQuest?.Number, Is.EqualTo(step));
            Assert.That(this._plugIn.GetTrial(player), Is.Null);
            Assert.That(trial.Monsters.Select(m => m.IsDisposed), Has.All.True);
        }

        Assert.That(character.CharacterClass?.Number, Is.EqualTo((byte)CharacterClassNumber.DragonKnight));
        Assert.That(character.LevelUpPoints - points, Is.EqualTo(100));
        Assert.That(player.Inventory!.Items.Select(i => i.Definition?.Name.ValueInNeutralLanguage), Does.Contain("Scroll of Gray Oblivion"));
        await this.ReselectAsync(player).ConfigureAwait(false);
        Assert.That(this._plugIn.GetState(player).NextEvolution, Is.EqualTo(ClassEvolution.Fifth));
        Assert.That(await this.ExperienceAsync(player).ConfigureAwait(false), Is.GreaterThan(0));
    }

    /// <summary>
    /// A new class goes the same way: a third class rune wizard becomes a fourth class one.
    /// </summary>
    [Test]
    public async ValueTask NewClassEvolvesTooAsync()
    {
        var thirdRuneWizard = ClassCodes.FromCode(ClassCodes.GetCode(BaseClassCode.RuneWizard, ClassEvolution.Third))!.Value;
        var player = await this.CreateThirdClassAsync((CharacterClassNumber)thirdRuneWizard, 400, 400).ConfigureAwait(false);
        Mock.Get(player.ViewPlugIns.GetPlugIn<ISkillListViewPlugIn>()!).Invocations.Clear();
        await this.TalkAsync(player, 764).ConfigureAwait(false);
        await this.TalkAsync(player, 766).ConfigureAwait(false);

        foreach (var step in new short[] { 7, 8, 9 })
        {
            await new QuestStartAction().StartQuestAsync(player, Group, step).ConfigureAwait(false);
            await this.KillAllAsync(player, player).ConfigureAwait(false);
            await new QuestCompletionAction().CompleteQuestAsync(player, Group, step).ConfigureAwait(false);
        }

        Assert.That(player.SelectedCharacter!.CharacterClass?.GetEvolution(), Is.EqualTo(ClassEvolution.Fourth));
        Assert.That(player.SelectedCharacter.CharacterClass?.GetClassCode() >> 3, Is.EqualTo((int)BaseClassCode.RuneWizard));
        Mock.Get(player.ViewPlugIns.GetPlugIn<ISkillListViewPlugIn>()!).Verify(v => v.UpdateSkillListAsync(), Times.Once, "the skill list carries the tree of the new class");
    }

    /// <summary>
    /// A third class character below the level of the fourth evolution can't enter the trial nor start the quest; the
    /// steps start one after the other.
    /// </summary>
    [Test]
    public async ValueTask LevelRuleAndOrderAreCheckedAsync()
    {
        var player = await this.CreateThirdClassAsync(CharacterClassNumber.GrandMaster, 400, 399).ConfigureAwait(false);
        await this.TalkAsync(player, 764).ConfigureAwait(false);
        Assert.That(player.CurrentMap?.Definition.Number, Is.EqualTo(0), "799 stays in Lorencia");

        player.Attributes![Stats.MasterLevel] = 400;
        await this.TalkAsync(player, 764).ConfigureAwait(false);
        await this.TalkAsync(player, 766).ConfigureAwait(false);
        player.Attributes[Stats.MasterLevel] = 399;
        Assert.That(player.GetAvailableQuestsOfOpenedNpc(), Is.Empty);
        await new QuestStartAction().StartQuestAsync(player, Group, 7).ConfigureAwait(false);
        Assert.That(player.GetQuestState(Group)?.ActiveQuest, Is.Null);

        player.Attributes[Stats.MasterLevel] = 400;
        await new QuestStartAction().StartQuestAsync(player, Group, 8).ConfigureAwait(false);
        Assert.That(player.GetQuestState(Group)?.ActiveQuest, Is.Null, "8 before 7");
        await new QuestStartAction().StartQuestAsync(player, Group, 7).ConfigureAwait(false);
        Assert.That(player.GetQuestState(Group)?.ActiveQuest?.Number, Is.EqualTo((short)7));
    }

    /// <summary>
    /// A second class character can't take the trial of the fourth class.
    /// </summary>
    [Test]
    public async ValueTask SecondClassIsRefusedAsync()
    {
        var player = await this.CreateThirdClassAsync(CharacterClassNumber.BladeKnight, 400, 0).ConfigureAwait(false);
        await this.TalkAsync(player, 764).ConfigureAwait(false);
        Assert.That(player.CurrentMap?.Definition.Number, Is.EqualTo(0));
    }

    /// <summary>
    /// A third class character stops gaining experience at total level 800 until it evolves; the hold is a setting.
    /// </summary>
    [Test]
    public async ValueTask ExperienceIsHeldAtTheFourthEvolutionAsync()
    {
        var player = await this.CreateThirdClassAsync(CharacterClassNumber.HighElf, 400, 399).ConfigureAwait(false);
        Assert.That(await this.ExperienceAsync(player).ConfigureAwait(false), Is.GreaterThan(0), "799");

        player.Attributes![Stats.MasterLevel] = 400;
        Assert.That(await this.ExperienceAsync(player).ConfigureAwait(false), Is.Zero, "800");
        var state = this._plugIn.GetState(player);
        Assert.That(state, Is.EqualTo(new EvolutionQuestState(ClassEvolution.Fourth, 800, ProgressionLevelKey.Season21Total, true, true)));

        this._plugIn.Configuration = new EvolutionQuestConfiguration { HoldExperienceUntilFourthEvolution = false };
        Assert.That(await this.ExperienceAsync(player).ConfigureAwait(false), Is.GreaterThan(0), "setting off");
    }

    /// <summary>
    /// A class whose next class has no evolution quest is never held.
    /// </summary>
    [Test]
    public async ValueTask ClassWithoutQuestIsNotHeldAsync()
    {
        var player = await this.CreateThirdClassAsync(CharacterClassNumber.DarkKnight, 400, 400).ConfigureAwait(false);
        Assert.That(this._plugIn.GetState(player).NextEvolution, Is.Null);
        Assert.That(await this.ExperienceAsync(player).ConfigureAwait(false), Is.GreaterThan(0));
    }

    /// <summary>
    /// The kill of a party member counts for the player whose trial it is.
    /// </summary>
    [Test]
    public async ValueTask PartyMemberKillsCountAsync()
    {
        var owner = await this.CreateThirdClassAsync(CharacterClassNumber.LordEmperor, 400, 400).ConfigureAwait(false);
        var helper = await this.CreateThirdClassAsync(CharacterClassNumber.DuelMaster, 400, 400).ConfigureAwait(false);
        var party = new Party(new PartyManager(5, new NullLogger<Party>()), 5, new NullLogger<Party>());
        await party.AddAsync(owner).ConfigureAwait(false);
        await party.AddAsync(helper).ConfigureAwait(false);
        await this.TalkAsync(owner, 764).ConfigureAwait(false);
        await this.TalkAsync(helper, 764).ConfigureAwait(false);
        await this.TalkAsync(owner, 766).ConfigureAwait(false);
        await new QuestStartAction().StartQuestAsync(owner, Group, 7).ConfigureAwait(false);

        await this.KillAllAsync(owner, helper).ConfigureAwait(false);
        await new QuestCompletionAction().CompleteQuestAsync(owner, Group, 7).ConfigureAwait(false);

        Assert.That(owner.GetQuestState(Group)?.LastFinishedQuest?.Number, Is.EqualTo((short)7));
    }

    /// <summary>
    /// The trial ends when its player leaves the map; its monsters leave too.
    /// </summary>
    [Test]
    public async ValueTask LeavingTheMapEndsTheTrialAsync()
    {
        var player = await this.CreateThirdClassAsync(CharacterClassNumber.FistMaster, 400, 400).ConfigureAwait(false);
        await this.TalkAsync(player, 764).ConfigureAwait(false);
        await this.TalkAsync(player, 766).ConfigureAwait(false);
        await new QuestStartAction().StartQuestAsync(player, Group, 7).ConfigureAwait(false);
        var trial = this._plugIn.GetTrial(player)!;

        await player.WarpToSafezoneAsync().ConfigureAwait(false);

        Assert.That(this._plugIn.GetTrial(player), Is.Null);
        Assert.That(trial.Monsters.Select(m => m.IsDisposed), Has.All.True);
    }

    private async ValueTask KillAllAsync(Player owner, Player killer)
    {
        var trial = this._plugIn.GetTrial(owner)!;
        foreach (var requirement in owner.GetQuestState(Group)!.ActiveQuest!.RequiredMonsterKills)
        {
            var monster = trial.Monsters.First(m => Equals(m.Definition, requirement.Monster));
            for (var i = 0; i < requirement.MinimumNumber; i++)
            {
                if (killer != owner)
                {
                    await this._plugIn.AttackableGotKilledAsync(monster, killer).ConfigureAwait(false);
                }

                await this._killCounter.AttackableGotKilledAsync(monster, killer).ConfigureAwait(false);
            }
        }
    }

    private async ValueTask<double> ExperienceAsync(Player player)
    {
        var killed = new Mock<IAttackable>();
        killed.Setup(k => k.Attributes).Returns(new Mock<AttributeSystem.IAttributeSystem>().Object);
        var args = new ExperienceCalculationArgs(killed.Object, true, 1000);
        await this._plugIn.CalculateExperienceAsync(player, args).ConfigureAwait(false);
        return args.Experience;
    }

    private async ValueTask TalkAsync(Player player, short npcNumber)
    {
        if (player.PlayerState.CurrentState == PlayerState.NpcDialogOpened)
        {
            await player.PlayerState.TryAdvanceToAsync(PlayerState.EnteredWorld).ConfigureAwait(false);
        }

        var npc = player.CurrentMap!.GetNpcsInRange(player.Position, 255).First(n => n.Definition.Number == npcNumber);
        await new TalkNpcAction().TalkToNpcAsync(player, npc).ConfigureAwait(false);
        if (player.PlayerState.CurrentState == PlayerState.ChangingMap || player.CurrentMap is null)
        {
            await player.ClientReadyAfterMapChangeAsync().ConfigureAwait(false);
        }
    }

    private async ValueTask ReselectAsync(Player player)
    {
        var character = player.SelectedCharacter!;
        await player.SetSelectedCharacterAsync(null).ConfigureAwait(false);
        await player.PlayerState.TryAdvanceToAsync(PlayerState.CharacterSelection).ConfigureAwait(false);
        await player.SetSelectedCharacterAsync(character).ConfigureAwait(false);
    }

    private async ValueTask<Player> CreateThirdClassAsync(CharacterClassNumber number, int level, int masterLevel)
    {
        var player = new InGamePlayer(this._gameContext);
        player.Account = player.PersistenceContext.CreateNew<Account>();
        player.Account.LoginName = Guid.NewGuid().ToString("N")[..10];
        await player.PlayerState.TryAdvanceToAsync(PlayerState.LoginScreen).ConfigureAwait(false);
        await player.PlayerState.TryAdvanceToAsync(PlayerState.Authenticated).ConfigureAwait(false);
        await player.PlayerState.TryAdvanceToAsync(PlayerState.CharacterSelection).ConfigureAwait(false);
        await new CreateCharacterAction().CreateCharacterAsync(player, player.Account.LoginName, (byte)CharacterClassNumber.DarkKnight).ConfigureAwait(false);
        var character = player.Account.Characters.Single();
        character.CharacterClass = this._gameConfiguration.CharacterClasses.Single(c => c.Number == (byte)number);
        character.CurrentMap = this._gameConfiguration.Maps.Single(m => m.Number == 0 && m.Discriminator == 0);
        (character.PositionX, character.PositionY) = (132, 148);
        await player.SetSelectedCharacterAsync(character).ConfigureAwait(false);
        await player.PlayerState.TryAdvanceToAsync(PlayerState.EnteredWorld).ConfigureAwait(false);
        player.Attributes![Stats.Level] = level;
        player.Attributes[Stats.MasterLevel] = masterLevel;
        return player;
    }

    private sealed class InGamePlayer : Player
    {
        public InGamePlayer(IGameContext gameContext)
            : base(gameContext)
        {
        }

        protected override ICustomPlugInContainer<IViewPlugIn> CreateViewPlugInContainer() => new MockViewPlugInContainer();
    }
}
