// <copyright file="FixSeason21DarkLordBuffsUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Gives the Dark Lord's Crown Force, Divine Force, Divine Aura and Battle Glory their effect duration and makes them buffs,
/// see <see cref="Season21DarkLordBuffs"/>. Fork feature id <c>s21_skills</c>.
/// </summary>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.FixSeason21DarkLordBuffsUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.FixSeason21DarkLordBuffsUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("C47E2A91-5D08-4B3F-A61C-9E83F2D05B74")]
public class FixSeason21DarkLordBuffsUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.FixSeason21DarkLordBuffsUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.FixSeason21DarkLordBuffsUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 20, 14, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21SkillsUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        Season21DarkLordBuffs.Apply(context, gameConfiguration);
        return ValueTask.CompletedTask;
    }
}
