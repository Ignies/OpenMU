// <copyright file="MuunServiceTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Muuns;

using Moq;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Muuns;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.Views.Muuns;
using BasicModel = MUnique.OpenMU.Persistence.BasicModel;

/// <summary>
/// Tests the Muun system in the game: equipping a Muun applies its option to the owner, the additional option, level
/// up, evolution, the energy converter, the exchange at Monica, riding and the health of the Muuns.
/// </summary>
[TestFixture]
public class MuunServiceTest
{
    private const byte Group = 16;
    private const short Pinocchio = 0;
    private const short PinocchioEvolved = 1;
    private const short DefenseMuun = 2;
    private const short Tibetton = 231;
    private const short EvolutionStone = 211;
    private const short Converter = 239;
    private const short WizardryStone = 240;
    private const short Branch = 411;
    private const short AttackMuun = 4;
    private const short RepairMuun = 8;
    private const short PlayTimeMuun = 40;
    private const short HeroMuun = 69;

    /// <summary>
    /// A Muun moved from the inventory into the main slot applies its option of its level to the owner; moved into the
    /// bag, it doesn't anymore.
    /// </summary>
    [Test]
    public async ValueTask EquippedMuunAppliesItsOptionAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var muun = await AddToInventoryAsync(player, DefenseMuun).ConfigureAwait(false);
        var defense = player.Attributes![Stats.DefenseBase];

        var result = await MuunService.MoveAsync(player, MuunStorageKind.Inventory, muun.ItemSlot, MuunStorageKind.MuunInventory, MuunSlots.Main).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(MuunActionResult.Success));
        Assert.That(player.Inventory!.Items, Is.Empty);
        Assert.That(player.SelectedCharacter!.MuunInventory!.Items, Is.EquivalentTo(new[] { muun }));
        Assert.That(player.Attributes[Stats.DefenseBase] - defense, Is.EqualTo(22), "level 1 of the defense option");
        Assert.That(MuunService.GetOptions(player), Is.EqualTo(new[] { new MuunActiveOption(MuunSlots.Main, MuunOptionType.Defense, 22, true, false) }));
        Mock.Get(player.ViewPlugIns.GetPlugIn<IMuunViewPlugIn>()!).Verify(v => v.MuunItemMovedAsync(MuunActionResult.Success, MuunStorageKind.Inventory, It.IsAny<byte>(), MuunStorageKind.MuunInventory, MuunSlots.Main, muun), Times.Once);

        await MuunService.MoveAsync(player, MuunStorageKind.MuunInventory, MuunSlots.Main, MuunStorageKind.MuunInventory, 10).ConfigureAwait(false);

        Assert.That(muun.ItemSlot, Is.EqualTo(10));
        Assert.That(player.Attributes[Stats.DefenseBase], Is.EqualTo(defense));
    }

    /// <summary>
    /// The attack power option of a Muun adds to the final damage against monsters.
    /// </summary>
    [Test]
    public async ValueTask AttackPowerAddsToTheDamageAgainstMonstersAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        player.Attributes!.AddElement(new SimpleElement(1000, AggregateType.AddRaw), Stats.AttackRatePvm);
        var monster = await CreateMonsterAsync(player).ConfigureAwait(false);
        var without = await player.CalculateDamageAsync(monster, null, false).ConfigureAwait(false);

        player.Attributes.AddElement(new SimpleElement(500, AggregateType.AddRaw), Stats.FinalDamageBonusPvm);
        var with = await player.CalculateDamageAsync(monster, null, false).ConfigureAwait(false);

        Assert.That(without.HealthDamage, Is.LessThan(500));
        Assert.That(with.HealthDamage, Is.GreaterThanOrEqualTo(500));
    }

    /// <summary>
    /// A Muun put into the Muun inventory for the first time starts its additional option, which multiplies its option
    /// for its days (x20 for 30 days); after them, the option is the plain one again.
    /// </summary>
    [Test]
    public async ValueTask AdditionalOptionMultipliesTheOptionForItsDaysAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var muun = await AddToInventoryAsync(player, Pinocchio).ConfigureAwait(false);

        await MuunService.MoveAsync(player, MuunStorageKind.Inventory, muun.ItemSlot, MuunStorageKind.MuunInventory, MuunSlots.Sub).ConfigureAwait(false);

        Assert.That(muun.TimedOptionExpiresAt!.Value, Is.EqualTo(DateTime.UtcNow.AddDays(30)).Within(TimeSpan.FromMinutes(1)));
        Assert.That(player.Attributes![Stats.FinalDamageBonusPvm], Is.EqualTo(14 * 20));
        Assert.That(MuunService.GetOptions(player).Single().IsBoosted, Is.True);

        muun.TimedOptionExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        await MuunService.RefreshAsync(player, false).ConfigureAwait(false);

        Assert.That(player.Attributes[Stats.FinalDamageBonusPvm], Is.EqualTo(14));
    }

    /// <summary>
    /// Only a Muun goes into an equipped slot, and only items of the Muun group into the Muun inventory.
    /// </summary>
    [Test]
    public async ValueTask OnlyMuunsAreEquippedAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var converter = await AddToInventoryAsync(player, Converter).ConfigureAwait(false);
        var jewel = await AddToInventoryAsync(player, ItemConstants.JewelOfLife.Number!.Value, ItemConstants.JewelOfLife.Group).ConfigureAwait(false);

        Assert.That(await MuunService.MoveAsync(player, MuunStorageKind.Inventory, converter.ItemSlot, MuunStorageKind.MuunInventory, MuunSlots.Main).ConfigureAwait(false), Is.EqualTo(MuunActionResult.NotAMuun));
        Assert.That(await MuunService.MoveAsync(player, MuunStorageKind.Inventory, jewel.ItemSlot, MuunStorageKind.MuunInventory, 5).ConfigureAwait(false), Is.EqualTo(MuunActionResult.NotAMuun));
        Assert.That(await MuunService.MoveAsync(player, MuunStorageKind.Inventory, converter.ItemSlot, MuunStorageKind.MuunInventory, 5).ConfigureAwait(false), Is.EqualTo(MuunActionResult.Success));
        Assert.That(converter.ItemSlot, Is.EqualTo(5));
    }

    /// <summary>
    /// A Muun of level 1 of the same kind raises the level of a Muun by one, up to its highest level (rank + 1).
    /// </summary>
    [Test]
    public async ValueTask LevelUpTakesALevelOneCopyAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var defense = player.Attributes![Stats.DefenseBase];
        var target = await AddToMuunInventoryAsync(player, DefenseMuun, MuunSlots.Main).ConfigureAwait(false);

        for (var level = 2; level <= 3; level++)
        {
            await AddToMuunInventoryAsync(player, DefenseMuun, 20).ConfigureAwait(false);
            var result = await MuunService.UseItemAsync(player, MuunAction.LevelUp, MuunStorageKind.MuunInventory, 20, MuunStorageKind.MuunInventory, MuunSlots.Main).ConfigureAwait(false);
            Assert.That(result, Is.EqualTo(MuunActionResult.Success));
            Assert.That(target.Level, Is.EqualTo(level));
        }

        Assert.That(player.SelectedCharacter!.MuunInventory!.Items, Is.EquivalentTo(new[] { target }), "the materials are used up");
        Assert.That(player.Attributes[Stats.DefenseBase] - defense, Is.EqualTo(30), "the value of level 3");

        await AddToMuunInventoryAsync(player, DefenseMuun, 20).ConfigureAwait(false);
        Assert.That(await MuunService.UseItemAsync(player, MuunAction.LevelUp, MuunStorageKind.MuunInventory, 20, MuunStorageKind.MuunInventory, MuunSlots.Main).ConfigureAwait(false), Is.EqualTo(MuunActionResult.MaximumLevel));

        await AddToMuunInventoryAsync(player, Pinocchio, 21).ConfigureAwait(false);
        Assert.That(await MuunService.UseItemAsync(player, MuunAction.LevelUp, MuunStorageKind.MuunInventory, 21, MuunStorageKind.MuunInventory, 20).ConfigureAwait(false), Is.EqualTo(MuunActionResult.WrongMaterial));
    }

    /// <summary>
    /// An evolution stone of the Muun evolves it at its highest level into its evolved form, whose option applies with
    /// its evolved value (against players too).
    /// </summary>
    [Test]
    public async ValueTask EvolutionStoneEvolvesAMuunOfTheHighestLevelAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var muun = await AddToMuunInventoryAsync(player, Pinocchio, MuunSlots.Main).ConfigureAwait(false);
        muun.TimedOptionExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        muun.Level = 4;
        await AddStoneAsync(player, 30, Pinocchio).ConfigureAwait(false);
        await AddStoneAsync(player, 31, DefenseMuun).ConfigureAwait(false);

        Assert.That(await MuunService.UseItemAsync(player, MuunAction.Evolve, MuunStorageKind.MuunInventory, 30, MuunStorageKind.MuunInventory, MuunSlots.Main).ConfigureAwait(false), Is.EqualTo(MuunActionResult.LevelTooLow));
        muun.Level = 5;
        Assert.That(await MuunService.UseItemAsync(player, MuunAction.Evolve, MuunStorageKind.MuunInventory, 31, MuunStorageKind.MuunInventory, MuunSlots.Main).ConfigureAwait(false), Is.EqualTo(MuunActionResult.WrongMaterial));

        var result = await MuunService.UseItemAsync(player, MuunAction.Evolve, MuunStorageKind.MuunInventory, 30, MuunStorageKind.MuunInventory, MuunSlots.Main).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(MuunActionResult.Success));
        Assert.That(muun.Definition!.Number, Is.EqualTo(PinocchioEvolved));
        Assert.That(MuunService.GetItem(player, MuunStorageKind.MuunInventory, 30), Is.Null, "the stone is used up");
        Assert.That(player.Attributes![Stats.FinalDamageBonus], Is.EqualTo(24), "the evolved value, against players too");
        Assert.That(player.Attributes[Stats.FinalDamageBonusPvm], Is.Zero);
    }

    /// <summary>
    /// A Muun gives the converter the energy of its rank and level, an evolution stone 2; at 120 the converter becomes a
    /// wizardry stone.
    /// </summary>
    [Test]
    public async ValueTask ConverterBecomesAWizardryStoneAtFullEnergyAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var converter = await AddToMuunInventoryAsync(player, Converter, 40).ConfigureAwait(false);
        converter.Durability = 0;
        var muun = await AddToMuunInventoryAsync(player, Pinocchio, 41).ConfigureAwait(false);
        muun.Level = 5;

        Assert.That(await MuunService.UseItemAsync(player, MuunAction.ChargeEnergy, MuunStorageKind.MuunInventory, 41, MuunStorageKind.MuunInventory, 40).ConfigureAwait(false), Is.EqualTo(MuunActionResult.Success));
        Assert.That(converter.Durability, Is.EqualTo(4 + 15), "rank 4 and level 5");

        await AddStoneAsync(player, 42, Branch).ConfigureAwait(false);
        await MuunService.UseItemAsync(player, MuunAction.ChargeEnergy, MuunStorageKind.MuunInventory, 42, MuunStorageKind.MuunInventory, 40).ConfigureAwait(false);
        Assert.That(converter.Durability, Is.EqualTo(21));

        converter.Durability = 115;
        await AddToMuunInventoryAsync(player, DefenseMuun, 43).ConfigureAwait(false);
        await MuunService.UseItemAsync(player, MuunAction.ChargeEnergy, MuunStorageKind.MuunInventory, 43, MuunStorageKind.MuunInventory, 40).ConfigureAwait(false);

        Assert.That(converter.Definition!.Number, Is.EqualTo(WizardryStone));
        Assert.That(player.SelectedCharacter!.MuunInventory!.Items, Is.EquivalentTo(new[] { converter }));
    }

    /// <summary>
    /// Monica exchanges two wizardry stones for the evolution stone of an offer, which lands in the Muun bag.
    /// </summary>
    [Test]
    public async ValueTask MonicaExchangesWizardryStonesForAnEvolutionStoneAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var first = await AddToInventoryAsync(player, WizardryStone).ConfigureAwait(false);
        Assert.That(await MuunService.ExchangeAsync(player, 0).ConfigureAwait(false), Is.EqualTo(MuunActionResult.NpcNotOpened));

        player.OpenedNpc = new NonPlayerCharacter(null!, new MonsterDefinition { Number = 682 }, null!);
        Assert.That(await MuunService.ExchangeAsync(player, 0).ConfigureAwait(false), Is.EqualTo(MuunActionResult.NotEnoughItems));

        await AddToMuunInventoryAsync(player, WizardryStone, 60).ConfigureAwait(false);
        var result = await MuunService.ExchangeAsync(player, 0).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(MuunActionResult.Success));
        Assert.That(player.Inventory!.Items, Does.Not.Contain(first));
        var stone = player.SelectedCharacter!.MuunInventory!.Items.Single();
        Assert.That((stone.Definition!.Number, MuunService.GetEvolutionTarget(stone), stone.ItemSlot), Is.EqualTo((EvolutionStone, (short?)Branch, MuunSlots.FirstBagSlot)));
    }

    /// <summary>
    /// Only a mount in the main slot is ridden; moving it out of the slot dismounts.
    /// </summary>
    [Test]
    public async ValueTask OnlyAMountInTheMainSlotIsRiddenAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        await AddToMuunInventoryAsync(player, DefenseMuun, MuunSlots.Main).ConfigureAwait(false);
        Assert.That(await MuunService.RideAsync(player, true).ConfigureAwait(false), Is.EqualTo(MuunActionResult.NotAMount));

        await MuunService.MoveAsync(player, MuunStorageKind.MuunInventory, MuunSlots.Main, MuunStorageKind.MuunInventory, 10).ConfigureAwait(false);
        await AddToMuunInventoryAsync(player, Tibetton, MuunSlots.Main).ConfigureAwait(false);

        Assert.That(await MuunService.RideAsync(player, true).ConfigureAwait(false), Is.EqualTo(MuunActionResult.Success));
        Assert.That(MuunService.IsRiding(player), Is.True);
        Mock.Get(player.ViewPlugIns.GetPlugIn<IMuunViewPlugIn>()!).Verify(v => v.ShowMuunAppearanceAsync(player, It.Is<Item>(i => i != null && i.Definition!.Number == Tibetton), true), Times.Once);

        await MuunService.MoveAsync(player, MuunStorageKind.MuunInventory, MuunSlots.Main, MuunStorageKind.MuunInventory, 11).ConfigureAwait(false);
        Assert.That(MuunService.IsRiding(player), Is.False);
    }

    /// <summary>
    /// An equipped Muun loses health; without health its option stops, and a jewel of life restores it.
    /// </summary>
    [Test]
    public async ValueTask JewelOfLifeRestoresAMuunWithoutHealthAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var muun = await AddToMuunInventoryAsync(player, DefenseMuun, MuunSlots.Main).ConfigureAwait(false);
        var defense = player.Attributes![Stats.DefenseBase];
        muun.Durability = 1;

        await MuunService.DecayHealthAsync(player, 1).ConfigureAwait(false);
        await MuunService.RefreshAsync(player, false).ConfigureAwait(false);

        Assert.That(muun.Durability, Is.Zero);
        Assert.That(player.Attributes[Stats.DefenseBase], Is.EqualTo(defense - 22));

        var jewel = await AddToInventoryAsync(player, ItemConstants.JewelOfLife.Number!.Value, ItemConstants.JewelOfLife.Group).ConfigureAwait(false);
        var result = await MuunService.UseItemAsync(player, MuunAction.Restore, MuunStorageKind.Inventory, jewel.ItemSlot, MuunStorageKind.MuunInventory, MuunSlots.Main).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(MuunActionResult.Success));
        Assert.That(muun.Durability, Is.EqualTo(255));
        Assert.That(player.Inventory!.Items, Is.Empty);
        Assert.That(player.Attributes[Stats.DefenseBase], Is.EqualTo(defense));
    }

    /// <summary>
    /// With the Muun system off, nothing moves.
    /// </summary>
    [Test]
    public async ValueTask NothingMovesWhenTheSystemIsOffAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        AddDefinitions(player);
        var muun = await AddToInventoryAsync(player, DefenseMuun).ConfigureAwait(false);

        var result = await MuunService.MoveAsync(player, MuunStorageKind.Inventory, muun.ItemSlot, MuunStorageKind.MuunInventory, MuunSlots.Main).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(MuunActionResult.Disabled));
        Assert.That(player.Inventory!.Items, Is.EquivalentTo(new[] { muun }));
    }

    /// <summary>
    /// A Muun with an attack skill hits the nearest monster for its level times the damage per level, then waits for
    /// its cooldown, which the additional option (kind 3) shortens by its seconds.
    /// </summary>
    [Test]
    public async ValueTask AttackSkillHitsTheNearestMonsterAfterItsCooldownAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var monster = await CreateMonsterAsync(player).ConfigureAwait(false);
        var muun = await AddToMuunInventoryAsync(player, AttackMuun, MuunSlots.Main).ConfigureAwait(false);
        muun.Level = 3;
        await MuunService.RefreshAsync(player, false).ConfigureAwait(false);
        var health = monster.Health;
        var now = DateTime.UtcNow;

        var target = await MuunService.UseAttackSkillsAsync(player, now).ConfigureAwait(false);

        Assert.That(target, Is.SameAs(monster));
        Assert.That(health - monster.Health, Is.EqualTo(8 * 100), "level 3 of the skill is 8, 100 damage each");
        Assert.That(await MuunService.UseAttackSkillsAsync(player, now.AddSeconds(7)).ConfigureAwait(false), Is.Null, "10 s cooldown minus 2 s");
        Assert.That(await MuunService.UseAttackSkillsAsync(player, now.AddSeconds(8)).ConfigureAwait(false), Is.SameAs(monster));
        Mock.Get(player.ViewPlugIns.GetPlugIn<IMuunViewPlugIn>()!).Verify(v => v.ShowMuunSkillAsync(player, monster, 8, 800), Times.Exactly(2));
    }

    /// <summary>
    /// The repair cost option lowers the repair price by its percentage, x5 while the additional option lasts.
    /// </summary>
    [Test]
    public async ValueTask RepairCostOptionLowersTheRepairPriceAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var sword = new BasicModel.Item { Definition = new BasicModel.ItemDefinition { Group = 0, Number = 0, Width = 1, Height = 3, Durability = 20, Value = 1000, DropLevel = 10 }, Durability = 5 };
        sword.Definition!.ItemSlot = new BasicModel.ItemSlotType();
        var price = GameLogic.PlayerActions.Items.ItemRepairAction.GetRepairPrice(player, sword);
        Assert.That(price, Is.GreaterThan(100));

        await AddToMuunInventoryAsync(player, RepairMuun, MuunSlots.Sub).ConfigureAwait(false);

        Assert.That(player.Attributes![Stats.ItemRepairCostDecrease], Is.EqualTo(0.05f).Within(0.0001f), "1 % x5");
        Assert.That(GameLogic.PlayerActions.Items.ItemRepairAction.GetRepairPrice(player, sword), Is.EqualTo((long)(price * 0.95f)));
    }

    /// <summary>
    /// The hero slot opens at its total level and takes hero Muuns only; the same Muun can't be equipped twice.
    /// </summary>
    [Test]
    public async ValueTask HeroSlotOpensAtItsLevelAndTheSameMuunIsEquippedOnceAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var hero = await AddToInventoryAsync(player, HeroMuun).ConfigureAwait(false);
        Assert.That(await MuunService.MoveAsync(player, MuunStorageKind.Inventory, hero.ItemSlot, MuunStorageKind.MuunInventory, MuunSlots.Hero).ConfigureAwait(false), Is.EqualTo(MuunActionResult.SlotLocked), "below 1750");

        MuunFeaturePlugIn.Get(player.GameContext)!.Configuration!.HeroSlotLevel = 0;
        var defense = await AddToInventoryAsync(player, DefenseMuun).ConfigureAwait(false);
        Assert.That(await MuunService.MoveAsync(player, MuunStorageKind.Inventory, defense.ItemSlot, MuunStorageKind.MuunInventory, MuunSlots.Hero).ConfigureAwait(false), Is.EqualTo(MuunActionResult.SlotLocked), "no hero Muun");
        Assert.That(await MuunService.MoveAsync(player, MuunStorageKind.Inventory, hero.ItemSlot, MuunStorageKind.MuunInventory, MuunSlots.Hero).ConfigureAwait(false), Is.EqualTo(MuunActionResult.Success));
        Assert.That(MuunService.GetOptions(player).Single().Slot, Is.EqualTo(MuunSlots.Hero));

        await MuunService.MoveAsync(player, MuunStorageKind.Inventory, defense.ItemSlot, MuunStorageKind.MuunInventory, MuunSlots.Main).ConfigureAwait(false);
        var second = await AddToInventoryAsync(player, DefenseMuun).ConfigureAwait(false);
        Assert.That(await MuunService.MoveAsync(player, MuunStorageKind.Inventory, second.ItemSlot, MuunStorageKind.MuunInventory, MuunSlots.Sub).ConfigureAwait(false), Is.EqualTo(MuunActionResult.SameMuunEquipped));
    }

    /// <summary>
    /// Riding a mount raises the movement speed to the riding speed; dismounting gives the old speed back.
    /// </summary>
    [Test]
    public async ValueTask RidingRaisesTheMovementSpeedAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        await AddToMuunInventoryAsync(player, Tibetton, MuunSlots.Main).ConfigureAwait(false);
        var speed = player.Attributes![Stats.MovementSpeed];

        await MuunService.RideAsync(player, true).ConfigureAwait(false);
        Assert.That(player.Attributes[Stats.MovementSpeed], Is.EqualTo(15));

        await MuunService.RideAsync(player, false).ConfigureAwait(false);
        Assert.That(player.Attributes[Stats.MovementSpeed], Is.EqualTo(speed));
    }

    /// <summary>
    /// A Muun with the play time condition counts the minutes it is equipped; after 5 hours its option applies.
    /// </summary>
    [Test]
    public async ValueTask PlayTimeConditionCountsTheEquippedMinutesAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var muun = await AddToMuunInventoryAsync(player, PlayTimeMuun, MuunSlots.Main).ConfigureAwait(false);
        var start = DateTime.UtcNow;
        MuunService.AddPlayTime(player, start);
        Assert.That(MuunService.GetOptions(player).Single().IsActive, Is.False);

        MuunService.AddPlayTime(player, start.AddHours(5));
        await MuunService.RefreshAsync(player, false).ConfigureAwait(false);

        Assert.That(muun.PetExperience, Is.EqualTo(300));
        Assert.That(MuunService.GetOptions(player).Single().IsActive, Is.True);
        Assert.That(player.Attributes![Stats.MoneyAmountRate], Is.EqualTo(1.02f).Within(0.0001f));
    }

    private static MuunConfiguration CreateConfiguration()
    {
        var always = new MuunCondition();
        return new MuunConfiguration
        {
            RankEnergy = Enumerable.Range(1, 6).Select(r => new MuunEnergyPoints { Key = r, Points = r }).ToList(),
            LevelEnergy = Enumerable.Range(0, 8).Select(l => new MuunEnergyPoints { Key = l, Points = l == 0 ? 4 : l * 3 }).ToList(),
            ExchangeOffers = [new MuunExchangeOffer { CostGroup = Group, CostNumber = WizardryStone, CostCount = 2, ResultGroup = Group, ResultNumber = EvolutionStone, TargetNumber = Branch }],
            Muuns =
            [
                new MuunDefinition { Number = Pinocchio, Grade = MuunGrade.Premium, Rank = 4, OptionGroup = 42, AddOptionType = 2, AddOptionDays = 30, EvolvesTo = PinocchioEvolved },
                new MuunDefinition { Number = PinocchioEvolved, Grade = MuunGrade.Premium, Rank = 4, OptionGroup = 43, AddOptionType = 2, AddOptionDays = 30, IsEvolved = true },
                new MuunDefinition { Number = DefenseMuun, Rank = 2, OptionGroup = 0 },
                new MuunDefinition { Number = Tibetton, Kind = MuunKind.Mount, Rank = 3, OptionGroup = 11 },
                new MuunDefinition { Number = AttackMuun, Rank = 2, OptionGroup = 25, AddOptionType = 5, AddOptionDays = 30 },
                new MuunDefinition { Number = RepairMuun, Rank = 2, OptionGroup = 38, AddOptionType = 3, AddOptionDays = 30 },
                new MuunDefinition { Number = PlayTimeMuun, Rank = 2, OptionGroup = 37 },
                new MuunDefinition { Number = HeroMuun, Kind = MuunKind.Hero, Rank = 4, OptionGroup = 69 },
            ],
            Options =
            [
                new MuunOptionDefinition { Group = 0, Type = MuunOptionType.Defense, Values = [38, 22, 26, 30, 34, 0], Condition = always, EvolvedCondition = always },
                new MuunOptionDefinition { Group = 42, Type = MuunOptionType.MaximumAttackPower, Values = [24, 14, 16, 18, 20, 22], Condition = always, EvolvedCondition = always },
                new MuunOptionDefinition { Group = 43, Type = MuunOptionType.MaximumAttackPowerPvp, Values = [24, 14, 16, 18, 20, 22], Condition = always, EvolvedCondition = always },
                new MuunOptionDefinition { Group = 11, Type = MuunOptionType.AttackPowerPvp, Values = [14, 6, 8, 10, 12, 0], Condition = always, EvolvedCondition = always },
                new MuunOptionDefinition { Group = 25, Type = MuunOptionType.AttackSkillPvm, Values = [10, 4, 6, 8, 0, 0], Condition = always, EvolvedCondition = always },
                new MuunOptionDefinition { Group = 38, Type = MuunOptionType.RepairCost, Values = [4, 1, 2, 3, 0, 0], Condition = always, EvolvedCondition = always },
                new MuunOptionDefinition { Group = 37, Type = MuunOptionType.Money, Values = [8, 2, 4, 6, 0, 0], Condition = new MuunCondition { Type = MuunConditionType.PlayTime, Value1 = 5 }, EvolvedCondition = always },
                new MuunOptionDefinition { Group = 69, Type = MuunOptionType.SkillAttackPower, Values = [15, 15, 15, 15, 15, 15], Condition = always, EvolvedCondition = always },
            ],
            AddOptions =
            [
                new MuunAddOptionDefinition { Type = 2, Kind = MuunAddOptionKind.Multiplier, Values = [20, 20, 20, 20, 20, 20] },
                new MuunAddOptionDefinition { Type = 3, Kind = MuunAddOptionKind.Multiplier, Values = [5, 5, 5, 5, 5, 5] },
                new MuunAddOptionDefinition { Type = 5, Kind = MuunAddOptionKind.SkillCooldownReduction, Values = [2, 2, 2, 2, 2, 2] },
            ],
        };
    }

    private static async ValueTask<Player> CreatePlayerAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        AddDefinitions(player);
        player.GameContext.FeaturePlugIns.AddPlugIn(new MuunFeaturePlugIn { Configuration = CreateConfiguration() }, true);
        return player;
    }

    private static void AddDefinitions(Player player)
    {
        var items = player.GameContext.Configuration.Items;
        foreach (var number in new[] { Pinocchio, PinocchioEvolved, DefenseMuun, Tibetton, Converter, WizardryStone, AttackMuun, RepairMuun, PlayTimeMuun, HeroMuun })
        {
            items.Add(new ItemDefinition { Group = Group, Number = number, Width = 1, Height = 1, Durability = number is Converter or WizardryStone ? (byte)1 : (byte)255 });
        }

        var target = new BasicModel.ItemOptionDefinition { Name = "Muun Evolution Target", MaximumOptionsPerItem = 1 };
        target.PossibleOptions.Add(new BasicModel.IncreasableItemOption { OptionType = Season21ItemOptionTypes.MuunEvolutionTarget });
        var stone = new BasicModel.ItemDefinition { Group = Group, Number = EvolutionStone, Width = 1, Height = 1, Durability = 1 };
        stone.PossibleItemOptions.Add(target);
        items.Add(stone);
        items.Add(new ItemDefinition { Group = ItemConstants.JewelOfLife.Group, Number = ItemConstants.JewelOfLife.Number!.Value, Width = 1, Height = 1, Durability = 1 });
    }

    private static Item CreateItem(Player player, short number, byte group = Group)
    {
        var definition = player.GameContext.Configuration.Items.Single(d => d.Group == group && d.Number == number);
        var item = player.PersistenceContext.CreateNew<Item>();
        item.Definition = definition;
        item.Durability = definition.Durability;
        return item;
    }

    private static async ValueTask<Item> AddToInventoryAsync(Player player, short number, byte group = Group)
    {
        var item = CreateItem(player, number, group);
        Assert.That(await player.Inventory!.AddItemAsync(item).ConfigureAwait(false), Is.True);
        return item;
    }

    private static async ValueTask<Item> AddToMuunInventoryAsync(Player player, short number, byte slot)
    {
        var item = await AddToInventoryAsync(player, number).ConfigureAwait(false);
        Assert.That(await MuunService.MoveAsync(player, MuunStorageKind.Inventory, item.ItemSlot, MuunStorageKind.MuunInventory, slot).ConfigureAwait(false), Is.EqualTo(MuunActionResult.Success));
        return item;
    }

    private static async ValueTask<Item> AddStoneAsync(Player player, byte slot, short target)
    {
        var stone = await AddToMuunInventoryAsync(player, EvolutionStone, slot).ConfigureAwait(false);
        Assert.That(MuunService.SetEvolutionTarget(stone, target, player.PersistenceContext), Is.True);
        return stone;
    }

    private static async ValueTask<Monster> CreateMonsterAsync(Player player)
    {
        var definition = new BasicModel.MonsterDefinition { Number = 999, Designation = "Muun Test", ObjectKind = NpcObjectKind.Monster };
        definition.Attributes.Add(new BasicModel.MonsterAttribute { AttributeDefinition = Stats.MaximumHealth, Value = 1_000_000 });
        definition.Attributes.Add(new BasicModel.MonsterAttribute { AttributeDefinition = Stats.Level, Value = 1 });
        var map = player.CurrentMap!;
        var area = new BasicModel.MonsterSpawnArea { MonsterDefinition = definition, GameMap = map.Definition, SpawnTrigger = SpawnTrigger.OnceAtEventStart, X1 = player.Position.X, Y1 = player.Position.Y, X2 = player.Position.X, Y2 = player.Position.Y, Quantity = 1 };
        var monster = new Monster(area, definition, map, NullDropGenerator.Instance, new BasicMonsterIntelligence(), player.GameContext.PlugInManager, player.GameContext.PathFinderPool);
        monster.Initialize();
        await map.AddAsync(monster).ConfigureAwait(false);
        return monster;
    }
}
