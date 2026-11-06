// <copyright file="AddSeason21ElementsUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the elements of Season 21: the pentagrams and errtels with their slot, element and rank options, the element columns of the monsters and the elements feature.
/// </summary>
/// <remarks>
/// It's optional and not part of the initialization of a new database, see <see cref="Season21ElementsInitializer"/>.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21ElementsUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21ElementsUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("41C19C45-C851-4DA2-A970-4C1312323A05")]
public class AddSeason21ElementsUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21ElementsUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21ElementsUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 06, 18, 11, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    /// <remarks>The item catalog defines the pentagrams and errtels.</remarks>
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21ItemCatalogUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21ElementsInitializer(context, gameConfiguration).AddElements();
        return ValueTask.CompletedTask;
    }
}
