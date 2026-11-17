// <copyright file="Season21GrowLancerBuffs.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// The Grow Lancer's buffs, which the skills update creates without a duration: Circle Shield (272; Improved 703, Enhanced 706),
/// Obsidian (273; Improved 693) and Wrath (278; 708-710 and the Strengthener 895).
/// </summary>
/// <remarks>
/// S21Emu <c>Data/Scripts/LuaCalc/Skill/SkillDamageCalc.lua</c> and <c>GameServerInfo - Skill.dat</c>:
/// <list type="bullet">
/// <item>Circle Shield (<c>SkillCircleShield</c>): value 10, 30 seconds (CircleShieldConstA 10, ConstB 40, TimeConstA 30). It is a
/// PvP skill which may lower the attacker's AG, which no OpenMU mechanism applies: the buff lasts, with no stat.</item>
/// <item>Obsidian (<c>SkillObsidian</c>): skill damage + strength / 20 (ObsidianConstA 20) on the caster's party, 120 seconds
/// (ObsidianTimeConstA 120; the Lua gives a caster without a party 360, which one duration can't say).
/// Skill.xml <c>Radio</c> 5. Read as <see cref="Stats.SkillDamageBonus"/>.</item>
/// <item>Wrath: 15 seconds (WrathTimeConstA 15); combat power +10 % for defense -5 % (Webzen's Grow Lancer guide and its
/// Season 19 renewal), read as <see cref="Stats.AttackDamageIncrease"/> 1.1 and <see cref="Stats.DefenseDecrement"/> 0.95.
/// WrathConstA 50 and ConstB 30 name no stat in any source and are left out.</item>
/// </list>
/// All is effect data an operator can change. Burst (700/701, BurstConstA 5) waits for a source of what its 5 raises.
/// </remarks>
internal static class Season21GrowLancerBuffs
{
    private const float CircleShieldSeconds = 30;
    private const float ObsidianSeconds = 120;
    private const float StrengthPerSkillDamage = 20;
    private const float WrathSeconds = 15;
    private const float WrathDamage = 1.1f;
    private const float WrathDefense = 0.95f;

    /// <summary>The radius of Obsidian's party (S21Emu Skill.xml <c>Radio</c> 5).</summary>
    private const short ObsidianRadius = 5;

    private static readonly short[] CircleShieldSkills = [272, 703, 706];
    private static readonly short[] ObsidianSkills = [273, 693];
    private static readonly short[] WrathSkills = [278, 708, 709, 710, 895];

    /// <summary>
    /// Gives the effects of these skills which have no duration theirs and their value, and makes the skills which are no
    /// buff yet buffs: Circle Shield and Wrath on the caster, Obsidian on the party. An operator's own duration or skill
    /// type stays.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public static void Apply(IContext context, GameConfiguration gameConfiguration)
    {
        foreach (var skill in gameConfiguration.Skills.Where(s => s.MagicEffectDef is not null))
        {
            var effect = skill.MagicEffectDef!;
            if (CircleShieldSkills.Contains(skill.Number))
            {
                MakeBuff(skill, SkillTarget.ImplicitPlayer);
                SetDuration(context, effect, CircleShieldSeconds);
            }
            else if (ObsidianSkills.Contains(skill.Number))
            {
                MakeBuff(skill, SkillTarget.ImplicitParty);
                if (SetDuration(context, effect, ObsidianSeconds))
                {
                    var powerUp = AddPowerUp(context, gameConfiguration, effect, Stats.SkillDamageBonus, 0, AggregateType.AddRaw);
                    var perStrength = context.CreateNew<AttributeRelationship>();
                    perStrength.InputAttribute = Stats.TotalStrength.GetPersistent(gameConfiguration);
                    perStrength.InputOperator = InputOperator.Multiply;
                    perStrength.InputOperand = 1f / StrengthPerSkillDamage;
                    powerUp.Boost!.RelatedValues.Add(perStrength);
                }
            }
            else if (WrathSkills.Contains(skill.Number))
            {
                MakeBuff(skill, SkillTarget.ImplicitPlayer);
                if (SetDuration(context, effect, WrathSeconds))
                {
                    AddPowerUp(context, gameConfiguration, effect, Stats.AttackDamageIncrease, WrathDamage, AggregateType.Multiplicate);
                    AddPowerUp(context, gameConfiguration, effect, Stats.DefenseDecrement, WrathDefense, AggregateType.Multiplicate);
                }
            }
        }
    }

    private static void MakeBuff(Skill skill, SkillTarget target)
    {
        if (skill.SkillType != SkillType.Other)
        {
            return;
        }

        skill.SkillType = SkillType.Buff;
        skill.Target = target;
        if (target == SkillTarget.ImplicitParty)
        {
            skill.TargetRestriction = SkillTargetRestriction.Party;
            skill.ImplicitTargetRange = ObsidianRadius;
        }
        else
        {
            skill.TargetRestriction = SkillTargetRestriction.Self;
        }
    }

    /// <summary>Sets the duration of an effect which has none; false when an operator set one (its power-ups are then left alone).</summary>
    private static bool SetDuration(IContext context, MagicEffectDefinition effect, float seconds)
    {
        if (effect.Duration is not null)
        {
            return false;
        }

        effect.Duration = context.CreateNew<PowerUpDefinitionValue>();
        effect.Duration.ConstantValue.Value = seconds;
        return true;
    }

    private static PowerUpDefinition AddPowerUp(IContext context, GameConfiguration gameConfiguration, MagicEffectDefinition effect, AttributeDefinition attribute, float value, AggregateType aggregateType)
    {
        var powerUp = context.CreateNew<PowerUpDefinition>();
        powerUp.TargetAttribute = attribute.GetPersistent(gameConfiguration);
        powerUp.Boost = context.CreateNew<PowerUpDefinitionValue>();
        powerUp.Boost.ConstantValue.Value = value;
        powerUp.Boost.ConstantValue.AggregateType = aggregateType;
        effect.PowerUpDefinitions.Add(powerUp);
        return powerUp;
    }
}
