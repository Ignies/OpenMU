// <copyright file="Season21ClassesInGameTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Classes;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.PlayerActions.Character;
using MUnique.OpenMU.GameLogic.PlugIns.ChatCommands;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.Character;
using MUnique.OpenMU.GameServer.MessageHandler.Babylon;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Creates and evolves characters of the Season 21 classes in an in-memory game.
/// </summary>
[TestFixture]
public class Season21ClassesInGameTest
{
    private GameConfiguration _gameConfiguration = null!;

    private IGameContext _gameContext = null!;

    /// <summary>
    /// Initializes the Season 6 data and applies the update of the Season 21 classes.
    /// </summary>
    [OneTimeSetUp]
    public async ValueTask SetupAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        await new DataInitialization(contextProvider, new NullLoggerFactory()).CreateInitialDataAsync(0, false).ConfigureAwait(false);
        using var context = contextProvider.CreateNewConfigurationContext();
        this._gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, this._gameConfiguration).ConfigureAwait(false);

        var mapInitializer = new MapInitializer(this._gameConfiguration, new NullLogger<MapInitializer>(), NullDropGenerator.Instance, null);
        var gameContext = new GameContext(this._gameConfiguration, contextProvider, mapInitializer, new NullLoggerFactory(), new PlugInManager(null, new NullLoggerFactory(), null, null), NullDropGenerator.Instance, new ConfigurationChangeMediator());
        mapInitializer.PlugInManager = gameContext.PlugInManager;
        mapInitializer.PathFinderPool = gameContext.PathFinderPool;
        this._gameContext = gameContext;
    }

    /// <summary>
    /// Each of the nine classes gets created and starts with the health and mana of the S21Emu data.
    /// </summary>
    [Test]
    public async ValueTask EveryNewClassGetsCreatedAsync()
    {
        foreach (var info in Season21ClassData.Classes)
        {
            var player = await this.CreatePlayerAtCharacterSelectionAsync().ConfigureAwait(false);
            var number = ClassCodes.FromCode(ClassCodes.GetCode(info.BaseClass, ClassEvolution.First))!.Value;

            await new CreateCharacterAction().CreateCharacterAsync(player, $"Test{(int)info.BaseClass}", number).ConfigureAwait(false);

            var character = player.Account!.Characters.Single();
            Assert.That(character.CharacterClass?.Number, Is.EqualTo(number), info.BaseClass.ToString());
            await player.SetSelectedCharacterAsync(character).ConfigureAwait(false);
            Assert.That(player.Attributes![Stats.MaximumHealth], Is.EqualTo(info.MaxLife).Within(0.01), info.BaseClass.ToString());
            Assert.That(player.Attributes[Stats.MaximumMana], Is.EqualTo(info.MaxMana).Within(0.01), info.BaseClass.ToString());
            Assert.That(player.Attributes[Stats.TotalStrength], Is.EqualTo(info.Strength), info.BaseClass.ToString());
            Assert.That(player.Attributes[Stats.MaximumPhysBaseDmg], Is.GreaterThan(0), info.BaseClass.ToString());
        }
    }

    /// <summary>
    /// A later evolution can't get created.
    /// </summary>
    [Test]
    public async ValueTask LaterEvolutionsDontGetCreatedAsync()
    {
        var player = await this.CreatePlayerAtCharacterSelectionAsync().ConfigureAwait(false);

        await new CreateCharacterAction().CreateCharacterAsync(player, "Mirage", (byte)CharacterClassNumber.MirageLancer).ConfigureAwait(false);
        await new CreateCharacterAction().CreateCharacterAsync(player, "SoulWiz", (byte)CharacterClassNumber.SoulWizard).ConfigureAwait(false);

        Assert.That(player.Account!.Characters, Is.Empty);
    }

    /// <summary>
    /// A creation level of a class which the Season 6 client has no unlock flag for is checked by the server: another
    /// character of the account needs the level, or the account has unlocked the class.
    /// </summary>
    [Test]
    public async ValueTask TheServerChecksTheCreationLevelOfNewClassesAsync()
    {
        var slayer = this.Class(CharacterClassNumber.Slayer);
        slayer.LevelRequirementByCreation = 200;
        try
        {
            var player = await this.CreatePlayerAtCharacterSelectionAsync().ConfigureAwait(false);
            await new CreateCharacterAction().CreateCharacterAsync(player, "Slayer1", slayer.Number).ConfigureAwait(false);
            Assert.That(player.Account!.Characters, Is.Empty);

            await new CreateCharacterAction().CreateCharacterAsync(player, "Knight1", (byte)CharacterClassNumber.DarkKnight).ConfigureAwait(false);
            var knight = player.Account.Characters.Single();
            knight.Attributes.Single(a => a.Definition == Stats.Level).Value = 200;
            await new CreateCharacterAction().CreateCharacterAsync(player, "Slayer2", slayer.Number).ConfigureAwait(false);
            Assert.That(player.Account.Characters.Select(c => c.CharacterClass), Does.Contain(slayer));

            var other = await this.CreatePlayerAtCharacterSelectionAsync().ConfigureAwait(false);
            other.Account!.UnlockedCharacterClasses.Add(slayer);
            await new CreateCharacterAction().CreateCharacterAsync(other, "Slayer3", slayer.Number).ConfigureAwait(false);
            Assert.That(other.Account.Characters.Select(c => c.CharacterClass), Does.Contain(slayer));
        }
        finally
        {
            slayer.LevelRequirementByCreation = 0;
        }
    }

    /// <summary>
    /// The Season 6 classes with an unlock flag keep their behaviour: the client checks their level, the server doesn't.
    /// </summary>
    [Test]
    public async ValueTask Season6UnlockClassesStayClientCheckedAsync()
    {
        var player = await this.CreatePlayerAtCharacterSelectionAsync().ConfigureAwait(false);

        await new CreateCharacterAction().CreateCharacterAsync(player, "Gladiator", (byte)CharacterClassNumber.MagicGladiator).ConfigureAwait(false);

        Assert.That(player.Account!.Characters.Select(c => c.CharacterClass?.Number), Is.EqualTo(new byte?[] { (byte)CharacterClassNumber.MagicGladiator }));
    }

    /// <summary>
    /// The evolution goes along the chain; the fourth evolution checks its level rule unless it's forced.
    /// </summary>
    [Test]
    public async ValueTask EvolutionFollowsTheChainAndItsRuleAsync()
    {
        var player = await this.CreatePlayerAtCharacterSelectionAsync().ConfigureAwait(false);
        await new CreateCharacterAction().CreateCharacterAsync(player, "Lancer", (byte)CharacterClassNumber.GrowLancer).ConfigureAwait(false);
        var character = player.Account!.Characters.Single();
        await player.SetSelectedCharacterAsync(character).ConfigureAwait(false);
        var action = new EvolveCharacterAction();

        Assert.That(action.TryEvolve(player, false), Is.True);
        Assert.That(character.CharacterClass?.Number, Is.EqualTo((byte)CharacterClassNumber.MirageLancer));

        await player.SetSelectedCharacterAsync(null).ConfigureAwait(false);
        await player.SetSelectedCharacterAsync(character).ConfigureAwait(false);
        player.Attributes![Stats.Level] = 400;
        Assert.That(action.TryEvolve(player, true), Is.False);
        Assert.That(character.CharacterClass?.Number, Is.EqualTo((byte)CharacterClassNumber.MirageLancer));

        player.Attributes[Stats.MasterLevel] = 400;
        Assert.That(action.TryEvolve(player, true), Is.True);
        Assert.That(character.CharacterClass?.Number, Is.EqualTo((byte)CharacterClassNumber.ShiningLancer));

        Assert.That(action.TryEvolve(player, false), Is.True);
        Assert.That(action.TryEvolve(player, false), Is.False);
        Assert.That(character.CharacterClass?.Number, Is.EqualTo((byte)CharacterClassNumber.ArcaneLancer));
    }

    /// <summary>
    /// A game master evolves a character with <c>/evolve</c>, without the level rules.
    /// </summary>
    [Test]
    public async ValueTask GameMasterCommandEvolvesAsync()
    {
        var player = await this.CreatePlayerAtCharacterSelectionAsync().ConfigureAwait(false);
        await new CreateCharacterAction().CreateCharacterAsync(player, "Crusader", (byte)CharacterClassNumber.Crusader).ConfigureAwait(false);
        var character = player.Account!.Characters.Single();
        character.CharacterStatus = CharacterStatus.GameMaster;
        await player.SetSelectedCharacterAsync(character).ConfigureAwait(false);

        var skillList = Mock.Get(player.ViewPlugIns.GetPlugIn<ISkillListViewPlugIn>()!);
        skillList.Invocations.Clear();

        await new EvolveChatCommandPlugIn().HandleCommandAsync(player, "/evolve").ConfigureAwait(false);

        Assert.That(character.CharacterClass?.Number, Is.EqualTo((byte)CharacterClassNumber.ImpactCrusader));
        skillList.Verify(v => v.UpdateSkillListAsync(), Times.Once, "the skill list carries the tree of the new class");
    }

    /// <summary>
    /// The Babylon client creates a character by its class code.
    /// </summary>
    [Test]
    public async ValueTask BabylonClientCreatesByClassCodeAsync()
    {
        var player = await this.CreatePlayerAtCharacterSelectionAsync().ConfigureAwait(false);
        var packet = new byte[CreateCharacterWide.Length];
        _ = new CreateCharacterWide(packet) { Name = "Alchemist", ClassCode = ClassCodes.GetCode(BaseClassCode.Alchemist, ClassEvolution.First) };

        await new CreateCharacterWideHandlerPlugIn().HandlePacketAsync(player, packet).ConfigureAwait(false);

        Assert.That(player.Account!.Characters.Single().CharacterClass?.Number, Is.EqualTo((byte)CharacterClassNumber.Alchemist));
    }

    private CharacterClass Class(CharacterClassNumber number) => this._gameConfiguration.CharacterClasses.Single(c => c.Number == (byte)number);

    private async ValueTask<Player> CreatePlayerAtCharacterSelectionAsync()
    {
        var player = new InGamePlayer(this._gameContext);
        player.Account = player.PersistenceContext.CreateNew<Account>();
        player.Account.LoginName = Guid.NewGuid().ToString("N")[..10];
        await player.PlayerState.TryAdvanceToAsync(PlayerState.LoginScreen).ConfigureAwait(false);
        await player.PlayerState.TryAdvanceToAsync(PlayerState.Authenticated).ConfigureAwait(false);
        await player.PlayerState.TryAdvanceToAsync(PlayerState.CharacterSelection).ConfigureAwait(false);
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
