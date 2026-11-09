// <copyright file="Season21BagItemsDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.PlayerActions.Craftings;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;

/// <summary>
/// Tests the update which gives the Season 21 items of group 19 their uses: the item bags, the [Lvl. 0] lucky tickets,
/// the pentagram talisman and the Blessing of Eunice.
/// </summary>
[TestFixture]
internal class Season21BagItemsDataTest
{
    private IContext _context = null!;

    private GameConfiguration _gameConfiguration = null!;

    /// <summary>
    /// Creates a Season 6 database with the items and elements of Season 21 and applies the update twice, so every test
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
        await new ActivateSeason21WideItemsUpdatePlugIn().ApplyUpdateAsync(this._context, this._gameConfiguration).ConfigureAwait(false);
        await new AddSeason21ElementsUpdatePlugIn().ApplyUpdateAsync(this._context, this._gameConfiguration).ConfigureAwait(false);
        await new AddSeason21BagItemsUpdatePlugIn().ApplyUpdateAsync(this._context, this._gameConfiguration).ConfigureAwait(false);
        await new AddSeason21BagItemsUpdatePlugIn().ApplyUpdateAsync(this._context, this._gameConfiguration).ConfigureAwait(false);
    }

    /// <summary>
    /// The bags hold the items of their S21Emu <c>EventItemBag</c> (294-297, 308), one drop group per section, and can be
    /// used once (durability 1).
    /// </summary>
    /// <param name="number">The bag number in group 19.</param>
    /// <param name="groupSizes">The item count of each group.</param>
    [TestCase(0, new[] { 1 })] // Pandora Box: the Pandora Pick
    [TestCase(1, new[] { 11 })] // Red Card
    [TestCase(2, new[] { 15 })] // Blue Card
    [TestCase(3, new[] { 179 })] // Premium Muun Box
    [TestCase(14, new[] { 15, 11, 53 })] // Red Card [Elemental]: a pentagram, an errtel or seed sphere, an excellent armor piece
    public void BagsHoldTheItemsOfTheirDataAsync(short number, int[] groupSizes)
    {
        var bag = this.Item(19, number);

        Assert.That(bag.DropItems.Select(g => g.PossibleItems.Count), Is.EqualTo(groupSizes));
        Assert.That(bag.DropItems.Select(g => g.Chance), Has.All.EqualTo(1.0));
        Assert.That(bag.Durability, Is.EqualTo(1));
    }

    /// <summary>
    /// The Pandora Box shows fireworks; the Muuns of the Premium Muun Box are at level 1; the armor of the Red Card
    /// [Elemental] is excellent.
    /// </summary>
    [Test]
    public void BagsKeepTheirDropDetails()
    {
        Assert.That(this.Item(19, 0).DropItems.Single().DropEffect, Is.EqualTo(ItemDropEffect.Fireworks));
        Assert.That(this.Item(19, 0).DropItems.Single().PossibleItems.Single(), Is.SameAs(this.Item(0, 41)));
        Assert.That(this.Item(19, 3).DropItems.Single().MinimumLevel, Is.EqualTo(1));
        Assert.That(this.Item(19, 14).DropItems.Select(g => g.ItemType), Is.EqualTo(new[] { SpecialItemType.RandomItem, SpecialItemType.RandomItem, SpecialItemType.Excellent }));
    }

    /// <summary>
    /// The [Lvl. 0] tickets are taken by David's exchange like the tickets of group 13 of the same piece.
    /// </summary>
    [Test]
    public void LevelZeroTicketsAreExchangedLikeTheirTickets()
    {
        var exchange = this._gameConfiguration.Monsters.SelectMany(m => m.ItemCraftings)
            .Single(c => c.ItemCraftingHandlerClassName == typeof(LuckyItemTicketExchangeCrafting).FullName).SimpleCraftingSettings!;
        for (short i = 0; i < 10; i++)
        {
            var required = exchange.RequiredItems.Single(r => r.PossibleItems.Any(p => p is { Group: 13 } && p.Number == 135 + i));
            Assert.That(required.PossibleItems, Does.Contain(this.Item(19, (short)(4 + i))));
        }

        Assert.That(exchange.RequiredItems.Count(r => r.PossibleItems.Contains(this.Item(19, 4))), Is.EqualTo(1));
    }

    /// <summary>
    /// The pentagram talisman (and the Elemental Talisman of Chaos) keeps the pentagram or errtel of a failed elemental
    /// upgrade, level up or rank up; the Elemental Talisman of Luck adds 10 % (S21Emu <c>ElementalTalismanOfLuckRate</c>).
    /// </summary>
    [Test]
    public void TalismansJoinTheElementalRefinery()
    {
        string[] handlers = [typeof(ElementalUpgradeCrafting).FullName!, typeof(ErrtelLevelUpCrafting).FullName!, typeof(ErrtelRankUpCrafting).FullName!];
        var refinery = this._gameConfiguration.Monsters.SelectMany(m => m.ItemCraftings).Where(c => handlers.Contains(c.ItemCraftingHandlerClassName)).ToList();
        Assert.That(refinery, Is.Not.Empty);
        foreach (var settings in refinery.Select(c => c.SimpleCraftingSettings!))
        {
            var chaos = settings.RequiredItems.Single(r => r.PossibleItems.Contains(this.Item(19, 16)));
            Assert.That(chaos.PossibleItems, Does.Contain(this.Item(14, 190)));
            Assert.That((chaos.MinimumAmount, chaos.MaximumAmount, chaos.FailResult), Is.EqualTo((0, 1, MixResult.DisappearAndProtectReferencedItems)));
            var luck = settings.RequiredItems.Single(r => r.PossibleItems.Contains(this.Item(14, 189)));
            Assert.That((luck.MinimumAmount, luck.MaximumAmount, luck.AddPercentage), Is.EqualTo((0, 1, 10)));
        }
    }

    /// <summary>
    /// The Blessing of Eunice gives 50 % more experience for 30 minutes (S21Emu <c>Effect.txt</c> 447).
    /// </summary>
    [Test]
    public void BlessingOfEuniceGivesMoreExperience()
    {
        var blessing = this.Item(19, 17);

        Assert.That(blessing.Durability, Is.EqualTo(1));
        var effect = blessing.ConsumeEffect!;
        Assert.That(effect.Number, Is.EqualTo(447));
        Assert.That(effect.Duration!.ConstantValue.Value, Is.EqualTo(1800));
        var powerUp = effect.PowerUpDefinitions.Single();
        Assert.That(powerUp.TargetAttribute, Is.EqualTo(Stats.ExperienceRate));
        Assert.That(powerUp.Boost!.ConstantValue.Value, Is.EqualTo(1.5f));
        Assert.That(this._gameConfiguration.MagicEffects.Count(e => e.Number == 447), Is.EqualTo(1));
    }

    private ItemDefinition Item(byte group, short number) => this._gameConfiguration.Items.Single(i => i.Group == group && i.Number == number);
}
