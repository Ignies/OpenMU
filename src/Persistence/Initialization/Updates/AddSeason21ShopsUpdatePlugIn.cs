// <copyright file="AddSeason21ShopsUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Sets the NPC shops of Season 21 with their tabs (<see cref="Season21ShopsInitializer"/>).
/// </summary>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21ShopsUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21ShopsUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("C6E1A853-4B2F-4D97-8E1C-7A3F9B5D2E04")]
public class AddSeason21ShopsUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21ShopsUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21ShopsUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 06, 17, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    /// <remarks>Version 2: Wizard Gillard (891), whose shop the data has, stands in Lorencia.</remarks>
    public override int Version => 2;

    /// <inheritdoc />
    public override DateTime UpdatedAt => new(2026, 11, 08, 13, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21ItemCatalogUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override async ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration, int? installedVersion)
    {
        // A later version keeps the shops the first one set, and what the operator changed in them.
        await new Season21ShopsInitializer(context, gameConfiguration).InitializeAsync(keepFilledTabs: installedVersion is not null).ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        return this.ApplyAsync(context, gameConfiguration, null);
    }
}
