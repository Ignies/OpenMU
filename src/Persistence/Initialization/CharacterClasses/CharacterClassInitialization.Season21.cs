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
        switch (info.Archetype)
        {
            case Season21ClassArchetype.Knight:
                this.AddKnightFormulas(result);
                break;
            case Season21ClassArchetype.Wizard:
                this.AddWizardFormulas(result);
                break;
            default:
                this.AddAgilityFormulas(result);
                break;
        }

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

    /// <summary>
    /// Adds the formulas of the dark knight (<see cref="CreateDarkKnight"/>) without its combo and weapon masteries.
    /// </summary>
    private void AddKnightFormulas(CharacterClass result)
    {
        var combinations = result.AttributeCombinations;
        combinations.Add(this.CreateAttributeRelationship(Stats.DefenseBase, 1.0f / 3, Stats.TotalAgility));
        combinations.Add(this.CreateAttributeRelationship(Stats.DefenseRatePvm, 1.0f / 3, Stats.TotalAgility));
        this.AddAttackRatePvm(combinations);
        combinations.Add(this.CreateAttributeRelationship(Stats.AttackSpeed, 1.0f / 15, Stats.TotalAgility));
        combinations.Add(this.CreateAttributeRelationship(Stats.MagicSpeed, 1.0f / 20, Stats.TotalAgility));
        this.AddMaximumAbility(combinations, 1, 0.3f, 0.2f, 0.15f);
        combinations.Add(this.CreateAttributeRelationship(Stats.MinimumPhysBaseDmg, 1.0f / 6, Stats.TotalStrength));
        combinations.Add(this.CreateAttributeRelationship(Stats.MaximumPhysBaseDmg, 1.0f / 4, Stats.TotalStrength));
        combinations.Add(this.CreateAttributeRelationship(Stats.SkillMultiplier, 0.001f, Stats.TotalEnergy));
        this.AddFenrirDamage(combinations, 1.0f / 3, 1.0f / 5, 1.0f / 5, 1.0f / 7);
        this.AddPvpRates(combinations, 0.5f, 4.5f);
        result.BaseAttributeValues.Add(this.CreateConstValueAttribute(2, Stats.SkillMultiplier));
        result.BaseAttributeValues.Add(this.CreateConstValueAttribute(0.05f, Stats.AbilityRecoveryMultiplier));
    }

    /// <summary>
    /// Adds the formulas of the dark wizard (<see cref="CreateDarkWizard"/>) without its staff bonuses.
    /// </summary>
    private void AddWizardFormulas(CharacterClass result)
    {
        var combinations = result.AttributeCombinations;
        combinations.Add(this.CreateAttributeRelationship(Stats.DefenseBase, 0.25f, Stats.TotalAgility));
        combinations.Add(this.CreateAttributeRelationship(Stats.DefenseRatePvm, 1.0f / 3, Stats.TotalAgility));
        this.AddAttackRatePvm(combinations);
        combinations.Add(this.CreateAttributeRelationship(Stats.AttackSpeed, 1.0f / 20, Stats.TotalAgility));
        combinations.Add(this.CreateAttributeRelationship(Stats.MagicSpeed, 1.0f / 10, Stats.TotalAgility));
        this.AddMaximumAbility(combinations, 0.2f, 0.3f, 0.4f, 0.2f);
        combinations.Add(this.CreateAttributeRelationship(Stats.MinimumPhysBaseDmg, 1.0f / 8, Stats.TotalStrength));
        combinations.Add(this.CreateAttributeRelationship(Stats.MaximumPhysBaseDmg, 1.0f / 4, Stats.TotalStrength));
        combinations.Add(this.CreateAttributeRelationship(Stats.MinimumWizBaseDmg, 1.0f / 9, Stats.TotalEnergy));
        combinations.Add(this.CreateAttributeRelationship(Stats.MaximumWizBaseDmg, 1.0f / 4, Stats.TotalEnergy));
        combinations.Add(this.CreateAttributeRelationship(Stats.MinimumWizBaseDmg, 1, Stats.BaseMinDamageBonus));
        combinations.Add(this.CreateAttributeRelationship(Stats.MaximumWizBaseDmg, 1, Stats.BaseMaxDamageBonus));
        combinations.Add(this.CreateAttributeRelationship(Stats.WizardryBaseDmg, 1, Stats.BaseDamageBonus));
        combinations.Add(this.CreateAttributeRelationship(Stats.MinimumWizBaseDmg, 1, Stats.WizardryBaseDmg));
        combinations.Add(this.CreateAttributeRelationship(Stats.MaximumWizBaseDmg, 1, Stats.WizardryBaseDmg));
        combinations.Add(this.CreateAttributeRelationship(Stats.MinimumWizBaseDmg, 1, Stats.WizardryBaseDmgIncrease, aggregateType: AggregateType.Multiplicate));
        combinations.Add(this.CreateAttributeRelationship(Stats.MaximumWizBaseDmg, 1, Stats.WizardryBaseDmgIncrease, aggregateType: AggregateType.Multiplicate));
        combinations.Add(this.CreateAttributeRelationship(Stats.WizardryAttackDamageIncrease, 1.0f / 100, Stats.StaffRise));
        this.AddFenrirDamage(combinations, 1.0f / 5, 1.0f / 5, 1.0f / 7, 1.0f / 3);
        this.AddPvpRates(combinations, 0.25f, 4);
        result.BaseAttributeValues.Add(this.CreateConstValueAttribute(1, Stats.SkillMultiplier));
        result.BaseAttributeValues.Add(this.CreateConstValueAttribute(1.0f, Stats.WizardryAttackDamageIncrease));
        result.BaseAttributeValues.Add(this.CreateConstValueAttribute(0.03f, Stats.AbilityRecoveryMultiplier));
        result.BaseAttributeValues.Add(this.CreateConstValueAttribute(1, Stats.WizardryBaseDmgIncrease));
    }

    /// <summary>
    /// Adds the melee formulas of the fairy elf (<see cref="CreateFairyElf"/>): strength and agility drive the damage.
    /// </summary>
    private void AddAgilityFormulas(CharacterClass result)
    {
        var combinations = result.AttributeCombinations;
        combinations.Add(this.CreateAttributeRelationship(Stats.TotalStrengthAndAgility, Stats.TotalAgility, Stats.TotalStrength, InputOperator.Add));
        combinations.Add(this.CreateAttributeRelationship(Stats.DefenseBase, 1.0f / 10, Stats.TotalAgility));
        combinations.Add(this.CreateAttributeRelationship(Stats.DefenseRatePvm, 0.25f, Stats.TotalAgility));
        this.AddAttackRatePvm(combinations);
        combinations.Add(this.CreateAttributeRelationship(Stats.AttackSpeed, 1.0f / 50, Stats.TotalAgility));
        combinations.Add(this.CreateAttributeRelationship(Stats.MagicSpeed, 1.0f / 50, Stats.TotalAgility));
        this.AddMaximumAbility(combinations, 0.2f, 0.3f, 0.2f, 0.3f);
        combinations.Add(this.CreateAttributeRelationship(Stats.MinimumPhysBaseDmg, 1.0f / 7, Stats.TotalStrengthAndAgility));
        combinations.Add(this.CreateAttributeRelationship(Stats.MaximumPhysBaseDmg, 1.0f / 4, Stats.TotalStrengthAndAgility));
        this.AddFenrirDamage(combinations, 1.0f / 5, 1.0f / 3, 1.0f / 7, 1.0f / 5);
        this.AddPvpRates(combinations, 0.1f, 0.6f);
        result.BaseAttributeValues.Add(this.CreateConstValueAttribute(1, Stats.SkillMultiplier));
        result.BaseAttributeValues.Add(this.CreateConstValueAttribute(0.03f, Stats.AbilityRecoveryMultiplier));
    }

    private void AddAttackRatePvm(ICollection<AttributeRelationship> combinations)
    {
        combinations.Add(this.CreateAttributeRelationship(Stats.AttackRatePvm, 5, Stats.TotalLevel));
        combinations.Add(this.CreateAttributeRelationship(Stats.AttackRatePvm, 1.5f, Stats.TotalAgility));
        combinations.Add(this.CreateAttributeRelationship(Stats.AttackRatePvm, 0.25f, Stats.TotalStrength));
    }

    private void AddMaximumAbility(ICollection<AttributeRelationship> combinations, float energy, float vitality, float agility, float strength)
    {
        combinations.Add(this.CreateAttributeRelationship(Stats.MaximumAbility, energy, Stats.TotalEnergy));
        combinations.Add(this.CreateAttributeRelationship(Stats.MaximumAbility, vitality, Stats.TotalVitality));
        combinations.Add(this.CreateAttributeRelationship(Stats.MaximumAbility, agility, Stats.TotalAgility));
        combinations.Add(this.CreateAttributeRelationship(Stats.MaximumAbility, strength, Stats.TotalStrength));
    }

    private void AddFenrirDamage(ICollection<AttributeRelationship> combinations, float strength, float agility, float vitality, float energy)
    {
        combinations.Add(this.CreateAttributeRelationship(Stats.FenrirBaseDmg, strength, Stats.TotalStrength));
        combinations.Add(this.CreateAttributeRelationship(Stats.FenrirBaseDmg, agility, Stats.TotalAgility));
        combinations.Add(this.CreateAttributeRelationship(Stats.FenrirBaseDmg, vitality, Stats.TotalVitality));
        combinations.Add(this.CreateAttributeRelationship(Stats.FenrirBaseDmg, energy, Stats.TotalEnergy));
    }

    private void AddPvpRates(ICollection<AttributeRelationship> combinations, float defenseRateByAgility, float attackRateByAgility)
    {
        if (this.UseClassicPvp)
        {
            return;
        }

        combinations.Add(this.CreateAttributeRelationship(Stats.DefenseRatePvp, defenseRateByAgility, Stats.TotalAgility));
        combinations.Add(this.CreateAttributeRelationship(Stats.DefenseRatePvp, 2, Stats.TotalLevel));
        combinations.Add(this.CreateAttributeRelationship(Stats.AttackRatePvp, attackRateByAgility, Stats.TotalAgility));
        combinations.Add(this.CreateAttributeRelationship(Stats.AttackRatePvp, 3, Stats.TotalLevel));
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
