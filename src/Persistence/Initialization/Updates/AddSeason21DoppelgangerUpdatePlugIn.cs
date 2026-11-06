// <copyright file="AddSeason21DoppelgangerUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Renews the Doppelganger for Season 21 (S21Emu <c>Event/DoubleGoerRenewal</c>): seven rooms by total level with their
/// monster multipliers, experience and chests, open to every level from 50, at least two players.
/// </summary>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21DoppelgangerUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21DoppelgangerUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("CD64810A-43B5-4E00-8891-2C16179BB1F4")]
public class AddSeason21DoppelgangerUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21DoppelgangerUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21DoppelgangerUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 07, 0, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21LevelCapsUpdatePlugIn).GUID, typeof(AddSeason21ItemCatalogUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        new Season21DoppelgangerInitializer(context, gameConfiguration).Initialize();
        return ValueTask.CompletedTask;
    }
}
