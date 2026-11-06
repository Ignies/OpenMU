// <copyright file="AddSeason21SphereLevelsUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the sphere levels 6 to 10 and the seed sphere enhancement of Season 21.
/// </summary>
/// <remarks>
/// It's optional and not part of the initialization of a new database, see <see cref="Season21ItemOptionsInitializer"/>.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21SphereLevelsUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21SphereLevelsUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("39F1383A-5543-48B6-938C-B71266500E5E")]
public class AddSeason21SphereLevelsUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21SphereLevelsUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21SphereLevelsUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 06, 12, 04, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    /// <remarks>The seed spheres of the levels 6 to 10 wait for the wide item format.</remarks>
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(ActivateSeason21WideItemsUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21ItemOptionsInitializer(context, gameConfiguration).AddSphereLevels();
        return ValueTask.CompletedTask;
    }
}
