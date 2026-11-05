// <copyright file="AddSeason21LevelCapsUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Sets the level caps of Season 21 and its master experience curve.
/// </summary>
/// <remarks>
/// Season 21 has 400 levels, 400 master levels and 950 enhanced levels. The enhanced levels count on the master level,
/// and every level field of the protocol has 16 bits, so they are a higher <see cref="GameConfiguration.MaximumMasterLevel"/>.
/// An enhanced level costs twice as much experience as the master level step of Season 6 at the same level, because
/// Season 21 gives half of the master experience for them; as there is one master experience rate, this is part of the
/// formula.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21LevelCapsUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21LevelCapsUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("049162C1-074B-41BC-A52E-D672958DD9ED")]
public class AddSeason21LevelCapsUpdatePlugIn : UpdatePlugInBase
{
    /// <summary>
    /// The master levels and enhanced levels of Season 21.
    /// </summary>
    private const short MaximumMasterLevel = 1350;

    /// <summary>
    /// The master experience formula of Season 6, as in <c>GameContext</c>, up to master level 400, and twice its step above.
    /// </summary>
    /// <remarks>
    /// <c>2 * f(level) - f(400)</c> continues the curve at master level 400, so a master level 400 character
    /// keeps its progress.
    /// </remarks>
    private const string MasterExperienceFormula = "if(level > 400, 2 * ((505 * level * level * level) + (35278500 * level) + (228045 * level * level)) - 82918600000, (505 * level * level * level) + (35278500 * level) + (228045 * level * level))";

    /// <summary>
    /// The master experience formula of Season 6, which the update replaces; a formula of the operator is kept.
    /// </summary>
    private const string SeasonSixMasterExperienceFormula = "(505 * level * level * level) + (35278500 * level) + (228045 * level * level)";

    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21LevelCapsUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21LevelCapsUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 05, 0, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        if (gameConfiguration.MaximumMasterLevel < MaximumMasterLevel)
        {
            gameConfiguration.MaximumMasterLevel = MaximumMasterLevel;
        }

        if (gameConfiguration.MasterExperienceFormula is null or SeasonSixMasterExperienceFormula)
        {
            gameConfiguration.MasterExperienceFormula = MasterExperienceFormula;
        }

        return ValueTask.CompletedTask;
    }
}
