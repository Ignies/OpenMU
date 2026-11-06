// <copyright file="ErrtelRankUp.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Elements;

/// <summary>
/// The rank up of an errtel from one rank: the success chance by the level of its highest rank option
/// (S21Emu ElementalUpgrade.xml, ErrtelUpgradeRateInfo). The money is the crafting's.
/// </summary>
public class ErrtelRankUp
{
    /// <summary>
    /// Gets or sets the rank the errtel has before.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ErrtelRankUp_Rank_Name))]
    public int Rank { get; set; }

    /// <summary>
    /// Gets or sets the success chances in percent by level: the first for level 0, and so on. 0 means the level is too low.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ErrtelRankUp_Rates_Name))]
    public List<int> Rates { get; set; } = [];

    /// <inheritdoc />
    public override string ToString() => $"Rank {this.Rank}: {string.Join(", ", this.Rates)} %";
}
