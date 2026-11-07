// <copyright file="AddSeason21EliteZonesUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Places the elites of the later maps in their five elemental elite zones and turns on the elite zones plugin.
/// </summary>
/// <remarks>
/// Optional, like the maps it depends on. It moves the elite spawns of each zone map into the zones (Twisted Karutan's zones, which the server terrain of the data walls in, are opened as the Season 21 client has them), adds the elites which the data defines but never spawns, and adds the zones to the plugin configuration, keeping the zones and settings an operator has.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21EliteZonesUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21EliteZonesUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("4C8E2F61-9A3B-4D57-B1E6-7F0A2C9D3E18")]
public class AddSeason21EliteZonesUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21EliteZonesUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21EliteZonesUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 06, 14, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    /// <remarks>Version 2: the elites of the Swamp of Doom (972-974) derived from its normals instead of the placeholder rows.</remarks>
    public override int Version => 2;

    /// <inheritdoc />
    public override DateTime UpdatedAt => new(2026, 11, 07, 13, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21MapsUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21EliteZonesInitializer(context, gameConfiguration).Initialize();
        Season21PlaceholderRows.Repair(gameConfiguration);
        return ValueTask.CompletedTask;
    }
}
