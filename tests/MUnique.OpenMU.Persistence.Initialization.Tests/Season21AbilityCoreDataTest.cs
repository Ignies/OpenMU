// <copyright file="Season21AbilityCoreDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.AbilityCore;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Tests the <see cref="AddSeason21AbilityCoreUpdatePlugIn"/>: the cards, their options and prices of the Ability
/// Enhancement Core of Season 21.
/// </summary>
[TestFixture]
internal class Season21AbilityCoreDataTest
{
    private const byte CardGroup = 20;

    /// <summary>
    /// A new database has the core off and without cards, and the update stays pending.
    /// </summary>
    [Test]
    public async Task NewDatabaseLeavesTheUpdatePendingAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);

        var plugIn = gameConfiguration.PlugInConfigurations.FirstOrDefault(c => c.TypeId == typeof(AbilityCoreFeaturePlugIn).GUID);
        Assert.That(plugIn?.IsActive ?? false, Is.False);
        Assert.That(plugIn?.GetConfiguration<AbilityCoreConfiguration>(new IdIgnoringReferenceHandler())?.Cards ?? [], Is.Empty);
        var installed = (await context.GetAsync<ConfigurationUpdate>().ConfigureAwait(false)).Select(u => u.Key);
        Assert.That(installed, Has.None.EqualTo(new AddSeason21AbilityCoreUpdatePlugIn().Key));
    }

    /// <summary>
    /// The update fills and turns on the core: 45 cards with their kinds, the skill cards with their skills, the prices.
    /// </summary>
    [Test]
    public async Task CoreIsConfiguredFromTheDataAsync()
    {
        var (_, gameConfiguration) = await CreateWithCoreAsync().ConfigureAwait(false);

        var plugIn = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(AbilityCoreFeaturePlugIn).GUID);
        var configuration = plugIn.GetConfiguration<AbilityCoreConfiguration>(new IdIgnoringReferenceHandler())!;
        Assert.That(plugIn.IsActive, Is.True);
        Assert.That(configuration.Cards, Has.Count.EqualTo(45));
        Assert.That(configuration.Cards.Count(c => c.Kind == AbilityCardKind.Skill), Is.EqualTo(24));
        Assert.That(configuration.Cards.Count(c => c.Kind == AbilityCardKind.Option), Is.EqualTo(15));
        var meteor = configuration.Cards.Single(c => c.ItemNumber == 165);
        Assert.That((meteor.Skill, meteor.RequiredSkill, meteor.IsFor(BaseClassCode.DarkWizard), meteor.IsFor(BaseClassCode.DarkKnight)), Is.EqualTo(((short)2037, (short)1076, true, false)));
        var blue = configuration.Cards.Single(c => c.ItemNumber == 213);
        Assert.That((blue.Kind, blue.Color, blue.Type), Is.EqualTo((AbilityCardKind.Option, AbilityCardColor.Blue, (byte)1)));
        Assert.That(blue.Options.Single(o => o.Number == 19).AppliesTo(BaseClassCode.DarkWizard), Is.False);
        Assert.That(configuration.LevelUps.Single(l => l.Kind == AbilityCardKind.Skill && l.Level == 4).Cards, Is.EqualTo(2));
        Assert.That(configuration.Reductions.Single(r => r.Kind == AbilityCardKind.Elite).Stones, Is.EqualTo(5));
    }

    /// <summary>
    /// A card carries its options as item options whose value follows the card level.
    /// </summary>
    [Test]
    public async Task CardsCarryTheirOptionsAsync()
    {
        var (_, gameConfiguration) = await CreateWithCoreAsync().ConfigureAwait(false);

        var options = Options(gameConfiguration, 213);
        Assert.That(options, Has.Count.EqualTo(13));
        var minimum = options.Single(o => o.Number == 19);
        Assert.That(minimum.LevelType, Is.EqualTo(LevelType.ItemLevel));
        Assert.That(minimum.PowerUpDefinition!.TargetAttribute, Is.EqualTo(Stats.MinimumPhysBaseDmg));
        Assert.That(minimum.LevelDependentOptions.Single(l => l.Level == 5).PowerUpDefinition!.Boost!.ConstantValue.Value, Is.EqualTo(108));
        var rate = Options(gameConfiguration, 228).Single(o => o.Number == 50);
        Assert.That(rate.PowerUpDefinition!.TargetAttribute, Is.EqualTo(Stats.CriticalDamageChance));
        Assert.That(rate.LevelDependentOptions.Single(l => l.Level == 0).PowerUpDefinition!.Boost!.ConstantValue.Value, Is.EqualTo(0.02f).Within(0.0001));
        Assert.That(Options(gameConfiguration, 165).Select(o => o.PowerUpDefinition?.TargetAttribute), Is.EqualTo(new[] { Stats.MaximumHealth, Stats.MaximumMana, Stats.MaximumAbility }));
        Assert.That(Options(gameConfiguration, 213).Single(o => o.Number == 29).PowerUpDefinition, Is.Null);
    }

    /// <summary>
    /// Applying the update twice changes nothing, and an operator's changes are kept.
    /// </summary>
    [Test]
    public async Task UpdateIsIdempotentAndKeepsOperatorEditsAsync()
    {
        var (context, gameConfiguration) = await CreateWithCoreAsync().ConfigureAwait(false);
        var plugIn = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(AbilityCoreFeaturePlugIn).GUID);
        var configuration = plugIn.GetConfiguration<AbilityCoreConfiguration>(new IdIgnoringReferenceHandler())!;
        configuration.SkillSlotLevels[0] = 1150;
        plugIn.SetConfiguration(configuration, null);
        plugIn.IsActive = false;
        var minimum = Options(gameConfiguration, 213).Single(o => o.Number == 19);
        minimum.LevelDependentOptions.Single(l => l.Level == 5).PowerUpDefinition!.Boost!.ConstantValue.Value = 99;
        var definitions = gameConfiguration.ItemOptions.Count;

        await new AddSeason21AbilityCoreUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(gameConfiguration.ItemOptions, Has.Count.EqualTo(definitions));
        Assert.That(plugIn.GetConfiguration<AbilityCoreConfiguration>(new IdIgnoringReferenceHandler())!.SkillSlotLevels[0], Is.EqualTo(1150));
        Assert.That(plugIn.IsActive, Is.False);
        Assert.That(minimum.LevelDependentOptions.Single(l => l.Level == 5).PowerUpDefinition!.Boost!.ConstantValue.Value, Is.EqualTo(99));
    }

    /// <summary>
    /// The chaos goblin crafts the cards and the stones from Ability Crystals for the Webzen prices.
    /// </summary>
    /// <param name="number">The crafting number.</param>
    /// <param name="crystals">The crystals.</param>
    /// <param name="blesses">The jewels of bless.</param>
    /// <param name="money">The zen.</param>
    /// <param name="results">The result items, or some of them.</param>
    [TestCase((byte)180, 50, 10, 100_000_000, new short[] { 165, 168, 171, 172, 175, 177, 179, 182, 186, 188, 189, 191, 193, 196, 199, 201, 204, 208, 210, 234 })]
    [TestCase((byte)181, 5, 1, 10_000_000, new short[] { 213, 218, 223 })]
    [TestCase((byte)185, 5, 1, 10_000_000, new short[] { 217, 222, 227 })]
    [TestCase((byte)186, 25, 5, 50_000_000, new short[] { 228 })]
    [TestCase((byte)188, 25, 5, 50_000_000, new short[] { 230 })]
    [TestCase((byte)189, 25, 5, 50_000_000, new short[] { 239 })]
    public async Task CardsAreCraftedAtTheChaosGoblinAsync(byte number, int crystals, int blesses, int money, short[] results)
    {
        var (_, gameConfiguration) = await CreateWithCoreAsync().ConfigureAwait(false);

        var goblin = gameConfiguration.Monsters.Single(m => m.NpcWindow == NpcWindow.ChaosMachine);
        Assert.That(goblin.ItemCraftings.Select(c => c.Number), Does.Contain(number));
        var crafting = goblin.ItemCraftings.Single(c => c.Number == number);
        var settings = crafting.SimpleCraftingSettings!;
        Assert.That(crafting.ItemCraftingHandlerClassName, Is.EqualTo("MUnique.OpenMU.GameLogic.PlayerActions.Craftings.AbilityCardCrafting"));
        Assert.That((settings.Money, settings.SuccessPercent, settings.ResultItemSelect), Is.EqualTo((money, (byte)100, ResultItemSelection.Any)));
        var crystal = settings.RequiredItems.Single(r => r.PossibleItems.Any(i => i.Group == CardGroup));
        Assert.That(crystal.PossibleItems.Select(i => i.Number), Is.EquivalentTo(new short[] { 164, 238 }));
        Assert.That((crystal.MinimumAmount, crystal.MaximumAmount), Is.EqualTo(((byte)crystals, (byte)crystals)));
        var bless = settings.RequiredItems.Single(r => r.PossibleItems.Any(i => i.Group == 14 && i.Number == 13));
        Assert.That((bless.MinimumAmount, bless.MaximumAmount), Is.EqualTo(((byte)blesses, (byte)blesses)));
        Assert.That(settings.ResultItems.Select(r => r.ItemDefinition!.Number), Is.SupersetOf(results));
        Assert.That(settings.ResultItems, Has.Count.EqualTo(number == 180 ? 24 : results.Length));
        Assert.That(settings.ResultItems.Single(r => r.ItemDefinition!.Number == results[0]).Durability, Is.EqualTo(number == 189 ? (byte)5 : (byte)1));
    }

    /// <summary>
    /// Version 2 adds the craftings which are missing and keeps the edited ones.
    /// </summary>
    [Test]
    public async Task VersionTwoAddsTheCraftingsAndKeepsTheirEditsAsync()
    {
        var (context, gameConfiguration) = await CreateWithCoreAsync().ConfigureAwait(false);
        var goblin = gameConfiguration.Monsters.Single(m => m.NpcWindow == NpcWindow.ChaosMachine);
        Assert.That(goblin.ItemCraftings.Select(c => c.Number), Is.SupersetOf(new byte[] { 180, 181 }));
        var skillCard = goblin.ItemCraftings.Single(c => c.Number == 180);
        skillCard.SimpleCraftingSettings!.Money = 1234;
        goblin.ItemCraftings.Remove(goblin.ItemCraftings.Single(c => c.Number == 181));
        var count = goblin.ItemCraftings.Count;

        await new AddSeason21AbilityCoreUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(new AddSeason21AbilityCoreUpdatePlugIn().Version, Is.GreaterThanOrEqualTo(2));
        Assert.That(goblin.ItemCraftings, Has.Count.EqualTo(count + 1));
        Assert.That(goblin.ItemCraftings.Single(c => c.Number == 180).SimpleCraftingSettings!.Money, Is.EqualTo(1234));
    }

    /// <summary>
    /// The extra defense, the pentagram defense and the pentagram attack options carry the attribute OpenMU has for them;
    /// version 3 gives them to an existing database whose options had none, and keeps the edited ones.
    /// </summary>
    [Test]
    public async Task VersionThreeMapsTheExtraDefenseAndPentagramOptionsAsync()
    {
        var (context, gameConfiguration) = await CreateWithCoreAsync().ConfigureAwait(false);
        var extraDefense = Options(gameConfiguration, 213).Single(o => o.Number == 3);
        Assert.That(extraDefense.PowerUpDefinition?.TargetAttribute, Is.EqualTo(Stats.DefenseBase));
        Assert.That(extraDefense.LevelDependentOptions.Single(l => l.Level == 5).PowerUpDefinition!.Boost!.ConstantValue.Value, Is.EqualTo(175));
        Assert.That(Options(gameConfiguration, 216).Single(o => o.Number == 10).PowerUpDefinition!.TargetAttribute, Is.EqualTo(Stats.ElementDefense));
        Assert.That(Options(gameConfiguration, 215).Single(o => o.Number == 47).PowerUpDefinition!.TargetAttribute, Is.EqualTo(Stats.ElementDamageBonus));

        var pentagramAttack = Options(gameConfiguration, 220).Single(o => o.Number == 47);
        pentagramAttack.PowerUpDefinition = null;
        pentagramAttack.LevelDependentOptions.Clear();
        extraDefense.LevelDependentOptions.Single(l => l.Level == 5).PowerUpDefinition!.Boost!.ConstantValue.Value = 99;

        await new AddSeason21AbilityCoreUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(new AddSeason21AbilityCoreUpdatePlugIn().Version, Is.EqualTo(3));
        Assert.That(pentagramAttack.PowerUpDefinition!.TargetAttribute, Is.EqualTo(Stats.ElementDamageBonus));
        Assert.That(pentagramAttack.LevelDependentOptions.Single(l => l.Level == 5).PowerUpDefinition!.Boost!.ConstantValue.Value, Is.EqualTo(162));
        Assert.That(extraDefense.LevelDependentOptions.Single(l => l.Level == 5).PowerUpDefinition!.Boost!.ConstantValue.Value, Is.EqualTo(99));
    }

    private static List<IncreasableItemOption> Options(GameConfiguration gameConfiguration, short number)
        => gameConfiguration.Items.Single(i => i.Group == CardGroup && i.Number == number).PossibleItemOptions
            .SelectMany(o => o.PossibleOptions)
            .Where(o => o.OptionType == Season21ItemOptionTypes.AbilityCardOption)
            .ToList();

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateWithCoreAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);
        await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21AbilityCoreUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        return (context, gameConfiguration);
    }

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateSeason6Async()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        var dataInitialization = new VersionSeasonSix.DataInitialization(contextProvider, new NullLoggerFactory());
        await dataInitialization.CreateInitialDataAsync(1, true).ConfigureAwait(false);
        var context = contextProvider.CreateNewContext();
        var gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        return (context, gameConfiguration);
    }
}
