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
/// (ObsidianTimeConstA 120); a caster without a party gets 360 (SOLO_DURATION), read as 360 - 240 x
/// <see cref="Stats.IsInParty"/> (<see cref="ApplySoloDuration"/>). Skill.xml <c>Radio</c> 5. Read as <see cref="Stats.SkillDamageBonus"/>.</item>
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
    private const float ObsidianSoloSeconds = 360;
    private const float StrengthPerSkillDamage = 20;
    private const float WrathSeconds = 15;
    private const float WrathDamage = 1.1f;
    private const float WrathDefense = 0.95f;

    /// <summary>The radius of Obsidian's party (S21Emu Skill.xml <c>Radio</c> 5).</summary>
    private const short ObsidianRadius = 5;

    private static readonly short[] CircleShieldEffects = [216, 221, 222];
    private static readonly short[] ObsidianEffects = [217, 220];
    private static readonly short[] WrathEffects = [424, 223, 224, 225, 425];
    private static readonly short[] CircleShieldSkills = [272, 703, 706];
    private static readonly short[] ObsidianSkills = [273, 693];
    private static readonly short[] WrathSkills = [278, 708, 709, 710, 895];

    /// <summary>
    /// Gives the effects which have no duration theirs and their value, and makes the skills which are no buff yet buffs:
    /// Circle Shield and Wrath on the caster, Obsidian on the party. An operator's own duration or skill type stays.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public static void Apply(IContext context, GameConfiguration gameConfiguration)
    {
        foreach (var effect in gameConfiguration.MagicEffects.Where(e => e.Duration is null))
        {
            if (CircleShieldEffects.Contains(effect.Number))
            {
                SetDuration(context, effect, CircleShieldSeconds);
            }
            else if (ObsidianEffects.Contains(effect.Number))
            {
                SetDuration(context, effect, ObsidianSeconds);
                var powerUp = AddPowerUp(context, gameConfiguration, effect, Stats.SkillDamageBonus, 0, AggregateType.AddRaw);
                var perStrength = context.CreateNew<AttributeRelationship>();
                perStrength.InputAttribute = Stats.TotalStrength.GetPersistent(gameConfiguration);
                perStrength.InputOperator = InputOperator.Multiply;
                perStrength.InputOperand = 1f / StrengthPerSkillDamage;
                powerUp.Boost!.RelatedValues.Add(perStrength);
            }
            else if (WrathEffects.Contains(effect.Number))
            {
                SetDuration(context, effect, WrathSeconds);
                AddPowerUp(context, gameConfiguration, effect, Stats.AttackDamageIncrease, WrathDamage, AggregateType.Multiplicate);
                AddPowerUp(context, gameConfiguration, effect, Stats.DefenseDecrement, WrathDefense, AggregateType.Multiplicate);
            }
        }

        foreach (var skill in gameConfiguration.Skills.Where(s => s.SkillType == SkillType.Other))
        {
            if (CircleShieldSkills.Contains(skill.Number) || WrathSkills.Contains(skill.Number))
            {
                MakeBuff(skill, SkillTarget.ImplicitPlayer);
            }
            else if (ObsidianSkills.Contains(skill.Number))
            {
                MakeBuff(skill, SkillTarget.ImplicitParty);
            }
        }
    }

    /// <summary>
    /// Gives every class <see cref="Stats.IsInParty"/> and makes Obsidian's party duration (still the 120 seconds of
    /// <see cref="Apply"/>) 360 seconds without a party: 360 - 240 x <see cref="Stats.IsInParty"/>. An operator's own duration stays.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public static void ApplySoloDuration(IContext context, GameConfiguration gameConfiguration)
    {
        var isInParty = gameConfiguration.Attributes.FirstOrDefault(a => a.Id == Stats.IsInParty.Id);
        if (isInParty is null)
        {
            isInParty = context.CreateNew<AttributeDefinition>(Stats.IsInParty.Id, Stats.IsInParty.Designation, Stats.IsInParty.Description);
            gameConfiguration.Attributes.Add(isInParty);
        }

        foreach (var characterClass in gameConfiguration.CharacterClasses.Where(c => c.StatAttributes.All(s => s.Attribute != isInParty)))
        {
            characterClass.StatAttributes.Add(context.CreateNew<StatAttributeDefinition>(isInParty, 0, false));
        }

        foreach (var effect in gameConfiguration.MagicEffects.Where(e => ObsidianEffects.Contains(e.Number)
                     && e.Duration is { RelatedValues.Count: 0 } duration && duration.ConstantValue.Value == ObsidianSeconds))
        {
            effect.Duration!.ConstantValue.Value = ObsidianSoloSeconds;
            var inParty = context.CreateNew<AttributeRelationship>();
            inParty.InputAttribute = isInParty;
            inParty.InputOperator = InputOperator.Multiply;
            inParty.InputOperand = ObsidianSeconds - ObsidianSoloSeconds;
            effect.Duration.RelatedValues.Add(inParty);
        }
    }

    private static void MakeBuff(Skill skill, SkillTarget target)
    {
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

    private static void SetDuration(IContext context, MagicEffectDefinition effect, float seconds)
    {
        effect.Duration = context.CreateNew<PowerUpDefinitionValue>();
        effect.Duration.ConstantValue.Value = seconds;
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
