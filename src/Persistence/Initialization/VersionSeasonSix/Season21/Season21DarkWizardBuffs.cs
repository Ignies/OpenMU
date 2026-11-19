// <copyright file="Season21DarkWizardBuffs.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration;

/// <summary>
/// The Dark Wizard's Season 21 buffs, which the skills update creates without a duration: Mana Shield Mastered (406).
/// </summary>
/// <remarks>
/// S21Emu <c>Data/Effect.txt</c>: effect 140 "Mana Shield Mastered" is Mana Shield's tier (group 4, as effect 4), and
/// <c>Data/Scripts/LuaCalc/Skill/SkillDamageCalc.lua</c> <c>SkillManaShield</c> computes one value for every tier. So the
/// skill is cast as Mana Shield (16) and its effect gets Mana Shield's duration and power-ups. All is data an operator can
/// change.
/// </remarks>
internal static class Season21DarkWizardBuffs
{
    private const short ManaShield = 16;
    private const short ManaShieldMastered = 406;

    /// <summary>
    /// Gives Mana Shield Mastered's effect, if it has no duration, Mana Shield's, and makes the skill, if it is no buff yet,
    /// a buff cast as Mana Shield. An operator's own duration or skill type stays.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public static void Apply(IContext context, GameConfiguration gameConfiguration)
    {
        if (gameConfiguration.Skills.FirstOrDefault(s => s.Number == ManaShield) is not { } manaShield
            || gameConfiguration.Skills.FirstOrDefault(s => s.Number == ManaShieldMastered) is not { } mastered)
        {
            return;
        }

        if (mastered.MagicEffectDef is { Duration: null } effect && manaShield.MagicEffectDef is { Duration: { } duration } original)
        {
            effect.Duration = Copy(context, duration);
            foreach (var powerUp in original.PowerUpDefinitions)
            {
                var copy = context.CreateNew<PowerUpDefinition>();
                copy.TargetAttribute = powerUp.TargetAttribute;
                copy.Boost = powerUp.Boost is { } boost ? Copy(context, boost) : null;
                effect.PowerUpDefinitions.Add(copy);
            }
        }

        if (mastered.SkillType == SkillType.Other)
        {
            mastered.SkillType = manaShield.SkillType;
            mastered.Target = manaShield.Target;
            mastered.TargetRestriction = manaShield.TargetRestriction;
            mastered.ImplicitTargetRange = manaShield.ImplicitTargetRange;
        }
    }

    private static PowerUpDefinitionValue Copy(IContext context, PowerUpDefinitionValue source)
    {
        var value = context.CreateNew<PowerUpDefinitionValue>();
        value.ConstantValue.Value = source.ConstantValue.Value;
        value.ConstantValue.AggregateType = source.ConstantValue.AggregateType;
        value.MaximumValue = source.MaximumValue;
        foreach (var related in source.RelatedValues)
        {
            var copy = context.CreateNew<AttributeRelationship>();
            copy.TargetAttribute = related.TargetAttribute;
            copy.InputAttribute = related.InputAttribute;
            copy.OperandAttribute = related.OperandAttribute;
            copy.InputOperator = related.InputOperator;
            copy.InputOperand = related.InputOperand;
            copy.AggregateType = related.AggregateType;
            value.RelatedValues.Add(copy);
        }

        return value;
    }
}
