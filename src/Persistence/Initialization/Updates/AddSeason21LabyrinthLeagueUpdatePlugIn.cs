// <copyright file="AddSeason21LabyrinthLeagueUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the scores and weekly leagues of the Labyrinth of Dimensions of Season 21 and lets its boxes be opened.
/// </summary>
/// <remarks>
/// Optional. A second run keeps the configuration and the switch an operator has.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21LabyrinthLeagueUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21LabyrinthLeagueUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("5C2A7E91-3B64-4D08-9F1E-A7B3C5D2E846")]
public class AddSeason21LabyrinthLeagueUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21LabyrinthLeagueUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21LabyrinthLeagueUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 07, 7, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21InstanceZoneUpdatePlugIn).GUID, typeof(AddSeason21ItemCatalogUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21LabyrinthLeagueInitializer(context, gameConfiguration).Initialize();
        return ValueTask.CompletedTask;
    }
}
