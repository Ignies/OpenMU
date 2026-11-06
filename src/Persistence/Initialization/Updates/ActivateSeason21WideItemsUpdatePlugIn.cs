// <copyright file="ActivateSeason21WideItemsUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Activates the items of Season 21 which only the wide item format of the Babylon client carries.
/// </summary>
/// <remarks>
/// It's optional and not part of the initialization of a new database, see <see cref="Season21ItemOptionsInitializer"/>.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.ActivateSeason21WideItemsUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.ActivateSeason21WideItemsUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("4EC3699B-2CCD-4C3E-B2C7-C8E2A513BD5D")]
public class ActivateSeason21WideItemsUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.ActivateSeason21WideItemsUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.ActivateSeason21WideItemsUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 06, 12, 01, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    /// <remarks>The item catalog defines the waiting items.</remarks>
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21ItemCatalogUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21ItemOptionsInitializer(context, gameConfiguration).ActivateWideItems();
        return ValueTask.CompletedTask;
    }
}
