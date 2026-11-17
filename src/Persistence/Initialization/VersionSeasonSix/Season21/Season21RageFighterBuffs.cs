// <copyright file="Season21RageFighterBuffs.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration;

/// <summary>
/// The Rage Fighter's buffs the skills update creates as an <c>Other</c> with an effect: Blood Howling (565, effect 166) and
/// its Strengthener (567, effect 167).
/// </summary>
/// <remarks>
/// S21Emu Skill.xml: Type 0, Range 0, Radio 0 (a buff on the caster). <c>GameServerInfo - Skill.dat</c>: BloodHowlingConstA
/// 10, BloodHowlingConstB 200, BloodHowlingTimeConstA 60: for 60 seconds the caster's skill hits bleed the target (10 + energy
/// / 200, the tooltip's "bleed damage"), which no OpenMU mechanism applies; an effect with no duration gets the 60 seconds.
/// </remarks>
internal static class Season21RageFighterBuffs
{
    private const float Seconds = 60;

    private static readonly short[] BloodHowlingEffects = [166, 167];
    private static readonly short[] BloodHowlingSkills = [565, 567];

    /// <summary>
    /// Makes the skills which are no buff yet buffs on the caster, and gives their effects without a duration the 60 seconds.
    /// An operator's own duration or skill type stays.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public static void Apply(IContext context, GameConfiguration gameConfiguration)
    {
        foreach (var effect in gameConfiguration.MagicEffects.Where(e => e.Duration is null && BloodHowlingEffects.Contains(e.Number)))
        {
            effect.Duration = context.CreateNew<PowerUpDefinitionValue>();
            effect.Duration.ConstantValue.Value = Seconds;
        }

        foreach (var skill in gameConfiguration.Skills.Where(s => BloodHowlingSkills.Contains(s.Number)))
        {
            if (skill.SkillType == SkillType.Other)
            {
                skill.SkillType = SkillType.Buff;
                skill.Target = SkillTarget.ImplicitPlayer;
                skill.TargetRestriction = SkillTargetRestriction.Self;
            }
        }
    }
}
