// <copyright file="RuudShopTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Economy;

using Moq;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlayerActions.Items;
using MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Ruud;
using MUnique.OpenMU.GameLogic.Views.Economy;

/// <summary>
/// Tests Ruud: the Ruud shop of Priest James, its class filter and the sources of Ruud.
/// </summary>
[TestFixture]
public class RuudShopTest
{
    private const short PriestJames = 725;
    private const int SpiritPrice = 50_000;

    /// <summary>
    /// A purchase takes the Ruud of the character and puts the item into the inventory.
    /// </summary>
    [Test]
    public async ValueTask PurchaseTakesTheRuudAndGivesTheItemAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        player.SelectedCharacter!.Ruud = 120_000;

        var result = await new BuyRuudShopItemAction().BuyAsync(player, 0).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(RuudShopBuyResult.Bought));
        Assert.That(player.SelectedCharacter.Ruud, Is.EqualTo(120_000 - SpiritPrice));
        var bought = player.Inventory!.Items.Single();
        Assert.That((bought.Definition!.Group, bought.Definition.Number), Is.EqualTo(((byte)14, (short)341)));
        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<IRuudViewPlugIn>()!);
        view.Verify(v => v.ShowRuudAsync(120_000 - SpiritPrice, -SpiritPrice), Times.Once);
        view.Verify(v => v.ShowRuudShopBuyResultAsync(RuudShopBuyResult.Bought, 0), Times.Once);
    }

    /// <summary>
    /// Without enough Ruud, nothing is bought.
    /// </summary>
    [Test]
    public async ValueTask PurchaseWithoutEnoughRuudFailsAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        player.SelectedCharacter!.Ruud = SpiritPrice - 1;

        var result = await new BuyRuudShopItemAction().BuyAsync(player, 0).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(RuudShopBuyResult.NotEnoughRuud));
        Assert.That(player.SelectedCharacter.Ruud, Is.EqualTo(SpiritPrice - 1));
        Assert.That(player.Inventory!.Items, Is.Empty);
    }

    /// <summary>
    /// An item of another class isn't sold to the character, and isn't in its list.
    /// </summary>
    [Test]
    public async ValueTask ItemOfAnotherClassIsNotSoldAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        player.SelectedCharacter!.Ruud = 1_000_000;

        var result = await new BuyRuudShopItemAction().BuyAsync(player, 1).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(RuudShopBuyResult.UnknownItem));
        Assert.That(RuudFeaturePlugIn.Get(player.GameContext)!.GetShopEntries(player).Select(e => e.Index), Is.EquivalentTo(new[] { 0 }));
    }

    /// <summary>
    /// The shop is closed when the character doesn't talk to its NPC.
    /// </summary>
    [Test]
    public async ValueTask PurchaseAwayFromTheNpcFailsAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        player.SelectedCharacter!.Ruud = 1_000_000;
        player.OpenedNpc = null;

        var result = await new BuyRuudShopItemAction().BuyAsync(player, 0).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(RuudShopBuyResult.ShopClosed));
        Assert.That(player.SelectedCharacter.Ruud, Is.EqualTo(1_000_000));
    }

    /// <summary>
    /// Talking to Priest James opens the shop with the items of the class.
    /// </summary>
    [Test]
    public async ValueTask TalkingToPriestJamesOpensTheShopAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var eventArgs = new NpcTalkEventArgs();

        await RuudFeaturePlugIn.Get(player.GameContext)!.PlayerTalksToNpcAsync(player, player.OpenedNpc!, eventArgs).ConfigureAwait(false);

        Assert.That((eventArgs.HasBeenHandled, eventArgs.LeavesDialogOpen), Is.EqualTo((true, true)));
        Mock.Get(player.ViewPlugIns.GetPlugIn<IRuudViewPlugIn>()!)
            .Verify(v => v.ShowRuudShopAsync(It.Is<IReadOnlyList<(int Index, RuudShopEntry Entry)>>(l => l.Count == 1 && l[0].Index == 0)), Times.Once);
    }

    /// <summary>
    /// With Ruud on, Priest James is the Ruud shop; the field bosses then leave his talk to it.
    /// </summary>
    [Test]
    public async ValueTask PriestJamesIsTheShopNpcOnlyWithRuudAsync()
    {
        var off = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        var on = await CreatePlayerAsync().ConfigureAwait(false);

        Assert.That(RuudFeaturePlugIn.IsShopNpc(off.GameContext, PriestJames), Is.False);
        Assert.That(RuudFeaturePlugIn.IsShopNpc(on.GameContext, PriestJames), Is.True);
        Assert.That(RuudFeaturePlugIn.IsShopNpc(on.GameContext, 251), Is.False);
    }

    /// <summary>
    /// Ruud stops at the configured maximum.
    /// </summary>
    [Test]
    public async ValueTask RuudStopsAtTheMaximumAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        RuudFeaturePlugIn.Get(player.GameContext)!.Configuration!.MaximumRuud = 1_000;
        player.SelectedCharacter!.Ruud = 900;

        var added = await player.AddRuudAsync(500).ConfigureAwait(false);

        Assert.That((added, player.SelectedCharacter.Ruud), Is.EqualTo((100, 1_000)));
    }

    /// <summary>
    /// Opening a Ruud box gives its Ruud and uses it up.
    /// </summary>
    [Test]
    public async ValueTask RuudBoxGivesItsRuudAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var box = player.PersistenceContext.CreateNew<Item>();
        box.Definition = new ItemDefinition { Group = 14, Number = 352, Width = 2, Height = 2, Durability = 1 };
        box.Durability = 1;

        var consumed = await new RuudBox1000ConsumeHandlerPlugIn().ConsumeItemAsync(player, box, null, FruitUsage.Undefined).ConfigureAwait(false);

        Assert.That(consumed, Is.True);
        Assert.That(player.SelectedCharacter!.Ruud, Is.EqualTo(1_000));
        Assert.That(box.Durability, Is.EqualTo(0));
    }

    private static async ValueTask<Player> CreatePlayerAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        var spirit = new ItemDefinition { Group = 14, Number = 341, Width = 1, Height = 1, Durability = 1, IsActive = true };
        var sword = new ItemDefinition { Group = 0, Number = 42, Width = 1, Height = 4, Durability = 50, IsActive = true };
        player.GameContext.Configuration.Items.Add(spirit);
        player.GameContext.Configuration.Items.Add(sword);
        var configuration = new RuudConfiguration
        {
            ShopNpcNumber = PriestJames,
            ShopEntries =
            {
                new RuudShopEntry { Group = 14, Number = 341, Price = SpiritPrice },
                new RuudShopEntry { Class = BaseClassCode.DarkKnight, Group = 0, Number = 42, Price = 100_000 },
            },
            BoxRewards = { new RuudBoxReward { Group = 14, Number = 352, Amount = 1_000 } },
        };
        player.GameContext.FeaturePlugIns.AddPlugIn(new RuudFeaturePlugIn { Configuration = configuration }, true);
        player.SelectedCharacter!.CharacterClass = new CharacterClass { Number = 0 };
        player.OpenedNpc = new NonPlayerCharacter(null!, new MonsterDefinition { Number = PriestJames }, null!);
        return player;
    }
}
