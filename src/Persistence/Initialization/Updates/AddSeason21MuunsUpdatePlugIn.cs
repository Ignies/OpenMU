// <copyright file="AddSeason21MuunsUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the Muuns of Season 21 (<see cref="Season21MuunsInitializer"/>): the Muun system with its data, the evolution
/// target of the evolution stones and Monica, the exchange NPC.
/// </summary>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21MuunsUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21MuunsUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("E4B19C73-2D6A-4F80-9B35-7A1C8E2F5D06")]
public class AddSeason21MuunsUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21MuunsUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21MuunsUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 06, 20, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(ActivateSeason21WideItemsUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21MuunsInitializer(context, gameConfiguration).Initialize();
        return ValueTask.CompletedTask;
    }
}
