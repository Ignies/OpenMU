// <copyright file="AddSeason21RewardsUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the level rewards and the event contribution boxes of Season 21 (<see cref="Season21RewardsInitializer"/>).
/// </summary>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21RewardsUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21RewardsUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("1F7A9C42-6D3B-4E85-B0A1-8C5E2D7F3B96")]
public class AddSeason21RewardsUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21RewardsUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21RewardsUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 06, 18, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21EconomyUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21RewardsInitializer(context, gameConfiguration).Initialize();
        return ValueTask.CompletedTask;
    }
}
