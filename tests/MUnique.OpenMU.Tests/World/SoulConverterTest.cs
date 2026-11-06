// <copyright file="SoulConverterTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.World;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;
using MUnique.OpenMU.GameLogic.SoulConverter;
using MonsterDefinition = MUnique.OpenMU.Persistence.BasicModel.MonsterDefinition;

/// <summary>
/// Tests the monster soul converter of Season 21 (<see cref="SoulConverterPlugIn"/>).
/// </summary>
[TestFixture]
public class SoulConverterTest
{
    /// <summary>A Friday; the week starts on Thursday.</summary>
    private static readonly DateTime Now = new(2026, 11, 6, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// A converter of the character's level draws its missions, counts as an activation of the week and expires when the
    /// next week starts.
    /// </summary>
    [Test]
    public async ValueTask ActivationDrawsTheMissionsAsync()
    {
        var plugIn = CreatePlugIn();
        var player = await CreatePlayerAsync(400).ConfigureAwait(false);

        Assert.That(await plugIn.ActivateAsync(player, Converter()).ConfigureAwait(false), Is.EqualTo(SoulConverterActivation.Activated));

        var (tier, left, expiresAt, missions) = plugIn.GetState(player);
        Assert.That((tier, left, expiresAt), Is.EqualTo(((byte)1, 2, (DateTime?)new DateTime(2026, 11, 12, 0, 0, 0, DateTimeKind.Utc))));
        Assert.That(missions.Select(m => m.MonsterNumber).Order(), Is.EqualTo(new short[] { 26, 27 }));
        Assert.That(missions.Select(m => (m.Souls, m.RequiredSouls)), Has.All.EqualTo((0, 2)));
    }

    /// <summary>
    /// A converter needs the total level of its tier, one converter is active at a time, and the activations of a week are limited.
    /// </summary>
    [Test]
    public async ValueTask ActivationRulesAsync()
    {
        var plugIn = CreatePlugIn();
        plugIn.Configuration!.ActivationsPerWeek = 1;
        var player = await CreatePlayerAsync(399).ConfigureAwait(false);
        Assert.That(await plugIn.ActivateAsync(player, Converter()).ConfigureAwait(false), Is.EqualTo(SoulConverterActivation.LevelTooLow));
        Assert.That(await plugIn.ActivateAsync(player, Converter(461)).ConfigureAwait(false), Is.EqualTo(SoulConverterActivation.NotAConverter));

        player.Attributes![Stats.MasterLevel] = 1;
        Assert.That(await plugIn.ActivateAsync(player, Converter()).ConfigureAwait(false), Is.EqualTo(SoulConverterActivation.Activated), "Level and master level count.");
        Assert.That(await plugIn.ActivateAsync(player, Converter()).ConfigureAwait(false), Is.EqualTo(SoulConverterActivation.AlreadyActive));

        plugIn.UtcNow = () => Now.AddDays(6);
        Assert.That(plugIn.GetState(player).Tier, Is.EqualTo(0), "The converter expired on Thursday.");
        Assert.That(await plugIn.ActivateAsync(player, Converter()).ConfigureAwait(false), Is.EqualTo(SoulConverterActivation.Activated), "A new week.");

        Assert.That(await plugIn.ClaimAsync(player, SoulRewardKind.Ruud).ConfigureAwait(false), Is.EqualTo(SoulConverterClaimResult.MissionsNotDone));
        await CompleteAsync(plugIn, player).ConfigureAwait(false);
        Assert.That(await plugIn.ClaimAsync(player, SoulRewardKind.Ruud).ConfigureAwait(false), Is.EqualTo(SoulConverterClaimResult.Rewarded));
        Assert.That(await plugIn.ActivateAsync(player, Converter()).ConfigureAwait(false), Is.EqualTo(SoulConverterActivation.WeeklyLimitReached));
    }

    /// <summary>
    /// A kill of a mission's monster on its map absorbs a soul, up to the required souls; party members on the map with the
    /// same mission get it too. Other monsters and an expired converter absorb nothing.
    /// </summary>
    [Test]
    public async ValueTask KillsAbsorbSoulsAsync()
    {
        var plugIn = CreatePlugIn();
        var player = await CreatePlayerAsync(400).ConfigureAwait(false);
        var member = await CreatePlayerAsync(400, player.GameContext).ConfigureAwait(false);
        var party = new Party(new PartyManager(5, new NullLogger<Party>()), 5, new NullLogger<Party>());
        await party.AddAsync(player).ConfigureAwait(false);
        await party.AddAsync(member).ConfigureAwait(false);
        await plugIn.ActivateAsync(player, Converter()).ConfigureAwait(false);
        await plugIn.ActivateAsync(member, Converter()).ConfigureAwait(false);

        for (var i = 0; i < 3; i++)
        {
            await plugIn.AttackableGotKilledAsync(CreateMonster(player, 26), player).ConfigureAwait(false);
        }

        await plugIn.AttackableGotKilledAsync(CreateMonster(player, 28), player).ConfigureAwait(false);

        Assert.That(Souls(plugIn, player), Is.EqualTo(new[] { (26, 2), (27, 0) }), "At most the required souls.");
        Assert.That(Souls(plugIn, member), Is.EqualTo(new[] { (26, 2), (27, 0) }), "The party shares the souls.");

        plugIn.UtcNow = () => Now.AddDays(6);
        await plugIn.AttackableGotKilledAsync(CreateMonster(player, 27), player).ConfigureAwait(false);
        Assert.That(plugIn.GetState(player).Missions, Is.Empty);
    }

    /// <summary>
    /// A claim gives the reward of the chosen kind and uses the converter up; a kind the tier doesn't offer is refused.
    /// </summary>
    [Test]
    public async ValueTask ClaimGivesTheRewardOfTheKindAsync()
    {
        var plugIn = CreatePlugIn();
        var player = await CreatePlayerAsync(400).ConfigureAwait(false);
        Assert.That(await plugIn.ClaimAsync(player, SoulRewardKind.Ruud).ConfigureAwait(false), Is.EqualTo(SoulConverterClaimResult.NoConverter));
        await plugIn.ActivateAsync(player, Converter()).ConfigureAwait(false);
        await CompleteAsync(plugIn, player).ConfigureAwait(false);

        Assert.That(await plugIn.ClaimAsync(player, SoulRewardKind.Accessory).ConfigureAwait(false), Is.EqualTo(SoulConverterClaimResult.KindNotOffered));
        Assert.That(await plugIn.ClaimAsync(player, SoulRewardKind.Ruud).ConfigureAwait(false), Is.EqualTo(SoulConverterClaimResult.Rewarded));

        Assert.That(player.SelectedCharacter!.Ruud, Is.InRange(400, 1000));
        Assert.That(plugIn.GetState(player).Tier, Is.EqualTo(0));
        Assert.That(await plugIn.ClaimAsync(player, SoulRewardKind.Ruud).ConfigureAwait(false), Is.EqualTo(SoulConverterClaimResult.NoConverter));
    }

    /// <summary>
    /// Using a converter item activates it and uses the item up; an item which can't be activated stays.
    /// </summary>
    [Test]
    public async ValueTask UsingTheItemActivatesTheConverterAsync()
    {
        var plugIn = CreatePlugIn();
        var player = await CreatePlayerAsync(400).ConfigureAwait(false);
        player.GameContext.FeaturePlugIns.AddPlugIn(plugIn, true);
        var first = Converter();
        var second = Converter();

        Assert.That(await new SoulConverter1ConsumeHandlerPlugIn().ConsumeItemAsync(player, first, null, FruitUsage.Undefined).ConfigureAwait(false), Is.True);
        Assert.That(await new SoulConverter1ConsumeHandlerPlugIn().ConsumeItemAsync(player, second, null, FruitUsage.Undefined).ConfigureAwait(false), Is.False, "One converter at a time.");

        Assert.That((first.Durability, second.Durability), Is.EqualTo(((byte)0, (byte)1)));
        Assert.That(plugIn.GetState(player).Tier, Is.EqualTo(1));
    }

    private static IEnumerable<(int Monster, int Souls)> Souls(SoulConverterPlugIn plugIn, Player player)
        => plugIn.GetState(player).Missions.OrderBy(m => m.MonsterNumber).Select(m => ((int)m.MonsterNumber, m.Souls));

    private static async ValueTask CompleteAsync(SoulConverterPlugIn plugIn, Player player)
    {
        foreach (var monster in new short[] { 26, 26, 27, 27 })
        {
            await plugIn.AttackableGotKilledAsync(CreateMonster(player, monster), player).ConfigureAwait(false);
        }
    }

    private static async ValueTask<Player> CreatePlayerAsync(int level, IGameContext? context = null)
    {
        var player = context is null ? await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false) : await PlayerTestHelper.CreatePlayerAsync(context).ConfigureAwait(false);
        player.SelectedCharacter!.Id = Guid.NewGuid();
        player.CurrentMap = await player.GameContext.GetMapAsync(0).ConfigureAwait(false);
        player.Attributes![Stats.Level] = level;
        return player;
    }

    private static DataModel.Entities.Item Converter(short number = 460)
        => new Persistence.BasicModel.Item { Definition = new Persistence.BasicModel.ItemDefinition { Group = 14, Number = number, Width = 1, Height = 1, Durability = 1 }, Durability = 1 };

    private static SoulConverterPlugIn CreatePlugIn()
        => new()
        {
            Configuration = new SoulConverterConfiguration
            {
                StateFile = string.Empty,
                Tiers =
                [
                    new SoulConverterTier
                    {
                        Tier = 1,
                        ItemGroup = 14,
                        ItemNumber = 460,
                        MinimumTotalLevel = 400,
                        MissionCount = 2,
                        Missions =
                        [
                            new SoulMission { MapNumber = 0, MonsterNumber = 26, RequiredSouls = 2, AbsorbRate = 1_000_000 },
                            new SoulMission { MapNumber = 0, MonsterNumber = 27, RequiredSouls = 2, AbsorbRate = 1_000_000 },
                        ],
                        Rewards = [new SoulReward { Kind = SoulRewardKind.Ruud, Draws = [new SoulRewardDraw { Chance = 10_000, MinimumRuud = 400, MaximumRuud = 1000 }] }],
                    },
                ],
            },
            UtcNow = () => Now,
        };

    private static Monster CreateMonster(Player player, short number)
        => new(new MonsterSpawnArea { GameMap = player.CurrentMap!.Definition }, new MonsterDefinition { Number = number }, player.CurrentMap, NullDropGenerator.Instance, new Moq.Mock<INpcIntelligence>().Object, player.GameContext.PlugInManager, player.GameContext.PathFinderPool);
}
