// <copyright file="AddSeason21BagItemsUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Gives the items of group 19 their uses (<see cref="Season21BagItemsInitializer"/>).
/// </summary>
/// <remarks>
/// It's optional and not part of the initialization of a new database. The bags open with the item rules of Season 21
/// (plugin "Item rules of Season 21", which the item mixes update turns on).
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21BagItemsUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21BagItemsUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("9D4A7E26-1C3B-4F58-8E92-B7A05C6D1F34")]
public class AddSeason21BagItemsUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21BagItemsUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21BagItemsUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 09, 14, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    /// <remarks>The items are of the item catalog, the refinery of the elements update.</remarks>
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21ItemCatalogUpdatePlugIn).GUID, typeof(AddSeason21ElementsUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21BagItemsInitializer(context, gameConfiguration).Initialize();
        return ValueTask.CompletedTask;
    }
}
