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
        Assert.That(condition.IsMet(0, 1, new DateTime(2026, 11, day, 10, 0, 0, DateTimeKind.Utc), 0), Is.EqualTo(expected));
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
        Assert.That(condition.IsMet(0, 1, Wednesday.Date.AddHours(hour), 0), Is.EqualTo(expected));
    }

    /// <summary>
    /// The level range is the total level, both ends included; the map is a map number.
    /// </summary>
    [Test]
    public void LevelAndMapConditions()
    {
        var level = new MuunCondition { Type = MuunConditionType.CharacterLevel, Value1 = 1, Value2 = 200 };
        Assert.That((level.IsMet(0, 200, Wednesday, 0), level.IsMet(0, 201, Wednesday, 0)), Is.EqualTo((true, false)));

        var map = new MuunCondition { Type = MuunConditionType.Map, Value1 = 51 };
        Assert.That((map.IsMet(51, 1, Wednesday, 0), map.IsMet(0, 1, Wednesday, 0)), Is.EqualTo((true, false)));
    }

    /// <summary>
    /// The play time condition ("play the game for longer than 5 hours") is met from 300 minutes on.
    /// </summary>
    [Test]
    public void PlayTimeCountsTheHoursPlayed()
    {
        var condition = new MuunCondition { Type = MuunConditionType.PlayTime, Value1 = 5 };
        Assert.That((condition.IsMet(0, 1, Wednesday, 299), condition.IsMet(0, 1, Wednesday, 300)), Is.EqualTo((false, true)));
    }

    /// <summary>
    /// The additional option of kind 3 reduces the cooldown of the attack skill and leaves the option value alone; kind
    /// 1 multiplies it.
    /// </summary>
    [Test]
    public void AdditionalOptionKinds()
    {
        var cooldown = new MuunAddOptionDefinition { Kind = MuunAddOptionKind.SkillCooldownReduction, Values = [3, 3, 3, 3, 3, 3] };
        Assert.That((cooldown.Apply(4, 4, false), cooldown.GetCooldownReduction(4, false)), Is.EqualTo((4, 3)));
        var multiplier = new MuunAddOptionDefinition { Kind = MuunAddOptionKind.Multiplier, Values = [5, 5, 5, 5, 5, 5] };
        Assert.That((multiplier.Apply(4, 1, false), multiplier.GetCooldownReduction(1, false)), Is.EqualTo((20, 0)));
    }

    /// <summary>
    /// The defaults of the numbers which the sources give or which are designed.
    /// </summary>
    [Test]
    public void ConfigurationDefaults()
    {
        var configuration = new MuunConfiguration();
        Assert.That(configuration.HealthDecayInterval, Is.EqualTo(TimeSpan.FromMinutes(40)), "guardian mount: 1 every 40 minutes");
        Assert.That((configuration.HeroSlotLevel, configuration.RidingMovementSpeed, configuration.WizardryStoneEnergy), Is.EqualTo((1750, 15f, 120)));
    }
}
