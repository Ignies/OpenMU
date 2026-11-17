// <copyright file="Season21SlayerBuffs.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// The Slayer's buffs, which the skills update creates without a duration: Detection (295, effect 316; its Strengthener 794,
/// 318) and Demolish (297, effect 317; Strengthener and Mastery 787/788, 319/320).
/// </summary>
/// <remarks>
/// S21Emu <c>GameServerInfo - Skill.dat</c>: DetectionConstA 45, DetectionTimeConstA 60; DemolishConstA 20, DemolishConstB 50,
/// DemolishTimeConstA 60. Read as the Rune Wizard's constants are (value = A + energy / B, seconds = TimeConstA): Demolish
/// "ignores %d of enemy Defense" (BuffEffect.txt 317), as a percent of OpenMU's <see cref="Stats.DefenseIgnoreChance"/>, the
/// one stat that lowers a target's defense; Detection "detects lifeforms nearby" for 60 seconds, which no stat models (its
/// 45 has nothing to raise). Skill.xml: Detection Radio 0 (the caster), Demolish Radio 5 (the caster and his party). Its
/// skill power drain (effect 321, 3%) has no OpenMU mechanism. All is effect data an operator can change.
/// </remarks>
internal static class Season21SlayerBuffs
{
    private const float Seconds = 60;
    private const float IgnoreBase = 20;
    private const float EnergyPerIgnore = 50;

    /// <summary>The radius of Demolish's party (S21Emu Skill.xml <c>Radio</c> 5).</summary>
    private const short DemolishRadius = 5;

    private static readonly short[] DetectionEffects = [316, 318];
    private static readonly short[] DemolishEffects = [317, 319, 320];
    private static readonly short[] DetectionSkills = [295, 794];
    private static readonly short[] DemolishSkills = [297, 787, 788];

    /// <summary>
    /// Gives the effects which have no duration theirs and their value, and makes the skills which are no buff yet buffs:
    /// Detection on the caster, Demolish on the party. An operator's own duration or skill type stays.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public static void Apply(IContext context, GameConfiguration gameConfiguration)
    {
        foreach (var effect in gameConfiguration.MagicEffects.Where(e => e.Duration is null))
        {
            var detection = DetectionEffects.Contains(effect.Number);
            var demolish = DemolishEffects.Contains(effect.Number);
            if (!detection && !demolish)
            {
                continue;
            }

            effect.Duration = context.CreateNew<PowerUpDefinitionValue>();
            effect.Duration.ConstantValue.Value = Seconds;
            if (demolish)
            {
                var powerUp = context.CreateNew<PowerUpDefinition>();
                powerUp.TargetAttribute = Stats.DefenseIgnoreChance.GetPersistent(gameConfiguration);
                powerUp.Boost = context.CreateNew<PowerUpDefinitionValue>();
                powerUp.Boost.ConstantValue.Value = IgnoreBase / 100f;
                var perEnergy = context.CreateNew<AttributeRelationship>();
                perEnergy.InputAttribute = Stats.TotalEnergy.GetPersistent(gameConfiguration);
                perEnergy.InputOperator = InputOperator.Multiply;
                perEnergy.InputOperand = 0.01f / EnergyPerIgnore;
                powerUp.Boost.RelatedValues.Add(perEnergy);
                effect.PowerUpDefinitions.Add(powerUp);
            }
        }

        foreach (var skill in gameConfiguration.Skills.Where(s => s.SkillType == SkillType.Other))
        {
            if (DetectionSkills.Contains(skill.Number))
            {
                skill.SkillType = SkillType.Buff;
                skill.Target = SkillTarget.ImplicitPlayer;
                skill.TargetRestriction = SkillTargetRestriction.Self;
            }
            else if (DemolishSkills.Contains(skill.Number))
            {
                skill.SkillType = SkillType.Buff;
                skill.Target = SkillTarget.ImplicitParty;
                skill.TargetRestriction = SkillTargetRestriction.Party;
                skill.ImplicitTargetRange = DemolishRadius;
            }
        }
    }
}
