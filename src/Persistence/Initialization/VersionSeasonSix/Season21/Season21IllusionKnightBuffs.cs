// <copyright file="Season21IllusionKnightBuffs.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// The Illusion Knight's Season 21 rows which the skills update creates without a duration: Illusion Blade (2032) and its tiers
/// (885, 886, 887), effects 405 to 408.
/// </summary>
/// <remarks>
/// S21Emu <c>Data/Scripts/LuaCalc/Skill/SkillDamageCalc.lua</c> (<c>SkillIllusionBlade</c>): the caster, 300 seconds,
/// value 110. A Season 22 client's <c>assets/data/skilltree.json</c> 885 names it skill power, read as the skill damage bonus;
/// its <c>formulas.json</c> 302 to 304 are the tiers' own rises, which the master skill system adds.
/// All is data an operator can change.
/// </remarks>
internal static class Season21IllusionKnightBuffs
{
    private const float Seconds = 300;
    private const float SkillDamage = 110;

    private static readonly short[] IllusionBladeEffects = [405, 406, 407, 408];
    private static readonly short[] IllusionBladeSkills = [2032, 885, 886, 887];

    /// <summary>
    /// Gives the effects which have no duration theirs and makes the rows which are an <c>Other</c> cast on the caster.
    /// An operator's own duration or skill type stays.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public static void Apply(IContext context, GameConfiguration gameConfiguration)
    {
        foreach (var effect in gameConfiguration.MagicEffects.Where(e => e.Duration is null && IllusionBladeEffects.Contains(e.Number)))
        {
            effect.Duration = context.CreateNew<PowerUpDefinitionValue>();
            effect.Duration.ConstantValue.Value = Seconds;

            var powerUp = context.CreateNew<PowerUpDefinition>();
            powerUp.TargetAttribute = Stats.SkillDamageBonus.GetPersistent(gameConfiguration);
            powerUp.Boost = context.CreateNew<PowerUpDefinitionValue>();
            powerUp.Boost.ConstantValue.Value = SkillDamage;
            powerUp.Boost.ConstantValue.AggregateType = AggregateType.AddRaw;
            effect.PowerUpDefinitions.Add(powerUp);
        }

        foreach (var skill in gameConfiguration.Skills.Where(s => s.SkillType == SkillType.Other && IllusionBladeSkills.Contains(s.Number)))
        {
            skill.SkillType = SkillType.Buff;
            skill.Target = SkillTarget.ImplicitPlayer;
            skill.TargetRestriction = SkillTargetRestriction.Self;
        }
    }
}
