// <copyright file="AddSeason21MasteryOptionsUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the mastery options and mastery bonus options to the mastery items of Season 21.
/// </summary>
/// <remarks>
/// It's optional and not part of the initialization of a new database, see <see cref="Season21ItemOptionsInitializer"/>.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21MasteryOptionsUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21MasteryOptionsUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("088BE1FB-8246-41F1-A2EB-EDAD9FB30285")]
public class AddSeason21MasteryOptionsUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21MasteryOptionsUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21MasteryOptionsUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 06, 12, 02, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    /// <remarks>The item catalog defines the mastery items.</remarks>
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21ItemCatalogUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21ItemOptionsInitializer(context, gameConfiguration).AddMasteryOptions();
        return ValueTask.CompletedTask;
    }
}
