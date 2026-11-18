// <copyright file="StarSmithRulesTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// Whether a smithing action (recolour, rename, lore, lock) may run (Readme.md, docs/star_smithing.md section 1): the
/// switch, the hammer in a hand and its durability, the place, the item, the lock, then the Zen of a recolour.
/// </summary>
[TestFixture]
public class StarSmithRulesTest
{
    private static readonly StarSmithCheck Ready = new(FeatureOn: true, HammerHeld: true, HammerDurability: 50, DurabilityCost: 5, PlaceAllowed: true, ItemFound: true, ItemLocked: false, Zen: 1_000_000, ZenCost: 500_000);

    /// <summary>
    /// Everything in place: done.
    /// </summary>
    [Test]
    public void Done()
    {
        Assert.That(StarSmithRules.Check(StarSmithAction.Recolor, Ready), Is.EqualTo(StarSmithResultCode.Done));
    }

    /// <summary>
    /// Each refusal, in the order they are checked.
    /// </summary>
    [Test]
    public void Refusals()
    {
        Assert.That(StarSmithRules.Check(StarSmithAction.Rename, Ready with { FeatureOn = false, HammerHeld = false }), Is.EqualTo(StarSmithResultCode.Off));
        Assert.That(StarSmithRules.Check(StarSmithAction.Rename, Ready with { HammerHeld = false, PlaceAllowed = false }), Is.EqualTo(StarSmithResultCode.NoHammer));
        Assert.That(StarSmithRules.Check(StarSmithAction.Rename, Ready with { HammerDurability = 4, ItemFound = false }), Is.EqualTo(StarSmithResultCode.HammerBroken));
        Assert.That(StarSmithRules.Check(StarSmithAction.Rename, Ready with { PlaceAllowed = false, ItemFound = false }), Is.EqualTo(StarSmithResultCode.NotHere));
        Assert.That(StarSmithRules.Check(StarSmithAction.Rename, Ready with { ItemFound = false, ItemLocked = true }), Is.EqualTo(StarSmithResultCode.NoItem));
        Assert.That(StarSmithRules.Check(StarSmithAction.Rename, Ready with { ItemLocked = true }), Is.EqualTo(StarSmithResultCode.Locked));
        Assert.That(StarSmithRules.Check(StarSmithAction.Recolor, Ready with { Zen = 499_999 }), Is.EqualTo(StarSmithResultCode.NotEnoughZen));
    }

    /// <summary>
    /// Only a recolour costs Zen; a lock does not wear the hammer.
    /// </summary>
    [Test]
    public void CostsByAction()
    {
        Assert.That(StarSmithRules.Check(StarSmithAction.Rename, Ready with { Zen = 0 }), Is.EqualTo(StarSmithResultCode.Done));
        Assert.That(StarSmithRules.Check(StarSmithAction.Lock, Ready with { HammerDurability = 0 }), Is.EqualTo(StarSmithResultCode.Done));
        Assert.That(StarSmithRules.WearsHammer(StarSmithAction.Lock), Is.False);
        Assert.That(StarSmithRules.WearsHammer(StarSmithAction.Lore), Is.True);
    }

    /// <summary>
    /// The codes on the wire (EE 97).
    /// </summary>
    [Test]
    public void WireCodes()
    {
        Assert.That(new[] { StarSmithAction.Recolor, StarSmithAction.Rename, StarSmithAction.Lore, StarSmithAction.Lock }.Select(a => (byte)a), Is.EqualTo(new byte[] { 1, 2, 3, 4 }));
        Assert.That(
            new[] { StarSmithResultCode.Done, StarSmithResultCode.NoHammer, StarSmithResultCode.HammerBroken, StarSmithResultCode.Locked, StarSmithResultCode.NotHere, StarSmithResultCode.NoItem, StarSmithResultCode.TextRefused, StarSmithResultCode.NotEnoughZen, StarSmithResultCode.Off }.Select(r => (byte)r),
            Is.EqualTo(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }));
    }
}
