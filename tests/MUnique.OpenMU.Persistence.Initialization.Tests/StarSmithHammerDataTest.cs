// <copyright file="StarSmithHammerDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;

/// <summary>
/// Tests the Star Smith's Hammer (2/1000), the tool of the star smithing, which Hanzo the Blacksmith sells.
/// </summary>
[TestFixture]
internal class StarSmithHammerDataTest
{
    private const byte HammerGroup = 2;
    private const short HammerNumber = 1000;
    private const short HanzoNumber = 251;
    private const string UpdateTypeName = "MUnique.OpenMU.Persistence.Initialization.Updates.AddStarSmithHammerUpdatePlugIn, MUnique.OpenMU.Persistence.Initialization";

    /// <summary>
    /// The buying price of the hammer: its value 1096 with the value based price, floor(1096^2 * 10 / 12) = 1,001,013, rounded down to hundreds.
    /// </summary>
    private const long ExpectedPrice = 1_001_000;

    /// <summary>
    /// Tests that a new season 6 database has the hammer, and that Hanzo sells it.
    /// </summary>
    /// <returns>The task.</returns>
    [Test]
    public async Task NewDatabaseHasTheHammerAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6ConfigurationAsync().ConfigureAwait(false);
        using var _ = context;

        AssertHammer(gameConfiguration);

        // The first free place of three rows: row 9, column 4, beside Kris, Gladius, Falchion, Serpent Sword and Blade.
        Assert.That(GetHanzo(gameConfiguration).MerchantStore!.Items.Single(IsHammer).ItemSlot, Is.EqualTo(76));
    }

    /// <summary>
    /// Tests that the update adds the hammer and Hanzo's offer to a database which doesn't have them,
    /// and that applying it a second time changes nothing.
    /// </summary>
    /// <returns>The task.</returns>
    [Test]
    public async Task UpdateAddsTheHammerOnceAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6ConfigurationAsync().ConfigureAwait(false);
        using var _ = context;
        var store = GetHanzo(gameConfiguration).MerchantStore!;
        foreach (var offer in store.Items.Where(IsHammer).ToList())
        {
            store.Items.Remove(offer);
        }

        foreach (var definition in gameConfiguration.Items.Where(IsHammer).ToList())
        {
            gameConfiguration.Items.Remove(definition);
        }

        var update = CreateUpdate();
        Assert.That(update.IsMandatory, Is.True);
        Assert.That(update.DataInitializationKey, Is.EqualTo(VersionSeasonSix.DataInitialization.Id));

        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        var itemCount = gameConfiguration.Items.Count;
        var offers = store.Items.Select(item => (item.Definition, item.ItemSlot)).ToList();
        var priceDefinitionCount = gameConfiguration.ItemPriceDefinitions.Count;

        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        AssertHammer(gameConfiguration);
        Assert.That(gameConfiguration.Items, Has.Count.EqualTo(itemCount));
        Assert.That(store.Items.Select(item => (item.Definition, item.ItemSlot)), Is.EqualTo(offers));
        Assert.That(gameConfiguration.ItemPriceDefinitions, Has.Count.EqualTo(priceDefinitionCount));
    }

    /// <summary>
    /// Tests that the update keeps the hammer which an operator already has, and adds only the missing offer.
    /// </summary>
    /// <returns>The task.</returns>
    [Test]
    public async Task UpdateKeepsAnExistingHammerAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6ConfigurationAsync().ConfigureAwait(false);
        using var _ = context;
        var store = GetHanzo(gameConfiguration).MerchantStore!;
        foreach (var offer in store.Items.Where(IsHammer).ToList())
        {
            store.Items.Remove(offer);
        }

        var hammer = gameConfiguration.Items.Single(IsHammer);
        hammer.Durability = 100;

        await CreateUpdate().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(gameConfiguration.Items.Count(IsHammer), Is.EqualTo(1));
        Assert.That(hammer.Durability, Is.EqualTo(100));
        Assert.That(store.Items.Count(IsHammer), Is.EqualTo(1));
    }

    /// <summary>
    /// Tests that the Season 21 shops, which refill Hanzo's first tab, keep the hammer in a free slot.
    /// </summary>
    /// <returns>The task.</returns>
    [Test]
    public async Task Season21ShopsKeepTheHammerAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6ConfigurationAsync().ConfigureAwait(false);
        using var _ = context;
        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        await new AddSeason21ShopsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var store = GetHanzo(gameConfiguration).MerchantStore!;
        Assert.That(store.Items.Any(i => i.Definition is { Group: 7, Number: 5 } && i.Level == 3), Is.True, "Hanzo has the Season 21 tab.");
        Assert.That(store.Items.Count(IsHammer), Is.EqualTo(1));
        AssertNoOverlap(store);
    }

    private static void AssertHammer(GameConfiguration gameConfiguration)
    {
        var hammer = gameConfiguration.Items.FirstOrDefault(IsHammer);
        Assert.That(hammer, Is.Not.Null, "The definition 2/1000 is missing.");
        var mace = gameConfiguration.Items.Single(item => item is { Group: 2, Number: 0 });
        var store = GetHanzo(gameConfiguration).MerchantStore;
        Assert.That(store, Is.Not.Null);
        var offer = store!.Items.SingleOrDefault(IsHammer);

        Assert.Multiple(() =>
        {
            Assert.That(gameConfiguration.Items.Count(IsHammer), Is.EqualTo(1));
            Assert.That(hammer!.Name.ValueInNeutralLanguage, Is.EqualTo("Star Smith's Hammer"));
            Assert.That(hammer.Name.SourceKey, Is.EqualTo("ItemNames/StarSmithsHammer"));
            Assert.That(hammer.Width, Is.EqualTo(mace.Width));
            Assert.That(hammer.Height, Is.EqualTo(mace.Height));
            Assert.That(hammer.DropLevel, Is.EqualTo(mace.DropLevel), "the excellent and ancient bonuses divide by it");
            Assert.That(hammer.ItemSlot, Is.SameAs(mace.ItemSlot), "one-handed, in the right or left hand like the mace");
            Assert.That(hammer.Durability, Is.EqualTo(255));
            Assert.That(hammer.MaximumItemLevel, Is.Zero);
            Assert.That(hammer.PossibleItemOptions, Is.Empty);
            Assert.That(hammer.PossibleItemSetGroups, Is.Empty);
            Assert.That(hammer.MaximumSockets, Is.Zero);
            Assert.That(hammer.Skill, Is.Null);
            Assert.That(hammer.DropsFromMonsters, Is.False);
            Assert.That(hammer.Requirements, Is.Empty);
            Assert.That(hammer.QualifiedCharacters, Is.EquivalentTo(gameConfiguration.CharacterClasses));
            Assert.That(GetBaseValue(hammer, Stats.MinimumPhysBaseDmgByWeapon), Is.EqualTo(4));
            Assert.That(GetBaseValue(hammer, Stats.MaximumPhysBaseDmgByWeapon), Is.EqualTo(8));
            Assert.That(GetBaseValue(hammer, Stats.AttackSpeedByWeapon), Is.EqualTo(GetBaseValue(mace, Stats.AttackSpeedByWeapon)));
            Assert.That(GetBaseValue(hammer, Stats.EquippedWeaponCount), Is.EqualTo(1));
            Assert.That(hammer.Value, Is.EqualTo(1096));
            Assert.That(hammer.PriceDefinition, Is.Not.Null);

            Assert.That(offer, Is.Not.Null, "Hanzo doesn't sell the hammer.");
            Assert.That(offer?.Level, Is.Zero);
            Assert.That(offer?.Durability, Is.EqualTo(255));
            Assert.That(offer?.ItemOptions, Is.Empty);
            Assert.That(offer is null ? 0 : new ItemPriceCalculator().CalculateFinalBuyingPrice(offer), Is.EqualTo(ExpectedPrice));
        });

        AssertNoOverlap(store);
    }

    private static void AssertNoOverlap(ItemStorage store)
    {
        const int columns = 8;
        const int rows = 15;
        var others = store.Items.Where(other => !IsHammer(other)).SelectMany(other => Cells(other, columns)).ToHashSet();
        foreach (var item in store.Items.Where(IsHammer))
        {
            Assert.That(item.ItemSlot % columns, Is.LessThanOrEqualTo(columns - item.Definition!.Width), $"The hammer at {item.ItemSlot} sticks out of the store.");
            Assert.That(Cells(item, columns).All(cell => cell < columns * rows), Is.True, $"The hammer at {item.ItemSlot} sticks out of the store.");
            Assert.That(Cells(item, columns).Any(others.Contains), Is.False, $"The hammer at {item.ItemSlot} overlaps another item.");
        }
    }

    private static IEnumerable<int> Cells(Item item, int columns)
    {
        var x = item.ItemSlot % columns;
        var y = item.ItemSlot / columns;
        for (var dy = 0; dy < item.Definition!.Height; dy++)
        {
            for (var dx = 0; dx < item.Definition.Width; dx++)
            {
                yield return ((y + dy) * columns) + x + dx;
            }
        }
    }

    private static float? GetBaseValue(ItemDefinition definition, AttributeDefinition attribute)
        => definition.BasePowerUpAttributes.FirstOrDefault(p => p.TargetAttribute == attribute)?.BaseValue;

    private static bool IsHammer(ItemDefinition definition) => definition.Group == HammerGroup && definition.Number == HammerNumber;

    private static bool IsHammer(Item item) => item.Definition is { } definition && IsHammer(definition);

    private static MonsterDefinition GetHanzo(GameConfiguration gameConfiguration)
        => gameConfiguration.Monsters.Single(monster => monster.Number == HanzoNumber);

    private static UpdatePlugInBase CreateUpdate()
    {
        var type = Type.GetType(UpdateTypeName);
        Assert.That(type, Is.Not.Null, "The update AddStarSmithHammerUpdatePlugIn is missing.");
        return (UpdatePlugInBase)Activator.CreateInstance(type!)!;
    }

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateSeason6ConfigurationAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        var dataInitialization = new VersionSeasonSix.DataInitialization(contextProvider, new NullLoggerFactory());
        await dataInitialization.CreateInitialDataAsync(1, true).ConfigureAwait(false);

        var context = contextProvider.CreateNewContext();
        var gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        return (context, gameConfiguration);
    }
}
