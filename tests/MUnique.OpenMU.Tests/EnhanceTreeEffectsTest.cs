// <copyright file="EnhanceTreeEffectsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.EnhanceTree;

/// <summary>
/// Tests the node effects of the skill enhancement tree (<see cref="EnhanceTreeEffects"/>).
/// </summary>
[TestFixture]
public class EnhanceTreeEffectsTest
{
    private const short MeteorStrike = 724;
    private const short MeteorStrikeExtended = 1075;

    /// <summary>
    /// "Add Ext Skill Damage" (1001) adds the value of its formula at its level to the skill which the extension node of its
    /// sheet improves (S21Emu EnhanceSkillTree.xml, DW sheet 0: formula 44 at level 20 = 600), once the extension is learned.
    /// </summary>
    [Test]
    public void ExtSkillDamageAddsToTheImprovedSkill()
    {
        var configuration = new EnhanceTreeConfiguration
        {
            Nodes =
            [
                new() { BaseClass = (byte)BaseClassCode.DarkWizard, Group = 0, Index = 0, Skill = MeteorStrikeExtended, RelatedSkill = MeteorStrike, FormulaId = 115 },
                new() { BaseClass = (byte)BaseClassCode.DarkWizard, Group = 0, Index = 5, Skill = EnhanceTreeEffects.ExtSkillDamage, FormulaId = 44 },
            ],
            Formulas = { [44] = "(((((20+level)^1.4)*2.402)/2.8)*4)", [115] = "((((((18+level)^2)/10)*5)/7)*4)" },
        };
        var levels = new Dictionary<short, int> { [MeteorStrikeExtended] = 1, [EnhanceTreeEffects.ExtSkillDamage] = 20 };
        int? LevelOf(short skill) => levels.TryGetValue(skill, out var level) ? level : null;

        Assert.That(EnhanceTreeEffects.GetExtraSkillDamage(configuration, BaseClassCode.DarkWizard, MeteorStrike, LevelOf), Is.EqualTo(600).Within(1));
        Assert.That(EnhanceTreeEffects.GetExtraSkillDamage(configuration, BaseClassCode.DarkWizard, 9, LevelOf), Is.Zero, "another skill");
        Assert.That(EnhanceTreeEffects.GetExtraSkillDamage(configuration, BaseClassCode.DarkKnight, MeteorStrike, LevelOf), Is.Zero, "another class");

        levels.Remove(MeteorStrikeExtended);
        Assert.That(EnhanceTreeEffects.GetExtraSkillDamage(configuration, BaseClassCode.DarkWizard, MeteorStrike, LevelOf), Is.Zero, "the extension isn't learned");
    }
}
