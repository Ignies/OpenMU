// <copyright file="MiningTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.World;

using Moq;
using MUnique.OpenMU.DataModel;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Mining;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Views.World;

/// <summary>
/// Tests the mining of Season 21 (<see cref="MiningFeaturePlugIn"/>).
/// </summary>
[TestFixture]
public class MiningTest
{
    private const short SmallSpot = 605;

    /// <summary>
    /// A successful stage raises the pending jewels and wears the pickaxe.
    /// </summary>
    [Test]
    public async ValueTask StageRaisesThePendingJewelsAsync()
    {
        var (player, plugIn, spot, pick) = await CreateAsync(successRate: 10000).ConfigureAwait(false);

        var result = await plugIn.MineAsync(player, spot).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(MiningResult.Succeeded));
        Assert.That(pick.Durability, Is.EqualTo(253));
        Mock.Get(player.ViewPlugIns.GetPlugIn<IMiningViewPlugIn>()!).Verify(v => v.ShowMiningResultAsync(spot, MiningResult.Succeeded, 1, 1, 0), Times.Once);
    }

    /// <summary>
    /// A failed stage loses the pending jewels; nothing is delivered.
    /// </summary>
    [Test]
    public async ValueTask FailedStageLosesThePendingJewelsAsync()
    {
        var (player, plugIn, spot, _) = await CreateAsync(successRate: 0).ConfigureAwait(false);

        Assert.That(await plugIn.MineAsync(player, spot).ConfigureAwait(false), Is.EqualTo(MiningResult.Failed));
        Assert.That(await plugIn.CollectAsync(player, spot).ConfigureAwait(false), Is.EqualTo(MiningResult.NoSpot), "Nothing is pending.");
        Assert.That(player.Inventory!.Items.Count(), Is.EqualTo(1), "Only the pickaxe.");
    }

    /// <summary>
    /// Collecting delivers the jewels of the stage reached, packed where a bundle fits.
    /// </summary>
    [Test]
    public async ValueTask CollectDeliversPackedAndSingleJewelsAsync()
    {
        var (player, plugIn, spot, _) = await CreateAsync(successRate: 10000, stages: 3).ConfigureAwait(false);
        plugIn.Configuration!.Spots[0].Stages[1].ItemCount = 23;

        await plugIn.MineAsync(player, spot).ConfigureAwait(false);
        await plugIn.MineAsync(player, spot).ConfigureAwait(false);
        var result = await plugIn.CollectAsync(player, spot).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(MiningResult.Collected));
        var items = player.Inventory!.Items.Where(i => i.Definition!.Group != 0).ToList();
        Assert.That(items.Count(i => i.Definition!.Group == 12 && i.Level == 1), Is.EqualTo(1), "One pack of 20.");
        Assert.That(items.Count(i => i.Definition!.Group == 14), Is.EqualTo(3), "Three single jewels.");
    }

    /// <summary>
    /// After the last stage the jewels and the jackpot are collected by themselves.
    /// </summary>
    [Test]
    public async ValueTask LastStageCollectsWithTheJackpotAsync()
    {
        var (player, plugIn, spot, _) = await CreateAsync(successRate: 10000, stages: 1).ConfigureAwait(false);
        plugIn.Configuration!.Spots[0].Stages[0].BonusRate = 10000;
        plugIn.Configuration.Spots[0].Stages[0].BonusCount = 10;

        var result = await plugIn.MineAsync(player, spot).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(MiningResult.Collected));
        Assert.That(player.Inventory!.Items.Count(i => i.Definition!.Group == 12 && i.Level == 0), Is.EqualTo(1), "The jackpot of 10 as one pack.");
        Assert.That(player.Inventory.Items.Count(i => i.Definition!.Group == 14), Is.EqualTo(1));
    }

    /// <summary>
    /// Mining needs the pickaxe in the hands, with durability left for the stage.
    /// </summary>
    [Test]
    public async ValueTask MiningNeedsAPickaxeWithDurabilityAsync()
    {
        var (player, plugIn, spot, pick) = await CreateAsync(successRate: 10000).ConfigureAwait(false);
        pick.Durability = 1;
        Assert.That(await plugIn.MineAsync(player, spot).ConfigureAwait(false), Is.EqualTo(MiningResult.PickaxeWorn));

        await player.Inventory!.RemoveItemAsync(pick).ConfigureAwait(false);
        Assert.That(await plugIn.MineAsync(player, spot).ConfigureAwait(false), Is.EqualTo(MiningResult.NoPickaxe));
    }

    /// <summary>
    /// A spot out of reach, or an NPC which is no spot, can't be mined.
    /// </summary>
    [Test]
    public async ValueTask SpotMustBeInReachAsync()
    {
        var (player, plugIn, spot, _) = await CreateAsync(successRate: 10000).ConfigureAwait(false);
        spot.Position = new Pathfinding.Point((byte)(player.Position.X + 10), player.Position.Y);
        Assert.That(await plugIn.MineAsync(player, spot).ConfigureAwait(false), Is.EqualTo(MiningResult.NoSpot));

        var other = new NonPlayerCharacter(null!, new MonsterDefinition { Number = 250 }, player.CurrentMap!) { Position = player.Position };
        Assert.That(await plugIn.MineAsync(player, other).ConfigureAwait(false), Is.EqualTo(MiningResult.NoSpot));
    }

    /// <summary>
    /// A spot serves one character at a time.
    /// </summary>
    [Test]
    public async ValueTask SpotServesOneMinerAsync()
    {
        var (player, plugIn, spot, _) = await CreateAsync(successRate: 10000, stages: 3).ConfigureAwait(false);
        var other = await PlayerTestHelper.CreatePlayerAsync(player.GameContext).ConfigureAwait(false);
        await AddItemsAsync(other).ConfigureAwait(false);
        other.Position = player.Position;

        Assert.That(await plugIn.MineAsync(player, spot).ConfigureAwait(false), Is.EqualTo(MiningResult.Succeeded));
        Assert.That(await plugIn.MineAsync(other, spot).ConfigureAwait(false), Is.EqualTo(MiningResult.Occupied));

        await plugIn.CollectAsync(player, spot).ConfigureAwait(false);
        Assert.That(await plugIn.MineAsync(other, spot).ConfigureAwait(false), Is.EqualTo(MiningResult.Succeeded), "Free again after the collect.");
    }

    /// <summary>
    /// Talking to a spot opens the mining window; without the plugin nothing happens.
    /// </summary>
    [Test]
    public async ValueTask TalkOpensTheMiningWindowAsync()
    {
        var (player, plugIn, spot, _) = await CreateAsync(successRate: 10000).ConfigureAwait(false);
        var args = new NpcTalkEventArgs();

        await plugIn.PlayerTalksToNpcAsync(player, spot, args).ConfigureAwait(false);

        Assert.That(args.HasBeenHandled, Is.True);
        Mock.Get(player.ViewPlugIns.GetPlugIn<IMiningViewPlugIn>()!).Verify(v => v.ShowMiningResultAsync(spot, MiningResult.Opened, 0, 0, 0), Times.Once);
    }

    private static async ValueTask<(Player Player, MiningFeaturePlugIn PlugIn, NonPlayerCharacter Spot, Item Pick)> CreateAsync(int successRate, int stages = 5)
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        var pick = await AddItemsAsync(player).ConfigureAwait(false);
        var spotDefinition = new MiningSpotDefinition { NpcNumber = SmallSpot };
        int[] counts = [1, 3, 7, 16, 33];
        for (var i = 0; i < stages; i++)
        {
            spotDefinition.Stages.Add(new MiningStage { DurabilityCost = 2, SuccessRate = successRate, ItemCount = counts[i] });
        }

        var configuration = new MiningConfiguration
        {
            Spots = { spotDefinition },
            Bundles =
            {
                new MiningBundle { ItemGroup = 12, ItemNumber = 30, ItemLevel = 2, JewelCount = 30 },
                new MiningBundle { ItemGroup = 12, ItemNumber = 30, ItemLevel = 1, JewelCount = 20 },
                new MiningBundle { ItemGroup = 12, ItemNumber = 30, ItemLevel = 0, JewelCount = 10 },
            },
        };
        var plugIn = new MiningFeaturePlugIn { Configuration = configuration };
        player.GameContext.FeaturePlugIns.AddPlugIn(plugIn, true);
        var spot = new NonPlayerCharacter(null!, new MonsterDefinition { Number = SmallSpot }, player.CurrentMap!) { Position = player.Position };
        return (player, plugIn, spot, pick);
    }

    private static async ValueTask<Item> AddItemsAsync(Player player)
    {
        var items = player.GameContext.Configuration.Items;
        if (items.Count == 0)
        {
            var pickDefinition = new Mock<ItemDefinition>();
            pickDefinition.SetupAllProperties();
            pickDefinition.Setup(d => d.PossibleItemSetGroups).Returns(new List<ItemSetGroup>());
            pickDefinition.Setup(d => d.BasePowerUpAttributes).Returns(new List<ItemBasePowerUpDefinition>());
            pickDefinition.Object.Number = 41;
            pickDefinition.Object.Width = 2;
            pickDefinition.Object.Height = 2;
            pickDefinition.Object.Durability = 255;
            pickDefinition.Object.IsActive = true;
            items.Add(pickDefinition.Object);
            items.Add(new ItemDefinition { Group = 14, Number = 13, Width = 1, Height = 1, Durability = 1, IsActive = true });
            items.Add(new ItemDefinition { Group = 12, Number = 30, Width = 1, Height = 1, Durability = 1, IsActive = true });
        }

        var pick = player.PersistenceContext.CreateNew<Item>();
        pick.Definition = items.First(d => d.Group == 0);
        pick.Durability = 255;
        await player.Inventory!.AddItemAsync(InventoryConstants.LeftHandSlot, pick).ConfigureAwait(false);
        return pick;
    }
}
