// <copyright file="Season21ItemsConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.WideItems;

/// <summary>
/// The configuration of <see cref="Season21ItemsFeaturePlugIn"/>: the grade rates of the fourth wings and the mastery
/// item mixes. See Readme.md.
/// </summary>
public class Season21ItemsConfiguration
{
    /// <summary>
    /// Gets or sets the weights of the grades 0, 1, 2, ... of a graded wing option when it's rolled
    /// (S21Emu ItemGradeOption.xml, RateList of the fourth wings).
    /// </summary>
    public List<int> GradeRates { get; set; } = [947500, 30000, 15000, 7500];

    /// <summary>
    /// Gets or sets the mastery item mixes (S21Emu Mastery - Settings/MasteryItemMix.xml).
    /// </summary>
    public List<MasteryMixRecipe> MasteryMixes { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the Jewel of Harmony takes ancient (set) items (S21Emu <c>Common.dat</c>
    /// <c>SetItemAcceptHarmonySwitch</c> 1; Webzen allows it since Season 16 Part 1-3). Season 6 refuses them.
    /// </summary>
    public bool HarmonyOnAncientItems { get; set; } = true;
}
