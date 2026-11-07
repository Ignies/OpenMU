// <copyright file="AddSeason21GoldenInvasionUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the golden invasion of Season 21: the ten later golden monsters and the roster and timetable of the data.
/// </summary>
/// <remarks>
/// Optional. A golden invasion which still has the Season 6 roster becomes the one of the data; one an operator changed keeps
/// its numbers and gets the missing monsters and maps only. A second run changes nothing. Version 2 adds the Underworld and
/// Summon invasions of <c>InvasionManager.xml</c> (<see cref="Season21GoldenInvasionInitializer.AddUnderworldAndSummon"/>).
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21GoldenInvasionUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21GoldenInvasionUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("A47C2E91-5B3D-4E68-9F10-3D2A8B6C4E57")]
public class AddSeason21GoldenInvasionUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21GoldenInvasionUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21GoldenInvasionUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 06, 23, 30, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override int Version => 2;

    /// <inheritdoc />
    public override DateTime UpdatedAt => new(2026, 11, 07, 21, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration, int? installedVersion)
    {
        var initializer = new Season21GoldenInvasionInitializer(context, gameConfiguration);
        if (installedVersion is null)
        {
            initializer.Initialize();
        }

        if (installedVersion is null or < 2)
        {
            initializer.AddUnderworldAndSummon();
        }

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        return this.ApplyAsync(context, gameConfiguration, null);
    }
}
