// <copyright file="AddSeason21ChaosCastleSurvivalUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the Chaos Castle Survival of the Fittest of Season 21 (S21Emu <c>Event/ChaosCastleSurvival.dat</c>, webzen guide 161):
/// map 97, the castle for up to 70 players and its monthly tournament, and switches its timetable and tournament on.
/// </summary>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21ChaosCastleSurvivalUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21ChaosCastleSurvivalUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("E3A9C716-48D2-4F5B-9B07-6D1E2C8A4F39")]
public class AddSeason21ChaosCastleSurvivalUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21ChaosCastleSurvivalUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21ChaosCastleSurvivalUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 07, 8, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21MapsUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override async ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        var gameServers = (await context.GetAsync<GameServerConfiguration>().ConfigureAwait(false)).ToList();
        new Season21EventsInitializer(context, gameConfiguration, gameServers).AddChaosCastleSurvival();
    }
}
