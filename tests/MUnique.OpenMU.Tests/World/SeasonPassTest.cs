// <copyright file="SeasonPassTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.World;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;
using MUnique.OpenMU.GameLogic.SeasonPass;
using MonsterDefinition = MUnique.OpenMU.Persistence.BasicModel.MonsterDefinition;

/// <summary>
/// Tests the season pass of Season 21 (<see cref="SeasonPassPlugIn"/>).
/// </summary>
[TestFixture]
public class SeasonPassTest
{
    private static readonly DateTime Now = new(2026, 11, 6, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// The schedule of the data repeats every year: on 2026-11-06 the November season runs until its end.
    /// </summary>
    [Test]
    public void TheScheduleRepeatsEveryYear()
    {
        var plugIn = CreatePlugIn();

        Assert.That(plugIn.GetSeason(Now), Is.EqualTo(((ushort)11, new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 11, 30, 23, 59, 0, DateTimeKind.Utc))));
        Assert.That(plugIn.GetSeason(new DateTime(2026, 12, 31, 12, 0, 0, DateTimeKind.Utc)).Number, Is.EqualTo(0), "The data leaves the 31st out.");
        plugIn.Configuration!.RepeatEveryYear = false;
        Assert.That(plugIn.GetSeason(Now).Number, Is.EqualTo(0));
    }

    /// <summary>
    /// A daily mission counts its kind up to its repeats a day and gives its experience per completion; the pass level
    /// follows the experience; the next day the missions start again.
    /// </summary>
    [Test]
    public async ValueTask DailyMissionsGiveExperienceAsync()
    {
        var plugIn = CreatePlugIn();
        var player = await CreatePlayerAsync().ConfigureAwait(false);

        for (var i = 0; i < 5; i++)
        {
            await plugIn.AttackableGotKilledAsync(CreateMonster(player), player).ConfigureAwait(false);
        }

        var state = plugIn.GetState(player);
        Assert.That((state.Experience, state.Level), Is.EqualTo((10, 1)));
        Assert.That(state.Missions.Single(m => m.Index == 0), Is.EqualTo(new SeasonPassMissionState(0, 2, 0)), "Two completions a day.");

        plugIn.UtcNow = () => Now.AddDays(1);
        await plugIn.AttackableGotKilledAsync(CreateMonster(player), player).ConfigureAwait(false);
        Assert.That(plugIn.GetState(player).Missions.Single(m => m.Index == 0), Is.EqualTo(new SeasonPassMissionState(0, 0, 1)));
    }

    /// <summary>
    /// The pass ticket gives its experience, the gold pass ticket the gold pass; the premium pass is off in the data, so its
    /// ticket stays.
    /// </summary>
    [Test]
    public async ValueTask TicketsGiveExperienceAndPassesAsync()
    {
        var plugIn = CreatePlugIn();
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        player.GameContext.FeaturePlugIns.AddPlugIn(plugIn, true);
        var premium = Ticket(124);

        Assert.That(await new SeasonPassTicketConsumeHandlerPlugIn().ConsumeItemAsync(player, Ticket(122), null, FruitUsage.Undefined).ConfigureAwait(false), Is.True);
        Assert.That(await new GoldPassTicketConsumeHandlerPlugIn().ConsumeItemAsync(player, Ticket(123), null, FruitUsage.Undefined).ConfigureAwait(false), Is.True);
        Assert.That(await new PremiumPassTicketConsumeHandlerPlugIn().ConsumeItemAsync(player, premium, null, FruitUsage.Undefined).ConfigureAwait(false), Is.False);

        var state = plugIn.GetState(player);
        Assert.That((state.Experience, state.Level, state.Passes), Is.EqualTo((300, 2, (byte)0b011)));
        Assert.That(premium.Durability, Is.EqualTo(1));
    }

    /// <summary>
    /// A reward of a reached level is claimed once per pass; a buff raises its attribute with the value of the claimed level.
    /// </summary>
    [Test]
    public async ValueTask ClaimsGiveTheRewardsOfTheLevelAsync()
    {
        var plugIn = CreatePlugIn();
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var health = player.Attributes![Stats.MaximumHealth];
        await plugIn.AddExperienceAsync(player, 10).ConfigureAwait(false);

        Assert.That(await plugIn.ClaimAsync(player, SeasonPassKind.Free, 2).ConfigureAwait(false), Is.EqualTo(SeasonPassClaimResult.LevelNotReached));
        Assert.That(await plugIn.ClaimAsync(player, SeasonPassKind.Gold, 1).ConfigureAwait(false), Is.EqualTo(SeasonPassClaimResult.PassNotOwned));
        Assert.That(await plugIn.ClaimAsync(player, SeasonPassKind.Free, 1).ConfigureAwait(false), Is.EqualTo(SeasonPassClaimResult.Claimed));
        Assert.That(await plugIn.ClaimAsync(player, SeasonPassKind.Free, 1).ConfigureAwait(false), Is.EqualTo(SeasonPassClaimResult.AlreadyClaimed));

        Assert.That(player.Attributes[Stats.MaximumHealth], Is.EqualTo(health + 30));
        Assert.That(plugIn.GetState(player).ClaimedFree, Is.EqualTo(1u));

        await plugIn.AddExperienceAsync(player, 20).ConfigureAwait(false);
        Assert.That(await plugIn.ClaimAsync(player, SeasonPassKind.Free, 2).ConfigureAwait(false), Is.EqualTo(SeasonPassClaimResult.NoReward));
    }

    /// <summary>
    /// A new season starts the pass again: no experience, no passes, no claims and no buffs.
    /// </summary>
    [Test]
    public async ValueTask NewSeasonStartsAgainAsync()
    {
        var plugIn = CreatePlugIn();
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var health = player.Attributes![Stats.MaximumHealth];
        await plugIn.AddExperienceAsync(player, 10).ConfigureAwait(false);
        await plugIn.ClaimAsync(player, SeasonPassKind.Free, 1).ConfigureAwait(false);

        plugIn.UtcNow = () => new DateTime(2026, 12, 2, 12, 0, 0, DateTimeKind.Utc);
        var state = plugIn.GetState(player);

        Assert.That((state.Season, state.Experience, state.Passes, state.ClaimedFree), Is.EqualTo(((ushort)12, 0, (byte)1, 0u)));
        await plugIn.RefreshBuffsAsync(player).ConfigureAwait(false);
        Assert.That(player.Attributes[Stats.MaximumHealth], Is.EqualTo(health));
    }

    private static async ValueTask<Player> CreatePlayerAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        player.SelectedCharacter!.Id = Guid.NewGuid();
        player.CurrentMap = await player.GameContext.GetMapAsync(0).ConfigureAwait(false);
        return player;
    }

    private static DataModel.Entities.Item Ticket(short number)
        => new Persistence.BasicModel.Item { Definition = new Persistence.BasicModel.ItemDefinition { Group = 20, Number = number, Width = 1, Height = 1, Durability = 1 }, Durability = 1 };

    private static SeasonPassPlugIn CreatePlugIn()
        => new()
        {
            Configuration = new SeasonPassConfiguration
            {
                StateFile = string.Empty,
                GoldPassEnabled = true,
                PremiumPassEnabled = false,
                TicketExperience = 300,
                Seasons =
                [
                    new SeasonPassSeason { Number = 11, Start = new DateTime(2025, 11, 1, 0, 0, 0, DateTimeKind.Utc), End = new DateTime(2025, 11, 30, 23, 59, 0, DateTimeKind.Utc) },
                    new SeasonPassSeason { Number = 12, Start = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc), End = new DateTime(2025, 12, 30, 23, 59, 0, DateTimeKind.Utc) },
                ],
                LevelExperience = [10, 30],
                Missions =
                [
                    new SeasonPassMission { Index = 0, Kind = SeasonPassMissionKind.MonsterKills, Experience = 5, Required = 2, Repeat = 2 },
                    new SeasonPassMission { Index = 1, Kind = SeasonPassMissionKind.None, Experience = 5, Required = 1, Repeat = 1 },
                ],
                Rewards = [new SeasonPassReward { Id = 0, EffectIndex = 411, Name = "Max HP" }],
                LevelRewards = [new SeasonPassLevelReward { Pass = SeasonPassKind.Free, Level = 1, RewardIds = [0] }],
                Buffs = [new SeasonPassBuff { EffectIndex = 411, AttributeId = Stats.MaximumHealth.Id, Values = [30, 30] }],
            },
            UtcNow = () => Now,
        };

    private static Monster CreateMonster(Player player)
        => new(new MonsterSpawnArea { GameMap = player.CurrentMap!.Definition }, new MonsterDefinition { Number = 26 }, player.CurrentMap, NullDropGenerator.Instance, new Moq.Mock<INpcIntelligence>().Object, player.GameContext.PlugInManager, player.GameContext.PathFinderPool);
}
