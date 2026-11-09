// <copyright file="AddSeason21GuideQuestsUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Sets the steps of the Season 21 quest guide and turns the guide quests on (<c>GameLogic/GuideQuests/Readme.md</c>).
/// </summary>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21GuideQuestsUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21GuideQuestsUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("4F8B2D63-1E97-4A50-B6C2-8D3E7F1A9C05")]
public class AddSeason21GuideQuestsUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21GuideQuestsUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21GuideQuestsUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 09, 12, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21ItemCatalogUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21GuideQuestsInitializer(context, gameConfiguration).Initialize();
        return ValueTask.CompletedTask;
    }
}
