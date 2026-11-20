// <copyright file="FixSeason21SummonerBuffsUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Gives the Summoner's Darkness, the Weakness and Innovation tiers and Blind their effects and makes them cast as they should,
/// see <see cref="Season21SummonerBuffs"/>. Fork feature id <c>s21_skills</c>.
/// </summary>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.FixSeason21SummonerBuffsUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.FixSeason21SummonerBuffsUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("7B3E5C20-91AF-4D6E-8C47-0E5A2F93B1D8")]
public class FixSeason21SummonerBuffsUpdatePlugIn : UpdatePlugInBase
{
    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.FixSeason21SummonerBuffsUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.FixSeason21SummonerBuffsUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 20, 22, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21SkillsUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        Season21SummonerBuffs.Apply(context, gameConfiguration);
        return ValueTask.CompletedTask;
    }
}
