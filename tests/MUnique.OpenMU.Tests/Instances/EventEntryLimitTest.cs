// <copyright file="EventEntryLimitTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Instances;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// Tests the daily entry limits of the events (S21Emu <c>Event/EventEntryLimit.dat</c>).
/// </summary>
[TestFixture]
public class EventEntryLimitTest
{
    private static readonly DateOnly Today = new(2026, 11, 7);

    /// <summary>
    /// A character enters up to the limit a day; the next day counts again, another character and an event without a
    /// limit aren't affected.
    /// </summary>
    [Test]
    public void EntriesStopAtTheLimitOfTheDay()
    {
        var counter = new EventEntryCounter();
        var limits = new List<EventEntryLimit> { new() { Type = MiniGameType.BloodCastle, Limit = 2 } };
        var character = Guid.NewGuid();

        Assert.That(counter.IsLimitReached(character, MiniGameType.BloodCastle, limits, Today), Is.False);
        counter.Count(character, MiniGameType.BloodCastle, Today);
        counter.Count(character, MiniGameType.BloodCastle, Today);

        Assert.That(counter.IsLimitReached(character, MiniGameType.BloodCastle, limits, Today), Is.True);
        Assert.That(counter.IsLimitReached(character, MiniGameType.BloodCastle, limits, Today.AddDays(1)), Is.False);
        Assert.That(counter.IsLimitReached(Guid.NewGuid(), MiniGameType.BloodCastle, limits, Today), Is.False);
        Assert.That(counter.IsLimitReached(character, MiniGameType.DevilSquare, limits, Today), Is.False);
    }

    /// <summary>
    /// A count of the day before starts again at one.
    /// </summary>
    [Test]
    public void NewDayStartsAgain()
    {
        var counter = new EventEntryCounter();
        var limits = new List<EventEntryLimit> { new() { Type = MiniGameType.ChaosCastle, Limit = 1 } };
        var character = Guid.NewGuid();
        counter.Count(character, MiniGameType.ChaosCastle, Today);

        counter.Count(character, MiniGameType.ChaosCastle, Today.AddDays(1));

        Assert.That(counter.IsLimitReached(character, MiniGameType.ChaosCastle, limits, Today.AddDays(1)), Is.True);
        Assert.That(counter.IsLimitReached(character, MiniGameType.ChaosCastle, limits, Today.AddDays(2)), Is.False);
    }
}
