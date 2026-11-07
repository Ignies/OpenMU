// <copyright file="AbilityCoreRulesTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.AbilityCore;

using MUnique.OpenMU.GameLogic.AbilityCore;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// Tests the <see cref="AbilityCoreRules"/>: the slots of the core open with the fifth evolution and the Season 21 total
/// levels of the configuration, read through the progression mode.
/// </summary>
[TestFixture]
public class AbilityCoreRulesTest
{
    private static readonly byte SecondSkillSlot = AbilityCoreSlots.FirstSkill + 1;

    private static readonly byte SecondRowSlot = AbilityCoreSlots.FirstOption + AbilityCoreSlots.OptionsPerRow;

    private readonly AbilityCoreConfiguration _configuration = new();

    /// <summary>
    /// Without the fifth evolution no slot is open.
    /// </summary>
    [Test]
    public void FourthEvolutionHasNoSlot()
    {
        Assert.That(AbilityCoreRules.GetOpenSlots(ClassEvolution.Fourth, 400, 1000, 0, null, this._configuration), Is.Zero);
    }

    /// <summary>
    /// In the original mode the total level opens the slots: at 1200 the extra slot, a skill slot and the first row.
    /// </summary>
    [Test]
    public void OriginalOpensBySeason21Total()
    {
        Assert.That(this.IsOpen(AbilityCoreSlots.Extra, 400, 800, 0, null), Is.True);
        Assert.That(this.IsOpen(AbilityCoreSlots.FirstSkill, 400, 800, 0, null), Is.True);
        Assert.That(this.IsOpen(AbilityCoreSlots.FirstOption, 400, 800, 0, null), Is.True);
        Assert.That(this.IsOpen(AbilityCoreSlots.FirstElite, 400, 800, 0, null), Is.True);
        Assert.That(this.IsOpen(SecondSkillSlot, 400, 899, 0, null), Is.False);
        Assert.That(this.IsOpen(SecondSkillSlot, 400, 900, 0, null), Is.True);
        Assert.That(this.IsOpen(SecondRowSlot, 400, 849, 0, null), Is.False);
        Assert.That(this.IsOpen(SecondRowSlot, 400, 850, 0, null), Is.True);
        Assert.That(this.IsOpen(AbilityCoreSlots.Count, 400, 2000, 0, null), Is.False);
    }

    /// <summary>
    /// The normalized mode asks for the mapped level plus master levels, never the raw total.
    /// </summary>
    [Test]
    public void NormalizedOpensByTheMappedLevel()
    {
        var progression = new ProgressionConfiguration { Mode = ProgressionMode.Normalized };
        var second = EvolutionRules.GetRequiredLevel(1300, progression);

        Assert.That(second, Is.LessThan(600));
        Assert.That(this.IsOpen(SecondSkillSlot, 400, second - 401, 0, progression), Is.False);
        Assert.That(this.IsOpen(SecondSkillSlot, 400, second - 400, 0, progression), Is.True);
    }

    /// <summary>
    /// The stars mode asks for the power level: stars open the slots.
    /// </summary>
    [Test]
    public void StarsOpenByThePowerLevel()
    {
        var progression = new ProgressionConfiguration { Mode = ProgressionMode.Stars };

        Assert.That(this.IsOpen(AbilityCoreSlots.FirstSkill, 400, 200, 0, progression), Is.False);
        Assert.That(this.IsOpen(AbilityCoreSlots.FirstSkill, 265, 0, 2, progression), Is.True);
        Assert.That(this.IsOpen(SecondSkillSlot, 265, 0, 2, progression), Is.False);
        Assert.That(this.IsOpen(SecondSkillSlot, 400, 0, 10, progression), Is.True);
    }

    private bool IsOpen(byte slot, int level, int masterLevel, int stars, ProgressionConfiguration? progression)
        => AbilityCoreRules.IsOpen(slot, ClassEvolution.Fifth, level, masterLevel, stars, progression, this._configuration);
}
