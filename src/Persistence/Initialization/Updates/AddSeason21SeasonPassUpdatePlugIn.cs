// <copyright file="AddSeason21SeasonPassUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Sets the season pass (MU Pass) of Season 21 and turns it on.
/// </summary>
/// <remarks>
/// Optional; it needs the item catalog (the tickets and the item rewards). A second run keeps the pass and the switch an operator has.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21SeasonPassUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21SeasonPassUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("A7D3C51E-9B28-4F64-8E0D-5C1B7A3F9D26")]
public class AddSeason21SeasonPassUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21SeasonPassUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21SeasonPassUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 07, 4, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21ItemCatalogUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21SeasonPassInitializer(context, gameConfiguration).Initialize();
        return ValueTask.CompletedTask;
    }
}
