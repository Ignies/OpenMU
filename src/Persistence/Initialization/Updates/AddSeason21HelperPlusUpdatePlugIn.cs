// <copyright file="AddSeason21HelperPlusUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Sets the hunting grounds and zones of the MU Helper Plus of Season 21 and turns it on.
/// </summary>
/// <remarks>
/// Optional; it needs the maps of the later seasons, whose warps the grounds use. A second run keeps the zones and the switch an operator has.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21HelperPlusUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21HelperPlusUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("E62B9D48-1F75-4A3C-B08E-4D9C2A7F5B16")]
public class AddSeason21HelperPlusUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21HelperPlusUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21HelperPlusUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 07, 3, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21MapsUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21HelperPlusInitializer(context, gameConfiguration).Initialize();
        return ValueTask.CompletedTask;
    }
}
