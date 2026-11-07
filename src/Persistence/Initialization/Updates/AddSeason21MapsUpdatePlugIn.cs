// <copyright file="AddSeason21MapsUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the maps which were introduced after Season 6, as the Season 21 server data defines them.
/// </summary>
/// <remarks>
/// It's optional and not part of the initialization of a new database, so a Season 6 server keeps its maps unless the
/// operator applies it. The data contains the entry levels and monster values of Season 21, which has a total level
/// cap of 1750, so it depends on <see cref="AddSeason21LevelCapsUpdatePlugIn"/>. The content is marked with the
/// version which introduced it (<see cref="GameMapDefinition.IntroducedIn"/>), so it can be deactivated again in the
/// content activation of the admin panel.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21MapsUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21MapsUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("7108C428-0C0F-4657-AF48-59873390C885")]
public class AddSeason21MapsUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21MapsUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21MapsUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 05, 1, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    /// <remarks>Version 2: the placeholder rows of the data replaced (Crimson Icarus, Under Persona, the elites of Atlans Abyss 3 and Tormenta).</remarks>
    public override int Version => 2;

    /// <inheritdoc />
    public override DateTime UpdatedAt => new(2026, 11, 07, 13, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21LevelCapsUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override async ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        var gameServers = (await context.GetAsync<GameServerConfiguration>().ConfigureAwait(false)).ToList();
        new Season21MapsInitializer(context, gameConfiguration, gameServers).Initialize();
        Season21PlaceholderRows.Repair(gameConfiguration);
    }
}
