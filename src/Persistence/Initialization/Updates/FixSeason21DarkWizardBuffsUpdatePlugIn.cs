// <copyright file="FixSeason21DarkWizardBuffsUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Gives the Dark Wizard's Mana Shield Mastered (406) Mana Shield's effect and makes it cast as Mana Shield,
/// see <see cref="Season21DarkWizardBuffs"/>. Fork feature id <c>s21_skills</c>.
/// </summary>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.FixSeason21DarkWizardBuffsUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.FixSeason21DarkWizardBuffsUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("3E0C9B54-7A21-4F8D-9C6E-52D1B7A4E9F3")]
public class FixSeason21DarkWizardBuffsUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.FixSeason21DarkWizardBuffsUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.FixSeason21DarkWizardBuffsUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 19, 16, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21SkillsUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        Season21DarkWizardBuffs.Apply(context, gameConfiguration);
        return ValueTask.CompletedTask;
    }
}
