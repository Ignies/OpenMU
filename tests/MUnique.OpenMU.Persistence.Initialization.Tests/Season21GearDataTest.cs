// <copyright file="Season21GearDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using System.Collections;
using System.Globalization;
using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.Persistence.Initialization.Properties;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;

/// <summary>
/// Tests the optional update which adds the gear of Season 21 to a Season 6 database, and David's lucky item refinery.
/// </summary>
[TestFixture]
internal class Season21GearDataTest
{
    /// <summary>
    /// A new Season 6 database doesn't contain the gear, and the update stays pending.
    /// </summary>
    [Test]
    public async Task NewDatabaseLeavesTheUpdatePendingAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);

        Assert.That(gameConfiguration.Items.Where(i => i is { Group: 13, Number: 173 }), Is.Empty);
        var installed = (await context.GetAsync<ConfigurationUpdate>().ConfigureAwait(false)).Select(u => u.Key);
        Assert.That(installed, Has.None.EqualTo(new AddSeason21GearUpdatePlugIn().Key));
    }

    /// <summary>
    /// The update adds the jewelry: a ring takes the options of the Ring of Ice and the values of Season 21.
    /// </summary>
    [Test]
    public async Task UpdateAddsTheJewelryAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);

        await new AddSeason21GearUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var ring = Item(gameConfiguration, 13, 173);
        var ringOfIce = Item(gameConfiguration, 13, 8);
        Assert.That(ring.Name.ValueInNeutralLanguage, Is.EqualTo("Ring of Ultimatum"));
        Assert.That(ring.Name.GetFromSource()?.ValueInNeutralLanguage, Is.EqualTo("Ring of Ultimatum"));
        Assert.That(ring.ItemSlot, Is.SameAs(ringOfIce.ItemSlot));
        Assert.That(ring.DropLevel, Is.EqualTo(98));
        Assert.That(ring.DropsFromMonsters, Is.True);
        Assert.That(ring.PossibleItemOptions, Is.SubsetOf(ringOfIce.PossibleItemOptions));
        Assert.That(ring.PossibleItemOptions, Is.Not.Empty);
        var box = Item(gameConfiguration, 14, 33).DropItems.Where(d => d.ItemType == SpecialItemType.Excellent);
        Assert.That(box.SelectMany(d => d.PossibleItems), Does.Contain(ring));

        var decoration = Item(gameConfiguration, 13, 169);
        Assert.That(decoration.IsDroppable, Is.False);
        Assert.That(decoration.IsRepairable, Is.False);
        Assert.That(decoration.BasePowerUpAttributes.Single(p => p.TargetAttribute == Stats.MinimumPhysBaseDmg).BaseValue, Is.EqualTo(1.1f));
        Assert.That(decoration.BasePowerUpAttributes.Single(p => p.TargetAttribute == Stats.AttackSpeedAny).BaseValue, Is.EqualTo(10));
    }

    /// <summary>
    /// The update adds a socket weapon: it takes the options and level tables of its template, its own damage and the
    /// seed options of a weapon.
    /// </summary>
    [Test]
    public async Task UpdateAddsTheWeaponsAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);

        await new AddSeason21GearUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var sonicBlade = Item(gameConfiguration, 0, 29);
        Assert.That(sonicBlade.BasePowerUpAttributes.Single(p => p.TargetAttribute == Stats.MinimumPhysBaseDmgByWeapon).BaseValue, Is.EqualTo(109));
        Assert.That(sonicBlade.BasePowerUpAttributes.Single(p => p.TargetAttribute == Stats.MaximumPhysBaseDmgByWeapon).BaseValue, Is.EqualTo(116));
        Assert.That(sonicBlade.MaximumSockets, Is.EqualTo(3));
        Assert.That(sonicBlade.PossibleItemOptions.Select(o => o.Name.ValueInNeutralLanguage), Does.Contain("Socket Options (Fire)"));
        Assert.That(sonicBlade.Requirements.Single(r => r.Attribute == Stats.Level).MinimumValue, Is.EqualTo(400));
        Assert.That(sonicBlade.QualifiedCharacters, Is.Not.Empty);
        Assert.That(sonicBlade.ItemSlot, Is.Not.Null);
        Assert.That(sonicBlade.StatProfile, Is.Not.Null);

        var wings = Item(gameConfiguration, 12, 268);
        Assert.That(wings.Name.ValueInNeutralLanguage, Is.EqualTo("Wings of Conqueror"));
        Assert.That(wings.ItemSlot, Is.SameAs(Item(gameConfiguration, 12, 36).ItemSlot));
    }

    /// <summary>
    /// Every item knows the version which introduced it, so content activation can switch the gear off by season.
    /// </summary>
    [Test]
    public async Task EveryItemHasItsVersionAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);

        await new AddSeason21GearUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var gear = VersionSeasonSix.Season21.Season21GearData.Items.Select(row => Item(gameConfiguration, row.Group, row.Number)).ToList();
        Assert.That(gear.Select(i => i.IntroducedIn), Has.None.EqualTo(GameVersion.Unknown));
        Assert.That(gear.Select(i => i.IntroducedIn), Has.All.GreaterThan(GameVersion.Season6Episode3));
        Assert.That(Item(gameConfiguration, 0, 29).IntroducedIn, Is.EqualTo(GameVersion.Season7Episode1)); // Sonic Blade
        Assert.That(Item(gameConfiguration, 8, 54).IntroducedIn, Is.EqualTo(GameVersion.Season7Episode2)); // Lazy Wind Armor
        Assert.That(Item(gameConfiguration, 13, 173).IntroducedIn, Is.EqualTo(GameVersion.Season8Episode2)); // Ring of Ultimatum
        Assert.That(Item(gameConfiguration, 12, 268).IntroducedIn, Is.EqualTo(GameVersion.Season21Episode1)); // Wings of Conqueror
    }

    /// <summary>
    /// Applying the update a second time changes nothing.
    /// </summary>
    [Test]
    public async Task UpdateTwiceChangesNothingAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);
        var update = new AddSeason21GearUpdatePlugIn();

        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        var items = gameConfiguration.Items.Count;
        var boxItems = gameConfiguration.Items.SelectMany(i => i.DropItems).Sum(d => d.PossibleItems.Count);
        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(gameConfiguration.Items.Count, Is.EqualTo(items));
        Assert.That(gameConfiguration.Items.SelectMany(i => i.DropItems).Sum(d => d.PossibleItems.Count), Is.EqualTo(boxItems));
        Assert.That(gameConfiguration.Items.GroupBy(i => (i.Group, i.Number)).Where(g => g.Count() > 1), Is.Empty);
    }

    /// <summary>
    /// Every name of the item names of Season 21 is used by the update, and the resources are registered as source.
    /// </summary>
    [Test]
    public async Task EveryNameIsUsedAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);

        await new AddSeason21GearUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        ConfigurationNameSources.Register();
        Assert.That(LocalizedStringResources.TryGetName(Season21ItemNames.ResourceManager, out var name), Is.True);
        Assert.That(name, Is.EqualTo(nameof(Season21ItemNames)));
        var keys = Season21ItemNames.ResourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, false)!
            .Cast<DictionaryEntry>().Select(entry => $"{nameof(Season21ItemNames)}/{entry.Key}");
        Assert.That(gameConfiguration.Items.Select(i => i.Name.SourceKey).Where(k => k?.StartsWith(nameof(Season21ItemNames), StringComparison.Ordinal) == true), Is.EquivalentTo(keys));
    }

    /// <summary>
    /// A new database contains David's refinery, which refines a lucky armor into a jewel of extension.
    /// </summary>
    [Test]
    public async Task NewDatabaseContainsTheLuckyItemRefineryAsync()
    {
        var (_, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);

        AssertRefinery(gameConfiguration);
    }

    /// <summary>
    /// The lucky items update adds the refinery to a database which got the lucky items by its first version.
    /// </summary>
    [Test]
    public async Task LuckyItemsUpdateAddsTheRefineryAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);
        var david = gameConfiguration.Monsters.Single(m => m.NpcWindow == NpcWindow.CombineLuckyItem);
        david.ItemCraftings.Remove(david.ItemCraftings.Single(c => c.Number == 52));

        await new AddLuckyItemsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddLuckyItemsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        AssertRefinery(gameConfiguration);
    }

    /// <summary>
    /// The Ring of Block (13/174) doesn't drop: the Season 21 client has no row for it, so it couldn't show it. A second
    /// run of the update keeps the drop switch an operator set.
    /// </summary>
    [Test]
    public async Task RingOfBlockDoesNotDropAndTheDropSwitchIsKeptAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);
        await new AddSeason21GearUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        var ring = Item(gameConfiguration, 13, 174);
        var symbol = Item(gameConfiguration, 13, 173);
        var dropsBefore = symbol.DropsFromMonsters;
        symbol.DropsFromMonsters = !dropsBefore;

        await new AddSeason21GearUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(ring.DropsFromMonsters, Is.False);
        Assert.That(symbol.DropsFromMonsters, Is.EqualTo(!dropsBefore));
    }

    private static void AssertRefinery(GameConfiguration gameConfiguration)
    {
        var david = gameConfiguration.Monsters.Single(m => m.NpcWindow == NpcWindow.CombineLuckyItem);
        var exchange = david.ItemCraftings.Single(c => c.Number == 51).SimpleCraftingSettings!;
        var refinery = david.ItemCraftings.Single(c => c.Number == 52);
        Assert.That(refinery.ItemCraftingHandlerClassName, Is.EqualTo("MUnique.OpenMU.GameLogic.PlayerActions.Craftings.LuckyItemRefineryCrafting"));
        Assert.That(refinery.SimpleCraftingSettings!.RequiredItems.Single().PossibleItems, Is.EquivalentTo(exchange.ResultItems.Select(r => r.ItemDefinition)));
        Assert.That(refinery.SimpleCraftingSettings.ResultItems.Select(r => r.ItemDefinition!.Number), Is.EqualTo(new short[] { 160 }));
        Assert.That(refinery.SimpleCraftingSettings.SuccessPercent, Is.EqualTo(60));
    }

    private static ItemDefinition Item(GameConfiguration gameConfiguration, byte group, short number)
        => gameConfiguration.Items.Single(i => i.Group == group && i.Number == number);

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
