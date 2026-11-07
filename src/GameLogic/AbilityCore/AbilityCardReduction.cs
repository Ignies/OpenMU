// <copyright file="AbilityCardReduction.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.AbilityCore;

/// <summary>
/// The reduction of a card of a kind into stones (S21Emu <c>AbilityReduction.xml</c>): it costs zen and gives stones.
/// </summary>
public class AbilityCardReduction
{
    /// <summary>
    /// Gets or sets the kind of the card.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCardReduction_Kind_Name))]
    public AbilityCardKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the zen it costs.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCardReduction_Money_Name))]
    public int Money { get; set; }

    /// <summary>
    /// Gets or sets the item group of the stones.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCardReduction_StoneGroup_Name))]
    public byte StoneGroup { get; set; } = 20;

    /// <summary>
    /// Gets or sets the item number of the stones.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCardReduction_StoneNumber_Name))]
    public short StoneNumber { get; set; } = 239;

    /// <summary>
    /// Gets or sets the number of stones it gives.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCardReduction_Stones_Name))]
    public int Stones { get; set; }
}
