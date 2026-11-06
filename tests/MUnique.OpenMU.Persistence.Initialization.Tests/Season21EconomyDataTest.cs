// <copyright file="Season21EconomyDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.GremoryCase;
using MUnique.OpenMU.GameLogic.ItemExpiry;
using MUnique.OpenMU.GameLogic.Ruud;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Tests the optional updates of the economy of Season 21: the gremory case, Ruud and its shop, and the NPC shops with tabs.
/// </summary>
[TestFixture]
internal class Season21EconomyDataTest
{
    private const short PriestJames = 725;
    private const short Hanzo = 251;

    /// <summary>
    /// The economy update places Priest James, fills the Ruud shop and switches the plugins on.
    /// </summary>
    [Test]
    public async Task EconomyUpdateAddsTheRuudShopAsync()
    {
        var (context, gameConfiguration) = await CreateAsync().ConfigureAwait(false);

        await new AddSeason21EconomyUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(gameConfiguration.Monsters.Single(m => m.Number == PriestJames).ObjectKind, Is.EqualTo(NpcObjectKind.PassiveNpc));
        var spawns = gameConfiguration.Maps.SelectMany(m => m.MonsterSpawns).Where(s => s.MonsterDefinition?.Number == PriestJames).ToList();
        Assert.That(spawns.Select(s => (s.GameMap!.Number, s.X1, s.Y1)), Is.EquivalentTo(new[] { ((short)51, (byte)32, (byte)239), ((short)79, (byte)215, (byte)56) }));

        var ruud = GetConfiguration<RuudConfiguration>(gameConfiguration, typeof(RuudFeaturePlugIn).GUID, out var ruudPlugIn);
        Assert.That(ruudPlugIn.IsActive, Is.True);
        Assert.That(ruud.ShopEntries, Has.Count.EqualTo(200));
        var spirit = ruud.ShopEntries.First();
        Assert.That((spirit.Class, spirit.Group, spirit.Number, spirit.Price), Is.EqualTo(((BaseClassCode?)null, (byte)14, (short)341, 50_000)), "Darkangel Spirit.");
        Assert.That(ruud.ShopEntries.Where(e => e.Class == BaseClassCode.DarkWizard).Select(e => e.Number), Does.Contain((short)359), "Sealed Bloodangel armor of the dark wizard.");
        Assert.That(ruud.BoxRewards.Single(b => b.Group == 14 && b.Number == 352).Amount, Is.EqualTo(1_000));
        Assert.That(ruud.MonsterRewards.Single(m => m.MonsterNumber == 55), Has.Property(nameof(RuudMonsterReward.Maximum)).EqualTo(100), "Skeleton King.");
        Assert.That(ruud.ShopEntries.Count(e => gameConfiguration.Items.Any(d => d.Group == e.Group && d.Number == e.Number)), Is.GreaterThan(150), "The item catalog has most items.");

        Assert.That(gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(GremoryCaseFeaturePlugIn).GUID).IsActive, Is.True);
        Assert.That(gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(ItemExpiryPlugIn).GUID).IsActive, Is.True);
    }

    /// <summary>
    /// The shops update gives Hanzo five tabs; the first is his Season 6 store, filled with the first tab of Season 21.
    /// </summary>
    [Test]
    public async Task ShopsUpdateSetsTheTabsAsync()
    {
        var (context, gameConfiguration) = await CreateAsync().ConfigureAwait(false);

        await new AddSeason21ShopsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var hanzo = gameConfiguration.Monsters.Single(m => m.Number == Hanzo);
        Assert.That(hanzo.GetStoreTabCount(), Is.EqualTo(5));
        Assert.That(hanzo.MerchantStoreTabs.Select(t => t.Index), Is.EquivalentTo(new byte[] { 1, 2, 3, 4 }));
        var first = hanzo.MerchantStore!.Items;
        Assert.That(first.Any(i => i.Definition!.Group == 7 && i.Definition.Number == 5 && i.Level == 3), Is.True, "Leather Helm +3 of the knight tab.");
        foreach (var storage in hanzo.MerchantStoreTabs.Select(t => t.Items!).Append(hanzo.MerchantStore))
        {
            var slots = storage.Items.Select(i => i.ItemSlot).ToList();
            Assert.That(slots, Is.Unique);
            Assert.That(slots, Has.All.LessThan(120));
            Assert.That(storage.Items.All(i => i.ItemSlot % 8 + i.Definition!.Width <= 8 && i.ItemSlot / 8 + i.Definition.Height <= 15), Is.True, "Every item fits the grid.");
        }
    }

    /// <summary>
    /// Both updates applied twice change nothing, and keep a switch the operator turned off.
    /// </summary>
    [Test]
    public async Task UpdatesTwiceChangeNothingAsync()
    {
        var (context, gameConfiguration) = await CreateAsync().ConfigureAwait(false);
        var economy = new AddSeason21EconomyUpdatePlugIn();
        var shops = new AddSeason21ShopsUpdatePlugIn();
        await economy.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await shops.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        var ruudPlugIn = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(RuudFeaturePlugIn).GUID);
        ruudPlugIn.IsActive = false;
        var first = Count(gameConfiguration);

        await economy.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await shops.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(Count(gameConfiguration), Is.EqualTo(first));
        Assert.That(ruudPlugIn.IsActive, Is.False);
    }

    /// <summary>
    /// The updates are optional and need the item catalog.
    /// </summary>
    [Test]
    public void UpdatesAreOptionalAfterTheItemCatalog()
    {
        foreach (var update in new UpdatePlugInBase[] { new AddSeason21EconomyUpdatePlugIn(), new AddSeason21ShopsUpdatePlugIn() })
        {
            Assert.That(update.IsMandatory, Is.False);
            Assert.That(update.DependsOn.Select(d => d.Key), Does.Contain(new AddSeason21ItemCatalogUpdatePlugIn().Key));
        }
    }

    private static T GetConfiguration<T>(GameConfiguration gameConfiguration, Guid typeId, out PlugInConfiguration plugIn)
        where T : class
    {
        plugIn = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeId);
        return plugIn.GetConfiguration<T>(new IdIgnoringReferenceHandler())!;
    }

    private static (int Monsters, int Spawns, int StoreItems, int Tabs, string Ruud) Count(GameConfiguration gameConfiguration)
        => (gameConfiguration.Monsters.Count,
            gameConfiguration.Maps.Sum(m => m.MonsterSpawns.Count),
            gameConfiguration.Monsters.Sum(m => (m.MerchantStore?.Items.Count ?? 0) + m.MerchantStoreTabs.Sum(t => t.Items?.Items.Count ?? 0)),
            gameConfiguration.Monsters.Sum(m => m.MerchantStoreTabs.Count),
            gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(RuudFeaturePlugIn).GUID).CustomConfiguration ?? string.Empty);

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        var dataInitialization = new VersionSeasonSix.DataInitialization(contextProvider, new NullLoggerFactory());
        await dataInitialization.CreateInitialDataAsync(1, true).ConfigureAwait(false);
        var context = contextProvider.CreateNewContext();
        var gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        return (context, gameConfiguration);
    }
}
