// <copyright file="AddSeason21ItemMixesUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.WideItems;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the item mixes of Season 21 and turns on the item rules of Season 21 (<see cref="Season21ItemsFeaturePlugIn"/>).
/// </summary>
/// <remarks>
/// It's optional and not part of the initialization of a new database, see <see cref="Season21ItemOptionsInitializer"/>.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21ItemMixesUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21ItemMixesUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("6D2F8B47-1E93-4C05-A7B6-3F9E0C5D8A12")]
public class AddSeason21ItemMixesUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21ItemMixesUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21ItemMixesUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 06, 18, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    /// <remarks>Version 2: the +10 to +15 combinations at 60, 60, 50, 50, 40 and 40 % (S21Emu ChaosMix.dat). Version 3: the
    /// archangel upgrade and the Archangel's Hammer (<see cref="Season21MixData.ArchangelMixes"/>). Version 4: the
    /// monster wings (<see cref="Season21MixData.MonsterWingMixes"/>).</remarks>
    public override int Version => 4;

    /// <inheritdoc />
    public override DateTime UpdatedAt => new(2026, 11, 08, 1, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    /// <remarks>The mixes give mastery items and graded wings.</remarks>
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21MasteryOptionsUpdatePlugIn).GUID, typeof(AddSeason21WingGradesUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        var typeId = typeof(Season21ItemsFeaturePlugIn).GUID;
        var plugIn = gameConfiguration.PlugInConfigurations.FirstOrDefault(c => c.TypeId == typeId);
        if (plugIn is null)
        {
            plugIn = context.CreateNew<PlugInConfiguration>();
            plugIn.SetGuid(typeId);
            plugIn.TypeId = typeId;
            gameConfiguration.PlugInConfigurations.Add(plugIn);
        }

        new Season21ItemOptionsInitializer(context, gameConfiguration).AddItemMixes(plugIn);
        return ValueTask.CompletedTask;
    }
}
