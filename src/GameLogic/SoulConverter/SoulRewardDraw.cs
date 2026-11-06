// <copyright file="SoulRewardDraw.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.SoulConverter;

using MUnique.OpenMU.GameLogic.Rewards;

/// <summary>
/// A draw of a reward (a section of the S21Emu item bag): with its chance it gives one of the items, or Ruud.
/// </summary>
public class SoulRewardDraw
{
    /// <summary>
    /// Gets or sets the chance of the draw, in 1/10000.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SoulRewardDraw_Chance_Name))]
    public int Chance { get; set; }

    /// <summary>
    /// Gets or sets the items of which the draw gives one, each as likely.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SoulRewardDraw_Items_Name))]
    public List<RewardItem> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the least Ruud the draw gives; a draw without items gives Ruud.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SoulRewardDraw_MinimumRuud_Name))]
    public int MinimumRuud { get; set; }

    /// <summary>
    /// Gets or sets the most Ruud the draw gives.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SoulRewardDraw_MaximumRuud_Name))]
    public int MaximumRuud { get; set; }
}
