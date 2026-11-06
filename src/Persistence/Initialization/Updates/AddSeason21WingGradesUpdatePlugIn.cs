// <copyright file="AddSeason21WingGradesUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the grade options of the fourth and fifth wings of Season 21.
/// </summary>
/// <remarks>
/// It's optional and not part of the initialization of a new database, see <see cref="Season21ItemOptionsInitializer"/>.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21WingGradesUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21WingGradesUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("0C3EFC84-BDF1-41BA-97D1-AEE19F63C472")]
public class AddSeason21WingGradesUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21WingGradesUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21WingGradesUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 06, 12, 03, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    /// <remarks>The wings wait for the wide item format.</remarks>
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(ActivateSeason21WideItemsUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21ItemOptionsInitializer(context, gameConfiguration).AddWingGrades();
        return ValueTask.CompletedTask;
    }
}
