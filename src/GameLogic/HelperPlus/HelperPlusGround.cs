// <copyright file="HelperPlusGround.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.HelperPlus;

/// <summary>
/// A hunting ground of the helper plus (S21Emu <c>Character/MuHelperPlus.xml</c>, <c>HuntingGrounds</c>): its move list
/// entry gives the level and the zen a move into one of its zones needs.
/// </summary>
public class HelperPlusGround
{
    /// <summary>
    /// Gets or sets the index of the ground.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HelperPlusGround_Index_Name))]
    public short Index { get; set; }

    /// <summary>
    /// Gets or sets the index of the warp list entry (<c>Move.txt</c>) whose requirements and costs a move has.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HelperPlusGround_WarpIndex_Name))]
    public int WarpIndex { get; set; }

    /// <summary>
    /// Gets or sets the name of the source.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HelperPlusGround_Name_Name))]
    public string Name { get; set; } = string.Empty;

    /// <inheritdoc />
    public override string ToString() => this.Name;
}
