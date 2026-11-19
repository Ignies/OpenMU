// <copyright file="Season21DarkKnightBuffs.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// The Dark Knight's Season 21 buffs, which the skills update creates without a duration: Greater Life Mastered (363),
/// Sword's Fury (1500; 801, 802), Firm Belief (1502; 810) and Strong Blessing (1503; 803-806, 1204).
/// </summary>
/// <remarks>
/// S21Emu <c>Data/Scripts/LuaCalc/Skill/SkillDamageCalc.lua</c> and <c>GameServerInfo - Skill.dat</c>:
/// <list type="bullet">
/// <item>Greater Life Mastered: effect 136 is Greater Fortitude's tier (Effect.txt group 8, as effect 8); <c>SkillGreaterLife</c>
/// computes one value for every tier, so it gets effect 8's duration and power-ups.</item>
/// <item>Sword's Fury (<c>SkillSwordFury</c>): 60 seconds on the caster (SwordFuryTimeConstA 60); attack skills reach 1 tile
/// further (SwordFuryConstA 1; Webzen's Season 15 guide: "the range of the attack skill ... increases"), read as
/// <see cref="Stats.AttackSkillRangeBonus"/>. SwordFuryConstB 1, the Blow type skills' damage range, waits for a list of
/// which skills are Blow type.</item>
/// <item>Firm Belief (<c>SkillStrongConviction</c>): 120 seconds on the caster (StrongConvictionTimeConstA 120); base defense
/// + 35 (ConstE) and elemental defense + (30 + dexterity / 60) % (ConstC 30, ConstD 60), read as
/// <see cref="Stats.DefenseBase"/> and <see cref="Stats.ElementDefenseIncrease"/>. The shield it needs is not checked.</item>
/// <item>Strong Blessing (<c>SkillSolidProtection</c>): 300 seconds on the party within 10 (MAX_DISTANCE); the party's damage
/// + 250 (SolidProtectionConstA), read as <see cref="Stats.BaseDamageBonus"/>. Taking a share of the party's damage and
/// turning their health into the caster's have no OpenMU mechanism yet (buff_rows.md, open items).</item>
/// </list>
/// All is effect data an operator can change.
/// </remarks>
internal static class Season21DarkKnightBuffs
{
    private const short GreaterFortitude = 8;
    private const float SwordsFurySeconds = 60;
    private const float SwordsFuryRange = 1;
    private const float FirmBeliefSeconds = 120;
    private const float FirmBeliefDefense = 35;
    private const float FirmBeliefElementalDefense = 0.30f;
    private const float DexterityPerElementalPercent = 60;
    private const float StrongBlessingSeconds = 300;
    private const float StrongBlessingDamage = 250;

    /// <summary>The radius of Strong Blessing's party (S21Emu <c>SkillSolidProtection</c> MAX_DISTANCE 10).</summary>
    private const short StrongBlessingRadius = 10;

    private static readonly short[] GreaterLifeEffects = [136];
    private static readonly short[] SwordsFuryEffects = [324, 327, 328];
    private static readonly short[] FirmBeliefEffects = [325, 336];
    private static readonly short[] StrongBlessingEffects = [326, 329, 330, 331, 332, 335];
    private static readonly short[] PartySkills = [363, 1503, 803, 804, 805, 806, 1204];
    private static readonly short[] CasterSkills = [1500, 801, 802, 1502, 810];

    /// <summary>
    /// Gives the effects which have no duration theirs and their value, and makes the skills which are no buff yet buffs:
    /// Sword's Fury and Firm Belief on the caster, Greater Life and Strong Blessing on the party. An operator's own duration
    /// or skill type stays.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public static void Apply(IContext context, GameConfiguration gameConfiguration)
    {
        if (gameConfiguration.Attributes.All(a => a.Id != Stats.AttackSkillRangeBonus.Id))
        {
            gameConfiguration.Attributes.Add(context.CreateNew<AttributeDefinition>(Stats.AttackSkillRangeBonus.Id, Stats.AttackSkillRangeBonus.Designation, Stats.AttackSkillRangeBonus.Description));
        }

        var fortitude = gameConfiguration.MagicEffects.FirstOrDefault(e => e.Number == GreaterFortitude);
        foreach (var effect in gameConfiguration.MagicEffects.Where(e => e.Duration is null))
        {
            if (GreaterLifeEffects.Contains(effect.Number) && fortitude?.Duration is { } fortitudeDuration)
            {
                effect.Duration = Copy(context, fortitudeDuration);
                foreach (var powerUp in fortitude.PowerUpDefinitions)
                {
                    var copy = context.CreateNew<PowerUpDefinition>();
                    copy.TargetAttribute = powerUp.TargetAttribute;
                    copy.Boost = powerUp.Boost is { } boost ? Copy(context, boost) : null;
                    effect.PowerUpDefinitions.Add(copy);
                }
            }
            else if (SwordsFuryEffects.Contains(effect.Number))
            {
                SetDuration(context, effect, SwordsFurySeconds);
                AddPowerUp(context, gameConfiguration, effect, Stats.AttackSkillRangeBonus, SwordsFuryRange);
            }
            else if (FirmBeliefEffects.Contains(effect.Number))
            {
                SetDuration(context, effect, FirmBeliefSeconds);
                AddPowerUp(context, gameConfiguration, effect, Stats.DefenseBase, FirmBeliefDefense);
                var elemental = AddPowerUp(context, gameConfiguration, effect, Stats.ElementDefenseIncrease, FirmBeliefElementalDefense);
                var perDexterity = context.CreateNew<AttributeRelationship>();
                perDexterity.InputAttribute = Stats.TotalAgility.GetPersistent(gameConfiguration);
                perDexterity.InputOperator = InputOperator.Multiply;
                perDexterity.InputOperand = 1f / (DexterityPerElementalPercent * 100);
                elemental.Boost!.RelatedValues.Add(perDexterity);
            }
            else if (StrongBlessingEffects.Contains(effect.Number))
            {
                SetDuration(context, effect, StrongBlessingSeconds);
                AddPowerUp(context, gameConfiguration, effect, Stats.BaseDamageBonus, StrongBlessingDamage);
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
            else if (PartySkills.Contains(skill.Number))
            {
                skill.SkillType = SkillType.Buff;
                skill.Target = SkillTarget.ImplicitParty;
                skill.TargetRestriction = SkillTargetRestriction.Party;
                if (skill.Number != 363)
                {
                    skill.ImplicitTargetRange = StrongBlessingRadius;
                }
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

    private static PowerUpDefinition AddPowerUp(IContext context, GameConfiguration gameConfiguration, MagicEffectDefinition effect, AttributeDefinition attribute, float value)
    {
        var powerUp = context.CreateNew<PowerUpDefinition>();
        powerUp.TargetAttribute = attribute.GetPersistent(gameConfiguration);
        powerUp.Boost = context.CreateNew<PowerUpDefinitionValue>();
        powerUp.Boost.ConstantValue.Value = value;
        effect.PowerUpDefinitions.Add(powerUp);
        return powerUp;
    }
}
