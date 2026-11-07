// <copyright file="AddSeason21ArcaWarUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the Arca War of Season 21.
/// </summary>
/// <remarks>
/// Optional. A second run keeps the configuration and the switch an operator has. Version 2 adds the reward of Acheron
/// (effect 187) and Sir Lesnar with the exchange of the Trophies of Battle; the new settings (Sign of Lord fee, team
/// damage reduction, bonus) come with their defaults into an existing configuration.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21ArcaWarUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21ArcaWarUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("6C1E0B7A-3F52-4D8E-9A41-2B7D5E8C9F03")]
public class AddSeason21ArcaWarUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21ArcaWarUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21ArcaWarUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override int Version => 2;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 07, 12, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21AcheronGuardianUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21ArcaWarInitializer(context, gameConfiguration).Initialize();
        return ValueTask.CompletedTask;
    }
}
