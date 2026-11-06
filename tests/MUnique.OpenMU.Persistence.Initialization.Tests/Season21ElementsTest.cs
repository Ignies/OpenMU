// <copyright file="Season21ElementsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Elements;
using MUnique.OpenMU.GameLogic.WideItems;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Tests the updates of the elements (pentagrams, errtels, monster elements) and the earrings of Season 21.
/// </summary>
[TestFixture]
public class Season21ElementsTest
{
    private IContext _context = null!;
    private GameConfiguration _gameConfiguration = null!;
    private int _itemOptionsAfterFirstRun;
    private int _monsterAttributesAfterFirstRun;

    /// <summary>
    /// Creates a Season 6 database with the item catalog of Season 21 and applies the updates twice, so every test
    /// also checks that a second run changes nothing.
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
        await new AddSeason21MapsUpdatePlugIn().ApplyUpdateAsync(this._context, this._gameConfiguration).ConfigureAwait(false);
        await new ActivateSeason21WideItemsUpdatePlugIn().ApplyUpdateAsync(this._context, this._gameConfiguration).ConfigureAwait(false);
        await new AddSeason21MasteryOptionsUpdatePlugIn().ApplyUpdateAsync(this._context, this._gameConfiguration).ConfigureAwait(false);
        for (var run = 0; run < 2; run++)
        {
            await new AddSeason21ElementsUpdatePlugIn().ApplyUpdateAsync(this._context, this._gameConfiguration).ConfigureAwait(false);
            await new AddSeason21EarringsUpdatePlugIn().ApplyUpdateAsync(this._context, this._gameConfiguration).ConfigureAwait(false);
            if (run == 0)
            {
                this._itemOptionsAfterFirstRun = this._gameConfiguration.ItemOptions.Count;
                this._monsterAttributesAfterFirstRun = this._gameConfiguration.Monsters.Sum(m => m.Attributes.Count);
            }
        }
    }

    /// <summary>
    /// The updates are optional; a new database leaves them pending.
    /// </summary>
    [Test]
    public void TheUpdatesAreOptional()
    {
        UpdatePlugInBase[] updates = [new AddSeason21ElementsUpdatePlugIn(), new AddSeason21EarringsUpdatePlugIn()];
        Assert.That(updates.Select(u => u.IsMandatory), Has.All.False);
        Assert.That(updates.Select(u => u.RunsOnNewDatabase), Has.All.True);
    }

    /// <summary>
    /// A second run adds nothing.
    /// </summary>
    [Test]
    public void ASecondRunChangesNothing()
    {
        Assert.That(this._gameConfiguration.ItemOptions, Has.Count.EqualTo(this._itemOptionsAfterFirstRun));
        Assert.That(this._gameConfiguration.Monsters.Sum(m => m.Attributes.Count), Is.EqualTo(this._monsterAttributesAfterFirstRun));
        Assert.That(this._gameConfiguration.ItemSlotTypes.Count(t => t.ItemSlots.Any(s => InventoryConstants.ExtraEquippedSlots.Contains((byte)s))), Is.EqualTo(3));
    }

    /// <summary>
    /// The 25 pentagrams go into slot 236, get a random element, five sockets and their elemental stats; they stay
    /// wide-only.
    /// </summary>
    [Test]
    public void PentagramsGetSlotElementSocketsAndStats()
    {
        var pentagrams = this._gameConfiguration.Items.Where(i => i.ItemSlot?.ItemSlots.Contains(InventoryConstants.PentagramSlot) is true).ToList();
        Assert.That(pentagrams, Has.Count.EqualTo(25));
        Assert.That(pentagrams.Select(p => p.MaximumSockets), Has.All.EqualTo(Pentagrams.Sockets - 1), "the fifth by the scroll");
        Assert.That(pentagrams.Where(p => !p.PossibleItemOptions.Any(o => o.AddsRandomly && o.PossibleOptions.Count == 5 && o.PossibleOptions.All(e => e.OptionType == Season21ItemOptionTypes.Element))), Is.Empty);
        Assert.That(pentagrams.Where(p => !WideItemRules.IsWideOnly(p)), Is.Empty);

        var beginners = pentagrams.Single(p => p.Number == 215);
        Assert.That(beginners.BasePowerUpAttributes.Single(a => a.TargetAttribute == Stats.ElementDamageMin).BaseValue, Is.EqualTo(25));
        Assert.That(beginners.BasePowerUpAttributes.Single(a => a.TargetAttribute == Stats.ElementDamageMax).BaseValue, Is.EqualTo(40));
        Assert.That(beginners.BasePowerUpAttributes.Single(a => a.TargetAttribute == Stats.ElementDefense).BaseValue, Is.EqualTo(25));
    }

    /// <summary>
    /// Each errtel rolls one rank 1 option when it drops and knows the options of its higher ranks; every option of
    /// the anger errtel of rank 1 has the 11 values of the levels 0 to 10.
    /// </summary>
    [Test]
    public void ErrtelsGetTheirRankOptions()
    {
        var errtels = this._gameConfiguration.Items.Where(i => i.Group == 12 && Pentagrams.GetErrtelSocket(i) is not null).ToList();
        Assert.That(errtels, Has.Count.EqualTo(13));
        var anger = errtels.Single(e => e.Number == 221);
        Assert.That(Pentagrams.GetErrtelSocket(anger), Is.EqualTo(0));
        Assert.That(Pentagrams.GetErrtelSocket(errtels.Single(e => e.Number == 261)), Is.EqualTo(4), "radiance");
        Assert.That(Pentagrams.GetErrtelSocket(errtels.Single(e => e.Number == 252)), Is.EqualTo(3), "divinity for beginners");

        var firstRank = anger.PossibleItemOptions.Single(o => o.AddsRandomly && o.PossibleOptions.All(p => p.OptionType == Season21ItemOptionTypes.ErrtelRank));
        var damage = firstRank.PossibleOptions.Single();
        Assert.That(damage.SubOptionType, Is.EqualTo(1));
        Assert.That(damage.PowerUpDefinition!.TargetAttribute, Is.EqualTo(Stats.ElementDamageBonus));
        Assert.That(damage.PowerUpDefinition.Boost!.ConstantValue.Value, Is.EqualTo(30));
        Assert.That(damage.LevelDependentOptions.Single(l => l.Level == 10).PowerUpDefinition!.Boost!.ConstantValue.Value, Is.EqualTo(168));
        var all = anger.PossibleItemOptions.Single(o => !o.AddsRandomly && o.PossibleOptions.Any(p => p.OptionType == Season21ItemOptionTypes.ErrtelRank));
        Assert.That(all.PossibleOptions.Select(o => o.SubOptionType).Distinct(), Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    /// <summary>
    /// The monsters of the Season 21 data get their element columns: Devilfairy (586) takes a random element and
    /// has 216 elemental defense and 318-342 elemental damage.
    /// </summary>
    [Test]
    public void MonstersGetTheirElements()
    {
        var devilfairy = this._gameConfiguration.Monsters.First(m => m.Number == 586);
        float Value(AttributeDefinition attribute) => devilfairy.Attributes.Single(a => a.AttributeDefinition == attribute).Value;
        Assert.That(Value(Stats.Element), Is.EqualTo((float)Element.Random));
        Assert.That(Value(Stats.ElementDefense), Is.EqualTo(216));
        Assert.That(Value(Stats.ElementDamageMin), Is.EqualTo(318));
        Assert.That(Value(Stats.ElementDamageMax), Is.EqualTo(342));
        Assert.That(Value(Stats.ElementAttackRate), Is.EqualTo(587));
        Assert.That(Value(Stats.ElementDefenseRate), Is.EqualTo(195));
        Assert.That(this._gameConfiguration.Monsters.Count(m => m.Attributes.Any(a => a.AttributeDefinition == Stats.ElementDefense)), Is.GreaterThan(500));
    }

    /// <summary>
    /// The update writes the configuration of the elements feature (imprints, removal rates) and turns it on.
    /// </summary>
    [Test]
    public void TheFeatureIsConfiguredAndOn()
    {
        var plugIn = this._gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(ElementsFeaturePlugIn).GUID);
        Assert.That(plugIn.IsActive, Is.True);
        var configuration = plugIn.GetConfiguration<ElementsConfiguration>(null)!;
        Assert.That(configuration.SkillImprints, Has.Count.EqualTo(314));
        Assert.That(configuration.RemovalRates, Has.Count.EqualTo(11));
        Assert.That(configuration.GetMultiplier(Element.Fire, Element.Darkness), Is.EqualTo(1.2f));
        Assert.That(configuration.GetMultiplier(Element.Wind, Element.Wind), Is.EqualTo(1.0f));
    }

    /// <summary>
    /// Version 2: the pentagram options with their errtel rules, the errtel sets of the mastery errtels and the set
    /// bonuses of the mastery pentagrams, the radiance absorb as a percentage, the earring basic stats as min-max, the
    /// mastery option values of the later earrings and the earring enhancement +1 to +15.
    /// </summary>
    [Test]
    public void Version2AddsOptionsSetsAndTheEarringEnhancement()
    {
        Assert.That(new AddSeason21ElementsUpdatePlugIn().Version, Is.EqualTo(2));
        var ironShield = this.Item(203);
        var option = ironShield.PossibleItemOptions.SelectMany(o => o.PossibleOptions).Single(o => o.OptionType == Season21ItemOptionTypes.PentagramOption);
        Assert.That((option.Number, option.PowerUpDefinition!.TargetAttribute), Is.EqualTo(((short)3, Stats.ElementDefenseIncrease)));
        Assert.That(option.PowerUpDefinition.Boost!.ConstantValue.Value, Is.EqualTo(0.1f).Within(0.0001));
        var configuration = this._gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(ElementsFeaturePlugIn).GUID).GetConfiguration<ElementsConfiguration>(null)!;
        Assert.That(configuration.PentagramOptions.Single(r => r.Number == 3).Needs.Single(), Has.Property(nameof(ErrtelRequirement.Rank)).EqualTo(1));
        Assert.That(configuration.PentagramUpgradeRates[11], Is.EqualTo(80));

        Assert.That(this.Item(505).PossibleItemOptions.SelectMany(o => o.PossibleOptions).Count(o => o.OptionType == Season21ItemOptionTypes.ErrtelSet), Is.EqualTo(2));
        Assert.That(this.Item(503).PossibleItemOptions.SelectMany(o => o.PossibleOptions).Count(o => o.OptionType == Season21ItemOptionTypes.ErrtelSetBonus), Is.EqualTo(6));
        var radiance = this.Item(261).PossibleItemOptions.SelectMany(o => o.PossibleOptions).Single(o => o.Number == 211);
        Assert.That(radiance.PowerUpDefinition!.Boost!.ConstantValue.Value, Is.EqualTo(0.01f).Within(0.0001), "absorb life 1 %");

        var allurement = this.Item(453);
        Assert.That(allurement.BasePowerUpAttributes.Single(p => p.TargetAttribute == Stats.MinimumPhysBaseDmg).BaseValue, Is.EqualTo(264));
        Assert.That(allurement.BasePowerUpAttributes.Single(p => p.TargetAttribute == Stats.MaximumWizBaseDmg).BaseValue, Is.EqualTo(267));
        Assert.That(allurement.BasePowerUpAttributes.Any(p => p.TargetAttribute == Stats.BaseDamageBonus), Is.False);
        var mastery = allurement.PossibleItemOptions.SelectMany(o => o.PossibleOptions).First(o => o.Number == 28);
        Assert.That(mastery.PowerUpDefinition!.Boost!.ConstantValue.Value, Is.EqualTo(92), "Webzen 5th mastery earrings");
        var goblin = this._gameConfiguration.Monsters.Single(m => m.NpcWindow == DataModel.Configuration.NpcWindow.ChaosMachine);
        Assert.That(goblin.ItemCraftings.Count(c => c.Name.ValueInNeutralLanguage.StartsWith("Earring Enhancement", StringComparison.Ordinal)), Is.EqualTo(15));
    }

    /// <summary>
    /// A database with version 1 (earring base damage as one bonus, mastery options without value, pentagrams with
    /// five sockets) is repaired by version 2.
    /// </summary>
    [Test]
    public async Task Version2RepairsVersion1Async()
    {
        var earring = this.Item(462);
        var minimum = earring.BasePowerUpAttributes.Single(p => p.TargetAttribute == Stats.MinimumPhysBaseDmg);
        earring.BasePowerUpAttributes.Remove(minimum);
        var oldBonus = this._context.CreateNew<ItemBasePowerUpDefinition>();
        oldBonus.TargetAttribute = Stats.BaseDamageBonus.GetPersistent(this._gameConfiguration);
        oldBonus.BaseValue = 371;
        earring.BasePowerUpAttributes.Add(oldBonus);
        var mastery = this.Item(455).PossibleItemOptions.SelectMany(o => o.PossibleOptions).First(o => o.Number == 29);
        mastery.PowerUpDefinition = null;
        this.Item(212).MaximumSockets = 5;

        await new AddSeason21EarringsUpdatePlugIn().ApplyUpdateAsync(this._context, this._gameConfiguration).ConfigureAwait(false);
        await new AddSeason21ElementsUpdatePlugIn().ApplyUpdateAsync(this._context, this._gameConfiguration).ConfigureAwait(false);

        Assert.That(earring.BasePowerUpAttributes.Any(p => p.TargetAttribute == Stats.BaseDamageBonus), Is.False);
        Assert.That(earring.BasePowerUpAttributes.Single(p => p.TargetAttribute == Stats.MinimumPhysBaseDmg).BaseValue, Is.EqualTo(370));
        Assert.That(mastery.PowerUpDefinition!.Boost!.ConstantValue.Value, Is.EqualTo(215), "7th tier, Webzen guide 351");
        Assert.That(this.Item(212).MaximumSockets, Is.EqualTo(4));
    }

    /// <summary>
    /// The chaos goblin gets the elemental refinery: 14 upgrades to the mastery pentagrams and errtels, 10 errtel level
    /// ups and the rank up, once.
    /// </summary>
    [Test]
    public void TheRefineryIsAtTheChaosGoblin()
    {
        var craftings = this._gameConfiguration.Monsters.Single(m => m.NpcWindow == DataModel.Configuration.NpcWindow.ChaosMachine).ItemCraftings
            .Where(c => c.Number >= VersionSeasonSix.Season21.Season21ElementsInitializer.FirstRefineryCraftingNumber && c.Number < 170)
            .ToList();
        Assert.That(craftings.Count(c => c.ItemCraftingHandlerClassName.EndsWith("ElementalUpgradeCrafting", StringComparison.Ordinal)), Is.EqualTo(14));
        Assert.That(craftings.Count(c => c.ItemCraftingHandlerClassName.EndsWith("ErrtelLevelUpCrafting", StringComparison.Ordinal)), Is.EqualTo(10));
        Assert.That(craftings.Count(c => c.ItemCraftingHandlerClassName.EndsWith("ErrtelRankUpCrafting", StringComparison.Ordinal)), Is.EqualTo(1));
        Assert.That(craftings.Select(c => c.Number).Distinct().Count(), Is.EqualTo(craftings.Count));
    }

    /// <summary>
    /// The earrings go into their slots (left 238, right 237), have one always rolled and two further options and a
    /// link set per tier.
    /// </summary>
    [Test]
    public void EarringsGetSlotsOptionsAndLinks()
    {
        var left = this._gameConfiguration.Items.Single(i => i.Group == 12 && i.Number == 449);
        var right = this._gameConfiguration.Items.Single(i => i.Group == 12 && i.Number == 457);
        Assert.That(left.ItemSlot!.ItemSlots, Is.EquivalentTo(new[] { (int)InventoryConstants.LeftEarringSlot }));
        Assert.That(right.ItemSlot!.ItemSlots, Is.EquivalentTo(new[] { (int)InventoryConstants.RightEarringSlot }));
        var options = left.PossibleItemOptions.Where(o => o.PossibleOptions.Any(p => p.OptionType == Season21ItemOptionTypes.EarringOption)).ToList();
        Assert.That(options.Select(o => o.MaximumOptionsPerItem), Is.EquivalentTo(new[] { 1, 2 }));
        var health = options[0].PossibleOptions.Single(o => o.Number == 11);
        Assert.That(health.PowerUpDefinition!.TargetAttribute, Is.EqualTo(Stats.MaximumHealth));
        Assert.That(health.PowerUpDefinition.Boost!.ConstantValue.Value, Is.EqualTo(200));

        var link = left.PossibleItemSetGroups.Single(g => g.Items.Any(i => i.ItemDefinition == right));
        Assert.That(link.MinimumItemCount, Is.EqualTo(2));
        Assert.That(link.Options!.PossibleOptions.Select(o => o.PowerUpDefinition!.TargetAttribute), Is.EquivalentTo(new[] { Stats.DefenseIgnoreChance, Stats.AllStatsBonus }));
        Assert.That(this._gameConfiguration.ItemSetGroups.Count(g => g.Name.ValueInNeutralLanguage.StartsWith("Earring Link", StringComparison.Ordinal)), Is.EqualTo(8));
    }

    private ItemDefinition Item(short number) => this._gameConfiguration.Items.Single(i => i.Group == 12 && i.Number == number);
}
