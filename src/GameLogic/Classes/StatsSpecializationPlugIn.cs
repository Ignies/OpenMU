// <copyright file="StatsSpecializationPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Classes;

using System.Runtime.InteropServices;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The stat specialization of Season 21: a stat of a class raises an option (attack power, rates, defense, wizardry or
/// curse damage) by the percent of the section the stat is in. Fork feature id <see cref="FeatureId"/>.
/// </summary>
/// <remarks>
/// The S21Emu data gives the sections and their values but not how they apply. This plugin reads each section as
/// "up to this stat, this percent", the literal reading of the table; it is off by default and every row is the
/// operator's to change in its configuration.
/// </remarks>
[PlugIn]
[Display(Name = nameof(PlugInResources.StatsSpecializationPlugIn_Name), Description = nameof(PlugInResources.StatsSpecializationPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("B3F0A7D2-5C19-4E8B-9A64-1D2E7C3F8B50")]
public class StatsSpecializationPlugIn : IPlayerStateChangedPlugIn, ISupportCustomConfiguration<StatsSpecializationConfiguration>, ISupportDefaultCustomConfiguration, IDisabledByDefault
{
    /// <summary>
    /// The id of the feature in the fork marker.
    /// </summary>
    public const string FeatureId = "stats_specialize";

    /// <inheritdoc />
    public StatsSpecializationConfiguration? Configuration { get; set; }

    /// <summary>
    /// Adds the specialization of the class to the attributes.
    /// </summary>
    /// <param name="attributes">The attributes of the character.</param>
    /// <param name="baseClass">The base class of the character.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The added elements.</returns>
    public static IReadOnlyList<IElement> Apply(IAttributeSystem attributes, BaseClassCode baseClass, StatsSpecializationConfiguration configuration)
    {
        var elements = new List<IElement>();
        foreach (var specialization in configuration.Specializations.Where(s => s.BaseClass == baseClass))
        {
            var element = new SpecializationElement(attributes.GetOrCreateAttribute(GetStat(specialization.Stat)), specialization);
            foreach (var target in GetTargets(specialization.Option))
            {
                attributes.AddElement(element, target);
            }

            elements.Add(element);
        }

        return elements;
    }

    /// <inheritdoc />
    public object CreateDefaultConfig() => StatsSpecializationConfiguration.CreateDefault();

    /// <inheritdoc />
    public ValueTask PlayerStateChangedAsync(Player player, State previousState, State currentState)
    {
        if (previousState == PlayerState.CharacterSelection
            && currentState == PlayerState.EnteredWorld
            && player.Attributes is { } attributes
            && player.SelectedCharacter?.CharacterClass is { } characterClass
            && ClassCodes.GetBaseClass(characterClass.Number) is { } baseClass)
        {
            Apply(attributes, baseClass, this.Configuration ??= StatsSpecializationConfiguration.CreateDefault());
        }

        return ValueTask.CompletedTask;
    }

    private static AttributeDefinition GetStat(StatsSpecializationStat stat) => stat switch
    {
        StatsSpecializationStat.Strength => Stats.TotalStrength,
        StatsSpecializationStat.Agility => Stats.TotalAgility,
        StatsSpecializationStat.Vitality => Stats.TotalVitality,
        StatsSpecializationStat.Energy => Stats.TotalEnergy,
        _ => Stats.TotalLeadership,
    };

    private static AttributeDefinition[] GetTargets(StatsSpecializationOption option) => option switch
    {
        StatsSpecializationOption.AttackPower => [Stats.MinimumPhysBaseDmg, Stats.MaximumPhysBaseDmg],
        StatsSpecializationOption.AttackRate => [Stats.AttackRatePvm],
        StatsSpecializationOption.AttackRatePvp => [Stats.AttackRatePvp],
        StatsSpecializationOption.Defense => [Stats.DefenseBase],
        StatsSpecializationOption.DefenseRate => [Stats.DefenseRatePvm],
        StatsSpecializationOption.DefenseRatePvp => [Stats.DefenseRatePvp],
        StatsSpecializationOption.WizardryDamage => [Stats.MinimumWizBaseDmg, Stats.MaximumWizBaseDmg],
        StatsSpecializationOption.CurseDamage => [Stats.MinimumCurseBaseDmg, Stats.MaximumCurseBaseDmg],
        _ => [],
    };

    /// <summary>
    /// The factor of a specialization (1.1 for +10 %), which follows its stat.
    /// </summary>
    private sealed class SpecializationElement : SimpleElement
    {
        private readonly IElement _stat;
        private readonly StatsSpecialization _specialization;

        public SpecializationElement(IElement stat, StatsSpecialization specialization)
            : base(1, AggregateType.Multiplicate)
        {
            this._stat = stat;
            this._specialization = specialization;
            this._stat.ValueChanged += this.OnStatChanged;
            this.Update();
        }

        private void OnStatChanged(object? sender, EventArgs e) => this.Update();

        private void Update() => this.Value = 1 + (this._specialization.GetPercent(this._stat.Value) / 100);
    }
}
