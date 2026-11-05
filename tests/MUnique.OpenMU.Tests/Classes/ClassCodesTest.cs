// <copyright file="ClassCodesTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Classes;

using MUnique.OpenMU.GameLogic.Classes;

/// <summary>
/// Tests the <see cref="ClassCodes"/>, the class code of fork worlds: <c>base &lt;&lt; 3 | evolution</c>.
/// </summary>
[TestFixture]
public class ClassCodesTest
{
    /// <summary>
    /// The Season 6 classes keep their numbers, and each of them has its code.
    /// </summary>
    /// <param name="number">The Season 6 class number.</param>
    /// <param name="code">The class code.</param>
    [TestCase(0, 0)] // Dark Wizard
    [TestCase(2, 1)] // Soul Master
    [TestCase(3, 2)] // Grand Master
    [TestCase(4, 8)] // Dark Knight
    [TestCase(7, 10)] // Blade Master
    [TestCase(11, 18)] // High Elf
    [TestCase(12, 24)] // Magic Gladiator
    [TestCase(13, 26)] // Duel Master, no second evolution
    [TestCase(17, 34)] // Lord Emperor
    [TestCase(23, 42)] // Dimension Master
    [TestCase(25, 50)] // Fist Master
    public void Season6NumbersMapToTheirCodes(int number, int code)
    {
        Assert.That(ClassCodes.ToCode((byte)number), Is.EqualTo((byte)code));
        Assert.That(ClassCodes.FromCode((byte)code), Is.EqualTo((byte)number));
    }

    /// <summary>
    /// The fourth and fifth evolutions and the new classes are numbered 128 + code.
    /// </summary>
    /// <param name="code">The class code.</param>
    [TestCase(3)] // Soul Wizard
    [TestCase(4)] // Darkness Wizard
    [TestCase(51)] // Fist Blazer
    [TestCase(56)] // Grow Lancer
    [TestCase(122)] // Master Paladin
    [TestCase(124)] // Templar Commander
    public void ForkClassesAreNumberedFromTheirCode(int code)
    {
        Assert.That(ClassCodes.FromCode((byte)code), Is.EqualTo((byte)(128 + code)));
        Assert.That(ClassCodes.ToCode((byte)(128 + code)), Is.EqualTo((byte)code));
    }

    /// <summary>
    /// The base class and the evolution come from the code.
    /// </summary>
    [Test]
    public void BaseAndEvolutionComeFromTheCode()
    {
        Assert.That(ClassCodes.GetBaseClass(13), Is.EqualTo(BaseClassCode.MagicGladiator));
        Assert.That(ClassCodes.GetEvolution(13), Is.EqualTo(ClassEvolution.Third));
        Assert.That(ClassCodes.GetBaseClass(128 + 124), Is.EqualTo(BaseClassCode.Crusader));
        Assert.That(ClassCodes.GetEvolution(128 + 124), Is.EqualTo(ClassEvolution.Fifth));
        Assert.That(ClassCodes.GetEvolution(128 + 3), Is.EqualTo(ClassEvolution.Fourth));
    }

    /// <summary>
    /// A code without a class has no number: a second evolution of a class which skips it, a reserved evolution, bit 7.
    /// </summary>
    /// <param name="code">The class code.</param>
    [TestCase(25)] // Magic Gladiator, second evolution
    [TestCase(57)] // Grow Lancer, second evolution
    [TestCase(5)] // reserved evolution
    [TestCase(7)] // reserved evolution
    [TestCase(128)] // bit 7
    public void CodesWithoutClassHaveNoNumber(int code)
    {
        Assert.That(ClassCodes.FromCode((byte)code), Is.Null);
    }

    /// <summary>
    /// A Season 6 number which no class has has no code.
    /// </summary>
    /// <param name="number">The number.</param>
    [TestCase(1)]
    [TestCase(14)]
    [TestCase(26)]
    [TestCase(127)]
    [TestCase(128 + 25)]
    public void NumbersWithoutClassHaveNoCode(int number)
    {
        Assert.That(ClassCodes.ToCode((byte)number), Is.Null);
    }

    /// <summary>
    /// A Season 6 client sees the fourth and fifth evolutions of the Season 6 classes as their third one, and doesn't
    /// know the new classes at all.
    /// </summary>
    [Test]
    public void Season6ClientsSeeTheThirdEvolution()
    {
        Assert.That(ClassCodes.GetSeason6Number(3), Is.EqualTo((byte)3));
        Assert.That(ClassCodes.GetSeason6Number(128 + 3), Is.EqualTo((byte)3)); // Soul Wizard as Grand Master
        Assert.That(ClassCodes.GetSeason6Number(128 + 12), Is.EqualTo((byte)7)); // Ignition Knight as Blade Master
        Assert.That(ClassCodes.GetSeason6Number(128 + 28), Is.EqualTo((byte)13)); // Duple Knight as Duel Master
        Assert.That(ClassCodes.GetSeason6Number(128 + 51), Is.EqualTo((byte)25)); // Fist Blazer as Fist Master
        Assert.That(ClassCodes.GetSeason6Number(128 + 56), Is.Null); // Grow Lancer
    }

    /// <summary>
    /// Every code of the sixteen classes and five evolutions round-trips, and the numbers are unique.
    /// </summary>
    [Test]
    public void AllCodesRoundTrip()
    {
        var numbers = new List<byte>();
        for (var code = 0; code < 128; code++)
        {
            if (ClassCodes.FromCode((byte)code) is { } number)
            {
                Assert.That(ClassCodes.ToCode(number), Is.EqualTo((byte)code), $"code {code}");
                numbers.Add(number);
            }
        }

        Assert.That(numbers, Is.Unique);
        Assert.That(numbers, Has.Count.EqualTo((16 * 5) - 4));
    }
}
