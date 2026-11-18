// <copyright file="Season21SkillsInGameTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Skills;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.EnhanceTree;
using MUnique.OpenMU.GameLogic.PlayerActions.Character;
using MUnique.OpenMU.GameLogic.PlayerActions.Skills;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Resets;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.Character;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.Persistence.Initialization.PlugIns.CharacterCreated;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.Pathfinding;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Tests that every class of Season 21 learns and uses a skill of the Season 21 data, and the skill enhancement tree.
/// </summary>
[TestFixture]
public class Season21SkillsInGameTest
{
    /// <summary>
    /// The first skill of each class which starts with one (S21Emu <c>CharacterSkillInit.xml</c>).
    /// </summary>
    private static readonly Dictionary<BaseClassCode, short> FirstSkillOfClass = new()
    {
        { BaseClassCode.GrowLancer, 275 },
        { BaseClassCode.RuneWizard, 240 },
        { BaseClassCode.Slayer, 44 },
        { BaseClassCode.LightWizard, 240 },
        { BaseClassCode.LemuriaMage, 240 },
        { BaseClassCode.IllusionKnight, 44 },
        { BaseClassCode.Alchemist, 240 },
        { BaseClassCode.Crusader, 74 },
    };

    /// <summary>
    /// The skill each class learns and uses without a weapon: its first skill, else (crescent moon slash needs a weapon,
    /// the gun crusher starts without a skill) its first area skill of the Season 21 data.
    /// </summary>
    private static readonly Dictionary<BaseClassCode, short> UsedSkillOfClass = new()
    {
        { BaseClassCode.GrowLancer, 275 },
        { BaseClassCode.RuneWizard, 240 },
        { BaseClassCode.Slayer, 292 },
        { BaseClassCode.GunCrusher, 2002 },
        { BaseClassCode.LightWizard, 240 },
        { BaseClassCode.LemuriaMage, 240 },
        { BaseClassCode.IllusionKnight, 2028 },
        { BaseClassCode.Alchemist, 240 },
        { BaseClassCode.Crusader, 74 },
    };

    private GameConfiguration _gameConfiguration = null!;

    private IGameContext _gameContext = null!;

    /// <summary>
    /// Sets up a Season 6 configuration with the classes and skills of Season 21.
    /// </summary>
    [OneTimeSetUp]
    public async ValueTask SetupAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        await new DataInitialization(contextProvider, new NullLoggerFactory()).CreateInitialDataAsync(0, false).ConfigureAwait(false);
        using var context = contextProvider.CreateNewConfigurationContext();
        this._gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, this._gameConfiguration).ConfigureAwait(false);
        await new AddSeason21SkillsUpdatePlugIn().ApplyUpdateAsync(context, this._gameConfiguration).ConfigureAwait(false);
        await new FixSeason21GrowLancerBuffsUpdatePlugIn().ApplyUpdateAsync(context, this._gameConfiguration).ConfigureAwait(false);
        await new FixSeason21DarkKnightBuffsUpdatePlugIn().ApplyUpdateAsync(context, this._gameConfiguration).ConfigureAwait(false);

        var mapInitializer = new MapInitializer(this._gameConfiguration, new NullLogger<MapInitializer>(), NullDropGenerator.Instance, null);
        var plugInManager = new PlugInManager(null, new NullLoggerFactory(), null, null);
        plugInManager.RegisterPlugInAtPlugInPoint<ICharacterCreatedPlugIn>(new AddSeason21InitialSkillsPlugIn());
        var gameContext = new GameContext(this._gameConfiguration, contextProvider, mapInitializer, new NullLoggerFactory(), plugInManager, NullDropGenerator.Instance, new ConfigurationChangeMediator());
        mapInitializer.PlugInManager = gameContext.PlugInManager;
        mapInitializer.PathFinderPool = gameContext.PathFinderPool;
        gameContext.FeaturePlugIns.AddPlugIn(new EnhanceTreeFeaturePlugIn { Configuration = Season21SkillsInitializer.CreateEnhanceTreeConfiguration() }, true);
        this._gameContext = gameContext;
    }

    /// <summary>
    /// A new character of each class starts with its first skills.
    /// </summary>
    [Test]
    public async ValueTask NewCharactersGetTheirFirstSkillsAsync()
    {
        foreach (var (baseClass, skillNumber) in FirstSkillOfClass)
        {
            var player = await this.CreatePlayerAsync(baseClass, ClassEvolution.First).ConfigureAwait(false);

            Assert.That(player.SkillList!.ContainsSkill((ushort)skillNumber), Is.True, baseClass.ToString());
        }
    }

    /// <summary>
    /// Each class of Season 21 learns its skill and uses it: the skill is accepted and takes its mana.
    /// </summary>
    [Test]
    public async ValueTask EveryNewClassLearnsAndUsesASkillAsync()
    {
        foreach (var (baseClass, skillNumber) in UsedSkillOfClass)
        {
            var player = await this.CreatePlayerAsync(baseClass, ClassEvolution.First).ConfigureAwait(false);
            var skill = this._gameConfiguration.Skills.Single(s => s.Number == skillNumber);
            Assert.That(skill.QualifiedCharacters, Has.Member(player.SelectedCharacter!.CharacterClass), baseClass.ToString());
            if (!player.SkillList!.ContainsSkill((ushort)skillNumber))
            {
                await player.SkillList.AddLearnedSkillAsync(skill).ConfigureAwait(false);
            }

            await this.EnterWorldAsync(player).ConfigureAwait(false);
            Assert.That(player.CompliesRequirements(skill), Is.True, baseClass.ToString());
            var mana = player.Attributes![Stats.CurrentMana];
            var manaCost = skill.ConsumeRequirements.Where(r => r.Attribute == Stats.CurrentMana).Sum(r => r.MinimumValue);

            await UseAsync(player, skill).ConfigureAwait(false);

            Assert.That(manaCost, Is.GreaterThan(0), baseClass.ToString());
            Assert.That(player.Attributes[Stats.CurrentMana], Is.EqualTo(mana - manaCost).Within(0.01), $"{baseClass} {skill.Name}");
        }
    }

    /// <summary>
    /// A fourth evolution adds points to its tree: the first step costs the node's minimum, requirements and the free
    /// points are checked.
    /// </summary>
    [Test]
    public async ValueTask FourthEvolutionAddsTreePointsAsync()
    {
        var player = await this.CreatePlayerAsync(BaseClassCode.GrowLancer, ClassEvolution.Fourth).ConfigureAwait(false);
        player.Attributes![Stats.Level] = 400;
        player.Attributes[Stats.MasterLevel] = 420;
        var configuration = EnhanceTreeFeaturePlugIn.GetConfiguration(this._gameContext)!;
        var action = new AddEnhanceTreePointAction();

        Assert.That(EnhanceTreePoints.GetFree(player, configuration), Is.EqualTo(20));
        Assert.That(await action.AddPointAsync(player, 1007).ConfigureAwait(false), Is.EqualTo(EnhanceTreePointResult.Added));
        Assert.That(await action.AddPointAsync(player, 1003).ConfigureAwait(false), Is.EqualTo(EnhanceTreePointResult.Added));
        Assert.That(EnhanceTreePoints.GetFree(player, configuration), Is.EqualTo(4));
        Assert.That(await action.AddPointAsync(player, 1003).ConfigureAwait(false), Is.EqualTo(EnhanceTreePointResult.MaximumReached));
        Assert.That(await action.AddPointAsync(player, 1118).ConfigureAwait(false), Is.EqualTo(EnhanceTreePointResult.RequirementMissing));
        Assert.That(await action.AddPointAsync(player, 271).ConfigureAwait(false), Is.EqualTo(EnhanceTreePointResult.UnknownNode));
        Assert.That(player.SelectedCharacter!.LearnedSkills.Single(s => s.Skill!.Number == 1003).Level, Is.EqualTo(15));
    }

    /// <summary>
    /// The passives of the tree raise their attribute by the S21Emu formula of their level (Formula.txt section SkillTree:
    /// Add Max HP 152 "70*level" up to 30 points, 222 "2100+((level-30)*42)" above).
    /// </summary>
    [Test]
    public async ValueTask TreePassivesRaiseTheirAttributeAsync()
    {
        var player = await this.CreatePlayerAsync(BaseClassCode.DarkWizard, ClassEvolution.Fourth).ConfigureAwait(false);
        await this.EnterWorldAsync(player).ConfigureAwait(false);
        var health = player.Attributes![Stats.MaximumHealth];
        var damage = player.Attributes[Stats.WizardryBaseDmg];
        await player.SkillList!.AddLearnedSkillAsync(this._gameConfiguration.Skills.Single(s => s.Number == 1137)).ConfigureAwait(false);
        await player.SkillList.AddLearnedSkillAsync(this._gameConfiguration.Skills.Single(s => s.Number == 1142)).ConfigureAwait(false);
        var maxHp = player.SelectedCharacter!.LearnedSkills.Single(s => s.Skill!.Number == 1137);
        var magic = player.SelectedCharacter.LearnedSkills.Single(s => s.Skill!.Number == 1142);

        maxHp.Level = 10;
        magic.Level = 35;

        Assert.That(player.Attributes[Stats.MaximumHealth], Is.EqualTo(health + 700).Within(0.01));
        Assert.That(player.Attributes[Stats.WizardryBaseDmg], Is.EqualTo(damage + 390 + (5 * 8)).Within(0.01));

        maxHp.Level = 40;
        Assert.That(player.Attributes[Stats.MaximumHealth], Is.EqualTo(health + 2100 + (10 * 42)).Within(0.01));
    }

    /// <summary>
    /// "Add Impenetrable Defense" (1139) raises the basic defense (later client tooltip "Increases the basic DEF of the
    /// player by %d"; Formula.txt SkillTree 154 "15*level" up to 30 points, 223 "450+((level-30)*9)" above).
    /// </summary>
    [Test]
    public async ValueTask ImpenetrableDefenseRaisesTheBasicDefenseAsync()
    {
        var player = await this.CreatePlayerAsync(BaseClassCode.DarkWizard, ClassEvolution.Fourth).ConfigureAwait(false);
        await this.EnterWorldAsync(player).ConfigureAwait(false);
        var defense = player.Attributes![Stats.DefenseBase];
        await player.SkillList!.AddLearnedSkillAsync(this._gameConfiguration.Skills.Single(s => s.Number == 1139)).ConfigureAwait(false);
        var passive = player.SelectedCharacter!.LearnedSkills.Single(s => s.Skill!.Number == 1139);

        passive.Level = 10;
        Assert.That(player.Attributes[Stats.DefenseBase], Is.EqualTo(defense + 150).Within(0.01));
        Assert.That(player.Attributes[Stats.ImpenetrableDefense], Is.EqualTo(150).Within(0.01));

        passive.Level = 35;
        Assert.That(player.Attributes[Stats.DefenseBase], Is.EqualTo(defense + 450 + (5 * 9)).Within(0.01));
    }

    /// <summary>
    /// "Add Stat Point" (1138) gives its formula's points (153 "8+(level*0)": 8) on each enhancement, to spend on a stat
    /// (later client tooltip "Gain %d Stat Point(s) upon enhancement"; Webzen guide 42 "Add Variable Stat").
    /// </summary>
    [Test]
    public async ValueTask StatPointPassiveGivesPointsOnEachEnhancementAsync()
    {
        var player = await this.CreatePlayerAsync(BaseClassCode.DarkWizard, ClassEvolution.Fourth).ConfigureAwait(false);
        player.Attributes![Stats.Level] = 400;
        player.Attributes[Stats.MasterLevel] = 450;
        await this.LearnAsync(player, 1075, 20).ConfigureAwait(false);
        await this.LearnAsync(player, EnhanceTreeEffects.ExtSkillDamage, 20).ConfigureAwait(false);
        var points = player.SelectedCharacter!.LevelUpPoints;
        var action = new AddEnhanceTreePointAction();

        Assert.That(await action.AddPointAsync(player, 1138).ConfigureAwait(false), Is.EqualTo(EnhanceTreePointResult.Added));
        Assert.That(await action.AddPointAsync(player, 1138).ConfigureAwait(false), Is.EqualTo(EnhanceTreePointResult.Added));

        Assert.That(player.SelectedCharacter.LevelUpPoints, Is.EqualTo(points + 16));
        Assert.That(EnhanceTreeEffects.GetStatPoints(player), Is.EqualTo(16));

        // The client sees the new free points at once: the level update carries them (base stats don't).
        Mock.Get(player.ViewPlugIns.GetPlugIn<IUpdateLevelPlugIn>()!).Verify(p => p.UpdateLevelAsync(), Times.Exactly(2));
    }

    /// <summary>
    /// A reset which sets the points anew keeps the stat points of the tree passives, like those of the quests: the
    /// passives stay learned and can't give them twice.
    /// </summary>
    [Test]
    public async ValueTask ResetKeepsTheStatPointsOfTheTreeAsync()
    {
        var player = await this.CreatePlayerAsync(BaseClassCode.DarkWizard, ClassEvolution.Fourth).ConfigureAwait(false);
        await this.LearnAsync(player, 1138, 2).ConfigureAwait(false);

        player.ApplyResetPoints(new ResetConfiguration { ResetStats = true, ReplacePointsPerReset = true }, new ResetProgression(1, 0, 0, 500, 500));

        Assert.That(player.SelectedCharacter!.LevelUpPoints, Is.EqualTo(500 + 16));
        Assert.That(EnhanceTreeEffects.GetStatPoints(player), Is.EqualTo(16));
    }

    /// <summary>
    /// "Add Ext Skill Speed" (1008, formula 54 = level) of the Dark Wizard's second sheet (Evil Spirit 9, extension 1078)
    /// raises the attack speed with which the server checks that skill (later client tooltip "Increases the attack speed
    /// of the connected skill by %d").
    /// </summary>
    [Test]
    public async ValueTask SpeedNodeRaisesTheSpeedOfTheImprovedSkillAsync()
    {
        var player = await this.CreatePlayerAsync(BaseClassCode.DarkWizard, ClassEvolution.Fourth).ConfigureAwait(false);
        await this.EnterWorldAsync(player).ConfigureAwait(false);
        var evilSpirit = this._gameConfiguration.Skills.Single(s => s.Number == 9);
        var speedConfiguration = new SpeedHackDetectConfiguration();
        var speed = AttackSpeedModel.Resolve(speedConfiguration, player, evilSpirit).Speed;
        await this.LearnAsync(player, 1008, 7).ConfigureAwait(false);
        Assert.That(AttackSpeedModel.Resolve(speedConfiguration, player, evilSpirit).Speed, Is.EqualTo(speed), "the extension isn't learned");

        await this.LearnAsync(player, 1078, 1).ConfigureAwait(false);

        Assert.That(AttackSpeedModel.Resolve(speedConfiguration, player, evilSpirit).Speed, Is.EqualTo(speed + 7));
        Assert.That(AttackSpeedModel.Resolve(speedConfiguration, player, this._gameConfiguration.Skills.Single(s => s.Number == 724)).Speed, Is.EqualTo(speed), "another sheet");
    }

    /// <summary>
    /// "Add Ext Skill Damage" (1001) of the Dark Wizard's first sheet adds formula 44 at its level (20: 600) to Meteor
    /// Strike (724), once its extension (1075) is learned.
    /// </summary>
    [Test]
    public async ValueTask ExtSkillDamageRaisesTheImprovedSkillAsync()
    {
        var player = await this.CreatePlayerAsync(BaseClassCode.DarkWizard, ClassEvolution.Fourth).ConfigureAwait(false);
        var meteorStrike = this._gameConfiguration.Skills.Single(s => s.Number == 724);
        await player.SkillList!.AddLearnedSkillAsync(meteorStrike).ConfigureAwait(false);
        await player.SkillList.AddLearnedSkillAsync(this._gameConfiguration.Skills.Single(s => s.Number == EnhanceTreeEffects.ExtSkillDamage)).ConfigureAwait(false);
        player.SelectedCharacter!.LearnedSkills.Single(s => s.Skill!.Number == EnhanceTreeEffects.ExtSkillDamage).Level = 20;
        Assert.That(EnhanceTreeEffects.GetExtraSkillDamage(player, meteorStrike), Is.Zero, "the extension isn't learned");

        await player.SkillList.AddLearnedSkillAsync(this._gameConfiguration.Skills.Single(s => s.Number == 1075)).ConfigureAwait(false);

        Assert.That(EnhanceTreeEffects.GetExtraSkillDamage(player, meteorStrike), Is.EqualTo(600).Within(1));
    }

    /// <summary>
    /// "Add Ext Skill Damage Count 1" (1002, learned at 15) of the Dark Wizard's first sheet gives Meteor Strike (724) one
    /// hit more, once its extension (1075) is learned; the sheet has no splash node.
    /// </summary>
    [Test]
    public async ValueTask DamageCountAddsAHitToTheImprovedSkillAsync()
    {
        var player = await this.CreatePlayerAsync(BaseClassCode.DarkWizard, ClassEvolution.Fourth).ConfigureAwait(false);
        var meteorStrike = this._gameConfiguration.Skills.Single(s => s.Number == 724);
        await player.SkillList!.AddLearnedSkillAsync(meteorStrike).ConfigureAwait(false);
        await player.SkillList.AddLearnedSkillAsync(this._gameConfiguration.Skills.Single(s => s.Number == EnhanceTreeEffects.DamageCount1)).ConfigureAwait(false);
        player.SelectedCharacter!.LearnedSkills.Single(s => s.Skill!.Number == EnhanceTreeEffects.DamageCount1).Level = 15;
        Assert.That(EnhanceTreeEffects.GetExtraHits(player, meteorStrike), Is.Zero, "the extension isn't learned");

        await player.SkillList.AddLearnedSkillAsync(this._gameConfiguration.Skills.Single(s => s.Number == 1075)).ConfigureAwait(false);

        Assert.That(EnhanceTreeEffects.GetExtraHits(player, meteorStrike), Is.EqualTo(1));
        Assert.That(EnhanceTreeEffects.GetSplash(player, meteorStrike), Is.EqualTo((0, 0)));
    }

    /// <summary>
    /// A tree passive is no master tree skill: master points can't be spent on it.
    /// </summary>
    [Test]
    public async ValueTask TreePassivesTakeNoMasterPointsAsync()
    {
        var player = await this.CreatePlayerAsync(BaseClassCode.DarkWizard, ClassEvolution.Fourth).ConfigureAwait(false);
        player.SelectedCharacter!.MasterLevelUpPoints = 10;

        await new AddMasterPointAction().AddMasterPointAsync(player, 1137).ConfigureAwait(false);

        Assert.That(player.SelectedCharacter.LearnedSkills.Any(s => s.Skill!.Number == 1137), Is.False);
        Assert.That(player.SelectedCharacter.MasterLevelUpPoints, Is.EqualTo(10));
    }

    /// <summary>
    /// A third evolution has no tree.
    /// </summary>
    [Test]
    public async ValueTask ThirdEvolutionHasNoTreeAsync()
    {
        var player = await this.CreatePlayerAsync(BaseClassCode.GrowLancer, ClassEvolution.Third).ConfigureAwait(false);
        player.Attributes![Stats.Level] = 400;
        player.Attributes[Stats.MasterLevel] = 420;

        Assert.That(await new AddEnhanceTreePointAction().AddPointAsync(player, 1007).ConfigureAwait(false), Is.EqualTo(EnhanceTreePointResult.NotAvailable));
    }

    /// <summary>
    /// The Grow Lancer's Circle Shield (effect 216), Obsidian (217) and Wrath (424), cast on himself, put their effect on him.
    /// </summary>
    [Test]
    public async ValueTask GrowLancerBuffsPutTheirEffectOnTheCasterAsync()
    {
        var player = await this.CreatePlayerAsync(BaseClassCode.GrowLancer, ClassEvolution.Fourth).ConfigureAwait(false);
        await this.EnterWorldAsync(player).ConfigureAwait(false);
        player.Attributes![Stats.BaseStrength] = 400;
        foreach (var (skill, effect) in new (short, short)[] { (272, 216), (273, 217), (278, 424) })
        {
            await this.LearnAsync(player, skill, 0).ConfigureAwait(false);
            player.Attributes[Stats.CurrentMana] = 1000;
            player.Attributes[Stats.CurrentAbility] = 200;

            await new TargetedSkillDefaultPlugin().PerformSkillAsync(player, player, (ushort)skill).ConfigureAwait(false);

            Assert.That(player.MagicEffectList.ContainsEffect(effect), Is.True, $"skill {skill}, effect {effect}");
        }
    }

    /// <summary>
    /// The Dark Knight's Sword's Fury, Firm Belief and Strong Blessing put their effect on the caster; under Sword's Fury an
    /// attack skill reaches 1 tile further, a buff does not.
    /// </summary>
    [Test]
    public async ValueTask DarkKnightBuffsPutTheirEffectOnTheCasterAsync()
    {
        var player = await this.CreatePlayerAsync(BaseClassCode.DarkKnight, ClassEvolution.Fourth).ConfigureAwait(false);
        await this.EnterWorldAsync(player).ConfigureAwait(false);
        var attack = this._gameConfiguration.Skills.First(s => s.Number == 19);
        var buff = this._gameConfiguration.Skills.Single(s => s.Number == 1502);
        var before = EnhanceTreeEffects.GetReach(player, attack).Range;
        foreach (var (skill, effect) in new (short, short)[] { (1500, 324), (1502, 325), (1503, 326) })
        {
            await this.LearnAsync(player, skill, 0).ConfigureAwait(false);
            player.Attributes![Stats.CurrentMana] = 2000;
            player.Attributes[Stats.CurrentAbility] = 500;

            await new TargetedSkillDefaultPlugin().PerformSkillAsync(player, player, (ushort)skill).ConfigureAwait(false);

            Assert.That(player.MagicEffectList.ContainsEffect(effect), Is.True, $"skill {skill}, effect {effect}");
        }

        Assert.That(EnhanceTreeEffects.GetReach(player, attack).Range, Is.EqualTo(before + 1));
        Assert.That(EnhanceTreeEffects.GetReach(player, buff).Range, Is.Zero);
    }

    private static async ValueTask UseAsync(Player player, Skill skill)
    {
        if (skill.SkillType is SkillType.AreaSkillAutomaticHits or SkillType.AreaSkillExplicitHits or SkillType.AreaSkillExplicitTarget)
        {
            await new AreaSkillAttackAction().AttackAsync(player, 0, (ushort)skill.Number, player.Position, 0).ConfigureAwait(false);
            return;
        }

        var target = new Mock<IAttackable>();
        target.Setup(t => t.IsAlive).Returns(true);
        target.Setup(t => t.Position).Returns(player.Position);
        target.Setup(t => t.Attributes).Returns(new Mock<IAttributeSystem>().Object);
        await new TargetedSkillDefaultPlugin().PerformSkillAsync(player, target.Object, (ushort)skill.Number).ConfigureAwait(false);
    }

    private async ValueTask LearnAsync(Player player, short skillNumber, int level)
    {
        await player.SkillList!.AddLearnedSkillAsync(this._gameConfiguration.Skills.Single(s => s.Number == skillNumber)).ConfigureAwait(false);
        player.SelectedCharacter!.LearnedSkills.Single(s => s.Skill!.Number == skillNumber).Level = level;
    }

    private async ValueTask EnterWorldAsync(Player player)
    {
        player.Attributes![Stats.Level] = 400;
        player.Attributes[Stats.BaseEnergy] = 2000;
        player.Attributes[Stats.CurrentMana] = 1000;
        player.Attributes[Stats.CurrentAbility] = 200;
        player.CurrentMap ??= await this._gameContext.GetMapAsync(0).ConfigureAwait(false);
        player.Position = new Point(180, 90);
        await player.PlayerState.TryAdvanceToAsync(PlayerState.EnteredWorld).ConfigureAwait(false);
        Assert.That(player.IsAtSafezone(), Is.False, $"{player.SelectedCharacter!.CharacterClass!.Name} {player.CurrentMap?.Definition.Number} {player.Position}");
    }

    private async ValueTask<Player> CreatePlayerAsync(BaseClassCode baseClass, ClassEvolution evolution)
    {
        var player = new InGamePlayer(this._gameContext);
        player.Account = player.PersistenceContext.CreateNew<Account>();
        player.Account.LoginName = Guid.NewGuid().ToString("N")[..10];
        await player.PlayerState.TryAdvanceToAsync(PlayerState.LoginScreen).ConfigureAwait(false);
        await player.PlayerState.TryAdvanceToAsync(PlayerState.Authenticated).ConfigureAwait(false);
        await player.PlayerState.TryAdvanceToAsync(PlayerState.CharacterSelection).ConfigureAwait(false);
        var first = ClassCodes.FromCode(ClassCodes.GetCode(baseClass, ClassEvolution.First))!.Value;
        await new CreateCharacterAction().CreateCharacterAsync(player, "S" + Guid.NewGuid().ToString("N")[..8], first).ConfigureAwait(false);
        var character = player.Account.Characters.Single();
        character.CharacterClass = this._gameConfiguration.FindByClassCode(ClassCodes.GetCode(baseClass, evolution));
        await player.SetSelectedCharacterAsync(character).ConfigureAwait(false);
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
