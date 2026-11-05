// <copyright file="AddSeason21GearUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.Items;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the items of Season 21 which a Season 6 character class can use, as the Season 21 server data defines them.
/// </summary>
/// <remarks>
/// It's optional and not part of the initialization of a new database, see <see cref="Season21GearInitializer"/>.
/// The items which the data lets monsters drop are dropped by monsters, and the red and blue chocolate boxes hold the
/// rings of the data.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21GearUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21GearUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("C1A024BF-97E6-4D92-9BA4-59DF8D9BE051")]
public class AddSeason21GearUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21GearUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21GearUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 05, 4, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    /// <remarks>
    /// The items get the item stat profiles, so these have to exist first.
    /// </remarks>
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddItemStatProfilesUpdatePlugInSeasonSix).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21GearInitializer(context, gameConfiguration).Initialize();
        ItemStatProfiles.Apply(context, gameConfiguration);
        return ValueTask.CompletedTask;
    }
}
