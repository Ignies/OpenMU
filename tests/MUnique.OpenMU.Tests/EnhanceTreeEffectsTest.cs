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

    /// <summary>
    /// The damage count nodes (1002-1004, formulas 45-47 = 15, 30, 45 / level, learned at 15) add 1, 2 or 3 hits to the
    /// skill of their sheet; a sheet holds one of them.
    /// </summary>
    [Test]
    public void DamageCountNodesAddHitsToTheImprovedSkill()
    {
        var configuration = CreateSheet(new EnhanceTreeNode { Index = 1, Skill = EnhanceTreeEffects.DamageCount2, FormulaId = 46 });
        configuration.Formulas[46] = "30/level";
        var levels = new Dictionary<short, int> { [MeteorStrikeExtended] = 1, [EnhanceTreeEffects.DamageCount2] = 15 };
        int? LevelOf(short skill) => levels.TryGetValue(skill, out var level) ? level : null;

        Assert.That(EnhanceTreeEffects.GetExtraHits(configuration, BaseClassCode.DarkWizard, MeteorStrike, LevelOf), Is.EqualTo(2));
        Assert.That(EnhanceTreeEffects.GetExtraHits(configuration, BaseClassCode.DarkWizard, 9, LevelOf), Is.Zero, "another skill");

        levels.Remove(MeteorStrikeExtended);
        Assert.That(EnhanceTreeEffects.GetExtraHits(configuration, BaseClassCode.DarkWizard, MeteorStrike, LevelOf), Is.Zero, "the extension isn't learned");
    }

    /// <summary>
    /// "Add Ext Skill Splash Damage" (1010, formula 61 = 10 / level, learned at 10) deals one hit to the monsters within one
    /// tile of the target; "Splash Count" (1154) adds a hit, "Splash Radio" (1153) a tile (muonlinefanz.com tree tool).
    /// </summary>
    [Test]
    public void SplashNodesGiveHitsAndRadius()
    {
        var configuration = CreateSheet(
            new EnhanceTreeNode { Index = 2, Skill = EnhanceTreeEffects.SplashDamage, FormulaId = 61 },
            new EnhanceTreeNode { Index = 3, Skill = EnhanceTreeEffects.SplashRadius, FormulaId = 168 },
            new EnhanceTreeNode { Index = 4, Skill = EnhanceTreeEffects.SplashCount, FormulaId = 169 });
        configuration.Formulas[61] = "10/level";
        configuration.Formulas[168] = "10/level";
        configuration.Formulas[169] = "10/level";
        var levels = new Dictionary<short, int> { [MeteorStrikeExtended] = 1, [EnhanceTreeEffects.SplashDamage] = 10 };
        int? LevelOf(short skill) => levels.TryGetValue(skill, out var level) ? level : null;

        Assert.That(EnhanceTreeEffects.GetSplash(configuration, BaseClassCode.DarkWizard, MeteorStrike, LevelOf), Is.EqualTo((1, 1)));

        levels[EnhanceTreeEffects.SplashRadius] = 10;
        levels[EnhanceTreeEffects.SplashCount] = 10;
        Assert.That(EnhanceTreeEffects.GetSplash(configuration, BaseClassCode.DarkWizard, MeteorStrike, LevelOf), Is.EqualTo((2, 2)));

        levels.Remove(EnhanceTreeEffects.SplashDamage);
        Assert.That(EnhanceTreeEffects.GetSplash(configuration, BaseClassCode.DarkWizard, MeteorStrike, LevelOf).Hits, Is.Zero, "no splash without its node");
    }

    private static EnhanceTreeConfiguration CreateSheet(params EnhanceTreeNode[] nodes)
    {
        var configuration = new EnhanceTreeConfiguration
        {
            Nodes = [new() { BaseClass = (byte)BaseClassCode.DarkWizard, Group = 0, Index = 0, Skill = MeteorStrikeExtended, RelatedSkill = MeteorStrike, FormulaId = 115 }],
            Formulas = { [115] = "((((((18+level)^2)/10)*5)/7)*4)" },
        };
        foreach (var node in nodes)
        {
            node.BaseClass = (byte)BaseClassCode.DarkWizard;
            configuration.Nodes.Add(node);
        }

        return configuration;
    }
}
