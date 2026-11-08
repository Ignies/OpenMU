// <copyright file="AddSeason21MapsUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.PlugIns;
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
    /// <remarks>
    /// Version 2: the placeholder rows of the data replaced (Crimson Icarus, Under Persona, the elites of Atlans Abyss 3 and Tormenta).
    /// Version 3: Nars asks 400 instead of the 300 of Acheron.
    /// Version 4: Atlans Abyss 834/836 take the muonlinefanz.com rows; Phoenix of Darkness 77 and Death Spirit 410 the S21Emu values.
    /// Version 5: Devias takes the 280 monsters of the Season 21 data in place of the 560 of Season 6.
    /// Version 6: the town NPCs of the Season 21 data which Season 6 lacks stand in Noria and Lorencia.
    /// Version 7: Kanturu Underground respawns in its own safe zone (<see cref="OwnMapRespawnPlugIn"/>).
    /// Version 8: Bass 744 and Vasuki 745 swap level, health and damage (muonlinefanz.com, muonline.net).
    /// Version 9: the Sanctuary of Aquilas (Aquilas Temple, map 145) with its monsters, gates and warp 75.
    /// </remarks>
    public override int Version => 9;

    /// <inheritdoc />
    public override DateTime UpdatedAt => new(2026, 11, 08, 18, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21LevelCapsUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override async ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        var gameServers = (await context.GetAsync<GameServerConfiguration>().ConfigureAwait(false)).ToList();
        new Season21MapsInitializer(context, gameConfiguration, gameServers).Initialize();
        Season21PlaceholderRows.Repair(gameConfiguration);
        Season21MapsInitializer.RepairEntryLevels(gameConfiguration);
        Season21ClassicSpawns.Repair(context, gameConfiguration);
        Season21ClassicSpawns.PlaceTownNpcs(context, gameConfiguration);
        EnableOwnMapRespawn(context, gameConfiguration);
    }

    private static void EnableOwnMapRespawn(IContext context, GameConfiguration gameConfiguration)
    {
        // A list of maps the operator configured stays.
        var configuration = Season21PlugInConfigurations.GetOrCreate<OwnMapRespawnConfiguration>(context, gameConfiguration, typeof(OwnMapRespawnPlugIn).GUID, out var plugIn);
        if (configuration is null)
        {
            plugIn.SetConfiguration(new OwnMapRespawnConfiguration(), null);
        }

        plugIn.IsActive = true;
    }
}
