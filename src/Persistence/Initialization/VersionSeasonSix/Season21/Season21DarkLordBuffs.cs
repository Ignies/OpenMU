// <copyright file="Season21DarkLordBuffs.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// The Dark Lord's Season 21 buffs, which the skills update creates without a duration: Crown Force (2097; 912), Divine
/// Force (2098; 913), Divine Aura (2099; 914) and Battle Glory (2100; 915).
/// </summary>
/// <remarks>
/// S21Emu <c>Data/Scripts/LuaCalc/Skill/SkillDamageCalc.lua</c>:
/// <list type="bullet">
/// <item>Crown Force (<c>SkillCrowForces</c>): the party within Radio 10 (the caster alone without one), 60 + energy / 30
/// seconds; its value comes from the tier only.</item>
/// <item>Battle Glory (<c>SkillBattleOfGlory</c>): the caster, 60 + energy / 30 seconds; its value comes from the tier only.</item>
/// <item>Divine Force (<c>SkillDivineForce</c>, value 100) and Divine Aura (<c>SkillDivineAura</c>, value 500): the caster,
/// 3 seconds.</item>
/// </list>
/// No source names the stat of these values, so the effects get their duration only (buff_rows.md, open items). All is
/// data an operator can change.
/// </remarks>
internal static class Season21DarkLordBuffs
{
    private const float ByEnergySeconds = 60;
    private const float SecondsPerEnergy = 1f / 30;
    private const float DivineSeconds = 3;

    /// <summary>The radius of Crown Force's party (S21Emu Skill.xml Radio 10).</summary>
    private const short CrownForceRadius = 10;

    private static readonly short[] ByEnergyEffects = [427, 436, 437];
    private static readonly short[] DivineEffects = [428, 429];
    private static readonly short[] PartySkills = [2097, 912];
    private static readonly short[] CasterSkills = [2098, 913, 2099, 914, 2100, 915];

    /// <summary>
    /// Gives the effects which have no duration theirs, and makes the skills which are no buff yet buffs: Crown Force on the
    /// party within its radius, the others on the caster. An operator's own duration or skill type stays.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public static void Apply(IContext context, GameConfiguration gameConfiguration)
    {
        foreach (var effect in gameConfiguration.MagicEffects.Where(e => e.Duration is null))
        {
            if (ByEnergyEffects.Contains(effect.Number))
            {
                effect.Duration = context.CreateNew<PowerUpDefinitionValue>();
                effect.Duration.ConstantValue.Value = ByEnergySeconds;
                var perEnergy = context.CreateNew<AttributeRelationship>();
                perEnergy.InputAttribute = Stats.TotalEnergy.GetPersistent(gameConfiguration);
                perEnergy.InputOperator = InputOperator.Multiply;
                perEnergy.InputOperand = SecondsPerEnergy;
                effect.Duration.RelatedValues.Add(perEnergy);
            }
            else if (DivineEffects.Contains(effect.Number))
            {
                effect.Duration = context.CreateNew<PowerUpDefinitionValue>();
                effect.Duration.ConstantValue.Value = DivineSeconds;
            }
        }

        foreach (var skill in gameConfiguration.Skills.Where(s => s.SkillType == SkillType.Other))
        {
            if (PartySkills.Contains(skill.Number))
            {
                skill.SkillType = SkillType.Buff;
                skill.Target = SkillTarget.ImplicitParty;
                skill.TargetRestriction = SkillTargetRestriction.Party;
                skill.ImplicitTargetRange = CrownForceRadius;
            }
            else if (CasterSkills.Contains(skill.Number))
            {
                skill.SkillType = SkillType.Buff;
                skill.Target = SkillTarget.ImplicitPlayer;
                skill.TargetRestriction = SkillTargetRestriction.Self;
            }
        }
    }
}
