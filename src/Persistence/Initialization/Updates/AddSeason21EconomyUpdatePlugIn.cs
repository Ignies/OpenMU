// <copyright file="AddSeason21EconomyUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the gremory case, Ruud and the Ruud shop of Season 21 (<see cref="Season21EconomyInitializer"/>).
/// </summary>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21EconomyUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21EconomyUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("9B4E7C25-1D6A-4F83-A2C9-5E0B3D8F7A16")]
public class AddSeason21EconomyUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21EconomyUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21EconomyUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 06, 16, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21ItemCatalogUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21EconomyInitializer(context, gameConfiguration).Initialize();
        return ValueTask.CompletedTask;
    }
}
