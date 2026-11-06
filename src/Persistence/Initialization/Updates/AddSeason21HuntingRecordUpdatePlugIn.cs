// <copyright file="AddSeason21HuntingRecordUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Sets the maps of the hunting record of Season 21 and turns the hunting record and ranking on.
/// </summary>
/// <remarks>
/// Optional. The maps of the later seasons count once they are there. A second run keeps the maps and the switch an operator has.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21HuntingRecordUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21HuntingRecordUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("8A1F4C36-5D2E-4B97-9E03-7C6B1A2D5F84")]
public class AddSeason21HuntingRecordUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21HuntingRecordUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21HuntingRecordUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 07, 1, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21HuntingRecordInitializer(context, gameConfiguration).Initialize();
        return ValueTask.CompletedTask;
    }
}
