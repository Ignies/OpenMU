// <copyright file="Season21NewPowerInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using System.Text.Json;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.AbilityCore;
using MUnique.OpenMU.GameLogic.EvolutionQuests;
using MUnique.OpenMU.GameLogic.NewPower;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.Persistence.Initialization.Skills;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the fifth class New Power where the Ability Enhancement Core is installed: the skills 2086-2088 of the extra
/// damage cards with their effects 421-423, the effects of the nuke, bolt and wide card options, and the one minute fifth
/// class trial steps with their power. What exists stays; a world without the core's cards is left as it is.
/// </summary>
internal sealed class Season21NewPowerInitializer : SkillsInitializerBase
{
    /// <summary>
    /// The time of the fifth class trial steps before this update.
    /// </summary>
    private static readonly TimeSpan OldFifthTimeLimit = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21NewPowerInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Season21NewPowerInitializer(IContext context, GameConfiguration gameConfiguration)
        : base(context, gameConfiguration)
    {
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        var kinds = new NewPowerConfiguration().Kinds;
        var cards = Season21AbilityCoreData.Data.Cards.Where(c => Season21AbilityCoreInitializer.NewPowerSkillOf(c) > 0).ToList();
        if (!this.GameConfiguration.Items.Any(i => cards.Any(c => c.Group == i.Group && c.Number == i.Number)))
        {
            return;
        }

        new Season21AbilityCoreInitializer(this.Context, this.GameConfiguration).MapCardOptions();
        foreach (var kind in kinds)
        {
            this.AddSkill(kind);
        }

        this.LinkCardSkills();
        this.AddTrialPower(kinds);
    }

    private static T? Read<T>(PlugInConfiguration plugIn)
        where T : class
    {
        try
        {
            return plugIn.GetConfiguration<T>(new IdIgnoringReferenceHandler());
        }
        catch (JsonException)
        {
            // A configuration which can't be read is the operator's; it stays.
            return null;
        }
    }

    private void AddSkill(NewPowerKindSettings kind)
    {
        var effect = this.GameConfiguration.MagicEffects.FirstOrDefault(e => e.Number == kind.Effect);
        if (effect is null)
        {
            var row = Season21SkillCatalog.Data.Effects.FirstOrDefault(e => e.Number == kind.Effect);
            effect = this.Context.CreateNew<MagicEffectDefinition>();
            effect.SetGuid(kind.Effect);
            effect.Number = kind.Effect;
            effect.Name = Season21SkillNames.ResourceManager.GetLocalizedString($"Effect{kind.Effect}");
            effect.SubType = row is { Group: > 0 and <= byte.MaxValue } ? (byte)row.Group : (byte)0;
            effect.InformObservers = true;
            effect.StopByDeath = true;
            this.GameConfiguration.MagicEffects.Add(effect);
        }

        if (this.GameConfiguration.Skills.All(s => s.Number != kind.Skill))
        {
            this.CreateSkill((SkillNumber)kind.Skill, Season21SkillNames.ResourceManager.GetLocalizedString($"Skill{kind.Skill}"), skillType: SkillType.Other);
            this.GameConfiguration.Skills.First(s => s.Number == kind.Skill).MagicEffectDef = effect;
        }

        // The extra damage cards are for every class, and an item skill only goes to the classes it qualifies.
        var skill = this.GameConfiguration.Skills.First(s => s.Number == kind.Skill);
        foreach (var characterClass in this.GameConfiguration.CharacterClasses.Where(c => !skill.QualifiedCharacters.Contains(c)).ToList())
        {
            skill.QualifiedCharacters.Add(characterClass);
        }
    }

    private void LinkCardSkills()
    {
        if (this.GameConfiguration.PlugInConfigurations.FirstOrDefault(c => c.TypeId == typeof(AbilityCoreFeaturePlugIn).GUID) is not { } plugIn
            || Read<AbilityCoreConfiguration>(plugIn) is not { } configuration)
        {
            return;
        }

        var changed = false;
        foreach (var card in configuration.Cards.Where(c => c is { Kind: AbilityCardKind.Extra, Skill: 0 }))
        {
            if (Season21AbilityCoreData.Data.Cards.FirstOrDefault(c => c.Group == card.ItemGroup && c.Number == card.ItemNumber) is { } row
                && Season21AbilityCoreInitializer.NewPowerSkillOf(row) is > 0 and var skill)
            {
                card.Skill = skill;
                changed = true;
            }
        }

        if (changed)
        {
            plugIn.SetConfiguration(configuration, null);
        }
    }

    private void AddTrialPower(IReadOnlyList<NewPowerKindSettings> kinds)
    {
        if (this.GameConfiguration.PlugInConfigurations.FirstOrDefault(c => c.TypeId == typeof(EvolutionQuestFeaturePlugIn).GUID) is not { } plugIn
            || Read<EvolutionQuestConfiguration>(plugIn) is not { } configuration)
        {
            return;
        }

        var defaults = new EvolutionQuestConfiguration().Steps.Where(s => s.PowerEffect > 0).ToList();
        var changed = false;
        foreach (var step in configuration.Steps)
        {
            if (defaults.FirstOrDefault(d => d.QuestGroup == step.QuestGroup && d.QuestNumber == step.QuestNumber) is not { } fifth)
            {
                continue;
            }

            if (step.TimeLimit == OldFifthTimeLimit)
            {
                step.TimeLimit = fifth.TimeLimit;
                changed = true;
            }

            if (step.PowerEffect == 0 && kinds.Any(k => k.Effect == fifth.PowerEffect))
            {
                step.PowerEffect = fifth.PowerEffect;
                changed = true;
            }
        }

        if (changed)
        {
            plugIn.SetConfiguration(configuration, null);
        }
    }
}
