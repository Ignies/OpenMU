// <copyright file="Season21LightWizardBuffs.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// The Light Wizard's Season 21 rows which the skills update creates without a duration: Reflection Barrier (244) and its
/// tiers (861, 862: effect 343; 863: effect 345).
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Duration: S21Emu <c>Data/Scripts/LuaCalc/Skill/SkillDamageCalc.lua</c> <c>ReflectionBarrier</c>, 60 + energy / 10
/// seconds.</item>
/// <item>Target: a Season 22 client's skill table casts it as Crown Force (target mode 3) within 6 tiles, so the caster's
/// party in that radius.</item>
/// <item>Reflect: no file gives the base value. S21Emu passes only the Mastery tier's value, and the Season 22 client's
/// <c>skilltree.json</c> and <c>formulas.json</c> give only the tiers' rises (861 reflect rate, 862 duration, 863 maximum
/// mana). Effects 343 and 345 share Effect.txt group 39 with the Summoner's Reflection (71), so they copy its power-ups.
/// Replace them here if a real value shows up.</item>
/// </list>
/// All is data an operator can change.
/// </remarks>
internal static class Season21LightWizardBuffs
{
    private const short Reflection = 71;
    private const short BarrierRadius = 6;
    private const float Seconds = 60;
    private const float SecondsPerEnergy = 1f / 10;

    private static readonly short[] BarrierEffects = [343, 345];
    private static readonly short[] PartySkills = [244, 861, 862, 863];

    /// <summary>
    /// Gives the effects which have no duration theirs and Reflection's power-ups, and makes the rows which are an
    /// <c>Other</c> cast on the caster's party. An operator's own duration or skill type stays.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public static void Apply(IContext context, GameConfiguration gameConfiguration)
    {
        var reflection = gameConfiguration.MagicEffects.FirstOrDefault(e => e.Number == Reflection);
        foreach (var effect in gameConfiguration.MagicEffects.Where(e => e.Duration is null && BarrierEffects.Contains(e.Number)))
        {
            effect.Duration = context.CreateNew<PowerUpDefinitionValue>();
            effect.Duration.ConstantValue.Value = Seconds;
            var related = context.CreateNew<AttributeRelationship>();
            related.InputAttribute = Stats.TotalEnergy.GetPersistent(gameConfiguration);
            related.InputOperator = InputOperator.Multiply;
            related.InputOperand = SecondsPerEnergy;
            effect.Duration.RelatedValues.Add(related);

            foreach (var powerUp in reflection?.PowerUpDefinitions ?? [])
            {
                var copy = context.CreateNew<PowerUpDefinition>();
                copy.TargetAttribute = powerUp.TargetAttribute;
                copy.Boost = powerUp.Boost is { } boost ? Copy(context, boost) : null;
                effect.PowerUpDefinitions.Add(copy);
            }
        }

        foreach (var skill in gameConfiguration.Skills.Where(s => s.SkillType == SkillType.Other && PartySkills.Contains(s.Number)))
        {
            skill.SkillType = SkillType.Buff;
            skill.Target = SkillTarget.ImplicitParty;
            skill.TargetRestriction = SkillTargetRestriction.Party;
            skill.ImplicitTargetRange = BarrierRadius;
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
