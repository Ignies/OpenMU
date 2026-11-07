// <copyright file="AbilityCardLevelUp.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.AbilityCore;

/// <summary>
/// The price to raise a card of a kind from a level to the next (S21Emu <c>AbilityLevelUP.xml</c>): other cards of the
/// same item, zen and stones.
/// </summary>
public class AbilityCardLevelUp
{
    /// <summary>
    /// Gets or sets the kind of the card.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCardLevelUp_Kind_Name))]
    public AbilityCardKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the level of the card before the level up.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCardLevelUp_Level_Name))]
    public byte Level { get; set; }

    /// <summary>
    /// Gets or sets the number of other cards of the same item which are used up.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCardLevelUp_Cards_Name))]
    public byte Cards { get; set; }

    /// <summary>
    /// Gets or sets the zen it costs.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCardLevelUp_Money_Name))]
    public int Money { get; set; }

    /// <summary>
    /// Gets or sets the item group of the stones.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCardLevelUp_StoneGroup_Name))]
    public byte StoneGroup { get; set; } = 20;

    /// <summary>
    /// Gets or sets the item number of the stones.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCardLevelUp_StoneNumber_Name))]
    public short StoneNumber { get; set; } = 239;

    /// <summary>
    /// Gets or sets the number of stones it costs.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCardLevelUp_Stones_Name))]
    public int Stones { get; set; }
}
