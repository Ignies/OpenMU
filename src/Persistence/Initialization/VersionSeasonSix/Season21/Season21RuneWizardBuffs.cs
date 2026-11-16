// <copyright file="Season21RuneWizardBuffs.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// The Rune Wizard's buffs, which the skills update creates without a duration: Burst (286, effect 293 Might) and Haste
/// (287, effect 294), and their Strengthener and Mastery (765/766 Might Improved and Enhanced 295/296, 768/769 298/299).
/// </summary>
/// <remarks>
/// S21Emu <c>GameServerInfo - Skill.dat</c>: Might and Haste ConstA 30, ConstB 20, TimeConstA 15, TimeConstB 10. Its
/// constants read value = A + energy / B (its Greater Damage 3 / 7 and Greater Defense 2 / 8 are OpenMU's own
/// formulas), seconds = TimeConstA + energy / TimeConstB. No data names the stat each raises: Might adds to the base
/// damage of every type, as Greater Damage does, Haste to the attack and magic speed. Both are effect data an operator
/// can change.
/// </remarks>
internal static class Season21RuneWizardBuffs
{
    private const float ValueBase = 30;
    private const float EnergyPerValue = 20;
    private const float SecondsBase = 15;
    private const float EnergyPerSecond = 10;

    private static readonly short[] MightEffects = [293, 295, 296];
    private static readonly short[] HasteEffects = [294, 298, 299];
    private static readonly short[] BurstSkills = [286, 765, 766];
    private static readonly short[] HasteSkills = [287, 768, 769];

    /// <summary>The radius of Haste's party (S21Emu Skill.xml <c>Radio</c> 7).</summary>
    private const short HasteRadius = 7;

    /// <summary>
    /// Gives the effects which have no duration theirs and their value, and makes the skills which are no buff yet
    /// buffs: Burst on the caster, Haste on the party. An operator's own duration or skill type stays.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public static void Apply(IContext context, GameConfiguration gameConfiguration)
    {
        AttributeDefinition[] damage = [Stats.PhysicalBaseDmg, Stats.WizardryBaseDmg, Stats.CurseBaseDmg];
        foreach (var effect in gameConfiguration.MagicEffects.Where(e => e.Duration is null))
        {
            var targets = MightEffects.Contains(effect.Number) ? damage : HasteEffects.Contains(effect.Number) ? [Stats.AttackSpeedAny] : null;
            if (targets is null)
            {
                continue;
            }

            effect.Duration = PerEnergy(context, gameConfiguration, SecondsBase, EnergyPerSecond);
            foreach (var target in targets)
            {
                var powerUp = context.CreateNew<PowerUpDefinition>();
                powerUp.TargetAttribute = target.GetPersistent(gameConfiguration);
                powerUp.Boost = PerEnergy(context, gameConfiguration, ValueBase, EnergyPerValue);
                effect.PowerUpDefinitions.Add(powerUp);
            }
        }

        foreach (var skill in gameConfiguration.Skills.Where(s => s.SkillType == SkillType.Other))
        {
            if (BurstSkills.Contains(skill.Number))
            {
                skill.SkillType = SkillType.Buff;
                skill.Target = SkillTarget.ImplicitPlayer;
                skill.TargetRestriction = SkillTargetRestriction.Self;
            }
            else if (HasteSkills.Contains(skill.Number))
            {
                skill.SkillType = SkillType.Buff;
                skill.Target = SkillTarget.ImplicitParty;
                skill.TargetRestriction = SkillTargetRestriction.Party;
                skill.ImplicitTargetRange = HasteRadius;
            }
        }
    }

    private static PowerUpDefinitionValue PerEnergy(IContext context, GameConfiguration gameConfiguration, float constant, float energyPerOne)
    {
        var value = context.CreateNew<PowerUpDefinitionValue>();
        value.ConstantValue.Value = constant;
        var perEnergy = context.CreateNew<AttributeRelationship>();
        perEnergy.InputAttribute = Stats.TotalEnergy.GetPersistent(gameConfiguration);
        perEnergy.InputOperator = InputOperator.Multiply;
        perEnergy.InputOperand = 1f / energyPerOne;
        value.RelatedValues.Add(perEnergy);
        return value;
    }
}
