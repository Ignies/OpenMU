// <copyright file="MuunConditionTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Muuns;

using MUnique.OpenMU.GameLogic.Muuns;

/// <summary>
/// Tests the conditions of the Muun options as the Season 21 data writes them.
/// </summary>
[TestFixture]
public class MuunConditionTest
{
    private static readonly DateTime Wednesday = new(2026, 11, 4, 12, 30, 0, DateTimeKind.Utc);

    /// <summary>
    /// The day mask: 79 is Wednesday to Sunday, 56 Monday to Wednesday, 7 Thursday to Saturday, -1 every day.
    /// </summary>
    /// <param name="mask">The day mask.</param>
    /// <param name="day">The day of November 2026 (the 2nd is a Monday).</param>
    /// <param name="expected">Whether it is met.</param>
    [TestCase(79, 4, true)]
    [TestCase(79, 8, true)]
    [TestCase(79, 3, false)]
    [TestCase(56, 2, true)]
    [TestCase(56, 5, false)]
    [TestCase(7, 7, true)]
    [TestCase(-1, 3, true)]
    public void WeekdaysAreAMask(int mask, int day, bool expected)
    {
        var condition = new MuunCondition { Type = MuunConditionType.Weekdays, Value1 = mask };
        Assert.That(condition.IsMet(0, 1, new DateTime(2026, 11, day, 10, 0, 0, DateTimeKind.Utc)), Is.EqualTo(expected));
    }

    /// <summary>
    /// The hours count from 1 (0:00 to 1:00) to 24: "1 ~ 12PM" is 13 to 24.
    /// </summary>
    /// <param name="hour">The hour of the day.</param>
    /// <param name="expected">Whether it is met.</param>
    [TestCase(11, false)]
    [TestCase(12, true)]
    [TestCase(23, true)]
    public void HoursCountFromOne(int hour, bool expected)
    {
        var condition = new MuunCondition { Type = MuunConditionType.Hours, Value1 = 13, Value2 = 24 };
        Assert.That(condition.IsMet(0, 1, Wednesday.Date.AddHours(hour)), Is.EqualTo(expected));
    }

    /// <summary>
    /// The level range is the total level, both ends included; the map is a map number.
    /// </summary>
    [Test]
    public void LevelAndMapConditions()
    {
        var level = new MuunCondition { Type = MuunConditionType.CharacterLevel, Value1 = 1, Value2 = 200 };
        Assert.That((level.IsMet(0, 200, Wednesday), level.IsMet(0, 201, Wednesday)), Is.EqualTo((true, false)));

        var map = new MuunCondition { Type = MuunConditionType.Map, Value1 = 51 };
        Assert.That((map.IsMet(51, 1, Wednesday), map.IsMet(0, 1, Wednesday)), Is.EqualTo((true, false)));
    }
}
