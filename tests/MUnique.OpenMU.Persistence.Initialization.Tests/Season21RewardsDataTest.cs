// <copyright file="Season21RewardsDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Rewards;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Tests the optional update of the level rewards and the event contribution boxes of Season 21.
/// </summary>
[TestFixture]
internal class Season21RewardsDataTest
{
    /// <summary>
    /// The update configures both plugins with the numbers of the data and switches them on.
    /// </summary>
    [Test]
    public async Task RewardsUpdateConfiguresBothPlugInsAsync()
    {
        var (context, gameConfiguration) = await CreateAsync().ConfigureAwait(false);

        await new AddSeason21RewardsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var levels = GetConfiguration<LevelRewardsConfiguration>(gameConfiguration, typeof(LevelRewardsPlugIn).GUID, out var levelPlugIn);
        Assert.That(levelPlugIn.IsActive, Is.True);
        Assert.That(levels.Rewards.Select(r => r.TotalLevel), Is.EqualTo(Enumerable.Range(0, 17).Select(i => 950 + (i * 50))), "950 to 1750 in steps of 50.");
        Assert.That((levels.Rewards[0].Group, levels.Rewards[0].Number, levels.Rewards[0].Classes.Count), Is.EqualTo(((byte)16, (short)69, 0)), "The first Muun, for every class.");

        var contribution = GetConfiguration<EventContributionConfiguration>(gameConfiguration, typeof(EventContributionPlugIn).GUID, out var contributionPlugIn);
        Assert.That(contributionPlugIn.IsActive, Is.True);
        Assert.That(contribution.Boxes.Select(b => (b.Number, b.MinimumPoints, b.MaximumPoints)), Is.EqualTo(new[] { ((short)282, 100, 400), ((short)283, 401, 800), ((short)284, 801, 0) }));
        var bloodCastle = contribution.Rules.Single(r => r.Type == MiniGameType.BloodCastle);
        Assert.That(bloodCastle.MonsterKillPoints, Is.EqualTo(5));
        Assert.That(bloodCastle.SpecialMonsters.Select(m => (m.MonsterNumber, m.Points)), Is.EquivalentTo(new[] { ((short)131, 100), ((short)132, 200) }));
        var guardian = contribution.Rules.Single(r => r.Type == MiniGameType.ImperialGuardian);
        Assert.That((guardian.MonsterKillPoints, guardian.SundayMonsterKillPoints), Is.EqualTo((45, 22)));
        Assert.That(contribution.Rules.Single(r => r.Type == MiniGameType.ChaosCastle), Has.Property(nameof(EventContributionRule.WinnerPoints)).EqualTo(99));
        Assert.That(contribution.Rules.Select(r => r.Type).Distinct().Count(), Is.EqualTo(6));
    }

    /// <summary>
    /// Applied twice, the update changes nothing and keeps a switch the operator turned off.
    /// </summary>
    [Test]
    public async Task RewardsUpdateTwiceChangesNothingAsync()
    {
        var (context, gameConfiguration) = await CreateAsync().ConfigureAwait(false);
        var update = new AddSeason21RewardsUpdatePlugIn();
        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        var plugIn = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(EventContributionPlugIn).GUID);
        plugIn.IsActive = false;
        var before = string.Join("|", gameConfiguration.PlugInConfigurations.Select(c => c.CustomConfiguration));

        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(string.Join("|", gameConfiguration.PlugInConfigurations.Select(c => c.CustomConfiguration)), Is.EqualTo(before));
        Assert.That(plugIn.IsActive, Is.False);
        Assert.That(update.IsMandatory, Is.False);
        Assert.That(update.DependsOn.Select(d => d.Key), Does.Contain(new AddSeason21EconomyUpdatePlugIn().Key));
    }

    private static T GetConfiguration<T>(GameConfiguration gameConfiguration, Guid typeId, out PlugInConfiguration plugIn)
        where T : class
    {
        plugIn = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeId);
        return plugIn.GetConfiguration<T>(new IdIgnoringReferenceHandler())!;
    }

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        var dataInitialization = new VersionSeasonSix.DataInitialization(contextProvider, new NullLoggerFactory());
        await dataInitialization.CreateInitialDataAsync(1, true).ConfigureAwait(false);
        var context = contextProvider.CreateNewContext();
        var gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        return (context, gameConfiguration);
    }
}
