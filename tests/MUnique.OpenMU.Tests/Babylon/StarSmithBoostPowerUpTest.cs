// <copyright file="StarSmithBoostPowerUpTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using Moq;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// Smithed boosts in play (Readme.md "PR 6 contract", "Boosts in play"): the power-up factory decorator adds a worn
/// item's boosts, capped per item and in total, PvE and PvP apart, never past the code ceiling.
/// </summary>
[TestFixture]
public class StarSmithBoostPowerUpTest
{
    private readonly List<Guid> _booked = [];

    private AttributeSystem _attributes = null!;

    private StarSmithBalance? _balance;

    /// <summary>
    /// Sets up an empty attribute system with a base defense and the default balance.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this._attributes = new AttributeSystem([], [], []);
        this._attributes.AddElement(new ConstantElement(100), Stats.DefensePvm);
        this._attributes.AddElement(new ConstantElement(100), Stats.DefensePvp);
        this._balance = new StarSmithBalance();
    }

    /// <summary>
    /// Forgets the boosts the test booked.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        this._booked.ForEach(StarSmithBoostBook.Forget);
        this._booked.Clear();
    }

    /// <summary>
    /// A worn item's boosts reach the attributes: PvE and PvP damage apart, defense multiplied, a flat stat added.
    /// </summary>
    [Test]
    public void AWornItemsBoostsApply()
    {
        this.Wear(new StarSmithBoost(StarSmithBoostStat.Damage, 500), new StarSmithBoost(StarSmithBoostStat.MaxHealth, 20_000), new StarSmithBoost(StarSmithBoostStat.Defense, 500));

        Assert.That(this.Value(Stats.FinalDamageIncreasePvm), Is.EqualTo(0.05f).Within(0.0001f));
        Assert.That(this.Value(Stats.StarSmithDamageIncreasePvp), Is.EqualTo(0.03f).Within(0.0001f), "the PvP table: 3% per item");
        Assert.That(this.Value(Stats.MaximumHealth), Is.EqualTo(200f).Within(0.01f));
        Assert.That(this.Value(Stats.DefensePvm), Is.EqualTo(105f).Within(0.01f));
        Assert.That(this.Value(Stats.DefensePvp), Is.EqualTo(103f).Within(0.01f));
    }

    /// <summary>
    /// A stored value above the per-item cap counts only up to it.
    /// </summary>
    [Test]
    public void CapsPerItem()
    {
        this.Wear(new StarSmithBoost(StarSmithBoostStat.Damage, 1_000), new StarSmithBoost(StarSmithBoostStat.CriticalRate, 1_000));
        Assert.That(this.Value(Stats.FinalDamageIncreasePvm), Is.EqualTo(0.06f).Within(0.0001f));
        Assert.That(this.Value(Stats.CriticalDamageChance), Is.EqualTo(0.02f).Within(0.0001f), "a stat without a PvP value takes the lower cap");
    }

    /// <summary>
    /// The total across worn items stops at the total cap, whichever item came first, and is recounted when one is
    /// taken off.
    /// </summary>
    [Test]
    public void CapsInTotal()
    {
        var first = this.Wear(new StarSmithBoost(StarSmithBoostStat.Damage, 600));
        this.Wear(new StarSmithBoost(StarSmithBoostStat.Damage, 600));
        this.Wear(new StarSmithBoost(StarSmithBoostStat.Damage, 600));
        Assert.That(this.Value(Stats.FinalDamageIncreasePvm), Is.EqualTo(0.15f).Within(0.0001f));
        Assert.That(this.Value(Stats.StarSmithDamageIncreasePvp), Is.EqualTo(0.08f).Within(0.0001f));

        first.ForEach(p => p.Dispose());
        Assert.That(this.Value(Stats.FinalDamageIncreasePvm), Is.EqualTo(0.12f).Within(0.0001f));
        Assert.That(this.Value(Stats.StarSmithDamageIncreasePvp), Is.EqualTo(0.06f).Within(0.0001f));
    }

    /// <summary>
    /// A cap raised past the code ceiling still stops there.
    /// </summary>
    [Test]
    public void TheCeilingHolds()
    {
        this._balance!.Pve[StarSmithBoostStat.Damage] = new StarSmithCap(500, 500);
        this.Wear(new StarSmithBoost(StarSmithBoostStat.Damage, 9_000));
        Assert.That(this.Value(Stats.FinalDamageIncreasePvm), Is.EqualTo(0.30f).Within(0.0001f));
    }

    /// <summary>
    /// Nothing is added while Star Smithing is off, nor for an item that is not worn.
    /// </summary>
    [Test]
    public void OffOrNotWornAddsNothing()
    {
        var item = Item(20);
        this.Book(item, new StarSmithBoost(StarSmithBoostStat.MaxMana, 10_000));
        Assert.That(this.Factory().GetPowerUps(item, this._attributes), Is.Empty, "in the bag");

        this._balance = null;
        Assert.That(this.Wear(new StarSmithBoost(StarSmithBoostStat.MaxMana, 10_000)), Is.Empty, "off");
        Assert.That(this.Value(Stats.MaximumMana), Is.EqualTo(0));
    }

    private static Item Item(byte slot) => new MUnique.OpenMU.Persistence.BasicModel.Item
    {
        Id = Guid.NewGuid(),
        ItemSlot = slot,
        Durability = 10,
        Definition = new MUnique.OpenMU.Persistence.BasicModel.ItemDefinition { Group = 8, Number = 1, Durability = 10 },
    };

    private float Value(AttributeDefinition attribute) => this._attributes.GetValueOfAttribute(attribute);

    private IItemPowerUpFactory Factory()
    {
        var inner = new Mock<IItemPowerUpFactory>();
        inner.Setup(f => f.GetPowerUps(It.IsAny<Item>(), It.IsAny<AttributeSystem>())).Returns([]);
        return new StarSmithItemPowerUpFactory(inner.Object, () => this._balance);
    }

    private void Book(Item item, params StarSmithBoost[] boosts)
    {
        var id = ((MUnique.OpenMU.Persistence.IIdentifiable)item).Id;
        this._booked.Add(id);
        StarSmithBoostBook.Set(id, boosts);
    }

    private List<PowerUpWrapper> Wear(params StarSmithBoost[] boosts)
    {
        var item = Item(InventoryConstants.ArmorSlot);
        this.Book(item, boosts);
        return this.Factory().GetPowerUps(item, this._attributes).ToList();
    }
}
