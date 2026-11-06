// <copyright file="HuntingRecordTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.World;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.HuntingRecord;
using MUnique.OpenMU.GameLogic.NPC;
using MonsterDefinition = MUnique.OpenMU.Persistence.BasicModel.MonsterDefinition;

/// <summary>
/// Tests the hunting record and the ranking of Season 21 (<see cref="HuntingRecordPlugIn"/>).
/// </summary>
[TestFixture]
public class HuntingRecordTest
{
    private static readonly DateTime Now = new(2026, 11, 6, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Kills, experience and damage on a recorded map land in the row of the day; an excluded monster doesn't count.
    /// </summary>
    [Test]
    public async ValueTask KillsExperienceAndDamageAreRecordedAsync()
    {
        var plugIn = CreatePlugIn();
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var monster = CreateMonster(player, 26);

        await plugIn.AttackableGotKilledAsync(monster, player).ConfigureAwait(false);
        await plugIn.PlayerGainedExperienceAsync(player, 1500, monster, false).ConfigureAwait(false);
        plugIn.AttackableGotHit(monster, player, new HitInfo(700, 300, DamageAttributes.Undefined));
        await plugIn.AttackableGotKilledAsync(CreateMonster(player, 413), player).ConfigureAwait(false);

        var day = plugIn.GetDays(player, 0).Single();
        Assert.That(day.Day, Is.EqualTo(Now.Date));
        Assert.That((day.Kills, day.Experience, day.Damage), Is.EqualTo((1L, 1500L, 1000L)), "The Moon Rabbit (413) doesn't count.");
    }

    /// <summary>
    /// The time on a recorded map counts from entering it to leaving it; while the character is there, today shows the running visit.
    /// </summary>
    [Test]
    public async ValueTask TimeOnTheMapIsRecordedAsync()
    {
        var plugIn = CreatePlugIn();
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var map = player.CurrentMap!;

        await plugIn.ObjectAddedToMapAsync(map, player).ConfigureAwait(false);
        plugIn.UtcNow = () => Now.AddMinutes(30);
        Assert.That(plugIn.GetDays(player, 0).Single().Seconds, Is.EqualTo(1800), "The running visit.");

        await plugIn.ObjectRemovedFromMapAsync(map, player).ConfigureAwait(false);
        plugIn.UtcNow = () => Now.AddMinutes(45);
        Assert.That(plugIn.GetDays(player, 0).Single().Seconds, Is.EqualTo(1800), "No time after leaving.");
    }

    /// <summary>
    /// Days older than the kept days are dropped with the next change.
    /// </summary>
    [Test]
    public async ValueTask OldDaysAreDroppedAsync()
    {
        var plugIn = CreatePlugIn();
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var monster = CreateMonster(player, 26);
        await plugIn.AttackableGotKilledAsync(monster, player).ConfigureAwait(false);

        plugIn.UtcNow = () => Now.AddDays(8);
        await plugIn.AttackableGotKilledAsync(monster, player).ConfigureAwait(false);

        Assert.That(plugIn.GetDays(player, 0).Select(d => d.Day), Is.EqualTo(new[] { Now.Date.AddDays(8) }));
    }

    /// <summary>
    /// The weekly kill ranking orders the characters by their kills since the week started, pages them and finds the own rank.
    /// </summary>
    [Test]
    public async ValueTask WeeklyKillRankingOrdersAndPagesAsync()
    {
        var plugIn = CreatePlugIn();
        plugIn.Configuration!.PageSize = 2;
        var context = (await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false)).GameContext;
        var players = new List<Player>();
        for (var i = 0; i < 3; i++)
        {
            var player = await CreatePlayerAsync(context).ConfigureAwait(false);
            player.SelectedCharacter!.Name = $"Hunter{i}";
            player.SelectedCharacter.Id = Guid.NewGuid();
            for (var kill = 0; kill <= i; kill++)
            {
                await plugIn.AttackableGotKilledAsync(CreateMonster(player, 26), player).ConfigureAwait(false);
            }

            players.Add(player);
        }

        var (entries, pageCount, ownRank) = plugIn.GetRankingPage(players[0], RankingKind.WeeklyKills, 0);

        Assert.That(entries.Select(e => (e.Rank, e.Name, e.Value)), Is.EqualTo(new[] { (1, "Hunter2", 3L), (2, "Hunter1", 2L) }));
        Assert.That((pageCount, ownRank), Is.EqualTo((2, 3)));
        Assert.That(plugIn.GetRankingPage(players[0], RankingKind.WeeklyKills, 1).Entries.Single().Name, Is.EqualTo("Hunter0"));
    }

    /// <summary>
    /// Kills of last week don't count for this week's ranking; the total level ranking uses the last known levels.
    /// </summary>
    [Test]
    public async ValueTask WeekStartsAgainAndTotalLevelRanksAsync()
    {
        var plugIn = CreatePlugIn();
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        player.Attributes![Stats.Level] = 400;
        player.Attributes[Stats.MasterLevel] = 120;
        plugIn.UtcNow = () => new DateTime(2026, 11, 2, 12, 0, 0, DateTimeKind.Utc);
        await plugIn.AttackableGotKilledAsync(CreateMonster(player, 26), player).ConfigureAwait(false);

        plugIn.UtcNow = () => new DateTime(2026, 11, 4, 12, 0, 0, DateTimeKind.Utc);
        Assert.That(plugIn.GetRankingPage(player, RankingKind.WeeklyKills, 0).Entries, Is.Empty, "The week started again on Tuesday the 3rd.");
        Assert.That(plugIn.GetRankingPage(player, RankingKind.TotalLevel, 0).Entries.Single().Value, Is.EqualTo(520));
    }

    private static async ValueTask<Player> CreatePlayerAsync(IGameContext? context = null)
    {
        var player = context is null ? await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false) : await PlayerTestHelper.CreatePlayerAsync(context).ConfigureAwait(false);
        player.CurrentMap = await player.GameContext.GetMapAsync(0).ConfigureAwait(false);
        return player;
    }

    private static HuntingRecordPlugIn CreatePlugIn()
        => new()
        {
            Configuration = new HuntingRecordConfiguration { Maps = [0], ExcludedMonsters = [413], StateFile = string.Empty, RankingRefreshInterval = TimeSpan.Zero },
            UtcNow = () => Now,
        };

    private static Monster CreateMonster(Player player, short number)
        => new(new MonsterSpawnArea { GameMap = player.CurrentMap!.Definition }, new MonsterDefinition { Number = number }, player.CurrentMap, NullDropGenerator.Instance, new Moq.Mock<INpcIntelligence>().Object, player.GameContext.PlugInManager, player.GameContext.PathFinderPool);
}
