// <copyright file="ClassCodesTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.GameServer.RemoteView.Babylon;
using MUnique.OpenMU.Network.Packets;

/// <summary>
/// Tests for the <see cref="ClassCodes"/>.
/// </summary>
[TestFixture]
public class ClassCodesTest
{
    /// <summary>
    /// Tests the class codes of the Season 6 classes: base &lt;&lt; 3 | evolution.
    /// </summary>
    /// <param name="number">The Season 6 class number.</param>
    /// <param name="expectedCode">The expected class code.</param>
    [TestCase(CharacterClassNumber.DarkWizard, 0)]
    [TestCase(CharacterClassNumber.SoulMaster, 1)]
    [TestCase(CharacterClassNumber.GrandMaster, 2)]
    [TestCase(CharacterClassNumber.DarkKnight, 8)]
    [TestCase(CharacterClassNumber.BladeMaster, 10)]
    [TestCase(CharacterClassNumber.HighElf, 18)]
    [TestCase(CharacterClassNumber.MagicGladiator, 24)]
    [TestCase(CharacterClassNumber.DuelMaster, 26)]
    [TestCase(CharacterClassNumber.LordEmperor, 34)]
    [TestCase(CharacterClassNumber.BloodySummoner, 41)]
    [TestCase(CharacterClassNumber.DimensionMaster, 42)]
    [TestCase(CharacterClassNumber.RageFighter, 48)]
    [TestCase(CharacterClassNumber.FistMaster, 50)]
    public void Season6Classes(CharacterClassNumber number, int expectedCode)
    {
        Assert.That(ClassCodes.ToCode((byte)number), Is.EqualTo(expectedCode));
        Assert.That(ClassCodes.FromCode((byte)expectedCode), Is.EqualTo((byte)number));
    }

    /// <summary>
    /// Tests that the classes and evolutions after Season 6 have the number 128 + their code.
    /// </summary>
    /// <param name="code">The class code.</param>
    [TestCase(3)] // Dark Wizard, 4th evolution
    [TestCase(4)] // Dark Wizard, 5th evolution
    [TestCase(56)] // Grow Lancer, 1st evolution
    [TestCase(122)] // Crusader, 3rd evolution
    public void NewClassesGetTheNumberOffset(int code)
    {
        Assert.That(ClassCodes.FromCode((byte)code), Is.EqualTo(128 + code));
        Assert.That(ClassCodes.ToCode((byte)(128 + code)), Is.EqualTo(code));
    }

    /// <summary>
    /// Tests that every Season 6 class number round trips.
    /// </summary>
    [Test]
    public void EverySeason6NumberRoundTrips()
    {
        foreach (var number in System.Enum.GetValues<CharacterClassNumber>())
        {
            Assert.That(ClassCodes.FromCode(ClassCodes.ToCode((byte)number)), Is.EqualTo((byte)number), number.ToString());
        }
    }

    /// <summary>
    /// Tests that numbers between the Season 6 ones and the offset, and codes with bit 7, are refused.
    /// </summary>
    [Test]
    public void RefusesNumbersAndCodesOutsideTheRanges()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ClassCodes.ToCode(28));
        Assert.Throws<ArgumentOutOfRangeException>(() => ClassCodes.ToCode(127));
        Assert.Throws<ArgumentOutOfRangeException>(() => ClassCodes.FromCode(128));
    }
}
