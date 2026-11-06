// <copyright file="MiningStage.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Mining;

/// <summary>
/// A stage of a mining spot (S21Emu <c>Mining.txt</c>, one row).
/// </summary>
public class MiningStage
{
    /// <summary>
    /// Gets or sets the durability the pickaxe loses for an attempt (the column <c>Value</c>, 2, 4 or 8 by spot size).
    /// </summary>
    /// <remarks>The data names the column only "Value"; reading it as the wear of the pickaxe is ours, see the readme.</remarks>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MiningStage_DurabilityCost_Name))]
    [Range(0, 255)]
    public byte DurabilityCost { get; set; }

    /// <summary>
    /// Gets or sets the chance of the stage to succeed, in 1/10000 (<c>ItemRate</c>, 4750 for every stage).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MiningStage_SuccessRate_Name))]
    [Range(0, 10000)]
    public int SuccessRate { get; set; }

    /// <summary>
    /// Gets or sets the jewels the character gets when it stops after this stage (<c>ItemCount</c>).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MiningStage_ItemCount_Name))]
    [Range(0, 10000)]
    public int ItemCount { get; set; }

    /// <summary>
    /// Gets or sets the chance of the jackpot after this stage succeeded, in 1/10000 (<c>BonusItemRate</c>, last stage only).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MiningStage_BonusRate_Name))]
    [Range(0, 10000)]
    public int BonusRate { get; set; }

    /// <summary>
    /// Gets or sets the jewels of the jackpot (<c>BonusItemCount</c>).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MiningStage_BonusCount_Name))]
    [Range(0, 10000)]
    public int BonusCount { get; set; }
}
