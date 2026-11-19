// <copyright file="FixSeason21ObsidianSoloDurationUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Makes the Grow Lancer's Obsidian last 360 seconds without a party and 120 seconds on a party, see
/// <see cref="Season21GrowLancerBuffs.ApplySoloDuration"/>. Fork feature id <c>s21_skills</c>.
/// </summary>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.FixSeason21ObsidianSoloDurationUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.FixSeason21ObsidianSoloDurationUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("5B58E969-FD76-452C-B5AA-7916E4465B13")]
public class FixSeason21ObsidianSoloDurationUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.FixSeason21ObsidianSoloDurationUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.FixSeason21ObsidianSoloDurationUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 19, 9, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(FixSeason21GrowLancerBuffsUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        Season21GrowLancerBuffs.ApplySoloDuration(context, gameConfiguration);
        return ValueTask.CompletedTask;
    }
}
