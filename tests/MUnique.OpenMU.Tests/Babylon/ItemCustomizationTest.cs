// <copyright file="ItemCustomizationTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using System.Text;
using MUnique.OpenMU.GameLogic.ItemCustomization;

/// <summary>
/// The item customisation channel: the stored look as the 31 wire bytes the Babylon client decodes (the body of the
/// Hidden plugin's E7 09 tint entry, byte for byte), and names and lore fitted to their fixed fields.
/// </summary>
[TestFixture]
public class ItemCustomizationTest
{
    /// <summary>
    /// A tone look with a glow and no weapon effect: the colours, the strength, the default tuning.
    /// </summary>
    [Test]
    public void ToneLookWithGlow()
    {
        var bytes = new byte[ItemLookBytes.Length];
        Assert.That(ItemLookBytes.TryWrite("""{"mode":"tone","color":"#1e90ff","strength":90,"hue":0,"glow":"#3366ff"}""", bytes), Is.True);
        Assert.That(bytes, Is.EqualTo(new byte[]
        {
            1, 2, 0x1E, 0x90, 0xFF, 90, 0, 0, 0, 0, 0, 0x33, 0x66, 0xFF, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0,
            100, 100, 100, 100, 100, 100,
        }));
    }

    /// <summary>
    /// A hue look keeping a hue wedge, with a fire effect in two colours and its own tuning.
    /// </summary>
    [Test]
    public void HueLookWithKeepAndFire()
    {
        var bytes = new byte[ItemLookBytes.Length];
        var json = """{"mode":"hue","color":"#3a7bff","strength":70,"hue":300,"keep_from":120,"keep_range":40,"fx":"fire","fx_colors":["#ff5a1a","#ffd000"],"fx_tune":{"size":110,"amount":90,"glow":100,"speed":250,"reach":0,"surface":100}}""";
        Assert.That(ItemLookBytes.TryWrite(json, bytes), Is.True);
        Assert.That(bytes, Is.EqualTo(new byte[]
        {
            2, 1, 0x3A, 0x7B, 0xFF, 70, 0x01, 0x2C, 0, 120, 40, 0, 0, 0, 2, 2,
            0xFF, 0x5A, 0x1A, 0xFF, 0xD0, 0x00, 0, 0, 0,
            110, 90, 100, 250, 0, 100,
        }));
    }

    /// <summary>
    /// No look, an empty one or broken JSON leave the bytes zero and say there's no look.
    /// </summary>
    /// <param name="json">The stored look.</param>
    [TestCase(null)]
    [TestCase("")]
    [TestCase("{not json")]
    [TestCase("""{"mode":"none"}""")]
    public void NoLook(string? json)
    {
        var bytes = new byte[ItemLookBytes.Length];
        Assert.That(ItemLookBytes.TryWrite(json, bytes), Is.False);
        Assert.That(bytes, Is.All.EqualTo(0));
    }

    /// <summary>
    /// A name is UTF-8, cut on a character, never in the middle of one.
    /// </summary>
    [Test]
    public void NameIsCutOnACharacter()
    {
        var field = new byte[ItemCustomizationText.NameBytes];
        ItemCustomizationText.WriteName("Ñandú's Blade", field);
        Assert.That(Encoding.UTF8.GetString(field).TrimEnd('\0'), Is.EqualTo("Ñandú's Blade"));

        var long3 = new string('語', 30);
        ItemCustomizationText.WriteName(long3, field);
        Assert.That(Encoding.UTF8.GetString(field).TrimEnd('\0'), Is.EqualTo(new string('語', 24)));
    }

    /// <summary>
    /// A lore keeps at most three lines and 140 characters.
    /// </summary>
    [Test]
    public void LoreKeepsThreeLines()
    {
        var field = new byte[ItemCustomizationText.LoreBytes];
        ItemCustomizationText.WriteLore("Forged in fire.\nCooled in snow.\nNamed by none.\nForgotten.", field);
        Assert.That(Encoding.UTF8.GetString(field).TrimEnd('\0'), Is.EqualTo("Forged in fire.\nCooled in snow.\nNamed by none."));

        ItemCustomizationText.WriteLore(new string('a', 200), field);
        Assert.That(Encoding.UTF8.GetString(field).TrimEnd('\0'), Has.Length.EqualTo(140));
    }

    /// <summary>
    /// The Gremory case storage names its case by the object id, as the client asks: 0 the character's, 1 the account's.
    /// </summary>
    [Test]
    public void GremoryCaseByObjectId()
    {
        Assert.That(ItemCustomizationCollector.GremoryCaseOf(0), Is.EqualTo(GameLogic.GremoryCase.GremoryCaseKind.Character));
        Assert.That(ItemCustomizationCollector.GremoryCaseOf(1), Is.EqualTo(GameLogic.GremoryCase.GremoryCaseKind.Account));
        Assert.That(ItemCustomizationCollector.GremoryCaseOf(7), Is.Null);
    }

    /// <summary>
    /// The flags say what an entry carries.
    /// </summary>
    [Test]
    public void FlagsSayWhatAnEntryCarries()
    {
        Assert.That(ItemCustomizationText.Flags(hasLook: true, name: "A", lore: null), Is.EqualTo(1 | 2));
        Assert.That(ItemCustomizationText.Flags(hasLook: false, name: null, lore: "Old"), Is.EqualTo(4));
        Assert.That(ItemCustomizationText.Flags(hasLook: false, name: " ", lore: " "), Is.EqualTo(0));
    }
}
