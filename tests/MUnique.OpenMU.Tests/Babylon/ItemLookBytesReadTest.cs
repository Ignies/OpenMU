// <copyright file="ItemLookBytesReadTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.GameLogic.ItemCustomization;

/// <summary>
/// A smith's recolour arrives as the 31 look bytes (EE 93) and is stored as the look JSON: read back, it writes the
/// same bytes again.
/// </summary>
[TestFixture]
public class ItemLookBytesReadTest
{
    /// <summary>
    /// Every look the site can make reads back to the same bytes.
    /// </summary>
    [TestCase("""{"mode":"tone","color":"#1e90ff","strength":90,"glow":"#3366ff"}""")]
    [TestCase("""{"mode":"hue","color":"#3a7bff","strength":70,"hue":300,"keep_from":120,"keep_range":40,"fx":"fire","fx_colors":["#ff5a1a","#ffd000"],"fx_tune":{"size":110,"amount":90,"glow":100,"speed":250,"reach":0,"surface":100}}""")]
    [TestCase("""{"mode":"none","glow":"#ff0000"}""")]
    public void ALookReadsBackToTheSameBytes(string json)
    {
        var bytes = new byte[ItemLookBytes.Length];
        Assert.That(ItemLookBytes.TryWrite(json, bytes), Is.True);
        var read = ItemLookBytes.TryRead(bytes);
        Assert.That(read, Is.Not.Null);
        var again = new byte[ItemLookBytes.Length];
        Assert.That(ItemLookBytes.TryWrite(read, again), Is.True);
        Assert.That(again, Is.EqualTo(bytes));
    }

    /// <summary>
    /// All zero is the item's own colours (no look); a mode or effect past the known ones is no look either.
    /// </summary>
    [Test]
    public void NoLook()
    {
        Assert.That(ItemLookBytes.TryRead(new byte[ItemLookBytes.Length]), Is.Null);
        var badMode = new byte[ItemLookBytes.Length];
        badMode[0] = 7;
        badMode[5] = 50;
        Assert.That(ItemLookBytes.TryRead(badMode), Is.Null);
        var badFx = new byte[ItemLookBytes.Length];
        badFx[14] = 99;
        Assert.That(ItemLookBytes.TryRead(badFx), Is.Null);
    }
}
