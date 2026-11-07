// <copyright file="Season21LuckySetsDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;

/// <summary>
/// Tests the set options of the lucky items (S21Emu SetItemType.xml and SetItemOption.xml).
/// </summary>
[TestFixture]
internal class Season21LuckySetsDataTest
{
    /// <summary>
    /// The Moros set (lucky armors 62) always applies: 30 health, 25 skill damage, 20 defense and 10 % double damage by
    /// the count, the 10 % critical and excellent chances and 10 critical and excellent damage on the full set.
    /// </summary>
    [Test]
    public async Task MorosSetHasItsOptionsAsync()
    {
        var gameConfiguration = await CreateAsync().ConfigureAwait(false);

        var armor = gameConfiguration.Items.Single(i => i.Group == 8 && i.Number == 62);
        var moros = armor.PossibleItemSetGroups.Single(g => g.Name.ValueInNeutralLanguage == "Moros");
        Assert.That((moros.AlwaysApplies, moros.CountDistinct, moros.MinimumItemCount, moros.Items.Count), Is.EqualTo((true, true, 2, 5)));
        var options = moros.Options!.PossibleOptions.OrderBy(o => o.Number).Select(o => (o.PowerUpDefinition!.TargetAttribute, o.PowerUpDefinition.Boost!.ConstantValue.Value)).ToList();
        Assert.That(options, Is.EqualTo(new[]
        {
            (Stats.MaximumHealth, 30f),
            (Stats.SkillDamageBonus, 25f),
            (Stats.DefenseBase, 20f),
            (Stats.DoubleDamageChance, 0.1f),
            (Stats.CriticalDamageChance, 0.1f),
            (Stats.ExcellentDamageChance, 0.1f),
            (Stats.CriticalDamageBonus, 10f),
            (Stats.ExcellentDamageBonus, 10f),
        }));
    }

    /// <summary>
    /// The 29 lucky sets of the data are added once; a second run adds nothing.
    /// </summary>
    [Test]
    public async Task UpdateAddsEveryLuckySetOnceAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        var (context, gameConfiguration) = await CreateContextAsync(contextProvider).ConfigureAwait(false);
        var before = gameConfiguration.ItemSetGroups.Count;
        await new AddSeason21LuckySetsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21LuckySetsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        Assert.That(gameConfiguration.ItemSetGroups.Count - before, Is.EqualTo(29));
        var amis = gameConfiguration.ItemSetGroups.Single(g => g.Name.ValueInNeutralLanguage == "Amis");
        Assert.That((amis.Items.Count, amis.Options!.PossibleOptions.Count), Is.EqualTo((4, 8)));
    }

    /// <summary>
    /// Version 2 gives the lucky pieces what the rolls of the ticket exchange need: the defense option (the Jewel of Life
    /// option) and the defense increase by level of the armors. The maximum level stays, so the jewels stay off them.
    /// </summary>
    [Test]
    public async Task LuckyPiecesTakeTheRolledLevelAndOptionAsync()
    {
        var gameConfiguration = await CreateAsync().ConfigureAwait(false);

        foreach (var piece in gameConfiguration.ItemSetGroups.Where(g => g.AlwaysApplies && g.Options?.Name.ValueInNeutralLanguage.EndsWith(" Lucky Set", StringComparison.Ordinal) is true).SelectMany(g => g.Items).Select(i => i.ItemDefinition!))
        {
            Assert.That(piece.PossibleItemOptions.SelectMany(o => o.PossibleOptions).Count(o => o.OptionType == ItemOptionTypes.Option), Is.EqualTo(1), piece.Name.ValueInNeutralLanguage);
            Assert.That(piece.BasePowerUpAttributes.Single(p => p.TargetAttribute == Stats.DefenseBase).BonusPerLevelTable, Is.Not.Null, piece.Name.ValueInNeutralLanguage);
            Assert.That(piece.MaximumItemLevel, Is.EqualTo(0));
        }

        Assert.That(new AddSeason21LuckySetsUpdatePlugIn().Version, Is.EqualTo(2));
    }

    private static async Task<GameConfiguration> CreateAsync()
    {
        var (context, gameConfiguration) = await CreateContextAsync(new InMemoryPersistenceContextProvider()).ConfigureAwait(false);
        await new AddSeason21LuckySetsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        return gameConfiguration;
    }

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateContextAsync(InMemoryPersistenceContextProvider contextProvider)
    {
        await new VersionSeasonSix.DataInitialization(contextProvider, new NullLoggerFactory()).CreateInitialDataAsync(1, true).ConfigureAwait(false);
        var context = contextProvider.CreateNewContext();
        var gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        if (gameConfiguration.Items.All(i => !(i.Group == 8 && i.Number == 62)))
        {
            await new AddLuckyItemsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        }

        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        return (context, gameConfiguration);
    }
}
