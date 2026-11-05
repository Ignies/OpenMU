// <copyright file="AddSeason21ItemCatalogUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.Items;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Defines every item of the Season 21 server data which the configuration doesn't define yet, in all item groups.
/// </summary>
/// <remarks>
/// It's optional and not part of the initialization of a new database, see <see cref="Season21ItemCatalogInitializer"/>.
/// Defined items are kept as they are, so the Season 6 items and an operator's changes stay. The items which the
/// Season 6 item format can't carry are defined but inactive. The drop lists of the later maps get the new items.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21ItemCatalogUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21ItemCatalogUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("05C3A143-AA5F-491D-B6FB-F5F9068F1B4B")]
public class AddSeason21ItemCatalogUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21ItemCatalogUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21ItemCatalogUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 05, 17, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    /// <remarks>
    /// The gear of Season 21 comes first, so its items keep the templates which were chosen for them by hand.
    /// </remarks>
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21GearUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21ItemCatalogInitializer(context, gameConfiguration).Initialize();
        ItemStatProfiles.Apply(context, gameConfiguration);
        return ValueTask.CompletedTask;
    }
}
