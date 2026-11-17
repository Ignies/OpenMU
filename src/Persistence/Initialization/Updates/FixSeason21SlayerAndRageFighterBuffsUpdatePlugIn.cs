// <copyright file="FixSeason21SlayerAndRageFighterBuffsUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Gives the Slayer's Detection and Demolish (and their masters) their effect duration and value and makes them buffs, see
/// <see cref="Season21SlayerBuffs"/>, and makes the Rage Fighter's Blood Howling the buff it is, see
/// <see cref="Season21RageFighterBuffs"/>. Fork feature id <c>s21_skills</c>.
/// </summary>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.FixSeason21SlayerAndRageFighterBuffsUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.FixSeason21SlayerAndRageFighterBuffsUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("8B3F1D27-6C4E-4A92-B05D-2E7A9C1F4D63")]
public class FixSeason21SlayerAndRageFighterBuffsUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.FixSeason21SlayerAndRageFighterBuffsUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.FixSeason21SlayerAndRageFighterBuffsUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 17, 18, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    /// <remarks>The skills update creates the skills and their effects.</remarks>
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21SkillsUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        Season21SlayerBuffs.Apply(context, gameConfiguration);
        Season21RageFighterBuffs.Apply(context, gameConfiguration);
        return ValueTask.CompletedTask;
    }
}
