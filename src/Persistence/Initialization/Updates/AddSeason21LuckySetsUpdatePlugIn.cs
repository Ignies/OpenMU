// <copyright file="AddSeason21LuckySetsUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the set options of the lucky items (<see cref="Season21LuckySets"/>).
/// </summary>
/// <remarks>
/// It's optional and not part of the initialization of a new database.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21LuckySetsUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21LuckySetsUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("3E8B5C21-7A4D-4F69-9B13-C6D2E0A87F45")]
public class AddSeason21LuckySetsUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21LuckySetsUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21LuckySetsUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 08, 15, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    /// <remarks>Version 2 gives the pieces the defense option and the defense by level, which the ticket exchange rolls.</remarks>
    public override int Version => 2;

    /// <inheritdoc />
    public override DateTime UpdatedAt => new(2026, 11, 08, 18, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    /// <remarks>The later lucky items are of the item catalog.</remarks>
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddLuckyItemsUpdatePlugIn).GUID, typeof(AddSeason21ItemCatalogUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        Season21LuckySets.Add(context, gameConfiguration);
        Season21LuckySets.AddRollTargets(gameConfiguration);
        return ValueTask.CompletedTask;
    }
}
