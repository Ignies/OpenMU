// <copyright file="MiniGameLevelBracketTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.PlayerActions.MiniGames;

/// <summary>
/// Tests the level which a mini game bracket checks, <see cref="MiniGameDefinitionExtensions.GetBracketLevel"/>.
/// </summary>
[TestFixture]
public class MiniGameLevelBracketTest
{
    private const short MaximumLevel = 400;

    /// <summary>
    /// A bracket within the normal level cap (Season 6) checks the normal level, so a master keeps the top grade.
    /// </summary>
    [Test]
    public void BracketWithinTheCapChecksTheNormalLevel()
    {
        var bloodCastle8 = new MiniGameDefinition { MinimumCharacterLevel = 331, MaximumCharacterLevel = 400 };

        Assert.That(bloodCastle8.GetBracketLevel(400, 500, MaximumLevel), Is.EqualTo(400));
    }

    /// <summary>
    /// A bracket above the normal level cap (Season 21 grades) checks the total level, so characters of different
    /// master levels get different grades.
    /// </summary>
    /// <param name="masterLevel">The master level of a level 400 character.</param>
    /// <param name="minimum">The minimum total level of the grade.</param>
    /// <param name="maximum">The maximum total level of the grade.</param>
    /// <param name="expected">Whether the character is in the grade.</param>
    [TestCase(399, 400, 799, true)]
    [TestCase(400, 400, 799, false)]
    [TestCase(400, 800, 999, true)]
    [TestCase(1350, 1600, int.MaxValue, true)]
    public void BracketAboveTheCapChecksTheTotalLevel(int masterLevel, int minimum, int maximum, bool expected)
    {
        var grade = new MiniGameDefinition { MinimumCharacterLevel = minimum, MaximumCharacterLevel = maximum };

        var level = grade.GetBracketLevel(400, masterLevel, MaximumLevel);

        Assert.That(level >= minimum && level <= maximum, Is.EqualTo(expected));
    }
}
