// <copyright file="LearnablesConsumeHandlerConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

/// <summary>
/// Configuration for the <see cref="LearnablesConsumeHandlerPlugIn"/>: the skills which some books need besides the
/// requirements of the item and its skill.
/// </summary>
public class LearnablesConsumeHandlerConfiguration
{
    /// <summary>
    /// Gets or sets the requirements of the books (Season 21: S21Emu <c>Skill/SkillRequire.txt</c>).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.LearnablesConsumeHandlerConfiguration_Requirements_Name))]
    public List<SkillBookRequirement> Requirements { get; set; } = [];
}
