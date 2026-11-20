// <copyright file="Season21GunCrusherBuffs.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// The Gun Crusher's Season 21 rows which the skills update creates without a duration: Fixed Fire (2021) and its tiers
/// (828, 829), effects 380, 381 and 382.
/// </summary>
/// <remarks>
/// S21Emu <c>Data/Scripts/LuaCalc/Skill/SkillDamageCalc.lua</c> (<c>SkillFixedFire</c>): the caster, 60 + energy / 10
/// seconds, value 8. A Season 22 client's <c>assets/data/skilltree.json</c> 829 names the value: it raises attack speed.
/// All is data an operator can change.
/// </remarks>
internal static class Season21GunCrusherBuffs
{
    private const float Seconds = 60;
    private const float SecondsPerEnergy = 1f / 10;
    private const float AttackSpeed = 8;

    private static readonly short[] FixedFireEffects = [380, 381, 382];
    private static readonly short[] FixedFireSkills = [2021, 828, 829];

    /// <summary>
    /// Gives the effects which have no duration theirs and makes the rows which are an <c>Other</c> cast on the caster.
    /// An operator's own duration or skill type stays.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public static void Apply(IContext context, GameConfiguration gameConfiguration)
    {
        foreach (var effect in gameConfiguration.MagicEffects.Where(e => e.Duration is null && FixedFireEffects.Contains(e.Number)))
        {
            effect.Duration = context.CreateNew<PowerUpDefinitionValue>();
            effect.Duration.ConstantValue.Value = Seconds;
            var related = context.CreateNew<AttributeRelationship>();
            related.InputAttribute = Stats.TotalEnergy.GetPersistent(gameConfiguration);
            related.InputOperator = InputOperator.Multiply;
            related.InputOperand = SecondsPerEnergy;
            effect.Duration.RelatedValues.Add(related);

            var powerUp = context.CreateNew<PowerUpDefinition>();
            powerUp.TargetAttribute = Stats.AttackSpeed.GetPersistent(gameConfiguration);
            powerUp.Boost = context.CreateNew<PowerUpDefinitionValue>();
            powerUp.Boost.ConstantValue.Value = AttackSpeed;
            powerUp.Boost.ConstantValue.AggregateType = AggregateType.AddRaw;
            effect.PowerUpDefinitions.Add(powerUp);
        }

        foreach (var skill in gameConfiguration.Skills.Where(s => s.SkillType == SkillType.Other && FixedFireSkills.Contains(s.Number)))
        {
            skill.SkillType = SkillType.Buff;
            skill.Target = SkillTarget.ImplicitPlayer;
            skill.TargetRestriction = SkillTargetRestriction.Self;
        }
    }
}
