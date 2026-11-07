// <copyright file="SkillBookRequirement.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

/// <summary>
/// The skills a book needs before it teaches its skill: a skill the character has, and a master skill with at least
/// a number of points.
/// </summary>
public class SkillBookRequirement
{
    /// <summary>
    /// Gets or sets the item group of the book.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SkillBookRequirement_ItemGroup_Name))]
    public byte ItemGroup { get; set; }

    /// <summary>
    /// Gets or sets the item number of the book.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SkillBookRequirement_ItemNumber_Name))]
    public short ItemNumber { get; set; }

    /// <summary>
    /// Gets or sets the skill the character must have, 0 for none. Its master skill counts as well.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SkillBookRequirement_RequiredSkill_Name))]
    public short RequiredSkill { get; set; }

    /// <summary>
    /// Gets or sets the master skill the character must have learned, 0 for none.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SkillBookRequirement_RequiredMasterSkill_Name))]
    public short RequiredMasterSkill { get; set; }

    /// <summary>
    /// Gets or sets the points the master skill needs.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SkillBookRequirement_RequiredMasterSkillLevel_Name))]
    public byte RequiredMasterSkillLevel { get; set; }
}
