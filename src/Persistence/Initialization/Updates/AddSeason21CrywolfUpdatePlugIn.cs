// <copyright file="AddSeason21CrywolfUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Applies the Crywolf renewal of Season 21 Part 1 Update 3: the element of the week of the army, Balgass at 6:40 into the
/// battle, the rewards into the gremory case and the state durations of the Season 21 data.
/// </summary>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21CrywolfUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21CrywolfUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("1C5D8E2B-7A43-4F96-B0E1-3D9A6C42F857")]
public class AddSeason21CrywolfUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21CrywolfUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21CrywolfUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 07, 6, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddCrywolfEventUpdatePlugIn).GUID, typeof(AddSeason21ElementsUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override async ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        var gameServers = (await context.GetAsync<GameServerConfiguration>().ConfigureAwait(false)).ToList();
        new Season21EventsInitializer(context, gameConfiguration, gameServers).ApplyCrywolfRenewal();
    }
}
