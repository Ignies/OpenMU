// <copyright file="AddSeason21EarringsUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the earrings of Season 21: their slots, base stats, random options and the link options of a pair.
/// </summary>
/// <remarks>
/// It's optional and not part of the initialization of a new database, see <see cref="Season21ElementsInitializer"/>.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21EarringsUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21EarringsUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("59B98449-65A1-4495-8D9E-FCED902E10FF")]
public class AddSeason21EarringsUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21EarringsUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21EarringsUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 06, 18, 12, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    /// <remarks>The item catalog defines the earrings; the mastery options give the classes the all stats bonus a pair raises.</remarks>
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21ItemCatalogUpdatePlugIn).GUID, typeof(AddSeason21MasteryOptionsUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21ElementsInitializer(context, gameConfiguration).AddEarrings();
        return ValueTask.CompletedTask;
    }
}
