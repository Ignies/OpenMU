// <copyright file="AddSeason21NewPowerUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the fifth class New Power of Season 21 (<see cref="Season21NewPowerInitializer"/>) to a world which has the
/// Ability Enhancement Core; one without it stays as it is, and gets it with the core's update. Mandatory and on new
/// databases, so installed worlds get it too; the New Power itself (fork feature id <c>new_power</c>) stays off.
/// </summary>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21NewPowerUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21NewPowerUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("C7D41E58-2B96-4F0A-8E37-6A1B5D9C2E84")]
public class AddSeason21NewPowerUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21NewPowerUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21NewPowerUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => true;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    /// <remarks>Version 2 qualifies the skills for every class: an item skill only goes to the classes it qualifies.</remarks>
    public override int Version => 2;

    /// <inheritdoc />
    public override DateTime UpdatedAt => new(2026, 11, 19, 8, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 19, 1, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21NewPowerInitializer(context, gameConfiguration).Initialize();
        return ValueTask.CompletedTask;
    }
}
