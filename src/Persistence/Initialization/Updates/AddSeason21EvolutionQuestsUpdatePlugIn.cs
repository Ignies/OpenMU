// <copyright file="AddSeason21EvolutionQuestsUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the fourth and fifth class quests of Season 21: the Trial Area, Sent and Eunice and the quests of every class.
/// </summary>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21EvolutionQuestsUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21EvolutionQuestsUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("1E1ACE8D-A2B3-4246-9273-5748F9215473")]
public class AddSeason21EvolutionQuestsUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21EvolutionQuestsUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21EvolutionQuestsUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 06, 12, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    /// <remarks>Version 2: the Barracks quest of the third evolution gives 20 points instead of 10.</remarks>
    public override int Version => 2;

    /// <inheritdoc />
    public override DateTime UpdatedAt => new(2026, 11, 08, 0, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn =>
    [
        typeof(AddSeason21MapsUpdatePlugIn).GUID,
        typeof(AddSeason21ClassesUpdatePlugIn).GUID,
        typeof(AddSeason21ItemCatalogUpdatePlugIn).GUID,
    ];

    /// <inheritdoc />
    protected override async ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        var gameServers = (await context.GetAsync<GameServerConfiguration>().ConfigureAwait(false)).ToList();
        new Season21EvolutionQuestsInitializer(context, gameConfiguration, gameServers).Initialize();
    }
}
