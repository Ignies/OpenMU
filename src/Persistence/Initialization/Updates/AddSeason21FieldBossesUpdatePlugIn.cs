// <copyright file="AddSeason21FieldBossesUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the field bosses of Season 21 and turns on the field bosses plugin.
/// </summary>
/// <remarks>
/// Optional, like the maps it depends on; it needs the item catalog for the item bags of the bosses. Lord Silvester and Core Magriffy, which the maps update spawns as plain monsters, spawn by the plugin afterwards. A second run keeps the bosses and settings an operator has.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21FieldBossesUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21FieldBossesUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("9D1A6B3C-2E74-4F08-8C5D-3B6E9A0F7C21")]
public class AddSeason21FieldBossesUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21FieldBossesUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21FieldBossesUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 06, 15, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21MapsUpdatePlugIn).GUID, typeof(AddSeason21ItemCatalogUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21FieldBossesInitializer(context, gameConfiguration).Initialize();
        return ValueTask.CompletedTask;
    }
}
