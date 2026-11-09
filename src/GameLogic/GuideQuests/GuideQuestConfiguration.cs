// <copyright file="GuideQuestConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.GuideQuests;

/// <summary>
/// Configuration of the <see cref="GuideQuestPlugIn"/>.
/// </summary>
public class GuideQuestConfiguration
{
    /// <summary>
    /// Gets or sets the steps, in the order the characters go through them (S21Emu <c>QuestGuide.txt</c>, 186 steps).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.GuideQuestConfiguration_Steps_Name))]
    public List<GuideQuestStep> Steps { get; set; } = [];

    /// <summary>
    /// Gets or sets how near the spot of a move step a character has to come, in tiles (design).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.GuideQuestConfiguration_ArrivalDistance_Name))]
    [Range(1, 30)]
    public byte ArrivalDistance { get; set; } = 5;

    /// <summary>
    /// Gets or sets the minimum time between two writes of the state file for a kill (design); accepting and completing write at once.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.GuideQuestConfiguration_SaveInterval_Name))]
    public TimeSpan SaveInterval { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets the file which keeps the states of the characters over a restart, relative to the server directory; empty for none.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.GuideQuestConfiguration_StateFile_Name))]
    public string StateFile { get; set; } = "state/guide_quests.json";
}
