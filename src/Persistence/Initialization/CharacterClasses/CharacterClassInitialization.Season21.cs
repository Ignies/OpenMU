// <copyright file="CharacterClassInitialization.Season21.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.CharacterClasses;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.Persistence.Initialization.Properties;

/// <summary>
/// The classes of Season 21: the fourth and fifth evolution of the Season 6 classes and the nine new classes with all
/// their evolutions, numbered by <see cref="ClassCodes"/>.
/// </summary>
internal partial class CharacterClassInitialization
{
    private static readonly Guid SkillRateAttributeId = new("6E1B5C2A-8F43-4D7E-9A21-3C5D7B9E0F14");

    private static readonly HashSet<AttributeDefinition?> FormulaInputs =
    [
        Stats.TotalStrength, Stats.TotalAgility, Stats.TotalVitality, Stats.TotalEnergy, Stats.TotalLevel, Stats.TotalStrengthAndAgility,
    ];

    private static readonly HashSet<AttributeDefinition?> FormulaTargets =
    [
        Stats.MinimumPhysBaseDmg, Stats.MaximumPhysBaseDmg, Stats.MinimumWizBaseDmg, Stats.MaximumWizBaseDmg, Stats.DefenseBase,
        Stats.DefenseRatePvm, Stats.DefenseRatePvp, Stats.AttackRatePvm, Stats.AttackRatePvp, Stats.AttackSpeed, Stats.MagicSpeed,
        Stats.MaximumAbility, Stats.SkillMultiplier,
    ];

    /// <summary>
    /// Adds the classes of Season 21 which the configuration doesn't have yet. Existing classes are kept as they are; a
    /// missing link of an evolution chain is set, an existing one is kept.
    /// </summary>
    /// <remarks>
    /// A new fourth or fifth evolution of a Season 6 class may use every item and skill its previous evolution may use.
    /// </remarks>
    public void InitializeSeason21()
    {
        for (var baseClass = BaseClassCode.DarkWizard; baseClass <= BaseClassCode.RageFighter; baseClass++)
        {
            this.AddSeason6Evolutions(baseClass);
        }

        foreach (var info in Season21ClassData.Classes)
        {
            this.AddSeason21Class(info);
        }
    }

    /// <summary>
    /// Replaces the stat formulas of a class of Season 21 with those of <see cref="Season21ClassFormulas"/>: the formula
    /// relationships of the total stats and the skill multiplier. Health, mana, shield and the archetype attributes stay.
    /// </summary>
    /// <param name="characterClass">The class of Season 21.</param>
    public void ReplaceSeason21Formulas(CharacterClass characterClass)
    {
        if (ClassCodes.GetBaseClass(characterClass.Number) is not { } baseClass
            || !Season21ClassFormulas.Classes.TryGetValue(baseClass, out var formulas))
        {
            return;
        }

        var skillRate = this.GetSkillRateAttribute();
        foreach (var relationship in characterClass.AttributeCombinations.Where(r => IsFormula(r, skillRate)).ToList())
        {
            characterClass.AttributeCombinations.Remove(relationship);
        }

        foreach (var value in characterClass.BaseAttributeValues.Where(v => v.Definition == Stats.SkillMultiplier || v.Definition == skillRate).ToList())
        {
            characterClass.BaseAttributeValues.Remove(value);
        }

        this.AddSeason21Formulas(characterClass, formulas);
    }

    private static LocalizedString GetClassName(byte number)
        => CharacterClassNames.ResourceManager.GetLocalizedString(((CharacterClassNumber)number).ToString());

    private static byte GetNumber(BaseClassCode baseClass, ClassEvolution evolution)
        => ClassCodes.FromCode(ClassCodes.GetCode(baseClass, evolution)) ?? throw new ArgumentException($"{baseClass} has no {evolution} evolution.");

    private CharacterClass? FindClass(byte number) => this.GameConfiguration.CharacterClasses.FirstOrDefault(c => c.Number == number);

    private void AddSeason6Evolutions(BaseClassCode baseClass)
    {
        if (this.FindClass(GetNumber(baseClass, ClassEvolution.Third)) is not { } third)
        {
            return;
        }

        var fourth = this.FindClass(GetNumber(baseClass, ClassEvolution.Fourth));
        if (fourth is null)
        {
            fourth = this.CreateSeason6Evolution(baseClass, ClassEvolution.Fourth);
            fourth.IntroducedIn = Season21ClassData.FourthEvolutionIntroducedIn;
            this.QualifyLike(third, fourth);
        }

        var fifth = this.FindClass(GetNumber(baseClass, ClassEvolution.Fifth));
        if (fifth is null)
        {
            fifth = this.CreateSeason6Evolution(baseClass, ClassEvolution.Fifth);
            fifth.IntroducedIn = Season21ClassData.FifthEvolutionIntroducedIn;
            this.QualifyLike(fourth, fifth);
        }

        fourth.NextGenerationClass ??= fifth;
        third.NextGenerationClass ??= fourth;
    }

    private void QualifyLike(CharacterClass previous, CharacterClass added)
    {
        var qualifications = this.GameConfiguration.Items.Select(i => i.QualifiedCharacters)
            .Concat(this.GameConfiguration.Skills.Select(s => s.QualifiedCharacters));
        foreach (var qualified in qualifications.Where(q => q.Contains(previous) && !q.Contains(added)))
        {
            qualified.Add(added);
        }
    }

    private CharacterClass CreateSeason6Evolution(BaseClassCode baseClass, ClassEvolution evolution)
    {
        Func<CharacterClassNumber, LocalizedString, bool, CharacterClass?, bool, CharacterClass> create = baseClass switch
        {
            BaseClassCode.DarkWizard => this.CreateDarkWizard,
            BaseClassCode.DarkKnight => this.CreateDarkKnight,
            BaseClassCode.FairyElf => this.CreateFairyElf,
            BaseClassCode.MagicGladiator => this.CreateMagicGladiator,
            BaseClassCode.DarkLord => this.CreateDarkLord,
            BaseClassCode.Summoner => this.CreateSummoner,
            _ => this.CreateRageFighter,
        };
        var number = GetNumber(baseClass, evolution);
        var result = create((CharacterClassNumber)number, GetClassName(number), true, null, false);
        result.StatAttributes.Add(this.CreateStatAttributeDefinition(Stats.MasterLevel, 0, false));
        return result;
    }

    private void AddSeason21Class(Season21ClassInfo info)
    {
        CharacterClass? next = null;
        for (var index = (int)ClassEvolution.Fifth; index >= (int)ClassEvolution.First; index--)
        {
            var evolution = (ClassEvolution)index;
            if (evolution == ClassEvolution.Second && !ClassCodes.HasSecondEvolution(info.BaseClass))
            {
                continue;
            }

            var characterClass = this.FindClass(GetNumber(info.BaseClass, evolution)) ?? this.CreateSeason21Class(info, evolution);
            characterClass.NextGenerationClass ??= next;
            next = characterClass;
        }
    }

    private CharacterClass CreateSeason21Class(Season21ClassInfo info, ClassEvolution evolution)
    {
        var number = GetNumber(info.BaseClass, evolution);
        var isMaster = evolution >= ClassEvolution.Third;
        var result = this.Context.CreateNew<CharacterClass>();
        result.SetGuid(number);
        this.GameConfiguration.CharacterClasses.Add(result);
        result.Number = number;
        result.Name = GetClassName(number);
        result.CanGetCreated = evolution == ClassEvolution.First;
        result.HomeMap = this.GameConfiguration.Maps.FirstOrDefault(map => map.Number == LorenciaMapId);
        result.IsMasterClass = isMaster;
        result.IntroducedIn = evolution switch
        {
            ClassEvolution.Fifth => Season21ClassData.FifthEvolutionIntroducedIn,
            ClassEvolution.Fourth when info.IntroducedIn < Season21ClassData.FourthEvolutionIntroducedIn => Season21ClassData.FourthEvolutionIntroducedIn,
            _ => info.IntroducedIn,
        };

        result.StatAttributes.Add(this.CreateStatAttributeDefinition(Stats.Level, 1, false));
        result.StatAttributes.Add(this.CreateStatAttributeDefinition(Stats.PointsPerLevelUp, info.PointsPerLevel, false));
        result.StatAttributes.Add(this.CreateStatAttributeDefinition(Stats.BaseStrength, info.Strength, true));
        result.StatAttributes.Add(this.CreateStatAttributeDefinition(Stats.BaseAgility, info.Agility, true));
        result.StatAttributes.Add(this.CreateStatAttributeDefinition(Stats.BaseVitality, info.Vitality, true));
        result.StatAttributes.Add(this.CreateStatAttributeDefinition(Stats.BaseEnergy, info.Energy, true));
        result.StatAttributes.Add(this.CreateStatAttributeDefinition(Stats.CurrentHealth, info.MaxLife, false));
        result.StatAttributes.Add(this.CreateStatAttributeDefinition(Stats.CurrentMana, info.MaxMana, false));
        result.StatAttributes.Add(this.CreateStatAttributeDefinition(Stats.CurrentAbility, 1, false));
        result.StatAttributes.Add(this.CreateStatAttributeDefinition(Stats.IsInSafezone, 1, false));
        result.StatAttributes.Add(this.CreateStatAttributeDefinition(Stats.IsResting, 0, false));
        result.StatAttributes.Add(this.CreateStatAttributeDefinition(Stats.Resets, 0, false));
        result.StatAttributes.Add(this.CreateStatAttributeDefinition(Stats.NearbyPartyMemberCount, 0, false));
        if (isMaster)
        {
            result.StatAttributes.Add(this.CreateStatAttributeDefinition(Stats.MasterLevel, 0, false));
        }

        this.AddCommonAttributeRelationships(result.AttributeCombinations);
        this.AddSeason21HealthAndMana(result, info);
        this.AddSeason21Formulas(result, Season21ClassFormulas.Classes[info.BaseClass]);
        this.AddArchetypeAttributes(result, info.Archetype);

        this.AddShield(result);
        result.BaseAttributeValues.Add(this.CreateConstValueAttribute(MovementSpeedConstants.RunningGearMovementSpeed, Stats.MovementSpeed, AggregateType.Maximum));
        result.BaseAttributeValues.Add(this.CreateConstValueAttribute(MovementSpeedConstants.RunningGearMovementSpeed, Stats.MovementSpeedUnderwater, AggregateType.Maximum));
        this.AddCommonBaseAttributeValues(result.BaseAttributeValues, isMaster);
        return result;
    }

    /// <summary>
    /// Adds health and mana as the S21Emu data defines them: <c>MaxLife + LevelLife * (level - 1) + VitalityToLife *
    /// (vitality - base vitality)</c>, the same for mana with energy. So a new character has exactly MaxLife and MaxMana.
    /// </summary>
    private void AddSeason21HealthAndMana(CharacterClass result, Season21ClassInfo info)
    {
        result.AttributeCombinations.Add(this.CreateAttributeRelationship(Stats.MaximumHealth, info.LevelLife, Stats.TotalLevel));
        result.AttributeCombinations.Add(this.CreateAttributeRelationship(Stats.MaximumHealth, info.VitalityToLife, Stats.TotalVitality));
        result.AttributeCombinations.Add(this.CreateAttributeRelationship(Stats.MaximumMana, info.LevelMana, Stats.TotalLevel));
        result.AttributeCombinations.Add(this.CreateAttributeRelationship(Stats.MaximumMana, info.EnergyToMana, Stats.TotalEnergy));
        result.BaseAttributeValues.Add(this.CreateConstValueAttribute(info.MaxLife - info.LevelLife - (info.VitalityToLife * info.Vitality), Stats.MaximumHealth));
        result.BaseAttributeValues.Add(this.CreateConstValueAttribute(info.MaxMana - info.LevelMana - (info.EnergyToMana * info.Energy), Stats.MaximumMana));
    }

    private static bool IsFormula(AttributeRelationship relationship, AttributeDefinition skillRate)
    {
        if (relationship.TargetAttribute == Stats.TotalStrengthAndAgility || relationship.TargetAttribute == skillRate)
        {
            return true;
        }

        return FormulaTargets.Contains(relationship.TargetAttribute)
            && (FormulaInputs.Contains(relationship.InputAttribute) || relationship.InputAttribute == skillRate);
    }

    private void AddSeason21Formulas(CharacterClass result, Season21ClassFormulas formulas)
    {
        this.AddFactors(result, Stats.MinimumPhysBaseDmg, formulas.MinimumPhysical);
        this.AddFactors(result, Stats.MaximumPhysBaseDmg, formulas.MaximumPhysical);
        this.AddFactors(result, Stats.MinimumWizBaseDmg, formulas.MinimumWizardry);
        this.AddFactors(result, Stats.MaximumWizBaseDmg, formulas.MaximumWizardry);
        this.AddFactors(result, Stats.DefenseBase, formulas.Defense);
        this.AddFactors(result, Stats.DefenseRatePvm, formulas.DefenseRate);
        this.AddFactors(result, Stats.AttackRatePvm, formulas.AttackRate);
        this.AddFactors(result, Stats.AttackSpeed, formulas.AttackSpeed);
        this.AddFactors(result, Stats.MagicSpeed, formulas.MagicSpeed);
        this.AddFactors(result, Stats.MaximumAbility, formulas.Ability);
        if (!this.UseClassicPvp)
        {
            this.AddFactors(result, Stats.DefenseRatePvp, formulas.DefenseRatePvp);
            this.AddFactors(result, Stats.AttackRatePvp, formulas.AttackRatePvp);
        }

        if (formulas.SkillMultiplier with { Constant = 0 } == default)
        {
            this.AddFactors(result, Stats.SkillMultiplier, formulas.SkillMultiplier);
            return;
        }

        var skillRate = this.GetSkillRateAttribute();
        this.AddFactors(result, skillRate, formulas.SkillMultiplier);
        result.AttributeCombinations.Add(this.CreateAttributeRelationship(Stats.SkillMultiplier, Season21ClassFormulas.MaximumSkillMultiplier, skillRate, InputOperator.Minimum));
    }

    private void AddFactors(CharacterClass result, AttributeDefinition target, Season21StatFactors factors)
    {
        Add(Stats.TotalStrength, factors.Strength);
        Add(Stats.TotalAgility, factors.Agility);
        Add(Stats.TotalVitality, factors.Vitality);
        Add(Stats.TotalEnergy, factors.Energy);
        Add(Stats.TotalLevel, factors.Level);
        if (factors.Constant != 0)
        {
            result.BaseAttributeValues.Add(this.CreateConstValueAttribute(factors.Constant, target));
        }

        void Add(AttributeDefinition input, float factor)
        {
            if (factor != 0)
            {
                result.AttributeCombinations.Add(this.CreateAttributeRelationship(target, factor, input));
            }
        }
    }

    private AttributeDefinition GetSkillRateAttribute()
    {
        if (this.GameConfiguration.Attributes.FirstOrDefault(a => a.Id == SkillRateAttributeId) is { } existing)
        {
            return existing;
        }

        var skillRate = this.Context.CreateNew<AttributeDefinition>(SkillRateAttributeId, "Season 21 skill damage rate", "The skill damage multiplier of a Season 21 class before its maximum.");
        this.GameConfiguration.Attributes.Add(skillRate);
        return skillRate;
    }

    /// <summary>
    /// Adds what the S21Emu calculation doesn't define, from the Season 6 class of the same kind: the fenrir damage, the
    /// ability recovery and, for a caster, the wizardry bonuses of items and buffs.
    /// </summary>
    private void AddArchetypeAttributes(CharacterClass result, Season21ClassArchetype archetype)
    {
        var combinations = result.AttributeCombinations;
        switch (archetype)
        {
            case Season21ClassArchetype.Knight:
                this.AddFenrirDamage(combinations, 1.0f / 3, 1.0f / 5, 1.0f / 5, 1.0f / 7);
                result.BaseAttributeValues.Add(this.CreateConstValueAttribute(0.05f, Stats.AbilityRecoveryMultiplier));
                break;
            case Season21ClassArchetype.Wizard:
                combinations.Add(this.CreateAttributeRelationship(Stats.MinimumWizBaseDmg, 1, Stats.BaseMinDamageBonus));
                combinations.Add(this.CreateAttributeRelationship(Stats.MaximumWizBaseDmg, 1, Stats.BaseMaxDamageBonus));
                combinations.Add(this.CreateAttributeRelationship(Stats.WizardryBaseDmg, 1, Stats.BaseDamageBonus));
                combinations.Add(this.CreateAttributeRelationship(Stats.MinimumWizBaseDmg, 1, Stats.WizardryBaseDmg));
                combinations.Add(this.CreateAttributeRelationship(Stats.MaximumWizBaseDmg, 1, Stats.WizardryBaseDmg));
                combinations.Add(this.CreateAttributeRelationship(Stats.MinimumWizBaseDmg, 1, Stats.WizardryBaseDmgIncrease, aggregateType: AggregateType.Multiplicate));
                combinations.Add(this.CreateAttributeRelationship(Stats.MaximumWizBaseDmg, 1, Stats.WizardryBaseDmgIncrease, aggregateType: AggregateType.Multiplicate));
                combinations.Add(this.CreateAttributeRelationship(Stats.WizardryAttackDamageIncrease, 1.0f / 100, Stats.StaffRise));
                this.AddFenrirDamage(combinations, 1.0f / 5, 1.0f / 5, 1.0f / 7, 1.0f / 3);
                result.BaseAttributeValues.Add(this.CreateConstValueAttribute(1.0f, Stats.WizardryAttackDamageIncrease));
                result.BaseAttributeValues.Add(this.CreateConstValueAttribute(0.03f, Stats.AbilityRecoveryMultiplier));
                result.BaseAttributeValues.Add(this.CreateConstValueAttribute(1, Stats.WizardryBaseDmgIncrease));
                break;
            default:
                this.AddFenrirDamage(combinations, 1.0f / 5, 1.0f / 3, 1.0f / 7, 1.0f / 5);
                result.BaseAttributeValues.Add(this.CreateConstValueAttribute(0.03f, Stats.AbilityRecoveryMultiplier));
                break;
        }
    }

    private void AddFenrirDamage(ICollection<AttributeRelationship> combinations, float strength, float agility, float vitality, float energy)
    {
        combinations.Add(this.CreateAttributeRelationship(Stats.FenrirBaseDmg, strength, Stats.TotalStrength));
        combinations.Add(this.CreateAttributeRelationship(Stats.FenrirBaseDmg, agility, Stats.TotalAgility));
        combinations.Add(this.CreateAttributeRelationship(Stats.FenrirBaseDmg, vitality, Stats.TotalVitality));
        combinations.Add(this.CreateAttributeRelationship(Stats.FenrirBaseDmg, energy, Stats.TotalEnergy));
    }

    private void AddShield(CharacterClass result)
    {
        if (this.UseClassicPvp)
        {
            return;
        }

        result.StatAttributes.Add(this.CreateStatAttributeDefinition(Stats.CurrentShield, 1, false));
        result.AttributeCombinations.Add(this.CreateAttributeRelationship(Stats.MaximumShield, 1.2f, Stats.TotalEnergy));
        result.AttributeCombinations.Add(this.CreateAttributeRelationship(Stats.MaximumShield, 1.2f, Stats.TotalVitality));
        result.AttributeCombinations.Add(this.CreateAttributeRelationship(Stats.MaximumShield, 1.2f, Stats.TotalAgility));
        result.AttributeCombinations.Add(this.CreateAttributeRelationship(Stats.MaximumShield, 1.2f, Stats.TotalStrength));
        result.AttributeCombinations.Add(this.CreateAttributeRelationship(Stats.MaximumShield, 1f, Stats.DefenseFinal));
        result.AttributeCombinations.Add(this.CreateAttributeRelationship(Stats.MaximumShieldTemp, 2f, Stats.TotalLevel, InputOperator.Exponentiate));
        result.AttributeCombinations.Add(this.CreateAttributeRelationship(Stats.MaximumShield, 1f / 30f, Stats.MaximumShieldTemp));
    }
}
