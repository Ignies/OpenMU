// <copyright file="SkillBookRequirementTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Skills;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.PlayerActions.Character;
using MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Tests the master skill requirement of the skill books of Season 21 (S21Emu <c>SkillRequire.txt</c>): a book teaches
/// its skill only when the character has the required skill and the master skill with its points.
/// </summary>
[TestFixture]
public class SkillBookRequirementTest
{
    private const byte BookGroup = 12;
    private const short Book = 289;
    private const short BookSkill = 723;
    private const short RequiredSkill = 43;
    private const short RequiredMasterSkill = 339;

    private GameConfiguration _gameConfiguration = null!;

    private IGameContext _gameContext = null!;

    private LearnablesConsumeHandlerPlugIn _handler = null!;

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
        await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, this._gameConfiguration).ConfigureAwait(false);
        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, this._gameConfiguration).ConfigureAwait(false);
        await new AddSeason21SkillsUpdatePlugIn().ApplyUpdateAsync(context, this._gameConfiguration).ConfigureAwait(false);
        var plugIn = this._gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(LearnablesConsumeHandlerPlugIn).GUID);
        this._handler = new LearnablesConsumeHandlerPlugIn { Configuration = plugIn.GetConfiguration<LearnablesConsumeHandlerConfiguration>(new IdIgnoringReferenceHandler()) };

        var mapInitializer = new MapInitializer(this._gameConfiguration, new NullLogger<MapInitializer>(), NullDropGenerator.Instance, null);
        var plugInManager = new PlugInManager(null, new NullLoggerFactory(), null, null);
        var gameContext = new GameContext(this._gameConfiguration, contextProvider, mapInitializer, new NullLoggerFactory(), plugInManager, NullDropGenerator.Instance, new ConfigurationChangeMediator());
        mapInitializer.PlugInManager = gameContext.PlugInManager;
        mapInitializer.PathFinderPool = gameContext.PathFinderPool;
        this._gameContext = gameContext;
    }

    /// <summary>
    /// The update gives the books of <c>SkillRequire.txt</c> their requirements.
    /// </summary>
    [Test]
    public void BooksHaveTheirRequirements()
    {
        var requirements = this._handler.Configuration?.Requirements ?? [];

        Assert.That(requirements.Select(r => (r.ItemGroup, r.ItemNumber, r.RequiredSkill, r.RequiredMasterSkill, r.RequiredMasterSkillLevel)), Has.Member((BookGroup, Book, RequiredSkill, RequiredMasterSkill, (byte)10)));
        Assert.That(requirements, Has.Count.GreaterThanOrEqualTo(22));
    }

    /// <summary>
    /// Without the master skill at its points the book is kept and teaches nothing; with it, the skill is learned.
    /// </summary>
    [Test]
    public async ValueTask BookNeedsTheMasterSkillPointsAsync()
    {
        var player = await this.CreatePlayerAsync().ConfigureAwait(false);
        var book = await this.AddBookAsync(player).ConfigureAwait(false);
        await player.SkillList!.AddLearnedSkillAsync(this.Skill(RequiredSkill)).ConfigureAwait(false);
        await player.SkillList.AddLearnedSkillAsync(this.Skill(RequiredMasterSkill)).ConfigureAwait(false);
        var master = player.SelectedCharacter!.LearnedSkills.Single(s => s.Skill!.Number == RequiredMasterSkill);
        master.Level = 9;

        Assert.That(await this._handler.ConsumeItemAsync(player, book, null, FruitUsage.Undefined).ConfigureAwait(false), Is.False);
        Assert.That(player.SkillList.ContainsSkill((ushort)BookSkill), Is.False);

        master.Level = 10;
        Assert.That(await this._handler.ConsumeItemAsync(player, book, null, FruitUsage.Undefined).ConfigureAwait(false), Is.True);
        Assert.That(player.SkillList.ContainsSkill((ushort)BookSkill), Is.True);
    }

    private Skill Skill(short number) => this._gameConfiguration.Skills.Single(s => s.Number == number);

    private async ValueTask<Item> AddBookAsync(Player player)
    {
        var item = player.PersistenceContext.CreateNew<Item>();
        item.Definition = this._gameConfiguration.Items.Single(d => d.Group == BookGroup && d.Number == Book);
        item.Durability = 1;
        Assert.That(await player.Inventory!.AddItemAsync(item).ConfigureAwait(false), Is.True);
        return item;
    }

    private async ValueTask<Player> CreatePlayerAsync()
    {
        var player = new InGamePlayer(this._gameContext);
        player.Account = player.PersistenceContext.CreateNew<Account>();
        player.Account.LoginName = Guid.NewGuid().ToString("N")[..10];
        await player.PlayerState.TryAdvanceToAsync(PlayerState.LoginScreen).ConfigureAwait(false);
        await player.PlayerState.TryAdvanceToAsync(PlayerState.Authenticated).ConfigureAwait(false);
        await player.PlayerState.TryAdvanceToAsync(PlayerState.CharacterSelection).ConfigureAwait(false);
        var first = ClassCodes.FromCode(ClassCodes.GetCode(BaseClassCode.DarkKnight, ClassEvolution.First))!.Value;
        await new CreateCharacterAction().CreateCharacterAsync(player, "B" + Guid.NewGuid().ToString("N")[..8], first).ConfigureAwait(false);
        var character = player.Account.Characters.Single();
        character.CharacterClass = this._gameConfiguration.FindByClassCode(ClassCodes.GetCode(BaseClassCode.DarkKnight, ClassEvolution.Third));
        await player.SetSelectedCharacterAsync(character).ConfigureAwait(false);
        player.Attributes![Stats.Level] = 400;
        player.Attributes[Stats.BaseStrength] = 3000;
        player.Attributes[Stats.BaseAgility] = 3000;
        player.Attributes[Stats.BaseEnergy] = 3000;
        player.Attributes[Stats.BaseVitality] = 3000;
        await player.PlayerState.TryAdvanceToAsync(PlayerState.EnteredWorld).ConfigureAwait(false);
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
