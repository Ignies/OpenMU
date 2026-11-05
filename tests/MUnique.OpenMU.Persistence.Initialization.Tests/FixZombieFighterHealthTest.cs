// <copyright file="FixZombieFighterHealthTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;

/// <summary>
/// Tests the health of the Zombie Fighter of Vulcanus (481): 500,000 like its siblings (513,000 to 530,000), not the
/// 5,000,000 of a digit slip, and the <see cref="FixZombieFighterHealthUpdatePlugIn"/>.
/// </summary>
[TestFixture]
internal class FixZombieFighterHealthTest
{
    private const short ZombieFighterNumber = 481;

    /// <summary>
    /// A new database has the right health.
    /// </summary>
    [Test]
    public async Task NewDatabaseHasTheRightHealthAsync()
    {
        var gameConfiguration = await CreateSeason6Async().ConfigureAwait(false);

        Assert.That(Health(gameConfiguration).Value, Is.EqualTo(500000));
    }

    /// <summary>
    /// The update corrects the slipped value, and keeps a value an operator chose.
    /// </summary>
    /// <param name="before">The health before the update.</param>
    /// <param name="after">The expected health after it.</param>
    [TestCase(5000000, 500000)]
    [TestCase(600000, 600000)]
    public async Task UpdateCorrectsOnlyTheSlippedValueAsync(float before, float after)
    {
        var gameConfiguration = await CreateSeason6Async().ConfigureAwait(false);
        Health(gameConfiguration).Value = before;

        await new FixZombieFighterHealthUpdatePlugIn().ApplyUpdateAsync(new InMemoryPersistenceContextProvider().CreateNewContext(), gameConfiguration).ConfigureAwait(false);

        Assert.That(Health(gameConfiguration).Value, Is.EqualTo(after));
    }

    private static MonsterAttribute Health(GameConfiguration gameConfiguration)
        => gameConfiguration.Monsters.Single(m => m.Number == ZombieFighterNumber).Attributes.Single(a => a.AttributeDefinition == Stats.MaximumHealth);

    private static async Task<GameConfiguration> CreateSeason6Async()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        await new VersionSeasonSix.DataInitialization(contextProvider, new NullLoggerFactory()).CreateInitialDataAsync(1, true).ConfigureAwait(false);
        return (await contextProvider.CreateNewContext().GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
    }
}
