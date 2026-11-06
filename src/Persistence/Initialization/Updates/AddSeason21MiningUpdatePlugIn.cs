// <copyright file="AddSeason21MiningUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the mining spots of Season 21 and turns on the mining plugin.
/// </summary>
/// <remarks>
/// Optional; it needs the shops of Season 21, in which Bolo sells the Pandora Pick of the item catalog. The spots of Kubera Mine
/// are only placed when the maps of the later seasons are there. A second run keeps the spots and settings an operator has.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21MiningUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21MiningUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("6D4B9E27-1A3C-4F85-B2E0-8C7F5A1D3E96")]
public class AddSeason21MiningUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21MiningUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21MiningUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 06, 23, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21ShopsUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21MiningInitializer(context, gameConfiguration).Initialize();
        return ValueTask.CompletedTask;
    }
}
