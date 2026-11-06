// <copyright file="AddSeason21LabyrinthRoomsUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the 10 x 10 rooms of the floors of the Labyrinth of Dimensions of Season 21.
/// </summary>
/// <remarks>
/// Optional. A second run keeps the configuration and the switch an operator has.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21LabyrinthRoomsUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21LabyrinthRoomsUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("38055A68-E5CF-475A-97FF-FD33650309E9")]
public class AddSeason21LabyrinthRoomsUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21LabyrinthRoomsUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21LabyrinthRoomsUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 06, 20, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21InstanceZoneUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21LabyrinthRoomsInitializer(context, gameConfiguration).Initialize();
        return ValueTask.CompletedTask;
    }
}
