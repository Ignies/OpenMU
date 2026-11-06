// <copyright file="AddSeason21InstanceZoneUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the Instance Zone (map 114) and the Labyrinth of Dimensions (map 115) of Season 21: the trials of the fourth and
/// fifth class quests are entered from the Instance Zone, the labyrinth is a solo instance of 41 floors.
/// </summary>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21InstanceZoneUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21InstanceZoneUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("2910BFA1-CF7E-4FE8-AB3A-0D3F176157C7")]
public class AddSeason21InstanceZoneUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21InstanceZoneUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21InstanceZoneUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 07, 2, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21InstancesUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override async ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        var gameServers = (await context.GetAsync<GameServerConfiguration>().ConfigureAwait(false)).ToList();
        new Season21EventsInitializer(context, gameConfiguration, gameServers).AddInstanceZone();
    }
}
