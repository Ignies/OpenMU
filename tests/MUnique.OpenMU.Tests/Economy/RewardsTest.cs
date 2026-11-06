// <copyright file="RewardsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Economy;

using Moq;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.GremoryCase;
using MUnique.OpenMU.GameLogic.Rewards;
using MUnique.OpenMU.GameLogic.Views.Economy;

/// <summary>
/// Tests the level rewards and the event contribution boxes of Season 21, which land in the gremory case.
/// </summary>
[TestFixture]
public class RewardsTest
{
    private const short MinorBox = 282;
    private const short StandardBox = 283;
    private const short GreaterBox = 284;
    private const short Muun = 69;

    /// <summary>
    /// Reaching the total level of a reward puts its item into the gremory case of the character.
    /// </summary>
    [Test]
    public async ValueTask LevelRewardLandsInTheGremoryCaseAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var plugIn = CreateLevelRewards(950);
        player.Attributes![Stats.Level] = 400;
        player.Attributes[Stats.MasterLevel] = 550;

        var given = await plugIn.RewardAsync(player).ConfigureAwait(false);

        Assert.That(given, Is.EqualTo(1));
        var reward = player.SelectedCharacter!.GremoryCase!.Items.Single();
        Assert.That((reward.Definition!.Group, reward.Definition.Number), Is.EqualTo(((byte)16, Muun)));
        Assert.That(reward.ExpiresAt, Is.Not.Null);
        Mock.Get(player.ViewPlugIns.GetPlugIn<IGremoryCaseViewPlugIn>()!).Verify(v => v.GremoryCaseItemAddedAsync(GremoryCaseKind.Character, reward), Times.Once);
    }

    /// <summary>
    /// Another total level or another class gives nothing.
    /// </summary>
    [Test]
    public async ValueTask LevelRewardNeedsItsLevelAndClassAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var plugIn = CreateLevelRewards(950, BaseClassCode.DarkKnight);
        player.Attributes![Stats.Level] = 400;
        player.Attributes[Stats.MasterLevel] = 550;

        Assert.That(await plugIn.RewardAsync(player).ConfigureAwait(false), Is.EqualTo(0), "A dark wizard isn't a dark knight.");

        plugIn.Configuration!.Rewards[0].Classes.Clear();
        player.Attributes[Stats.MasterLevel] = 549;
        Assert.That(await plugIn.RewardAsync(player).ConfigureAwait(false), Is.EqualTo(0), "Total level 949.");
        Assert.That(player.SelectedCharacter!.GremoryCase?.Items ?? [], Is.Empty);
    }

    /// <summary>
    /// Without the gremory case, the reward goes into the inventory.
    /// </summary>
    [Test]
    public async ValueTask LevelRewardWithoutTheCaseGoesIntoTheInventoryAsync()
    {
        var player = await CreatePlayerAsync(withGremoryCase: false).ConfigureAwait(false);
        var plugIn = CreateLevelRewards(950);
        player.Attributes![Stats.Level] = 400;
        player.Attributes[Stats.MasterLevel] = 550;

        Assert.That(await plugIn.RewardAsync(player).ConfigureAwait(false), Is.EqualTo(1));
        Assert.That(player.Inventory!.Items.Single().Definition!.Number, Is.EqualTo(Muun));
        Assert.That(player.Inventory.Items.Single().ExpiresAt, Is.Null);
    }

    /// <summary>
    /// Kills in a blood castle give their points; the castle gate its own; at the end the player gets the box of the points.
    /// </summary>
    [Test]
    public async ValueTask ContributionBoxLandsInTheGremoryCaseAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var plugIn = CreateContribution();
        var castle = new object();

        for (var i = 0; i < 21; i++)
        {
            plugIn.AddMonsterKill(castle, MiniGameType.BloodCastle, player, 100, DayOfWeek.Monday);
        }

        plugIn.AddMonsterKill(castle, MiniGameType.BloodCastle, player, 131, DayOfWeek.Monday);
        plugIn.AddMonsterKill(castle, MiniGameType.BloodCastle, player, 132, DayOfWeek.Monday);
        Assert.That(plugIn.GetPoints(castle, player), Is.EqualTo((21 * 5) + 100 + 200));

        var delivered = await plugIn.EndAsync(castle, MiniGameType.BloodCastle, null).ConfigureAwait(false);

        Assert.That(delivered, Is.EqualTo(1));
        Assert.That(player.SelectedCharacter!.GremoryCase!.Items.Single().Definition!.Number, Is.EqualTo(StandardBox), "405 points: the standard box.");
        Assert.That(plugIn.GetPoints(castle, player), Is.EqualTo(0), "The points end with the event.");
    }

    /// <summary>
    /// The boxes follow the point ranges of Season 21; below 100 points there is none.
    /// </summary>
    [Test]
    public void BoxesFollowThePointRanges()
    {
        var plugIn = CreateContribution();

        Assert.That(plugIn.GetBox(99), Is.Null);
        Assert.That(plugIn.GetBox(100)!.Number, Is.EqualTo(MinorBox));
        Assert.That(plugIn.GetBox(400)!.Number, Is.EqualTo(MinorBox));
        Assert.That(plugIn.GetBox(401)!.Number, Is.EqualTo(StandardBox));
        Assert.That(plugIn.GetBox(801)!.Number, Is.EqualTo(GreaterBox));
        Assert.That(plugIn.GetBox(100_000)!.Number, Is.EqualTo(GreaterBox));
    }

    /// <summary>
    /// The winner and the player kills of a chaos castle count; the Sunday points of the imperial guardian apply on Sundays.
    /// </summary>
    [Test]
    public async ValueTask WinnerPlayerKillsAndSundaysCountAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var plugIn = CreateContribution();
        var chaosCastle = new object();
        plugIn.AddPlayerKill(chaosCastle, MiniGameType.ChaosCastle, player);
        plugIn.AddMonsterKill(chaosCastle, MiniGameType.ChaosCastle, player, 100, DayOfWeek.Monday);

        await plugIn.EndAsync(chaosCastle, MiniGameType.ChaosCastle, player).ConfigureAwait(false);

        Assert.That(player.SelectedCharacter!.GremoryCase!.Items.Single().Definition!.Number, Is.EqualTo(MinorBox), "50 + 13 + 99 = 162 points.");

        var guardian = new object();
        plugIn.AddMonsterKill(guardian, MiniGameType.ImperialGuardian, player, 100, DayOfWeek.Sunday);
        plugIn.AddMonsterKill(guardian, MiniGameType.ImperialGuardian, player, 100, DayOfWeek.Monday);
        Assert.That(plugIn.GetPoints(guardian, player), Is.EqualTo(22 + 45));
    }

    /// <summary>
    /// An event without a rule gives no points.
    /// </summary>
    [Test]
    public async ValueTask EventWithoutARuleGivesNothingAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var plugIn = CreateContribution();
        var kanturu = new object();
        plugIn.AddMonsterKill(kanturu, MiniGameType.Kanturu, player, 100, DayOfWeek.Monday);

        Assert.That(plugIn.GetPoints(kanturu, player), Is.EqualTo(0));
        Assert.That(await plugIn.EndAsync(kanturu, MiniGameType.Kanturu, player).ConfigureAwait(false), Is.EqualTo(0));
    }

    private static LevelRewardsPlugIn CreateLevelRewards(int totalLevel, params BaseClassCode[] classes)
    {
        var reward = new LevelReward { TotalLevel = totalLevel, Group = 16, Number = Muun };
        reward.Classes.AddRange(classes);
        return new LevelRewardsPlugIn { Configuration = new LevelRewardsConfiguration { Rewards = { reward } } };
    }

    private static EventContributionPlugIn CreateContribution()
    {
        return new EventContributionPlugIn
        {
            Configuration = new EventContributionConfiguration
            {
                Rules =
                {
                    new EventContributionRule
                    {
                        Type = MiniGameType.BloodCastle,
                        MonsterKillPoints = 5,
                        SpecialMonsters = { new MonsterContributionPoints { MonsterNumber = 131, Points = 100 }, new MonsterContributionPoints { MonsterNumber = 132, Points = 200 } },
                    },
                    new EventContributionRule { Type = MiniGameType.ChaosCastle, MonsterKillPoints = 13, PlayerKillPoints = 50, WinnerPoints = 99 },
                    new EventContributionRule { Type = MiniGameType.ImperialGuardian, MonsterKillPoints = 45, SundayMonsterKillPoints = 22 },
                },
                Boxes =
                {
                    new ContributionBox { MinimumPoints = 100, MaximumPoints = 400, Group = 14, Number = MinorBox },
                    new ContributionBox { MinimumPoints = 401, MaximumPoints = 800, Group = 14, Number = StandardBox },
                    new ContributionBox { MinimumPoints = 801, Group = 14, Number = GreaterBox },
                },
            },
        };
    }

    private static async ValueTask<Player> CreatePlayerAsync(bool withGremoryCase = true)
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        foreach (var (group, number) in new[] { ((byte)16, Muun), ((byte)14, MinorBox), ((byte)14, StandardBox), ((byte)14, GreaterBox) })
        {
            player.GameContext.Configuration.Items.Add(new ItemDefinition { Group = group, Number = number, Width = 1, Height = 1, Durability = 1, IsActive = true });
        }

        if (withGremoryCase)
        {
            player.GameContext.FeaturePlugIns.AddPlugIn(new GremoryCaseFeaturePlugIn { Configuration = new GremoryCaseConfiguration() }, true);
        }

        player.SelectedCharacter!.CharacterClass = new CharacterClass { Number = 0 };
        return player;
    }
}
