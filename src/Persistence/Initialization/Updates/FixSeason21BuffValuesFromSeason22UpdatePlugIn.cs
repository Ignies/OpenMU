// <copyright file="FixSeason21BuffValuesFromSeason22UpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Gives Evasion and the Dark Lord's Divine Force and Divine Aura the values a Season 22 client text names,
/// see <see cref="Season21BuffValuesFromSeason22"/>. Fork feature id <c>s21_skills</c>.
/// </summary>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.FixSeason21BuffValuesFromSeason22UpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.FixSeason21BuffValuesFromSeason22UpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("E2B9D04C-6F13-4A87-B5D2-31C8F7A69E05")]
public class FixSeason21BuffValuesFromSeason22UpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.FixSeason21BuffValuesFromSeason22UpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.FixSeason21BuffValuesFromSeason22UpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 20, 18, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(FixSeason21DarkLordBuffsUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        Season21BuffValuesFromSeason22.Apply(context, gameConfiguration);
        return ValueTask.CompletedTask;
    }
}
