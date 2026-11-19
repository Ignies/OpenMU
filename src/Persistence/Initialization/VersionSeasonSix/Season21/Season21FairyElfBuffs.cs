// <copyright file="Season21FairyElfBuffs.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// The Fairy Elf's Season 21 buffs, which the skills update creates without a duration: Bless (430, 433, 1230), Elemental
/// Damage Enhancement (2026, 1224), Elemental Defense Enhancement (2027, 1225) and the Greater Damage (1228) and Greater
/// Defense (1229) enhancement rows.
/// </summary>
/// <remarks>
/// S21Emu <c>Data/Scripts/LuaCalc/Skill/SkillDamageCalc.lua</c>:
/// <list type="bullet">
/// <item>Bless (<c>SkillBless</c>): 300 seconds; strength, agility, vitality, energy and command + energy / 100. The value
/// reads the base energy, since a value which reads the total energy and raises it would feed itself on a self cast.</item>
/// <item>Elemental Damage Enhancement (<c>SkillElementalDamageEnhanced</c>): 60 seconds; elemental damage + 3 + energy / 10.</item>
/// <item>Elemental Defense Enhancement (<c>SkillElementalDefenseEnhanced</c>): 60 seconds; elemental defense
/// + (2 + energy / 12) x 2, which is 4 + energy / 6.</item>
/// <item>The Greater Damage and Greater Defense enhancement rows are those buffs' tiers (<c>SkillAttack</c>,
/// <c>SkillDefense</c> compute one value for every tier), so they get Season 6's effect 1 and 2.</item>
/// </list>
/// Evasion (652) and Dex Booster (2024) stay Pending: S21Emu has no values for them. All is effect data an operator can
/// change.
/// </remarks>
internal static class Season21FairyElfBuffs
{
    private const float BlessSeconds = 300;
    private const float BlessPerBaseEnergy = 0.01f;
    private const float ElementalSeconds = 60;
    private const float ElementalDamage = 3;
    private const float ElementalDamagePerEnergy = 1f / 10;
    private const float ElementalDefense = 4;
    private const float ElementalDefensePerEnergy = 1f / 6;
    private const short GreaterDamage = 1;
    private const short GreaterDefense = 2;

    private static readonly short[] BlessSkills = [430, 433, 1230];
    private static readonly short[] ElementalDamageSkills = [2026, 1224];
    private static readonly short[] ElementalDefenseSkills = [2027, 1225];
    private static readonly short GreaterDamageSkill = 1228;
    private static readonly short GreaterDefenseSkill = 1229;

    /// <summary>
    /// Gets the skills which are buffs the elf casts on a player of its choice (as Season 6's Greater Damage). The client's
    /// skill rows read them.
    /// </summary>
    public static short[] TargetSkills { get; } = [.. BlessSkills, .. ElementalDamageSkills, .. ElementalDefenseSkills, GreaterDamageSkill, GreaterDefenseSkill];

    /// <summary>
    /// Gives the effects which have no duration theirs and their value, and makes the skills which are no buff yet buffs on
    /// a player of the caster's choice. An operator's own duration or skill type stays.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public static void Apply(IContext context, GameConfiguration gameConfiguration)
    {
        foreach (var skill in TargetSkills.Select(n => gameConfiguration.Skills.FirstOrDefault(s => s.Number == n)).OfType<Skill>())
        {
            if (skill.MagicEffectDef is { Duration: null } effect)
            {
                if (BlessSkills.Contains(skill.Number))
                {
                    SetDuration(context, effect, BlessSeconds);
                    foreach (var stat in new[] { Stats.TotalStrength, Stats.TotalAgility, Stats.TotalVitality, Stats.TotalEnergy, Stats.TotalLeadership })
                    {
                        AddPowerUp(context, gameConfiguration, effect, stat, 0, Stats.BaseEnergy, BlessPerBaseEnergy);
                    }
                }
                else if (ElementalDamageSkills.Contains(skill.Number))
                {
                    SetDuration(context, effect, ElementalSeconds);
                    AddPowerUp(context, gameConfiguration, effect, Stats.ElementDamageBonus, ElementalDamage, Stats.TotalEnergy, ElementalDamagePerEnergy);
                }
                else if (ElementalDefenseSkills.Contains(skill.Number))
                {
                    SetDuration(context, effect, ElementalSeconds);
                    AddPowerUp(context, gameConfiguration, effect, Stats.ElementDefense, ElementalDefense, Stats.TotalEnergy, ElementalDefensePerEnergy);
                }
                else
                {
                    var season6 = skill.Number == GreaterDamageSkill ? GreaterDamage : GreaterDefense;
                    if (gameConfiguration.MagicEffects.FirstOrDefault(e => e.Number == season6) is { Duration: { } duration } original)
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
                }
            }

            if (skill.SkillType == SkillType.Other)
            {
                skill.SkillType = SkillType.Buff;
                skill.Target = SkillTarget.Explicit;
                skill.TargetRestriction = SkillTargetRestriction.Player;
            }
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

    private static void SetDuration(IContext context, MagicEffectDefinition effect, float seconds)
    {
        effect.Duration = context.CreateNew<PowerUpDefinitionValue>();
        effect.Duration.ConstantValue.Value = seconds;
    }

    private static void AddPowerUp(IContext context, GameConfiguration gameConfiguration, MagicEffectDefinition effect, AttributeDefinition attribute, float constant, AttributeDefinition input, float perInput)
    {
        var powerUp = context.CreateNew<PowerUpDefinition>();
        powerUp.TargetAttribute = attribute.GetPersistent(gameConfiguration);
        powerUp.Boost = context.CreateNew<PowerUpDefinitionValue>();
        powerUp.Boost.ConstantValue.Value = constant;
        var related = context.CreateNew<AttributeRelationship>();
        related.InputAttribute = input.GetPersistent(gameConfiguration);
        related.InputOperator = InputOperator.Multiply;
        related.InputOperand = perInput;
        powerUp.Boost.RelatedValues.Add(related);
        effect.PowerUpDefinitions.Add(powerUp);
    }
}
