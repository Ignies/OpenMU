// <copyright file="Season21SummonerBuffs.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// The Summoner's Season 21 rows which the skills update creates without a duration: Darkness (289; 770, 771), the Weakness
/// and Innovation tiers (459, 772; 460, 773) and Blind (461, 463). 459, 460, 461 and 463 are Season 6 master skills the
/// Season 21 data turned into an <c>Other</c>.
/// </summary>
/// <remarks>
/// S21Emu <c>Data/Scripts/LuaCalc/Skill/SkillDamageCalc.lua</c> and <c>Data/Effect.txt</c>:
/// <list type="bullet">
/// <item>Darkness (<c>SkillDarkness</c>): the caster, 30 + energy / 60 seconds, attack speed + 28 (ATTACK_SPEED). Its curse
/// damage, defense and life reduction come from formula natives no file gives (buff_rows.md, open items).</item>
/// <item>Effects 162, 304 are in Lesser Damage's group (43) and 163, 305 in Lesser Defense's (44), the Season 6 Weakness
/// and Innovation; <c>SkillLesserDamage</c> and <c>SkillLesserDefense</c> compute one value for every tier. So the tiers
/// are cast as Weakness (221) and Innovation (222), and their effects copy those skills' effects.</item>
/// <item>Blind (<c>SkillBlind</c>): a hit which blinds the target for 5 seconds with a chance of 20 + energy / 25 %
/// (value 50); a Season 22 client's <c>assets/data/skilltree.json</c> 461 names it: it "reduces the target's attack
/// success rate", read as the attack rate halved.</item>
/// </list>
/// All is data an operator can change.
/// </remarks>
internal static class Season21SummonerBuffs
{
    private const short Weakness = 221;
    private const short Innovation = 222;
    private const float DarknessSeconds = 30;
    private const float DarknessSecondsPerEnergy = 1f / 60;
    private const float DarknessAttackSpeed = 28;
    private const float BlindSeconds = 5;
    private const float BlindChance = 0.20f;
    private const float BlindChancePerEnergy = 1f / 2500;
    private const float BlindAttackRate = 0.5f;

    private static readonly short[] DarknessEffects = [301, 302, 303];
    private static readonly short[] BlindEffects = [73, 144];
    private static readonly short[] CasterSkills = [289, 770, 771];
    private static readonly short[] BlindSkills = [461, 463];
    private static readonly short[] WeaknessTiers = [459, 772];
    private static readonly short[] InnovationTiers = [460, 773];

    /// <summary>
    /// Gives the effects which have no duration theirs and makes the rows which are an <c>Other</c> cast as they should.
    /// An operator's own duration or skill type stays.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public static void Apply(IContext context, GameConfiguration gameConfiguration)
    {
        foreach (var effect in gameConfiguration.MagicEffects.Where(e => e.Duration is null))
        {
            if (DarknessEffects.Contains(effect.Number))
            {
                effect.Duration = ByInput(context, gameConfiguration, DarknessSeconds, Stats.TotalEnergy, DarknessSecondsPerEnergy);
                AddPowerUp(context, effect.PowerUpDefinitions, gameConfiguration, Stats.AttackSpeed, DarknessAttackSpeed, AggregateType.AddRaw);
            }
            else if (BlindEffects.Contains(effect.Number))
            {
                effect.Duration = context.CreateNew<PowerUpDefinitionValue>();
                effect.Duration.ConstantValue.Value = BlindSeconds;
                effect.Chance = ByInput(context, gameConfiguration, BlindChance, Stats.TotalEnergy, BlindChancePerEnergy);
                effect.ChancePvp = ByInput(context, gameConfiguration, BlindChance, Stats.TotalEnergy, BlindChancePerEnergy);
                AddPowerUp(context, effect.PowerUpDefinitions, gameConfiguration, Stats.AttackRatePvm, BlindAttackRate, AggregateType.Multiplicate);
                AddPowerUp(context, effect.PowerUpDefinitionsPvp, gameConfiguration, Stats.AttackRatePvp, BlindAttackRate, AggregateType.Multiplicate);
            }
        }

        foreach (var (root, tiers) in new[] { (Weakness, WeaknessTiers), (Innovation, InnovationTiers) })
        {
            if (gameConfiguration.Skills.FirstOrDefault(s => s.Number == root) is not { } original)
            {
                continue;
            }

            foreach (var tier in gameConfiguration.Skills.Where(s => tiers.Contains(s.Number)))
            {
                if (tier.MagicEffectDef is { Duration: null } effect && original.MagicEffectDef is { } source)
                {
                    CopyEffect(context, source, effect);
                }

                if (tier.SkillType == SkillType.Other)
                {
                    tier.SkillType = original.SkillType;
                    tier.Target = original.Target;
                    tier.TargetRestriction = original.TargetRestriction;
                    tier.ImplicitTargetRange = original.ImplicitTargetRange;
                    tier.AreaSkillSettings = original.AreaSkillSettings is { } area ? CopyArea(context, area) : null;
                }
            }
        }

        foreach (var skill in gameConfiguration.Skills.Where(s => s.SkillType == SkillType.Other))
        {
            if (CasterSkills.Contains(skill.Number))
            {
                skill.SkillType = SkillType.Buff;
                skill.Target = SkillTarget.ImplicitPlayer;
                skill.TargetRestriction = SkillTargetRestriction.Self;
            }
            else if (BlindSkills.Contains(skill.Number))
            {
                skill.SkillType = SkillType.DirectHit;
                skill.Target = SkillTarget.Explicit;
                skill.TargetRestriction = SkillTargetRestriction.Undefined;
            }
        }
    }

    private static PowerUpDefinitionValue ByInput(IContext context, GameConfiguration gameConfiguration, float constant, AttributeDefinition input, float perInput)
    {
        var value = context.CreateNew<PowerUpDefinitionValue>();
        value.ConstantValue.Value = constant;
        var related = context.CreateNew<AttributeRelationship>();
        related.InputAttribute = input.GetPersistent(gameConfiguration);
        related.InputOperator = InputOperator.Multiply;
        related.InputOperand = perInput;
        value.RelatedValues.Add(related);
        return value;
    }

    private static void AddPowerUp(IContext context, ICollection<PowerUpDefinition> powerUps, GameConfiguration gameConfiguration, AttributeDefinition attribute, float value, AggregateType aggregateType)
    {
        var powerUp = context.CreateNew<PowerUpDefinition>();
        powerUp.TargetAttribute = attribute.GetPersistent(gameConfiguration);
        powerUp.Boost = context.CreateNew<PowerUpDefinitionValue>();
        powerUp.Boost.ConstantValue.Value = value;
        powerUp.Boost.ConstantValue.AggregateType = aggregateType;
        powerUps.Add(powerUp);
    }

    private static void CopyEffect(IContext context, MagicEffectDefinition source, MagicEffectDefinition target)
    {
        target.InformObservers = source.InformObservers;
        target.StopByDeath = source.StopByDeath;
        target.SendDuration = source.SendDuration;
        target.DurationDependsOnTargetLevel = source.DurationDependsOnTargetLevel;
        target.MonsterTargetLevelDivisor = source.MonsterTargetLevelDivisor;
        target.PlayerTargetLevelDivisor = source.PlayerTargetLevelDivisor;
        target.Chance = Copy(context, source.Chance);
        target.ChancePvp = Copy(context, source.ChancePvp);
        target.Duration = Copy(context, source.Duration);
        target.DurationPvp = Copy(context, source.DurationPvp);
        foreach (var (from, to) in new[] { (source.PowerUpDefinitions, target.PowerUpDefinitions), (source.PowerUpDefinitionsPvp, target.PowerUpDefinitionsPvp) })
        {
            foreach (var powerUp in from)
            {
                var copy = context.CreateNew<PowerUpDefinition>();
                copy.TargetAttribute = powerUp.TargetAttribute;
                copy.Boost = Copy(context, powerUp.Boost);
                to.Add(copy);
            }
        }
    }

    private static AreaSkillSettings CopyArea(IContext context, AreaSkillSettings source)
    {
        var copy = context.CreateNew<AreaSkillSettings>();
        copy.UseFrustumFilter = source.UseFrustumFilter;
        copy.FrustumStartWidth = source.FrustumStartWidth;
        copy.FrustumEndWidth = source.FrustumEndWidth;
        copy.FrustumDistance = source.FrustumDistance;
        copy.UseTargetAreaFilter = source.UseTargetAreaFilter;
        copy.TargetAreaDiameter = source.TargetAreaDiameter;
        copy.UseDeferredHits = source.UseDeferredHits;
        copy.DelayPerOneDistance = source.DelayPerOneDistance;
        copy.DelayBetweenHits = source.DelayBetweenHits;
        copy.MinimumNumberOfHitsPerTarget = source.MinimumNumberOfHitsPerTarget;
        copy.MaximumNumberOfHitsPerTarget = source.MaximumNumberOfHitsPerTarget;
        copy.MinimumNumberOfHitsPerAttack = source.MinimumNumberOfHitsPerAttack;
        copy.MaximumNumberOfHitsPerAttack = source.MaximumNumberOfHitsPerAttack;
        copy.HitChancePerDistanceMultiplier = source.HitChancePerDistanceMultiplier;
        copy.ProjectileCount = source.ProjectileCount;
        copy.EffectRange = source.EffectRange;
        return copy;
    }

    private static PowerUpDefinitionValue? Copy(IContext context, PowerUpDefinitionValue? source)
    {
        if (source is null)
        {
            return null;
        }

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
