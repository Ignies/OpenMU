// <copyright file="ErrtelLevelUp.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Elements;

/// <summary>
/// The level up of an errtel from one level: the chance and the elemental runes by its rank
/// (S21Emu ElementalItemRefinemenMixRate.xml, LevelUp). The money is the crafting's.
/// </summary>
public class ErrtelLevelUp
{
    /// <summary>
    /// Gets or sets the level the errtel has before.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ErrtelLevelUp_Level_Name))]
    public int Level { get; set; }

    /// <summary>
    /// Gets or sets the success chances in percent by rank: the first for rank 1, and so on.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ErrtelLevelUp_Rates_Name))]
    public List<int> Rates { get; set; } = [];

    /// <summary>
    /// Gets or sets the elemental runes it takes by rank.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ErrtelLevelUp_Runes_Name))]
    public List<int> Runes { get; set; } = [];

    /// <inheritdoc />
    public override string ToString() => $"Level {this.Level}: {string.Join(", ", this.Rates)} %, runes {string.Join(", ", this.Runes)}";
}
