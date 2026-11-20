// <copyright file="Season21Season22TextBuffs.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// The Season 21 buff values which S21Emu leaves out and a Season 22 client text names (a secondary source: S21Emu wins
/// where both speak): Evasion (652) and the stats of Divine Force (2098; 913) and Divine Aura (2099; 914).
/// </summary>
/// <remarks>
/// Season 22 web client <c>assets/data/skilltree.json</c>:
/// <list type="bullet">
/// <item>652: Evasion raises the basic defense rate by 50 % for 7 seconds (usable every 60 seconds). S21Emu's Lua has 0
/// for it; the caster gets it.</item>
/// <item>913: "Divine Force ... ATQ ... aumenta", so S21Emu's <c>SkillDivineForce</c> value 100 is attack.</item>
/// <item>914: "Divine Aura ... DEF ... aumenta", so S21Emu's <c>SkillDivineAura</c> value 500 is defense.</item>
/// </list>
/// The Season 22 text speaks of party members; S21Emu puts both on the caster for 3 seconds, which stays. All is data an
/// operator can change.
/// </remarks>
internal static class Season21Season22TextBuffs
{
    private const short Evasion = 652;
    private const float EvasionSeconds = 7;
    private const float EvasionDefenseRate = 1.5f;
    private const float DivineForceAttack = 100;
    private const float DivineAuraDefense = 500;

    private static readonly short[] EvasionEffects = [214];
    private static readonly short[] DivineForceEffects = [428];
    private static readonly short[] DivineAuraEffects = [429];

    /// <summary>
    /// Gives Evasion its duration, defense rate and a buff on the caster, and Divine Force and Divine Aura their stat, where
    /// the effect has none yet. An operator's own duration, power-ups or skill type stay.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public static void Apply(IContext context, GameConfiguration gameConfiguration)
    {
        foreach (var effect in gameConfiguration.MagicEffects.Where(e => e.PowerUpDefinitions.Count == 0))
        {
            if (EvasionEffects.Contains(effect.Number) && effect.Duration is null)
            {
                effect.Duration = context.CreateNew<PowerUpDefinitionValue>();
                effect.Duration.ConstantValue.Value = EvasionSeconds;
                foreach (var rate in new[] { Stats.DefenseRatePvm, Stats.DefenseRatePvp })
                {
                    AddPowerUp(context, gameConfiguration, effect, rate, EvasionDefenseRate, AggregateType.Multiplicate);
                }
            }
            else if (DivineForceEffects.Contains(effect.Number))
            {
                AddPowerUp(context, gameConfiguration, effect, Stats.BaseDamageBonus, DivineForceAttack, AggregateType.AddRaw);
            }
            else if (DivineAuraEffects.Contains(effect.Number))
            {
                AddPowerUp(context, gameConfiguration, effect, Stats.DefenseBase, DivineAuraDefense, AggregateType.AddRaw);
            }
        }

        if (gameConfiguration.Skills.FirstOrDefault(s => s.Number == Evasion) is { SkillType: SkillType.Other } evasion)
        {
            evasion.SkillType = SkillType.Buff;
            evasion.Target = SkillTarget.ImplicitPlayer;
            evasion.TargetRestriction = SkillTargetRestriction.Self;
        }
    }

    private static void AddPowerUp(IContext context, GameConfiguration gameConfiguration, MagicEffectDefinition effect, AttributeDefinition attribute, float value, AggregateType aggregateType)
    {
        var powerUp = context.CreateNew<PowerUpDefinition>();
        powerUp.TargetAttribute = attribute.GetPersistent(gameConfiguration);
        powerUp.Boost = context.CreateNew<PowerUpDefinitionValue>();
        powerUp.Boost.ConstantValue.Value = value;
        powerUp.Boost.ConstantValue.AggregateType = aggregateType;
        effect.PowerUpDefinitions.Add(powerUp);
    }
}
