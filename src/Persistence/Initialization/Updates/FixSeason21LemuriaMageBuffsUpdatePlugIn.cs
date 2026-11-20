// <copyright file="FixSeason21LemuriaMageBuffsUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Gives the Lemuria Mage's beginner buffs their durations and values and makes them cast on a chosen player,
/// see <see cref="Season21LemuriaMageBuffs"/>. Fork feature id <c>s21_skills</c>.
/// </summary>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.FixSeason21LemuriaMageBuffsUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.FixSeason21LemuriaMageBuffsUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("A3D06F48-72B1-4E5C-9A1D-6C84E2B95F07")]
public class FixSeason21LemuriaMageBuffsUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.FixSeason21LemuriaMageBuffsUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.FixSeason21LemuriaMageBuffsUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 21, 1, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21SkillsUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        Season21LemuriaMageBuffs.Apply(context, gameConfiguration);
        return ValueTask.CompletedTask;
    }
}
