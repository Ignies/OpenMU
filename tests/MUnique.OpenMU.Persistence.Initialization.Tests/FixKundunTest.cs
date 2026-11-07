// <copyright file="FixKundunTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;

/// <summary>
/// Tests Kundun (275), the boss of Kalima 7: level 147 and 5,000,000 health, not a copy of the Illusion of Kundun of
/// Kalima 6 (338), and the <see cref="FixKundunUpdatePlugIn"/>.
/// </summary>
[TestFixture]
internal class FixKundunTest
{
    private const short KundunNumber = 275;

    /// <summary>
    /// A new database has the boss numbers.
    /// </summary>
    [Test]
    public async Task NewDatabaseHasTheBossNumbersAsync()
    {
        var kundun = Kundun(await CreateSeason6Async().ConfigureAwait(false));

        AssertBossNumbers(kundun);
    }

    /// <summary>
    /// The update replaces the copied illusion numbers.
    /// </summary>
    [Test]
    public async Task UpdateReplacesTheCopiedIllusionAsync()
    {
        var gameConfiguration = await CreateSeason6Async().ConfigureAwait(false);
        var kundun = Kundun(gameConfiguration);
        SetAttribute(kundun, Stats.Level, 140);
        SetAttribute(kundun, Stats.MaximumHealth, 140000);
        SetAttribute(kundun, Stats.DefenseBase, 892);
        SetAttribute(kundun, Stats.PoisonResistance, 60f / 255);
        kundun.RespawnDelay = TimeSpan.FromSeconds(10800);

        await new FixKundunUpdatePlugIn().ApplyUpdateAsync(new InMemoryPersistenceContextProvider().CreateNewContext(), gameConfiguration).ConfigureAwait(false);

        AssertBossNumbers(kundun);
    }

    /// <summary>
    /// The update keeps the health an operator chose.
    /// </summary>
    [Test]
    public async Task UpdateKeepsAnOperatorValueAsync()
    {
        var gameConfiguration = await CreateSeason6Async().ConfigureAwait(false);
        var kundun = Kundun(gameConfiguration);
        SetAttribute(kundun, Stats.MaximumHealth, 2000000);

        await new FixKundunUpdatePlugIn().ApplyUpdateAsync(new InMemoryPersistenceContextProvider().CreateNewContext(), gameConfiguration).ConfigureAwait(false);

        Assert.That(kundun[Stats.MaximumHealth], Is.EqualTo(2000000));
    }

    private static void AssertBossNumbers(MonsterDefinition kundun)
    {
        Assert.That(kundun[Stats.Level], Is.EqualTo(147));
        Assert.That(kundun[Stats.MaximumHealth], Is.EqualTo(5000000));
        Assert.That(kundun[Stats.MinimumPhysBaseDmg], Is.EqualTo(2000));
        Assert.That(kundun[Stats.MaximumPhysBaseDmg], Is.EqualTo(2500));
        Assert.That(kundun[Stats.DefenseBase], Is.EqualTo(1500));
        Assert.That(kundun[Stats.AttackRatePvm], Is.EqualTo(2000));
        Assert.That(kundun[Stats.DefenseRatePvm], Is.EqualTo(1000));
        Assert.That(kundun[Stats.PoisonResistance], Is.EqualTo(254f / 255));
        Assert.That(kundun[Stats.IceResistance], Is.EqualTo(100f / 255));
        Assert.That(kundun.RespawnDelay, Is.EqualTo(TimeSpan.FromSeconds(43200)));
        Assert.That(kundun.Designation.ToString(), Does.Not.Contain("Illusion"));
    }

    private static void SetAttribute(MonsterDefinition monster, AttributeSystem.AttributeDefinition attribute, float value)
        => monster.Attributes.Single(a => a.AttributeDefinition == attribute).Value = value;

    private static MonsterDefinition Kundun(GameConfiguration gameConfiguration)
        => gameConfiguration.Monsters.Single(m => m.Number == KundunNumber);

    private static async Task<GameConfiguration> CreateSeason6Async()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        await new VersionSeasonSix.DataInitialization(contextProvider, new NullLoggerFactory()).CreateInitialDataAsync(1, true).ConfigureAwait(false);
        return (await contextProvider.CreateNewContext().GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
    }
}
