// <copyright file="StarSmithDyesTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.GameLogic.ItemCustomization;
using MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The dye rules of a recolour (Readme.md "Dyes"): a colour dye for any new look, one of its swatches, at most its
/// strength, the hue shift only with a Prismatic Dye, the keep mask only with a rare dye, and a glow only with a Radiant
/// Dye and in a swatch of the colour dye. The original colours spend no dye.
/// </summary>
[TestFixture]
public class StarSmithDyesTest
{
    private const int Azure = 0x1e90ff;

    private static readonly StarSmithDye AzureDye = new() { Number = 1004, Swatches = [0x7fc4ff, Azure, 0x1060c0, 0x0a3470], MaximumStrength = 85 };
    private static readonly StarSmithDye Frostfire = new() { Number = 1009, Swatches = [0x9ff6ff, 0xff7a2a, 0x3a1cff], MaximumStrength = 100, Keep = true };
    private static readonly StarSmithDye Prismatic = new() { Number = 1013, Swatches = [0xff0000, 0x00ff00, 0x0000ff], MaximumStrength = 100, Hue = true };
    private static readonly StarSmithDye RadiantDye = new() { Number = 1014, Radiant = true };

    /// <summary>
    /// A tone in one of the dye's swatches, within its strength: done; no dye, or a colour or strength it does not hold: refused.
    /// </summary>
    [Test]
    public void ToneNeedsTheDyesSwatchAndStrength()
    {
        Assert.That(StarSmithDyes.Check(Look(mode: 1, colour: Azure, strength: 85), AzureDye, null), Is.EqualTo(StarSmithResultCode.Done));
        Assert.That(StarSmithDyes.Check(Look(mode: 1, colour: Azure, strength: 85), null, null), Is.EqualTo(StarSmithResultCode.NoDye));
        Assert.That(StarSmithDyes.Check(Look(mode: 1, colour: 0xff0000, strength: 85), AzureDye, null), Is.EqualTo(StarSmithResultCode.WrongDye));
        Assert.That(StarSmithDyes.Check(Look(mode: 1, colour: Azure, strength: 90), AzureDye, null), Is.EqualTo(StarSmithResultCode.WrongDye));
        Assert.That(StarSmithDyes.Check(Look(mode: 1, colour: Azure, strength: 85), RadiantDye, null), Is.EqualTo(StarSmithResultCode.NoDye));
    }

    /// <summary>
    /// The hue shift needs a Prismatic Dye; the keep mask needs a rare dye.
    /// </summary>
    [Test]
    public void HueAndKeepNeedTheirDyes()
    {
        Assert.That(StarSmithDyes.Check(Look(mode: 2, strength: 70, hue: 200), Prismatic, null), Is.EqualTo(StarSmithResultCode.Done));
        Assert.That(StarSmithDyes.Check(Look(mode: 2, strength: 70, hue: 200), AzureDye, null), Is.EqualTo(StarSmithResultCode.WrongDye));
        Assert.That(StarSmithDyes.Check(Look(mode: 1, colour: 0xff7a2a, strength: 100, keep: true), Frostfire, null), Is.EqualTo(StarSmithResultCode.Done));
        Assert.That(StarSmithDyes.Check(Look(mode: 1, colour: Azure, strength: 80, keep: true), AzureDye, null), Is.EqualTo(StarSmithResultCode.WrongDye));
    }

    /// <summary>
    /// A glow needs a Radiant Dye beside the colour dye, in one of the colour dye's swatches.
    /// </summary>
    [Test]
    public void GlowNeedsARadiantDye()
    {
        Assert.That(StarSmithDyes.Check(Look(mode: 1, colour: Azure, strength: 80, glow: 0x7fc4ff), AzureDye, RadiantDye), Is.EqualTo(StarSmithResultCode.Done));
        Assert.That(StarSmithDyes.Check(Look(mode: 1, colour: Azure, strength: 80, glow: 0x7fc4ff), AzureDye, null), Is.EqualTo(StarSmithResultCode.NoDye));
        Assert.That(StarSmithDyes.Check(Look(mode: 1, colour: Azure, strength: 80, glow: 0xffd700), AzureDye, RadiantDye), Is.EqualTo(StarSmithResultCode.WrongDye));
        Assert.That(StarSmithDyes.Check(Look(mode: 1, colour: Azure, strength: 80, glow: 0x7fc4ff), AzureDye, AzureDye), Is.EqualTo(StarSmithResultCode.NoDye));
    }

    /// <summary>
    /// The original colours (all zero) spend no dye.
    /// </summary>
    [Test]
    public void OriginalColoursNeedNoDye()
    {
        Assert.That(StarSmithDyes.Check(new byte[ItemLookBytes.Length], null, null), Is.EqualTo(StarSmithResultCode.Done));
        Assert.That(StarSmithDyes.NeedsDye(new byte[ItemLookBytes.Length]), Is.False);
        Assert.That(StarSmithDyes.NeedsDye(Look(mode: 1, colour: Azure, strength: 85)), Is.True);
    }

    /// <summary>
    /// The dye a bag item is, by its group and number.
    /// </summary>
    [Test]
    public void DyeOfAnItem()
    {
        var dyes = new[] { AzureDye, RadiantDye };
        Assert.That(StarSmithDyes.Find(dyes, 14, 1004), Is.SameAs(AzureDye));
        Assert.That(StarSmithDyes.Find(dyes, 14, 1005), Is.Null);
        Assert.That(StarSmithDyes.Find(dyes, 13, 1004), Is.Null);
    }

    /// <summary>
    /// The default dyes: eight basic, five rare, the Prismatic and the Radiant Dye, numbers 1000 to 1014 of group 14, each
    /// colour dye with swatches.
    /// </summary>
    [Test]
    public void DefaultDyes()
    {
        var dyes = new StarSmithingConfiguration().Dyes;
        Assert.That(dyes.Select(d => (d.Group, d.Number)), Is.EqualTo(Enumerable.Range(1000, 15).Select(n => ((byte)14, (short)n))));
        Assert.That(dyes.Count(d => d.Keep && !d.Hue), Is.EqualTo(5));
        Assert.That(dyes.Single(d => d.Hue).Number, Is.EqualTo(1013));
        Assert.That(dyes.Single(d => d.Radiant).Number, Is.EqualTo(1014));
        Assert.That(dyes.Where(d => !d.Radiant).All(d => d.Swatches.Count is > 0 and <= StarSmithDye.MaximumSwatches), Is.True);
    }

    private static byte[] Look(byte mode, int colour = 0, byte strength = 85, ushort hue = 0, bool keep = false, int glow = -1)
    {
        var look = new byte[ItemLookBytes.Length];
        look[0] = mode;
        look[1] = (byte)((keep ? 1 : 0) | (glow >= 0 ? 2 : 0));
        look[2] = (byte)(colour >> 16);
        look[3] = (byte)(colour >> 8);
        look[4] = (byte)colour;
        look[5] = strength;
        look[6] = (byte)(hue >> 8);
        look[7] = (byte)hue;
        if (keep)
        {
            look[8] = 0;
            look[9] = 20;
            look[10] = 30;
        }

        if (glow >= 0)
        {
            look[11] = (byte)(glow >> 16);
            look[12] = (byte)(glow >> 8);
            look[13] = (byte)glow;
        }

        return look;
    }
}
