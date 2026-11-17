// <copyright file="AddSeason21AcheronGuardianUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the Protector of Acheron of Season 21 (S21Emu <c>Event/AcheronGuardian.dat</c>): the event copies of Acheron
/// and Deventer, 30 waves of monsters, the reward bag, and switches its timetable on.
/// </summary>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21AcheronGuardianUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21AcheronGuardianUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("B9F897BE-8C73-4C8A-8D72-9A4377FAD54D")]
public class AddSeason21AcheronGuardianUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21AcheronGuardianUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21AcheronGuardianUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    /// <remarks>Version 2 places Jin's copy (604) with his shop on map 92 when the shops update is in.</remarks>
    public override int Version => 2;

    /// <inheritdoc />
    public override DateTime UpdatedAt => new(2026, 11, 17, 18, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 07, 4, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21MapsUpdatePlugIn).GUID, typeof(AddSeason21ItemCatalogUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override async ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        var gameServers = (await context.GetAsync<GameServerConfiguration>().ConfigureAwait(false)).ToList();
        new Season21EventsInitializer(context, gameConfiguration, gameServers).AddAcheronGuardian();

        // Jin (583) comes only with the shops update; with it in, his copy 604 joins map 92 (filled tabs stay the operator's).
        if (gameConfiguration.Monsters.Any(m => m.Number == 583))
        {
            await new Season21ShopsInitializer(context, gameConfiguration).InitializeAsync(keepFilledTabs: true).ConfigureAwait(false);
        }
    }
}
