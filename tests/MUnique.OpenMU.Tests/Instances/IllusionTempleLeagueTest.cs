// <copyright file="IllusionTempleLeagueTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Instances;

using System.Reflection;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.MiniGames;
using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlayerActions.MiniGames;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.Pathfinding;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.Updates;

/// <summary>
/// Tests the Illusion Temple League (S21Emu <c>Event/IlusionTempleLeague</c>): the stages by the day of the month, the
/// entries of the eliminations, the ranking by occupied stones, kills and level sum, the semi-final pairing, the final,
/// the reward of the champion, and the renewed temple on maps 98 and 99 (Cursed Statue, ten Captured Stone Statues).
/// </summary>
[TestFixture]
public class IllusionTempleLeagueTest
{
    private const short CursedStatue = 658;

    private static readonly DateTime EliminationDay = new(2026, 11, 3, 13, 0, 0);

    private static readonly DateTime SemiFinalDay = new(2026, 11, 11, 14, 55, 0);

    private static readonly DateTime FinalDay = new(2026, 11, 12, 14, 55, 0);

    private static readonly DateTime RewardDay = new(2026, 11, 13, 10, 0, 0);

    /// <summary>
    /// Eliminations on the 3rd to the 9th at every time, the semi-finals on the 11th and the final on the 12th at their
    /// time only, the reward on the 13th.
    /// </summary>
    [Test]
    public void StagesFollowTheDaysOfTheData()
    {
        var plugIn = CreatePlugIn();

        Assert.That(plugIn.GetStage(new DateTime(2026, 11, 2, 13, 0, 0)), Is.EqualTo(IllusionTempleLeagueStage.None));
        Assert.That(plugIn.GetStage(EliminationDay), Is.EqualTo(IllusionTempleLeagueStage.Elimination));
        Assert.That(plugIn.GetStage(new DateTime(2026, 11, 9, 21, 55, 0)), Is.EqualTo(IllusionTempleLeagueStage.Elimination));
        Assert.That(plugIn.GetStage(new DateTime(2026, 11, 10, 13, 0, 0)), Is.EqualTo(IllusionTempleLeagueStage.None));
        Assert.That(plugIn.GetStage(SemiFinalDay), Is.EqualTo(IllusionTempleLeagueStage.SemiFinal));
        Assert.That(plugIn.GetStage(FinalDay), Is.EqualTo(IllusionTempleLeagueStage.Final));
        Assert.That(plugIn.GetStage(RewardDay), Is.EqualTo(IllusionTempleLeagueStage.Reward));
        Assert.That(plugIn.MayStart(EliminationDay), Is.True);
        Assert.That(plugIn.MayStart(SemiFinalDay), Is.True);
        Assert.That(plugIn.MayStart(SemiFinalDay.AddHours(2)), Is.False, "the semi-finals run once");
        Assert.That(plugIn.MayStart(FinalDay.AddHours(-1)), Is.False);
        Assert.That(plugIn.MayStart(RewardDay), Is.False);
    }

    /// <summary>
    /// A guild plays at most five elimination matches; a player limit applies when it's set.
    /// </summary>
    [Test]
    public void EntriesOfTheEliminationsAreLimited()
    {
        var plugIn = CreatePlugIn(c => c.MaximumPlayerEntries = 1);
        var (guild, other) = (Guid.NewGuid(), Guid.NewGuid());
        var (player, second) = (Guid.NewGuid(), Guid.NewGuid());

        Assert.That(plugIn.CheckEntry(guild, player, 1, EliminationDay), Is.EqualTo(IllusionTempleLeagueEntryCheck.Allowed));
        plugIn.Record(IllusionTempleLeagueStage.Elimination, [Result(guild, 1, 0, 400, player), Result(other, 0, 0, 400)], EliminationDay);
        Assert.That(plugIn.CheckEntry(guild, player, 1, EliminationDay), Is.EqualTo(IllusionTempleLeagueEntryCheck.PlayerEntriesUsed));

        for (var i = 0; i < 4; i++)
        {
            plugIn.Record(IllusionTempleLeagueStage.Elimination, [Result(guild, 1, 0, 400), Result(other, 0, 0, 400)], EliminationDay);
        }

        Assert.That(plugIn.CheckEntry(guild, second, 1, EliminationDay), Is.EqualTo(IllusionTempleLeagueEntryCheck.GuildEntriesUsed));
        Assert.That(plugIn.CheckEntry(guild, second, 1, EliminationDay.AddMonths(1)), Is.EqualTo(IllusionTempleLeagueEntryCheck.Allowed), "a new month starts over");
        Assert.That(plugIn.CheckEntry(guild, second, 1, new DateTime(2026, 11, 10, 13, 0, 0)), Is.EqualTo(IllusionTempleLeagueEntryCheck.NotOpen));
    }

    /// <summary>
    /// The eliminations rank by occupied stones, then kills, then the level sum; the best four play the semi-finals as
    /// first against fourth in the first temple and second against third in the second; the others don't.
    /// </summary>
    [Test]
    public void BestFourArePairedForTheSemiFinals()
    {
        var plugIn = CreatePlugIn();
        var guilds = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToArray();
        plugIn.Record(IllusionTempleLeagueStage.Elimination, [Result(guilds[0], 7, 1, 400), Result(guilds[1], 3, 9, 400)], EliminationDay);
        plugIn.Record(IllusionTempleLeagueStage.Elimination, [Result(guilds[2], 3, 5, 500), Result(guilds[3], 3, 5, 450)], EliminationDay);
        plugIn.Record(IllusionTempleLeagueStage.Elimination, [Result(guilds[4], 1, 0, 400), Result(guilds[3], 0, 0, 0)], EliminationDay);

        Assert.That(plugIn.GetRanking(SemiFinalDay), Is.EqualTo(guilds));
        Assert.That(plugIn.GetSemiFinalLevel(guilds[0], SemiFinalDay), Is.EqualTo(1));
        Assert.That(plugIn.GetSemiFinalLevel(guilds[3], SemiFinalDay), Is.EqualTo(1));
        Assert.That(plugIn.GetSemiFinalLevel(guilds[1], SemiFinalDay), Is.EqualTo(2));
        Assert.That(plugIn.GetSemiFinalLevel(guilds[2], SemiFinalDay), Is.EqualTo(2));
        Assert.That(plugIn.CheckEntry(guilds[0], Guid.NewGuid(), 1, SemiFinalDay), Is.EqualTo(IllusionTempleLeagueEntryCheck.Allowed));
        Assert.That(plugIn.CheckEntry(guilds[0], Guid.NewGuid(), 2, SemiFinalDay), Is.EqualTo(IllusionTempleLeagueEntryCheck.NotQualified));
        Assert.That(plugIn.CheckEntry(guilds[4], Guid.NewGuid(), 1, SemiFinalDay), Is.EqualTo(IllusionTempleLeagueEntryCheck.NotQualified));
    }

    /// <summary>
    /// The winners of the semi-finals play the final in the first temple; the winner of the final is the champion, whose
    /// players of the final get the Wings of Conqueror once from the reward day on; a draw in the final has no champion.
    /// </summary>
    [Test]
    public void ChampionGetsTheWingsOnTheRewardDay()
    {
        var plugIn = CreatePlugIn();
        var (a, b, c, d) = (Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var (finalPlayer, loserPlayer) = (Guid.NewGuid(), Guid.NewGuid());

        Assert.That(plugIn.Record(IllusionTempleLeagueStage.SemiFinal, [Result(a, 2, 0, 400), Result(d, 2, 1, 400)], SemiFinalDay), Is.EqualTo(d));
        Assert.That(plugIn.Record(IllusionTempleLeagueStage.SemiFinal, [Result(b, 1, 0, 401), Result(c, 1, 0, 400)], SemiFinalDay), Is.EqualTo(b));
        Assert.That(plugIn.CheckEntry(d, finalPlayer, 1, FinalDay), Is.EqualTo(IllusionTempleLeagueEntryCheck.Allowed));
        Assert.That(plugIn.CheckEntry(a, finalPlayer, 1, FinalDay), Is.EqualTo(IllusionTempleLeagueEntryCheck.NotQualified));
        Assert.That(plugIn.CheckEntry(d, finalPlayer, 2, FinalDay), Is.EqualTo(IllusionTempleLeagueEntryCheck.NotQualified));

        Assert.That(plugIn.Record(IllusionTempleLeagueStage.Final, [Result(d, 1, 0, 400, finalPlayer), Result(b, 1, 0, 400, loserPlayer)], FinalDay), Is.Null, "a draw");
        Assert.That(plugIn.Record(IllusionTempleLeagueStage.Final, [Result(d, 4, 0, 400, finalPlayer), Result(b, 1, 0, 400, loserPlayer)], FinalDay), Is.EqualTo(d));

        Assert.That(plugIn.ClaimReward(finalPlayer, FinalDay), Is.Null, "not before the reward day");
        Assert.That(plugIn.ClaimReward(loserPlayer, RewardDay), Is.Null);
        Assert.That(plugIn.ClaimReward(finalPlayer, RewardDay), Is.Not.Null.And.Property(nameof(GameLogic.Rewards.RewardItem.Number)).EqualTo(268));
        Assert.That(plugIn.ClaimReward(finalPlayer, RewardDay.AddDays(1)), Is.Null, "once");
    }

    /// <summary>
    /// The update adds the league temples on maps 98 and 99 with the Cursed Statue and the ten stones. Only a party of
    /// one guild gets in on an elimination day; the guilds are the teams; the carrier of the Cursed Hollow seals a stone
    /// for a point, the other guild unseals it and takes the hollow; the result of the match goes into the league.
    /// </summary>
    [Test]
    public async ValueTask GuildPartiesPlayTheRenewedTempleAsync()
    {
        var world = await Season21EventWorld.CreateAsync(new AddSeason21IllusionTempleLeagueUpdatePlugIn()).ConfigureAwait(false);
        var now = DateTime.SpecifyKind(EliminationDay, DateTimeKind.Utc);
        var plugIn = CreatePlugIn(c => c.MinimumGuildPartyMembers = 2, () => now);
        world.AddFeaturePlugIn(plugIn);
        var definitions = world.Configuration.MiniGameDefinitions.Where(d => d.Type == MiniGameType.IllusionTempleLeague).OrderBy(d => d.GameLevel).ToList();
        Assert.That(definitions.Select(d => d.Entrance?.Map?.Number), Is.EqualTo(new short?[] { 98, 99 }));
        Assert.That(definitions[0].Entrance!.Map!.MonsterSpawns.Count(s => s.MonsterDefinition?.Number is >= CursedStatue and <= 668), Is.EqualTo(11));

        var loner = await CreateGuildPlayerAsync(world, 1).ConfigureAwait(false);
        var (a1, a2) = (await CreateGuildPlayerAsync(world, 1).ConfigureAwait(false), await CreateGuildPlayerAsync(world, 1).ConfigureAwait(false));
        var (b1, b2) = (await CreateGuildPlayerAsync(world, 2).ConfigureAwait(false), await CreateGuildPlayerAsync(world, 2).ConfigureAwait(false));
        await Season21EventWorld.CreatePartyAsync(a1, a2).ConfigureAwait(false);
        await Season21EventWorld.CreatePartyAsync(b1, b2).ConfigureAwait(false);

        foreach (var player in new[] { loner, a1, a2, b1, b2 })
        {
            await new EnterMiniGameAction().TryEnterMiniGameAsync(player, MiniGameType.IllusionTempleLeague, 1, 0xFF).ConfigureAwait(false);
        }

        Assert.That(loner.CurrentMiniGame, Is.Null, "no guild party");
        var temple = (IllusionTempleContext)a1.CurrentMiniGame!;
        Assert.That(new[] { a2, b1, b2 }.Select(p => p.CurrentMiniGame), Has.All.SameAs(temple));
        temple.PreparationDuration = TimeSpan.Zero;
        foreach (var player in new[] { a1, a2, b1, b2 })
        {
            await Season21EventWorld.CompleteMapChangeAsync(player).ConfigureAwait(false);
        }

        await Season21EventWorld.SkipUntilPlayingAsync(temple).ConfigureAwait(false);
        var (teamA, teamB) = (GetTeam(temple, a1), GetTeam(temple, b1));
        Assert.That(GetTeam(temple, a2), Is.EqualTo(teamA));
        Assert.That(GetTeam(temple, b2), Is.EqualTo(teamB));
        Assert.That(teamA, Is.Not.EqualTo(teamB));
        await Season21EventWorld.WaitUntilAsync(() => FindNpcs(temple, CursedStatue).Count == 1 && FindNpcs(temple, 659).Count == 1).ConfigureAwait(false);

        a1.OpenedNpc = FindNpcs(temple, CursedStatue)[0];
        await temple.TalkToNpcStoneStatueAsync(a1).ConfigureAwait(false);
        var stone = FindNpcs(temple, 659)[0];
        await temple.TalkToNpcCapturedStoneAsync(a1, stone).ConfigureAwait(false);
        Assert.That(temple.Score.GetScore(teamA), Is.EqualTo(1));
        Assert.That(a1.Inventory!.Items.Any(IsHollow), Is.False);

        await temple.TalkToNpcCapturedStoneAsync(b1, stone).ConfigureAwait(false);
        Assert.That(temple.Score.GetScore(teamA), Is.EqualTo(0), "unsealed");
        Assert.That(b1.Inventory!.Items.Any(IsHollow), Is.True, "the hollow goes to the one who unsealed it");
        await temple.TalkToNpcCapturedStoneAsync(b1, FindNpcs(temple, 660)[0]).ConfigureAwait(false);
        Assert.That(temple.Score.GetScore(teamB), Is.EqualTo(1));

        typeof(MiniGameContext).GetMethod("FinishEvent", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(temple, null);
        await Season21EventWorld.WaitUntilAsync(() => plugIn.GetRanking(now).Count == 2).ConfigureAwait(false);
        Assert.That(plugIn.GetRanking(now)[0], Is.EqualTo(IllusionTempleLeaguePlugIn.GetFallbackGuildKey(2)));
    }

    private static IllusionTempleLeaguePlugIn CreatePlugIn(Action<IllusionTempleLeagueConfiguration>? configure = null, Func<DateTime>? utcNow = null)
    {
        var configuration = new IllusionTempleLeagueConfiguration { GuildStateFile = string.Empty, PlayerStateFile = string.Empty, StoneSealProtection = TimeSpan.Zero };
        configure?.Invoke(configuration);
        var plugIn = new IllusionTempleLeaguePlugIn { Configuration = configuration };
        if (utcNow is not null)
        {
            plugIn.UtcNow = utcNow;
        }

        return plugIn;
    }

    private static IllusionTempleLeagueResult Result(Guid guild, int stones, int kills, int levelSum, params Guid[] characters)
        => new(guild, string.Empty, stones, kills, levelSum, characters);

    private static async ValueTask<Player> CreateGuildPlayerAsync(Season21EventWorld world, uint guildId)
    {
        var player = await world.CreatePlayerAsync(CharacterClassNumber.BladeMaster, 400, 0).ConfigureAwait(false);
        player.GuildStatus = new GuildMemberStatus(guildId, GuildPosition.NormalMember);
        return player;
    }

    private static IllusionTempleTeam GetTeam(IllusionTempleContext temple, Player player)
        => temple.GetSpawnGate(player)!.X1 < 150 ? IllusionTempleTeam.AlliedForces : IllusionTempleTeam.IllusionForces;

    private static List<NonPlayerCharacter> FindNpcs(IllusionTempleContext temple, short number)
        => [.. temple.Map.GetNpcsInRange(new Point(170, 85), 255).Where(n => n.Definition.Number == number)];

    private static bool IsHollow(DataModel.Entities.Item item) => item.Definition is { Group: 14, Number: 64 };
}
