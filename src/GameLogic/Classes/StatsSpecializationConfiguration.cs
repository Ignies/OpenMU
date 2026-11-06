// <copyright file="StatsSpecializationConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Classes;

/// <summary>
/// The configuration of the <see cref="StatsSpecializationPlugIn"/>.
/// </summary>
public class StatsSpecializationConfiguration
{
    /// <summary>
    /// Gets or sets the rows of the specialization.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.StatsSpecializationConfiguration_Specializations_Name))]
    public List<StatsSpecialization> Specializations { get; set; } = [];

    /// <summary>
    /// Creates the configuration of the S21Emu data (<c>Character/StatsSpecialize.txt</c>, version S21.1-1).
    /// </summary>
    /// <returns>The configuration.</returns>
    public static StatsSpecializationConfiguration CreateDefault()
    {
        var configuration = new StatsSpecializationConfiguration();
        AddCommon(BaseClassCode.DarkWizard, StatsSpecializationStat.Energy, StatsSpecializationOption.WizardryDamage);
        AddCommon(BaseClassCode.DarkWizard, StatsSpecializationStat.Agility, StatsSpecializationOption.AttackRate);
        AddCommon(BaseClassCode.DarkWizard, StatsSpecializationStat.Agility, StatsSpecializationOption.AttackRatePvp);
        AddCommon(BaseClassCode.DarkKnight, StatsSpecializationStat.Strength, StatsSpecializationOption.AttackPower);
        AddCommon(BaseClassCode.DarkKnight, StatsSpecializationStat.Agility, StatsSpecializationOption.Defense);
        Add(BaseClassCode.FairyElf, StatsSpecializationStat.Agility, StatsSpecializationOption.AttackPower, 1800, 2100);
        Add(BaseClassCode.FairyElf, StatsSpecializationStat.Agility, StatsSpecializationOption.DefenseRate, 1800, 2100);
        Add(BaseClassCode.FairyElf, StatsSpecializationStat.Agility, StatsSpecializationOption.DefenseRatePvp, 1800, 2100);
        AddCommon(BaseClassCode.MagicGladiator, StatsSpecializationStat.Strength, StatsSpecializationOption.AttackPower);
        Add(BaseClassCode.MagicGladiator, StatsSpecializationStat.Energy, StatsSpecializationOption.WizardryDamage, 1800, 2500);
        AddCommon(BaseClassCode.DarkLord, StatsSpecializationStat.Strength, StatsSpecializationOption.AttackPower);
        AddCommon(BaseClassCode.DarkLord, StatsSpecializationStat.Agility, StatsSpecializationOption.Defense);
        AddCommon(BaseClassCode.Summoner, StatsSpecializationStat.Energy, StatsSpecializationOption.WizardryDamage);
        AddCommon(BaseClassCode.Summoner, StatsSpecializationStat.Energy, StatsSpecializationOption.CurseDamage);
        AddCommon(BaseClassCode.RageFighter, StatsSpecializationStat.Strength, StatsSpecializationOption.AttackPower);
        AddCommon(BaseClassCode.GrowLancer, StatsSpecializationStat.Strength, StatsSpecializationOption.AttackPower);
        AddCommon(BaseClassCode.GrowLancer, StatsSpecializationStat.Agility, StatsSpecializationOption.Defense);
        AddCommon(BaseClassCode.RuneWizard, StatsSpecializationStat.Energy, StatsSpecializationOption.WizardryDamage);
        AddCommon(BaseClassCode.RuneWizard, StatsSpecializationStat.Agility, StatsSpecializationOption.AttackRate);
        AddCommon(BaseClassCode.RuneWizard, StatsSpecializationStat.Agility, StatsSpecializationOption.AttackRatePvp);
        AddCommon(BaseClassCode.Slayer, StatsSpecializationStat.Agility, StatsSpecializationOption.AttackPower);
        Add(BaseClassCode.Slayer, StatsSpecializationStat.Agility, StatsSpecializationOption.DefenseRate, 1800, 2500);
        AddCommon(BaseClassCode.GunCrusher, StatsSpecializationStat.Energy, StatsSpecializationOption.WizardryDamage);
        AddCommon(BaseClassCode.GunCrusher, StatsSpecializationStat.Agility, StatsSpecializationOption.AttackRate);
        AddCommon(BaseClassCode.GunCrusher, StatsSpecializationStat.Agility, StatsSpecializationOption.AttackRatePvp);
        AddCommon(BaseClassCode.LightWizard, StatsSpecializationStat.Energy, StatsSpecializationOption.WizardryDamage);
        AddCommon(BaseClassCode.LightWizard, StatsSpecializationStat.Agility, StatsSpecializationOption.AttackRate);
        AddCommon(BaseClassCode.LightWizard, StatsSpecializationStat.Agility, StatsSpecializationOption.AttackRatePvp);
        AddCommon(BaseClassCode.LemuriaMage, StatsSpecializationStat.Energy, StatsSpecializationOption.WizardryDamage);
        AddCommon(BaseClassCode.LemuriaMage, StatsSpecializationStat.Agility, StatsSpecializationOption.DefenseRate);
        AddCommon(BaseClassCode.LemuriaMage, StatsSpecializationStat.Agility, StatsSpecializationOption.DefenseRatePvp);
        AddCommon(BaseClassCode.IllusionKnight, StatsSpecializationStat.Agility, StatsSpecializationOption.AttackPower);
        AddCommon(BaseClassCode.IllusionKnight, StatsSpecializationStat.Agility, StatsSpecializationOption.Defense);
        AddCommon(BaseClassCode.Alchemist, StatsSpecializationStat.Energy, StatsSpecializationOption.WizardryDamage);
        AddCommon(BaseClassCode.Alchemist, StatsSpecializationStat.Agility, StatsSpecializationOption.AttackRate);
        AddCommon(BaseClassCode.Alchemist, StatsSpecializationStat.Agility, StatsSpecializationOption.AttackRatePvp);
        AddCommon(BaseClassCode.Crusader, StatsSpecializationStat.Strength, StatsSpecializationOption.AttackPower);
        AddCommon(BaseClassCode.Crusader, StatsSpecializationStat.Agility, StatsSpecializationOption.AttackRate);
        AddCommon(BaseClassCode.Crusader, StatsSpecializationStat.Agility, StatsSpecializationOption.Defense);
        return configuration;

        void AddCommon(BaseClassCode baseClass, StatsSpecializationStat stat, StatsSpecializationOption option)
            => Add(baseClass, stat, option, 1500, 2000);

        void Add(BaseClassCode baseClass, StatsSpecializationStat stat, StatsSpecializationOption option, int first, int second)
        {
            configuration.Specializations.Add(new StatsSpecialization
            {
                BaseClass = baseClass,
                Stat = stat,
                Option = option,
                Sections =
                [
                    new() { UpToStat = first, Percent = 10 },
                    new() { UpToStat = second, Percent = 30 },
                    new() { UpToStat = ushort.MaxValue, Percent = 45 },
                ],
            });
        }
    }
}
