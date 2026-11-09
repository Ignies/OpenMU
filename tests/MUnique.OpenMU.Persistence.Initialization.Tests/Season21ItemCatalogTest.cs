// <copyright file="Season21ItemCatalogTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.Persistence.InMemory;

/// <summary>
/// Tests the optional update which defines every item of the Season 21 server data.
/// </summary>
[TestFixture]
internal class Season21ItemCatalogTest
{
    private static HashSet<short> Catalogued { get; } = Season21ItemCatalog.Items.Select(i => (short)((i.Group * 512) + i.Number)).ToHashSet();

    /// <summary>
    /// The catalog holds every row of the data, in all 22 item groups.
    /// </summary>
    [Test]
    public void CatalogHoldsEveryItemOfTheData()
    {
        var items = Season21ItemCatalog.Items;

        Assert.That(items, Has.Count.EqualTo(4701));
        Assert.That(items.Select(i => (i.Group, i.Number)).Distinct().Count(), Is.EqualTo(items.Count));
        Assert.That(items.Select(i => i.Group).Distinct(), Is.EquivalentTo(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 19, 20, 21 }));
        Assert.That(items.Where(i => i.Group > 15).Select(i => i.WaitsFor), Has.All.EqualTo("group"));
        Assert.That(items.Where(i => i.Group <= 15).Select(i => i.Number), Has.All.LessThan(512));
        Assert.That(items.Single(i => i is { Group: 12, Number: 449 }).WaitsFor, Is.EqualTo("earring"));
        Assert.That(items.Single(i => i is { Group: 0, Number: 42 }).Mastery, Is.EqualTo(new[] { 0, 0 }));
    }

    /// <summary>
    /// A new Season 6 database doesn't contain the items, and the update stays pending.
    /// </summary>
    [Test]
    public async Task NewDatabaseLeavesTheUpdatePendingAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);

        Assert.That(gameConfiguration.Items.Where(i => i is { Group: 7, Number: 151 }), Is.Empty);
        var installed = (await context.GetAsync<ConfigurationUpdate>().ConfigureAwait(false)).Select(u => u.Key);
        Assert.That(installed, Has.None.EqualTo(new AddSeason21ItemCatalogUpdatePlugIn().Key));
    }

    /// <summary>
    /// The update defines every item of the catalog, once, and keeps the items which were defined before.
    /// </summary>
    [Test]
    public async Task UpdateDefinesEveryItemAndKeepsTheDefinedOnesAsync()
    {
        var (context, gameConfiguration) = await CreateWithGearAsync().ConfigureAwait(false);
        var before = gameConfiguration.Items.ToDictionary(i => (i.Group, i.Number), i => (i.Name.ValueInNeutralLanguage, i.Durability, i.DropsFromMonsters));

        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var defined = gameConfiguration.Items.Select(i => (i.Group, i.Number)).ToList();
        Assert.That(defined, Is.Unique);
        Assert.That(Season21ItemCatalog.Items.Select(i => (i.Group, i.Number)), Is.SubsetOf(defined));
        Assert.That(gameConfiguration.Items.Where(i => before.ContainsKey((i.Group, i.Number))).Select(i => (i.Group, i.Number, (i.Name.ValueInNeutralLanguage, i.Durability, i.DropsFromMonsters))),
            Is.EquivalentTo(before.Select(b => (b.Key.Group, b.Key.Number, b.Value))));
    }

    /// <summary>
    /// What the Season 6 item format can't carry is defined but inactive; nothing new joins the random drops.
    /// </summary>
    [Test]
    public async Task ItemsWhichWaitForTheWideFormatAreInactiveAsync()
    {
        var (context, gameConfiguration) = await CreateWithGearAsync().ConfigureAwait(false);
        var before = gameConfiguration.Items.Select(i => (i.Group, i.Number)).ToHashSet();

        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var added = gameConfiguration.Items.Where(i => !before.Contains((i.Group, i.Number))).ToList();
        var waiting = Season21ItemCatalog.Items.Where(r => r.WaitsFor is not null).Select(r => (r.Group, r.Number)).ToHashSet();
        Assert.That(added.Where(i => waiting.Contains((i.Group, i.Number))).Select(i => i.IsActive), Has.All.False);
        Assert.That(added.Where(i => !waiting.Contains((i.Group, i.Number))).Select(i => i.IsActive), Has.All.True);
        Assert.That(added.Where(i => i.Group > 15), Is.Not.Empty);
        Assert.That(added.Select(i => i.DropsFromMonsters), Has.All.False);
        Assert.That(Item(gameConfiguration, 12, 449).ItemSlot, Is.Null);
    }

    /// <summary>
    /// A new armor piece copies a Season 6 piece of its kind: slot, options and level tables, with its own values,
    /// requirements and classes.
    /// </summary>
    [Test]
    public async Task GearCopiesTheItemOfItsKindAsync()
    {
        var (context, gameConfiguration) = await CreateWithGearAsync().ConfigureAwait(false);

        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var helm = Item(gameConfiguration, 7, 151); // Holyangel Wizard Helm
        Assert.That(helm.Name.ValueInNeutralLanguage, Is.EqualTo("Holyangel Wizard Helm"));
        Assert.That(helm.Name.GetFromSource()?.ValueInNeutralLanguage, Is.EqualTo("Holyangel Wizard Helm"));
        Assert.That(helm.ItemSlot, Is.SameAs(Item(gameConfiguration, 7, 0).ItemSlot));
        Assert.That(helm.BasePowerUpAttributes.Single(p => p.TargetAttribute == Stats.DefenseBase).BaseValue, Is.EqualTo(121));
        Assert.That(helm.BasePowerUpAttributes.Single(p => p.TargetAttribute == Stats.DefenseBase).BonusPerLevelTable, Is.Not.Null);
        Assert.That(helm.PossibleItemOptions.Select(o => o.PossibleOptions.First().OptionType), Does.Contain(ItemOptionTypes.Excellent).And.Contain(ItemOptionTypes.Luck));
        Assert.That(helm.QualifiedCharacters.Select(c => c.Number), Is.EquivalentTo(new[] { 3 })); // Grand Master
        Assert.That(helm.Requirements.Single(r => r.Attribute == Stats.Level).MinimumValue, Is.EqualTo(Row(7, 151).Requirements[0]));
        Assert.That(helm.MaximumItemLevel, Is.EqualTo(15));
        Assert.That(helm.StatProfile, Is.Not.Null);

        var sword = Item(gameConfiguration, 0, 42); // Bloodangel Sword
        Assert.That(sword.BasePowerUpAttributes.Single(p => p.TargetAttribute == Stats.MinimumPhysBaseDmgByWeapon).BaseValue, Is.EqualTo(252));
        Assert.That(sword.Skill?.Number, Is.EqualTo(22));

        var newClassOnly = Item(gameConfiguration, 8, 94); // Hirat Armor, Grow Lancer only
        Assert.That(newClassOnly.IsActive, Is.True);
        Assert.That(newClassOnly.QualifiedCharacters, Is.Empty);
    }

    /// <summary>
    /// The cloaks of the Crusader (group 21) are wings: level, defense and durability of the client's item table, the
    /// required level of the wings of their kind and the damage rates of <c>ItemOptionWing.txt</c>.
    /// </summary>
    /// <param name="number">The cloak number.</param>
    /// <param name="dropLevel">The item level.</param>
    /// <param name="defense">The defense.</param>
    /// <param name="defenseAtLevel15">The defense of a +15 cloak, or the plain defense without a level table.</param>
    /// <param name="durability">The durability.</param>
    /// <param name="level">The required level.</param>
    /// <param name="increase">The damage increase factor.</param>
    /// <param name="increaseAtLevel15">The damage increase factor of a +15 cloak.</param>
    /// <param name="absorb">The damage receive factor.</param>
    /// <param name="excellent">Whether the cloak takes the excellent options of the third wings.</param>
    [TestCase(0, 100, 20, 86, 200, 150, 1.12f, 1.42f, 0.88f, false)] // Cloak of Protection
    [TestCase(1, 150, 45, 126, 220, 400, 1.39f, 1.69f, 0.61f, true)] // Cloak of Desire
    [TestCase(2, 150, 45, 126, 220, 400, 1.39f, 1.69f, 0.61f, true)] // [Bound] Cloak of Desire
    [TestCase(3, 153, 155, 155, 220, 800, 1.55f, 1.70f, 0.57f, false)] // Cloak of Sanctuary
    [TestCase(5, 156, 175, 175, 220, 1200, 1.55f, 1.70f, 0.57f, false)] // Cloak of Light
    public async Task CrusaderCloaksAreWingsAsync(short number, int dropLevel, int defense, int defenseAtLevel15, int durability, int level, float increase, float increaseAtLevel15, float absorb, bool excellent)
    {
        var (context, gameConfiguration) = await CreateWithGearAsync().ConfigureAwait(false);

        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var cloak = Item(gameConfiguration, 21, number);
        float At(ItemBasePowerUpDefinition powerUp, int itemLevel) => powerUp.BaseValue + (powerUp.BonusPerLevelTable?.BonusPerLevel.FirstOrDefault(b => b.Level == itemLevel)?.AdditionalValue ?? 0);
        ItemBasePowerUpDefinition PowerUp(AttributeSystem.AttributeDefinition attribute) => cloak.BasePowerUpAttributes.Single(p => p.TargetAttribute == attribute);
        Assert.That(cloak.ItemSlot, Is.SameAs(Item(gameConfiguration, 12, 0).ItemSlot));
        Assert.That(cloak.DropLevel, Is.EqualTo(dropLevel));
        Assert.That(cloak.Durability, Is.EqualTo(durability));
        Assert.That(cloak.MaximumItemLevel, Is.EqualTo(15));
        Assert.That(cloak.Requirements.Single(r => r.Attribute == Stats.Level).MinimumValue, Is.EqualTo(level));
        Assert.That(PowerUp(Stats.DefenseBase).BaseValue, Is.EqualTo(defense));
        Assert.That(At(PowerUp(Stats.DefenseBase), 15), Is.EqualTo(defenseAtLevel15));
        Assert.That(PowerUp(Stats.AttackDamageIncrease).BaseValue, Is.EqualTo(increase).Within(1e-5));
        Assert.That(At(PowerUp(Stats.AttackDamageIncrease), 15), Is.EqualTo(increaseAtLevel15).Within(1e-5));
        Assert.That(PowerUp(Stats.DamageReceiveDecrement).BaseValue, Is.EqualTo(absorb).Within(1e-5));
        Assert.That(At(PowerUp(Stats.DamageReceiveDecrement), 15), Is.EqualTo(absorb - 0.3f).Within(1e-5));
        Assert.That(PowerUp(Stats.CanFly).BaseValue, Is.EqualTo(1));
        var optionTypes = cloak.PossibleItemOptions.SelectMany(o => o.PossibleOptions).Select(o => o.OptionType).ToList();
        Assert.That(optionTypes, Does.Contain(ItemOptionTypes.Luck).And.Contain(ItemOptionTypes.Option));
        Assert.That(optionTypes.Contains(ItemOptionTypes.Wing), Is.EqualTo(excellent));
    }

    /// <summary>
    /// Version 2 of the update gives the cloaks of a database of version 1 their values, and keeps what an operator
    /// changed on them.
    /// </summary>
    [Test]
    public async Task CloakValuesKeepOperatorEditsAsync()
    {
        var (context, gameConfiguration) = await CreateWithGearAsync().ConfigureAwait(false);
        new Season21ItemCatalogInitializer(context, gameConfiguration).Initialize();
        var edited = Item(gameConfiguration, 21, 1);
        edited.Durability = 5;
        edited.DropLevel = 7;

        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        var powerUps = edited.BasePowerUpAttributes.Count;
        edited.BasePowerUpAttributes.Remove(edited.BasePowerUpAttributes.First(p => p.TargetAttribute == Stats.DamageReceiveDecrement));
        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(new AddSeason21ItemCatalogUpdatePlugIn().Version, Is.EqualTo(2));
        Assert.That(edited.Durability, Is.EqualTo(5));
        Assert.That(edited.DropLevel, Is.EqualTo(7));
        Assert.That(edited.Requirements.Single(r => r.Attribute == Stats.Level).MinimumValue, Is.EqualTo(400));
        Assert.That(edited.BasePowerUpAttributes, Has.Count.EqualTo(powerUps - 1));
    }

    /// <summary>
    /// Every new item knows the version which introduced it.
    /// </summary>
    [Test]
    public async Task EveryItemHasItsVersionAsync()
    {
        var (context, gameConfiguration) = await CreateWithGearAsync().ConfigureAwait(false);
        var before = gameConfiguration.Items.Select(i => (i.Group, i.Number)).ToHashSet();

        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var added = gameConfiguration.Items.Where(i => !before.Contains((i.Group, i.Number))).ToList();
        Assert.That(added, Has.Count.GreaterThan(3000));
        Assert.That(added.Select(i => i.IntroducedIn), Has.None.EqualTo(GameVersion.Unknown));
        Assert.That(Item(gameConfiguration, 8, 94).IntroducedIn, Is.EqualTo(GameVersion.Season10Episode2)); // Hirat Armor
        Assert.That(Item(gameConfiguration, 0, 42).IntroducedIn, Is.EqualTo(GameVersion.Season14Episode1)); // Bloodangel Sword
        Assert.That(Item(gameConfiguration, 8, 130).IntroducedIn, Is.EqualTo(GameVersion.Season21Episode1)); // [Bound] Dragon Armor, by the data
    }

    /// <summary>
    /// Applying the update a second time changes nothing, and keeps what an operator changed.
    /// </summary>
    [Test]
    public async Task UpdateTwiceChangesNothingAndKeepsOperatorEditsAsync()
    {
        var (context, gameConfiguration) = await CreateWithMapsAndGearAsync().ConfigureAwait(false);
        var update = new AddSeason21ItemCatalogUpdatePlugIn();

        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        var edited = Item(gameConfiguration, 7, 151);
        edited.Durability = 1;
        edited.IsActive = false;
        var items = gameConfiguration.Items.Count;
        var groups = gameConfiguration.DropItemGroups.Count;
        var chances = gameConfiguration.DropItemGroups.Sum(g => g.Chance);
        var dropItems = gameConfiguration.DropItemGroups.Sum(g => g.PossibleItems.Count);
        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(gameConfiguration.Items.Count, Is.EqualTo(items));
        Assert.That(gameConfiguration.DropItemGroups.Count, Is.EqualTo(groups));
        Assert.That(gameConfiguration.DropItemGroups.Sum(g => g.PossibleItems.Count), Is.EqualTo(dropItems));
        Assert.That(gameConfiguration.DropItemGroups.Sum(g => g.Chance), Is.EqualTo(chances).Within(1e-9));
        Assert.That(edited.Durability, Is.EqualTo(1));
        Assert.That(edited.IsActive, Is.False);
    }

    /// <summary>
    /// The drop lists of the later maps hold every item of the data once the items are defined: before the update,
    /// more than a thousand of their items are undefined and left out, after it only the nine which the item data
    /// itself doesn't define (helms and gloves of classes which wear none).
    /// </summary>
    [Test]
    public async Task DropListsPickUpTheNewItemsAsync()
    {
        var (context, gameConfiguration) = await CreateWithMapsAndGearAsync().ConfigureAwait(false);
        var listed = Season21DropData.All.SelectMany(l => l.Buckets).SelectMany(b => b.Items).ToHashSet();
        var undefinedBefore = listed.Except(gameConfiguration.Items.Select(Key)).Count();
        var groupsBefore = gameConfiguration.DropItemGroups.Count;

        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var undefinedAfter = listed.Except(gameConfiguration.Items.Select(Key)).Count();
        var dropped = gameConfiguration.Monsters.SelectMany(m => m.DropItemGroups)
            .Where(g => g.Description.ValueInNeutralLanguage == Properties.DropGroupDescriptions.Season21MonsterDrops)
            .Distinct().ToList();
        TestContext.Out.WriteLine($"S21 drop list items: {listed.Count}, undefined before: {undefinedBefore}, after: {undefinedAfter}; groups {groupsBefore} -> {gameConfiguration.DropItemGroups.Count}");
        Assert.That(undefinedBefore, Is.GreaterThan(1000));
        Assert.That(undefinedAfter, Is.EqualTo(listed.Count(key => !Catalogued.Contains(key))).And.EqualTo(9));
        Assert.That(dropped.SelectMany(g => g.PossibleItems).Select(Key).ToHashSet(), Is.EquivalentTo(DroppedByMonstersOf(gameConfiguration)));
        foreach (var monster in gameConfiguration.Monsters.Where(m => m.DropItemGroups.Any(dropped.Contains)))
        {
            Assert.That(monster.DropItemGroups.Where(dropped.Contains).Sum(g => g.Chance), Is.EqualTo(0.3).Within(0.0001), monster.Designation.ValueInNeutralLanguage);
        }
    }

    /// <summary>
    /// When the items come before the maps, the maps' drop lists hold them all the same.
    /// </summary>
    [Test]
    public async Task MapsAfterTheItemsHoldThemAsync()
    {
        var (context, gameConfiguration) = await CreateWithGearAsync().ConfigureAwait(false);
        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21LevelCapsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21MapsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var listed = Season21DropData.All.SelectMany(l => l.Buckets).SelectMany(b => b.Items).ToHashSet();
        var dropped = gameConfiguration.DropItemGroups.Where(g => g.Description.ValueInNeutralLanguage == Properties.DropGroupDescriptions.Season21MonsterDrops);
        Assert.That(dropped.SelectMany(g => g.PossibleItems).Select(Key).ToHashSet(), Is.EquivalentTo(DroppedByMonstersOf(gameConfiguration)));
    }

    /// <summary>
    /// Gets the catalogued items of the drop lists whose monsters the configuration has.
    /// </summary>
    private static IEnumerable<short> DroppedByMonstersOf(GameConfiguration gameConfiguration)
    {
        var monsters = gameConfiguration.Monsters.Select(m => m.Number).ToHashSet();
        return Season21DropData.All.Where(l => l.Monsters.Any(monsters.Contains)).SelectMany(l => l.Buckets).SelectMany(b => b.Items).Where(Catalogued.Contains).Distinct();
    }

    private static short Key(ItemDefinition item) => (short)((item.Group * 512) + item.Number);

    /// <summary>
    /// The nine new classes and the fourth and fifth evolutions wear what their column of the data allows, whichever
    /// of the catalog and the classes installs first: on the first live run a Grow Lancer could wear nothing. An item
    /// without classes is for everybody.
    /// </summary>
    /// <param name="classesFirst">Whether the classes update installs before the catalog.</param>
    [TestCase(true)]
    [TestCase(false)]
    public async Task EveryClassWearsWhatItsColumnAllowsAsync(bool classesFirst)
    {
        var (context, gameConfiguration) = await CreateWithGearAsync().ConfigureAwait(false);
        if (classesFirst)
        {
            await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        }

        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        if (!classesFirst)
        {
            await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        }

        var growLancer = gameConfiguration.CharacterClasses.Single(c => c.Number == 184);
        var dragonKnight = gameConfiguration.CharacterClasses.Single(c => c.Number == 139);
        var lancerRows = Season21ItemCatalog.Items.Where(r => r.Classes[7] == 1).ToList();
        var knightRows = Season21ItemCatalog.Items.Where(r => r.Classes[1] is >= 1 and <= 4).ToList();
        Assert.That(lancerRows.Count(r => Item(gameConfiguration, r.Group, r.Number).QualifiedCharacters.Contains(growLancer)), Is.GreaterThan(100));
        Assert.That(lancerRows.Where(r => Item(gameConfiguration, r.Group, r.Number).QualifiedCharacters is { Count: > 0 } classes && !classes.Contains(growLancer)).Select(r => (r.Group, r.Number)), Is.Empty);
        Assert.That(knightRows.Where(r => Item(gameConfiguration, r.Group, r.Number).QualifiedCharacters is { Count: > 0 } classes && !classes.Contains(dragonKnight)).Select(r => (r.Group, r.Number)), Is.Empty);
    }

    private static Season21CatalogItem Row(byte group, short number) => Season21ItemCatalog.Items.Single(i => i.Group == group && i.Number == number);

    private static ItemDefinition Item(GameConfiguration gameConfiguration, byte group, short number)
        => gameConfiguration.Items.Single(i => i.Group == group && i.Number == number);

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateWithMapsAndGearAsync()
    {
        var (context, gameConfiguration) = await CreateWithGearAsync().ConfigureAwait(false);
        await new AddSeason21LevelCapsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21MapsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        return (context, gameConfiguration);
    }

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateWithGearAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);
        await new AddSeason21GearUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
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
