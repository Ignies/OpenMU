// <copyright file="EventEntryLimitTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Instances;

using System.IO;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
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

    /// <summary>
    /// The counts are kept in the state file (<see cref="Season21EventsConfiguration.EntryLimitStateFile"/>): a restart keeps
    /// them, and every game server of the process counts into the same store.
    /// </summary>
    [Test]
    public void CountsAreKeptInTheStateFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"openmu-entries-{Guid.NewGuid():N}.json");
        var limits = new List<EventEntryLimit> { new() { Type = MiniGameType.BloodCastle, Limit = 2 } };
        var character = Guid.NewGuid();
        try
        {
            var server1 = new EventEntryCounter(path);
            var server2 = new EventEntryCounter(path);
            server1.Count(character, MiniGameType.BloodCastle, Today);
            server2.Count(character, MiniGameType.BloodCastle, Today);

            Assert.That(server1.IsLimitReached(character, MiniGameType.BloodCastle, limits, Today), Is.True, "both servers count");
            var stored = new WorldStateFile(path).Load<Dictionary<Guid, CharacterEventEntries>>();
            Assert.That(stored?[character].Day, Is.EqualTo(Today));
            Assert.That(stored?[character].Counts[MiniGameType.BloodCastle], Is.EqualTo(2));
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// The plugin keeps its counts in the state file of its configuration; existing configurations get the default file.
    /// </summary>
    [Test]
    public void PlugInUsesTheStateFileOfItsConfiguration()
    {
        Assert.That(new Season21EventsConfiguration().EntryLimitStateFile, Is.EqualTo("state/event_entries.json"));
        var path = Path.Combine(Path.GetTempPath(), $"openmu-entries-{Guid.NewGuid():N}.json");
        var character = Guid.NewGuid();
        try
        {
            var plugIn = new Season21EventsFeaturePlugIn { Configuration = new Season21EventsConfiguration { EntryLimitStateFile = path } };
            plugIn.Entries.Count(character, MiniGameType.ChaosCastle, Today);

            Assert.That(new WorldStateFile(path).Load<Dictionary<Guid, CharacterEventEntries>>()?[character].Counts[MiniGameType.ChaosCastle], Is.EqualTo(1));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
