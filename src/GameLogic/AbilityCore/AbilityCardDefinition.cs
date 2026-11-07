// <copyright file="AbilityCardDefinition.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.AbilityCore;

using MUnique.OpenMU.GameLogic.Classes;

/// <summary>
/// An ability card: the item which is one, its kind, color and type, and for a skill card the skill it gives.
/// </summary>
public class AbilityCardDefinition
{
    /// <summary>
    /// Gets or sets the item group of the card.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCardDefinition_ItemGroup_Name))]
    public byte ItemGroup { get; set; } = 20;

    /// <summary>
    /// Gets or sets the item number of the card.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCardDefinition_ItemNumber_Name))]
    public short ItemNumber { get; set; }

    /// <summary>
    /// Gets or sets the kind.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCardDefinition_Kind_Name))]
    public AbilityCardKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the color of an option or elite card.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCardDefinition_Color_Name))]
    public AbilityCardColor Color { get; set; }

    /// <summary>
    /// Gets or sets the type: 1 to 5 for an option card, 0 or 1 for a skill card (S21Emu <c>CardType</c>, the two
    /// cards of a class convert into each other).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCardDefinition_Type_Name))]
    public byte Type { get; set; }

    /// <summary>
    /// Gets or sets the base classes which may put the card into the core, a bit per <see cref="BaseClassCode"/>.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCardDefinition_BaseClasses_Name))]
    public int BaseClasses { get; set; } = 0xFFFF;

    /// <summary>
    /// Gets or sets the fifth evolution skill which a skill card gives, 0 for none.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCardDefinition_Skill_Name))]
    public short Skill { get; set; }

    /// <summary>
    /// Gets or sets the fourth evolution skill of the skill enhancement tree which the character must have learned
    /// to use a skill card, 0 for none.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCardDefinition_RequiredSkill_Name))]
    public short RequiredSkill { get; set; }

    /// <summary>
    /// Gets or sets the static option of a skill card, which changes its skill (0 hits, 1 attack speed, 2 range,
    /// 3 splash damage, 4 enemy defense); data only, no skill reads it yet.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCardDefinition_StaticOption_Name))]
    public byte StaticOption { get; set; }

    /// <summary>
    /// Gets or sets the values of the static option by card level.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCardDefinition_StaticValues_Name))]
    public List<float> StaticValues { get; set; } = [];

    /// <summary>
    /// Gets or sets the options a card of this kind can carry.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCardDefinition_Options_Name))]
    public List<AbilityCardOption> Options { get; set; } = [];

    /// <summary>
    /// Determines whether a base class may use the card.
    /// </summary>
    /// <param name="baseClass">The base class.</param>
    /// <returns><c>true</c>, if it may.</returns>
    public bool IsFor(BaseClassCode baseClass) => (this.BaseClasses & (1 << (int)baseClass)) != 0;
}
