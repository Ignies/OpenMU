// <copyright file="Season21LemuriaMageBuffs.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// The Lemuria Mage's Season 21 beginner buffs, which the skills update creates without a duration: Beginner Greater Damage
/// (2010; 853, 854), Beginner Greater Defense (2009; 851, 852) and Beginner Bless (2011; 857), effects 346, 347 and 348.
/// </summary>
/// <remarks>
/// S21Emu <c>Data/Scripts/LuaCalc/Skill/SkillDamageCalc.lua</c>:
/// <list type="bullet">
/// <item>Greater Damage (<c>SkillBeginnerGreaterDamage</c>): 60 seconds; + 3 + energy / 14 (Effect.txt group 1, Greater
/// Damage's).</item>
/// <item>Greater Defense (<c>SkillBeginnerGreaterDefense</c>): 60 seconds; + (2 + energy / 16) x 2, which is 4 + energy / 8
/// (group 2, Greater Defense's).</item>
/// <item>Bless (<c>SkillBeginnerBless</c>): 300 seconds; every stat + energy / 200, read from the base energy as the elf's
/// Bless is, so a self cast does not feed itself.</item>
/// </list>
/// A Season 22 client's skill table casts them as Greater Damage is cast (target mode 4, range 6): on a player of the
/// mage's choice. The tiers' rises are the master skill system's. All is effect data an operator can change.
/// </remarks>
internal static class Season21LemuriaMageBuffs
{
    private const float GreaterSeconds = 60;
    private const float DamageBase = 3;
    private const float DamagePerEnergy = 1f / 14;
    private const float DefenseBase = 4;
    private const float DefensePerEnergy = 1f / 8;
    private const float BlessSeconds = 300;
    private const float BlessPerBaseEnergy = 1f / 200;

    private static readonly short[] DamageSkills = [2010, 853, 854];
    private static readonly short[] DefenseSkills = [2009, 851, 852];
    private static readonly short[] BlessSkills = [2011, 857];

    /// <summary>
    /// Gets the skills which are buffs the mage casts on a player of its choice (as Season 6's Greater Damage). The client's
    /// skill rows read them.
    /// </summary>
    public static short[] TargetSkills { get; } = [.. DamageSkills, .. DefenseSkills, .. BlessSkills];

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
                if (DamageSkills.Contains(skill.Number))
                {
                    SetDuration(context, effect, GreaterSeconds);
                    AddPowerUp(context, gameConfiguration, effect, Stats.GreaterDamageBonus, DamageBase, Stats.TotalEnergy, DamagePerEnergy);
                }
                else if (DefenseSkills.Contains(skill.Number))
                {
                    SetDuration(context, effect, GreaterSeconds);
                    AddPowerUp(context, gameConfiguration, effect, Stats.GreaterDefenseBonus, DefenseBase, Stats.TotalEnergy, DefensePerEnergy);
                }
                else
                {
                    SetDuration(context, effect, BlessSeconds);
                    foreach (var stat in new[] { Stats.TotalStrength, Stats.TotalAgility, Stats.TotalVitality, Stats.TotalEnergy, Stats.TotalLeadership })
                    {
                        AddPowerUp(context, gameConfiguration, effect, stat, 0, Stats.BaseEnergy, BlessPerBaseEnergy);
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
        powerUp.Boost.ConstantValue.AggregateType = AggregateType.AddRaw;
        var related = context.CreateNew<AttributeRelationship>();
        related.InputAttribute = input.GetPersistent(gameConfiguration);
        related.InputOperator = InputOperator.Multiply;
        related.InputOperand = perInput;
        powerUp.Boost.RelatedValues.Add(related);
        effect.PowerUpDefinitions.Add(powerUp);
    }
}
