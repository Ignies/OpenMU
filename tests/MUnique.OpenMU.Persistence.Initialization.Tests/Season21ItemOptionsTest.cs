// <copyright file="Season21ItemOptionsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.PlayerActions.Craftings;
using MUnique.OpenMU.GameLogic.WideItems;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.Persistence.InMemory;

/// <summary>
/// Tests the updates of the Season 21 item options and the activation of the items of the wide item format.
/// </summary>
[TestFixture]
public class Season21ItemOptionsTest
{
    private IContext _context = null!;
    private GameConfiguration _gameConfiguration = null!;

    /// <summary>
    /// Creates a Season 6 database with the gear and the item catalog of Season 21 and applies the updates twice,
    /// so every test also checks that a second run changes nothing.
    /// </summary>
    [OneTimeSetUp]
    public async Task SetUpAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        await new VersionSeasonSix.DataInitialization(contextProvider, new NullLoggerFactory()).CreateInitialDataAsync(1, true).ConfigureAwait(false);
        this._context = contextProvider.CreateNewContext();
        this._gameConfiguration = (await this._context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        await new AddSeason21GearUpdatePlugIn().ApplyUpdateAsync(this._context, this._gameConfiguration).ConfigureAwait(false);
        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(this._context, this._gameConfiguration).ConfigureAwait(false);
        Assert.That(this.Waiting().Where(d => d.IsActive), Is.Empty, "the catalog leaves them inactive");
        for (var run = 0; run < 2; run++)
        {
            await new ActivateSeason21WideItemsUpdatePlugIn().ApplyUpdateAsync(this._context, this._gameConfiguration).ConfigureAwait(false);
            await new AddSeason21MasteryOptionsUpdatePlugIn().ApplyUpdateAsync(this._context, this._gameConfiguration).ConfigureAwait(false);
            await new AddSeason21WingGradesUpdatePlugIn().ApplyUpdateAsync(this._context, this._gameConfiguration).ConfigureAwait(false);
            await new AddSeason21SphereLevelsUpdatePlugIn().ApplyUpdateAsync(this._context, this._gameConfiguration).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// A new database leaves the updates pending: they are optional.
    /// </summary>
    [Test]
    public void TheUpdatesAreOptional()
    {
        UpdatePlugInBase[] updates = [new ActivateSeason21WideItemsUpdatePlugIn(), new AddSeason21MasteryOptionsUpdatePlugIn(), new AddSeason21WingGradesUpdatePlugIn(), new AddSeason21SphereLevelsUpdatePlugIn()];
        Assert.That(updates.Select(u => u.IsMandatory), Has.All.False);
        Assert.That(updates.Select(u => u.RunsOnNewDatabase), Has.All.True);
    }

    /// <summary>
    /// The 1,187 waiting items are active and wide-only; every item a Season 6 client got before still reaches it.
    /// </summary>
    [Test]
    public void TheWaitingItemsAreActiveAndOnlyTheyAreWideOnly()
    {
        var waiting = this.Waiting().ToList();
        Assert.That(waiting, Has.Count.EqualTo(1187));
        Assert.That(waiting.Where(d => !d.IsActive), Is.Empty);
        Assert.That(waiting.Where(d => !WideItemRules.IsWideOnly(d)), Is.Empty);

        var waitingSet = waiting.ToHashSet();
        Assert.That(this._gameConfiguration.Items.Where(d => !waitingSet.Contains(d) && WideItemRules.IsWideOnly(d)), Is.Empty);
        var marker = this._gameConfiguration.ItemOptions.Single(o => o.PossibleOptions.Any(p => p.OptionType == Season21ItemOptionTypes.WideFormat));
        Assert.That(waiting.Select(d => d.PossibleItemOptions.Count(o => o == marker)), Has.All.LessThanOrEqualTo(1), "marked once");
    }

    /// <summary>
    /// The 890 mastery items get their options; weapons and armors their own.
    /// </summary>
    [Test]
    public void MasteryItemsGetTheirOptions()
    {
        var mastery = Season21ItemCatalog.Items.Where(r => r.Mastery is { Length: 2 }).ToList();
        Assert.That(mastery, Has.Count.EqualTo(890));
        foreach (var row in mastery)
        {
            var options = this.Item(row.Group, row.Number).PossibleItemOptions.SelectMany(o => o.PossibleOptions).ToList();
            Assert.That(options.Count(o => o.OptionType == Season21ItemOptionTypes.Mastery), Is.EqualTo(row.Mastery![0] == 0 ? 2 : 5), row.Name);
            Assert.That(options.Count(o => o.OptionType == Season21ItemOptionTypes.MasteryBonus), Is.EqualTo(1), row.Name);
        }

        var armor = this.Item(mastery.First(r => r.Mastery![0] == 1).Group, mastery.First(r => r.Mastery![0] == 1).Number);
        var health = armor.PossibleItemOptions.SelectMany(o => o.PossibleOptions).Single(o => o.OptionType == Season21ItemOptionTypes.Mastery && o.Number == 6);
        Assert.That(health.PowerUpDefinition!.TargetAttribute, Is.EqualTo(Stats.MaximumHealth));
        Assert.That(health.PowerUpDefinition.Boost!.ConstantValue.Value, Is.EqualTo(165));
        Assert.That(this._gameConfiguration.ItemOptions.Count(o => o.Name.ValueInNeutralLanguage == "Mastery Armor Options"), Is.EqualTo(1));
    }

    /// <summary>
    /// The all stats bonus of the mastery weapons raises the stats of every class through its relationships.
    /// </summary>
    [Test]
    public void AllStatsBonusRaisesTheStats()
    {
        foreach (var characterClass in this._gameConfiguration.CharacterClasses)
        {
            var targets = characterClass.AttributeCombinations.Where(r => r.InputAttribute == Stats.AllStatsBonus).Select(r => r.TargetAttribute).ToList();
            Assert.That(targets, Does.Contain(Stats.TotalStrength).And.Contain(Stats.TotalAgility).And.Contain(Stats.TotalVitality).And.Contain(Stats.TotalEnergy), characterClass.Name.ValueInNeutralLanguage);
            Assert.That(targets, Has.Count.EqualTo(targets.Distinct().Count()), "added once");
        }
    }

    /// <summary>
    /// The fourth wings get four grades of each option, the fifth wings one.
    /// </summary>
    [Test]
    public void WingsGetTheirGrades()
    {
        var wings = Season21ItemCatalog.Items.Where(r => r.KindA is 76 or 106).ToList();
        Assert.That(wings, Has.Count.GreaterThanOrEqualTo(30));
        foreach (var row in wings)
        {
            var grades = this.Item(row.Group, row.Number).PossibleItemOptions.SelectMany(o => o.PossibleOptions).Where(o => o.OptionType == Season21ItemOptionTypes.WingGrade).ToList();
            Assert.That(grades, Has.Count.EqualTo(11), row.Name);
            Assert.That(grades.Select(g => g.LevelDependentOptions.Count), Has.All.EqualTo(row.KindA == 76 ? 4 : 1), row.Name);
            Assert.That(this.Item(row.Group, row.Number).ItemSlot?.ItemSlots, Does.Contain(7), "wearable as wings");
        }

        var speed = this.Item(12, 414).PossibleItemOptions.SelectMany(o => o.PossibleOptions).Single(o => o.OptionType == Season21ItemOptionTypes.WingGrade && o.Number == 4);
        Assert.That(speed.LevelDependentOptions.Single(l => l.Level == 4).PowerUpDefinition!.Boost!.ConstantValue.Value, Is.EqualTo(12), "grade 3 attack speed");
    }

    /// <summary>
    /// The socket options reach level 10, scaled from their Season 6 level 5 by the Season 21 values.
    /// </summary>
    [Test]
    public void SocketOptionsReachLevel10()
    {
        var socketOptions = this._gameConfiguration.ItemOptions.SelectMany(o => o.PossibleOptions).Where(o => o.OptionType == ItemOptionTypes.SocketOption).ToList();
        Assert.That(socketOptions, Is.Not.Empty);
        Assert.That(socketOptions.Select(o => o.LevelDependentOptions.Max(l => l.Level)), Has.All.EqualTo(10));
        Assert.That(socketOptions.Select(o => o.LevelDependentOptions.Count(l => l.Level == 6)), Has.All.EqualTo(1), "added once");

        var maximumDamage = socketOptions.Single(o => o.SubOptionType == (int)SocketSubOptionType.Fire && o.Number == 2);
        Assert.That(maximumDamage.LevelDependentOptions.Single(l => l.Level == 6).PowerUpDefinition!.Boost!.ConstantValue.Value, Is.EqualTo(50f * 50 / 45).Within(0.01f));
        var health = socketOptions.Single(o => o.SubOptionType == (int)SocketSubOptionType.Wind && o.Number == 1);
        Assert.That(health.LevelDependentOptions.Single(l => l.Level == 10).PowerUpDefinition!.Boost!.ConstantValue.Value, Is.EqualTo(1 + (0.08f * 295 / 165)).Within(0.001f), "a multiplier scales its bonus");
    }

    /// <summary>
    /// The seed spheres of the levels 6 to 10 carry their element's options, can be mounted and enhanced.
    /// </summary>
    [Test]
    public void SeedSpheresOfTheLevels6To10()
    {
        var mounting = this._gameConfiguration.Monsters.SelectMany(m => m.ItemCraftings).Single(c => c.Number == 44)
            .SimpleCraftingSettings!.RequiredItems.Single(r => r.Reference == MountSeedSphereCrafting.SeedSphereReference);
        for (short number = 311; number <= 340; number++)
        {
            var sphere = this.Item(12, number);
            var template = this.Item(12, (short)(100 + ((number - 311) % 6)));
            Assert.That(sphere.PossibleItemOptions, Is.SupersetOf(template.PossibleItemOptions), sphere.Name.ValueInNeutralLanguage);
            Assert.That(MountSeedSphereCrafting.GetSphereLevel(sphere), Is.EqualTo(6 + ((number - 311) / 6)));
            Assert.That(mounting.PossibleItems, Does.Contain(sphere));
        }

        var researcher = this._gameConfiguration.Monsters.Single(m => m.ItemCraftings.Any(c => c.Number == 44));
        var enhancements = researcher.ItemCraftings.Where(c => c.Number is >= 100 and <= 104).OrderBy(c => c.Number).ToList();
        Assert.That(enhancements.Select(c => c.SimpleCraftingSettings!.RequiredItems.Single(r => r.Reference != SeedSphereEnhanceCrafting.SeedSphereReference).MinimumAmount), Is.EqualTo(new byte[] { 7, 8, 9, 10, 11 }));
        Assert.That(enhancements[0].SimpleCraftingSettings!.RequiredItems.Single(r => r.Reference == SeedSphereEnhanceCrafting.SeedSphereReference).PossibleItems.Select(i => (int)i.Number), Is.EquivalentTo(Enumerable.Range(124, 6)));
        Assert.That(enhancements[0].ItemCraftingHandlerClassName, Is.EqualTo(typeof(SeedSphereEnhanceCrafting).FullName));
    }

    private IEnumerable<ItemDefinition> Waiting()
        => Season21ItemCatalog.Items.Where(r => r.WaitsFor is not null).Select(r => this.Item(r.Group, r.Number));

    private ItemDefinition Item(byte group, short number)
        => this._gameConfiguration.Items.Single(i => i.Group == group && i.Number == number);
}
