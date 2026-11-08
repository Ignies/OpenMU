// <copyright file="AddSeason21IllusionTempleLeagueUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the Illusion Temple League of Season 21 (S21Emu <c>Event/IlusionTempleLeague</c>): the renewed temple on maps 98
/// and 99 for two guild parties and its monthly league, and switches its timetable and league on.
/// </summary>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21IllusionTempleLeagueUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21IllusionTempleLeagueUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("7B2D9E41-3C68-4A1F-8E25-D06F4B9A1C83")]
public class AddSeason21IllusionTempleLeagueUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21IllusionTempleLeagueUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21IllusionTempleLeagueUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 08, 12, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21MapsUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override async ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        var gameServers = (await context.GetAsync<GameServerConfiguration>().ConfigureAwait(false)).ToList();
        new Season21EventsInitializer(context, gameConfiguration, gameServers).AddIllusionTempleLeague();
    }
}
