// <copyright file="Season21WingPassivesInGameTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Skills;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.EnhanceTree;
using MUnique.OpenMU.GameLogic.PlayerActions.Character;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Tests the 4th wing passives of the skill enhancement tree: they add while a 4th wing (or cape) is worn (later client
/// tooltips "Increases the wing DEF by %d when 4th wings are equipped", "When Lv. 4 Wings(Cape) are equipped, increase
/// Wizardry by %d"; Formula.txt SkillTree 160, 155, 210-212 "7*level").
/// </summary>
[TestFixture]
public class Season21WingPassivesInGameTest
{
    private const short WingsOfElysium = 414;

    private const short ThirdWing = 36;

    private GameConfiguration _gameConfiguration = null!;

    private IGameContext _gameContext = null!;

    /// <summary>
    /// Sets up a Season 6 configuration with the classes, items and skills of Season 21.
    /// </summary>
    [OneTimeSetUp]
    public async ValueTask SetupAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        await new DataInitialization(contextProvider, new NullLoggerFactory()).CreateInitialDataAsync(0, false).ConfigureAwait(false);
        using var context = contextProvider.CreateNewConfigurationContext();
        this._gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        UpdatePlugInBase[] updates =
        [
            new AddSeason21ClassesUpdatePlugIn(),
            new AddSeason21GearUpdatePlugIn(),
            new AddSeason21ItemCatalogUpdatePlugIn(),
            new ActivateSeason21WideItemsUpdatePlugIn(),
            new AddSeason21SkillsUpdatePlugIn(),
        ];
        foreach (var update in updates)
        {
            await update.ApplyUpdateAsync(context, this._gameConfiguration).ConfigureAwait(false);
        }

        var mapInitializer = new MapInitializer(this._gameConfiguration, new NullLogger<MapInitializer>(), NullDropGenerator.Instance, null);
        var gameContext = new GameContext(this._gameConfiguration, contextProvider, mapInitializer, new NullLoggerFactory(), new PlugInManager(null, new NullLoggerFactory(), null, null), NullDropGenerator.Instance, new ConfigurationChangeMediator());
        mapInitializer.PlugInManager = gameContext.PlugInManager;
        mapInitializer.PathFinderPool = gameContext.PathFinderPool;
        gameContext.FeaturePlugIns.AddPlugIn(new EnhanceTreeFeaturePlugIn { Configuration = Season21SkillsInitializer.CreateEnhanceTreeConfiguration() }, true);
        this._gameContext = gameContext;
    }

    /// <summary>
    /// "Increase Defense of Lv. 4 Wings/Cape" (1140) and "Add 4th Wing Defense" of the Dark Wizard (1193, wizardry) add
    /// their value while a 4th wing is worn, nothing with a third wing.
    /// </summary>
    [Test]
    public async ValueTask WingPassivesAddWhileAFourthWingIsWornAsync()
    {
        var fourth = await this.CreatePlayerAsync(WingsOfElysium).ConfigureAwait(false);
        var third = await this.CreatePlayerAsync(ThirdWing).ConfigureAwait(false);
        var defense = fourth.Attributes![Stats.DefenseBase];
        var wizardry = fourth.Attributes[Stats.WizardryBaseDmg];
        var thirdDefense = third.Attributes![Stats.DefenseBase];

        foreach (var player in new[] { fourth, third })
        {
            await LearnAsync(player, 1140, 10).ConfigureAwait(false);
            await LearnAsync(player, 1193, 10).ConfigureAwait(false);
        }

        Assert.That(fourth.Attributes[Stats.DefenseBase], Is.EqualTo(defense + 70).Within(0.01));
        Assert.That(fourth.Attributes[Stats.WizardryBaseDmg], Is.EqualTo(wizardry + 70).Within(0.01));
        Assert.That(third.Attributes[Stats.DefenseBase], Is.EqualTo(thirdDefense).Within(0.01), "a third wing");
    }

    /// <summary>
    /// "Increase ATK of Lv. 4 Wings/Cape" (1145) of the Dark Knight raises the damage by its value with a 4th wing.
    /// </summary>
    [Test]
    public async ValueTask WingDamagePassiveRaisesTheDamageAsync()
    {
        var player = await this.CreatePlayerAsync(WingsOfElysium, BaseClassCode.DarkKnight).ConfigureAwait(false);
        var physical = player.Attributes![Stats.PhysicalBaseDmg];

        await LearnAsync(player, 1145, 10).ConfigureAwait(false);

        Assert.That(player.Attributes[Stats.PhysicalBaseDmg], Is.EqualTo(physical + 70).Within(0.01));
    }

    private static async ValueTask LearnAsync(Player player, short skillNumber, int level)
    {
        var skill = player.GameContext.Configuration.Skills.Single(s => s.Number == skillNumber);
        await player.SkillList!.AddLearnedSkillAsync(skill).ConfigureAwait(false);
        player.SelectedCharacter!.LearnedSkills.Single(s => s.Skill == skill).Level = level;
    }

    private async ValueTask<Player> CreatePlayerAsync(short wing, BaseClassCode baseClass = BaseClassCode.DarkWizard)
    {
        var player = new InGamePlayer(this._gameContext);
        player.Account = player.PersistenceContext.CreateNew<Account>();
        player.Account.LoginName = Guid.NewGuid().ToString("N")[..10];
        await player.PlayerState.TryAdvanceToAsync(PlayerState.LoginScreen).ConfigureAwait(false);
        await player.PlayerState.TryAdvanceToAsync(PlayerState.Authenticated).ConfigureAwait(false);
        await player.PlayerState.TryAdvanceToAsync(PlayerState.CharacterSelection).ConfigureAwait(false);
        var first = ClassCodes.FromCode(ClassCodes.GetCode(baseClass, ClassEvolution.First))!.Value;
        await new CreateCharacterAction().CreateCharacterAsync(player, "W" + Guid.NewGuid().ToString("N")[..8], first).ConfigureAwait(false);
        var character = player.Account.Characters.Single();
        character.CharacterClass = this._gameConfiguration.FindByClassCode(ClassCodes.GetCode(baseClass, ClassEvolution.Fourth));
        await player.SetSelectedCharacterAsync(character).ConfigureAwait(false);
        player.Attributes![Stats.Level] = 400;

        var item = player.PersistenceContext.CreateNew<Item>();
        item.Definition = this._gameConfiguration.Items.Single(i => i.Group == 12 && i.Number == wing);
        item.Durability = Math.Max((byte)1, item.Definition.Durability);
        Assert.That(await player.Inventory!.AddItemAsync(InventoryConstants.WingsSlot, item).ConfigureAwait(false), Is.True);
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
