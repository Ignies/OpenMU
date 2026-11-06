// <copyright file="AddSeason21SoulConverterUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Sets the tiers of the monster soul converter of Season 21 and turns the converter on.
/// </summary>
/// <remarks>
/// Optional. The missions on the maps of the later seasons need those maps. A second run keeps the tiers and the switch an operator has.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21SoulConverterUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21SoulConverterUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("C3E85A27-6F14-4D9B-A2C0-1B7E4F8D6A53")]
public class AddSeason21SoulConverterUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21SoulConverterUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21SoulConverterUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 07, 2, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21ItemCatalogUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21SoulConverterInitializer(context, gameConfiguration).Initialize();
        return ValueTask.CompletedTask;
    }
}
