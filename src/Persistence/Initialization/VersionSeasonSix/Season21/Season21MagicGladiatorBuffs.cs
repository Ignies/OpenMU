// <copyright file="Season21MagicGladiatorBuffs.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// The Magic Gladiator's Season 21 charges, which the skills update creates without a duration: Spiral Charge (2014; 865,
/// 866), Crusher Charge (2015; 867, 868) and Elemental Charge (2016; 869, 870).
/// </summary>
/// <remarks>
/// S21Emu <c>Data/Scripts/LuaCalc/Skill/SkillDamageCalc.lua</c> <c>SkillSpiralCharge</c>, <c>SkillCrusherCharge</c> and
/// <c>SkillElementalCharge</c>: the effect goes on the caster for 60 + energy / 10 seconds (DURATION_BASE 60,
/// ENERGY_DIVISOR 10), with no value; the charge is read by the attack it powers, which has no OpenMU mechanism yet
/// (buff_rows.md, open items). All is data an operator can change.
/// </remarks>
internal static class Season21MagicGladiatorBuffs
{
    private const float ChargeSeconds = 60;
    private const float ChargeSecondsPerEnergy = 1f / 10;

    private static readonly short[] ChargeEffects = [365, 366, 367, 368, 369, 370, 371, 372, 373];
    private static readonly short[] CasterSkills = [2014, 2015, 2016, 865, 866, 867, 868, 869, 870];

    /// <summary>
    /// Gives the charge effects which have no duration theirs, and makes the charge skills which are no buff yet buffs on the
    /// caster. An operator's own duration or skill type stays.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public static void Apply(IContext context, GameConfiguration gameConfiguration)
    {
        foreach (var effect in gameConfiguration.MagicEffects.Where(e => e.Duration is null && ChargeEffects.Contains(e.Number)))
        {
            effect.Duration = context.CreateNew<PowerUpDefinitionValue>();
            effect.Duration.ConstantValue.Value = ChargeSeconds;
            var perEnergy = context.CreateNew<AttributeRelationship>();
            perEnergy.InputAttribute = Stats.TotalEnergy.GetPersistent(gameConfiguration);
            perEnergy.InputOperator = InputOperator.Multiply;
            perEnergy.InputOperand = ChargeSecondsPerEnergy;
            effect.Duration.RelatedValues.Add(perEnergy);
        }

        foreach (var skill in gameConfiguration.Skills.Where(s => s.SkillType == SkillType.Other && CasterSkills.Contains(s.Number)))
        {
            skill.SkillType = SkillType.Buff;
            skill.Target = SkillTarget.ImplicitPlayer;
            skill.TargetRestriction = SkillTargetRestriction.Self;
        }
    }
}
