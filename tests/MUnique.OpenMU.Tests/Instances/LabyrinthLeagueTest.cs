// <copyright file="LabyrinthLeagueTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Instances;

using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.GameLogic.Rewards;

/// <summary>
/// Tests the scores and the weekly leagues of the Labyrinth of Dimensions (S21Emu <c>LabyrinthOfDimension_League.xml</c>):
/// the points of a floor by its first and repeated clear of the day, the first play and the fast finish, the groups of five,
/// the promotion and demotion at the start of a league week, the reward of the tier on the reward day and the reset of
/// the tiers with the next month.
/// </summary>
[TestFixture]
public class LabyrinthLeagueTest
{
    /// <summary>
    /// A Monday of November 2026, day 2 of the month: a play day of the first league week.
    /// </summary>
    private static readonly DateTime PlayDay = new(2026, 11, 2, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// The first clear of a floor of the day gives its first points, the first clear of the day the first play bonus, and
    /// a quick clear the fast finish points (250 less a percent per percent of 20 minutes); a repeated clear gives the
    /// repeat points. The record resets with the day.
    /// </summary>
    [Test]
    public void FloorPointsFollowTheDailyRecord()
    {
        var league = CreateLeague(new LabyrinthLeagueConfiguration());
        var id = Guid.NewGuid();
        var tenMinutes = TimeSpan.FromMinutes(10);

        Assert.That(league.ScoreFloorClear(id, 3, tenMinutes, PlayDay), Is.EqualTo(150 + 1000 + 125));
        Assert.That(league.ScoreFloorClear(id, 3, tenMinutes, PlayDay), Is.EqualTo(75 + 125));
        Assert.That(league.ScoreFloorClear(id, 4, TimeSpan.FromMinutes(20), PlayDay), Is.EqualTo(200), "no fast finish after 20 minutes");
        Assert.That(league.ScoreFloorClear(id, 3, tenMinutes, PlayDay.AddDays(1)), Is.EqualTo(150 + 1000 + 125));
    }

    /// <summary>
    /// The players of a week are put into groups of five of their tier.
    /// </summary>
    [Test]
    public void GroupsHaveFivePlayers()
    {
        var league = CreateLeague(new LabyrinthLeagueConfiguration());
        var ids = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToList();
        foreach (var id in ids)
        {
            league.AddPoints(id, 100, PlayDay);
        }

        Assert.That(ids.Select(id => league.GetStanding(id, PlayDay).GroupSize), Is.EqualTo(new[] { 5, 5, 5, 5, 5, 1 }));
        Assert.That(league.GetStanding(ids[5], PlayDay).Rank, Is.EqualTo(1));
    }

    /// <summary>
    /// At the start of the next week, the first two of a group go up a tier and the fifth goes down; Bronze is the lowest.
    /// </summary>
    [Test]
    public void WeekPromotesAndDemotes()
    {
        var league = CreateLeague(new LabyrinthLeagueConfiguration());
        var ids = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToList();
        for (var i = 0; i < ids.Count; i++)
        {
            league.AddPoints(ids[i], 500 - (i * 100), PlayDay);
        }

        var nextWeek = PlayDay.AddDays(7);
        Assert.That(ids.Select(id => league.GetStanding(id, nextWeek).Tier), Is.EqualTo(new[] { LabyrinthLeagueTier.Silver, LabyrinthLeagueTier.Silver, LabyrinthLeagueTier.Bronze, LabyrinthLeagueTier.Bronze, LabyrinthLeagueTier.Bronze }));

        var small = CreateLeague(new LabyrinthLeagueConfiguration { GroupSize = 2, PromotedRanks = 1, DemotedRank = 2 });
        var (a, b, c, d) = (Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        small.AddPoints(a, 100, PlayDay);
        small.AddPoints(b, 50, PlayDay);
        small.AddPoints(c, 100, PlayDay);
        small.AddPoints(d, 50, PlayDay);
        small.AddPoints(a, 10, nextWeek);
        small.AddPoints(c, 20, nextWeek);

        var third = nextWeek.AddDays(7);
        Assert.That((small.GetStanding(a, third).Tier, small.GetStanding(c, third).Tier), Is.EqualTo((LabyrinthLeagueTier.Bronze, LabyrinthLeagueTier.Gold)));
    }

    /// <summary>
    /// Days outside of the schedule (the 30th and 31st) and the reward day count no points.
    /// </summary>
    [Test]
    public void OnlyLeagueDaysCount()
    {
        var league = CreateLeague(new LabyrinthLeagueConfiguration());
        var id = Guid.NewGuid();

        league.AddPoints(id, 100, new DateTime(2026, 11, 30, 12, 0, 0, DateTimeKind.Utc));
        league.AddPoints(id, 100, new DateTime(2026, 11, 29, 12, 0, 0, DateTimeKind.Utc));

        Assert.That(league.GetStanding(id, new DateTime(2026, 11, 29, 13, 0, 0, DateTimeKind.Utc)).Points, Is.Zero);
    }

    /// <summary>
    /// On the reward day, a character gets the reward of its tier once, after the last week moved it; with the next month
    /// every character starts in Bronze again.
    /// </summary>
    [Test]
    public void RewardDayGivesTheTierRewardOnce()
    {
        var configuration = new LabyrinthLeagueConfiguration
        {
            TierRewards = [new LabyrinthTierReward { Tier = LabyrinthLeagueTier.Silver, Item = new RewardItem { Group = 14, Number = 405, Count = 2 } }],
        };
        var league = CreateLeague(configuration);
        var (winner, other) = (Guid.NewGuid(), Guid.NewGuid());
        var lastWeek = new DateTime(2026, 11, 23, 12, 0, 0, DateTimeKind.Utc);
        league.AddPoints(winner, 200, lastWeek);
        league.AddPoints(other, 100, lastWeek);

        var rewardDay = new DateTime(2026, 11, 29, 12, 0, 0, DateTimeKind.Utc);
        Assert.That(league.ClaimReward(winner, rewardDay.AddDays(-1)), Is.Null, "not before the reward day");
        Assert.That(league.ClaimReward(winner, rewardDay)?.Count, Is.EqualTo(2));
        Assert.That(league.ClaimReward(winner, rewardDay), Is.Null, "only once");

        var nextMonth = new DateTime(2026, 12, 1, 12, 0, 0, DateTimeKind.Utc);
        league.AddPoints(winner, 100, nextMonth);
        Assert.That(league.GetStanding(winner, nextMonth).Tier, Is.EqualTo(LabyrinthLeagueTier.Bronze));
    }

    /// <summary>
    /// A cleared floor gives one of the three boxes by the chances of the floor.
    /// </summary>
    [Test]
    public void FloorBoxFollowsTheChances()
    {
        var league = CreateLeague(new LabyrinthLeagueConfiguration());

        Assert.That(Enumerable.Range(0, 20).Select(_ => league.RollFloorBox(1)?.Number), Has.All.EqualTo((short)403));
        Assert.That(Enumerable.Range(0, 20).Select(_ => league.RollFloorBox(2)?.Number), Has.All.EqualTo((short)404));
        Assert.That(league.RollFloorBox(42), Is.Null);
    }

    private static LabyrinthLeaguePlugIn CreateLeague(LabyrinthLeagueConfiguration configuration)
    {
        configuration.Floors =
        [
            .. Enumerable.Range(1, 41).Select(floor => new LabyrinthFloorScore
            {
                FirstPoints = 50 * floor,
                RepeatPoints = 25 * floor,
                BoxChances = floor == 2 ? [0, 1, 0] : [1, 0, 0],
            }),
        ];
        configuration.FloorBoxes = [new RewardItem { Group = 14, Number = 403 }, new RewardItem { Group = 14, Number = 404 }, new RewardItem { Group = 14, Number = 405 }];
        configuration.Schedule =
        [
            .. Enumerable.Range(1, 28).Select(day => new LabyrinthLeagueDay { Day = (byte)day, Kind = day % 7 == 1 ? LabyrinthDayKind.Start : LabyrinthDayKind.Play }),
            new LabyrinthLeagueDay { Day = 29, Kind = LabyrinthDayKind.Reward },
        ];
        configuration.StateFile = string.Empty;
        return new LabyrinthLeaguePlugIn { Configuration = configuration };
    }
}
