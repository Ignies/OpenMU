// <copyright file="FixSeason21FairyElfBuffsUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Gives the Fairy Elf's Bless, Elemental Damage and Defense Enhancement and the Greater Damage and Defense enhancement rows
/// their effect duration and value and makes them buffs, see <see cref="Season21FairyElfBuffs"/>. Fork feature id <c>s21_skills</c>.
/// </summary>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.FixSeason21FairyElfBuffsUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.FixSeason21FairyElfBuffsUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("87FC72D1-9F2E-4535-B550-1BA3B940A6F7")]
public class FixSeason21FairyElfBuffsUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.FixSeason21FairyElfBuffsUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.FixSeason21FairyElfBuffsUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21SkillsUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        Season21FairyElfBuffs.Apply(context, gameConfiguration);
        return ValueTask.CompletedTask;
    }
}
