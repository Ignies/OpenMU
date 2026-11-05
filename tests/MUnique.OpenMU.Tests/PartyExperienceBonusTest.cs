// <copyright file="PartyExperienceBonusTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using MUnique.OpenMU.GameLogic.PlugIns;

/// <summary>
/// Tests the <see cref="PartyExperienceBonusConfiguration"/>.
/// </summary>
[TestFixture]
public class PartyExperienceBonusTest
{
    /// <summary>
    /// The default keeps the bonus of OpenMU, 1.05 per additional member, whatever the classes are.
    /// </summary>
    /// <param name="members">The number of members.</param>
    /// <param name="classes">The number of different classes.</param>
    [TestCase(1, 1)]
    [TestCase(3, 3)]
    [TestCase(5, 1)]
    [TestCase(5, 5)]
    public void DefaultIsOnePointZeroFivePerMember(int members, int classes)
    {
        Assert.That(new PartyExperienceBonusConfiguration().GetBonus(members, classes), Is.EqualTo(Math.Pow(1.05, members - 1)).Within(1e-9));
    }

    /// <summary>
    /// The Season 21 renewal bonus: 1.0 to 3.2 by party size, 1.5 to 6.4 for a party of at least three classes.
    /// </summary>
    /// <param name="members">The number of members.</param>
    /// <param name="classes">The number of different classes.</param>
    /// <param name="expected">The expected bonus.</param>
    [TestCase(1, 1, 1.0)]
    [TestCase(2, 2, 1.3)]
    [TestCase(5, 2, 3.2)]
    [TestCase(3, 3, 4.7)]
    [TestCase(5, 3, 6.4)]
    [TestCase(7, 1, 3.2)]
    public void Season21RenewalBonus(int members, int classes, double expected)
    {
        Assert.That(PartyExperienceBonusConfiguration.CreateSeason21().GetBonus(members, classes), Is.EqualTo(expected).Within(1e-9));
    }
}
