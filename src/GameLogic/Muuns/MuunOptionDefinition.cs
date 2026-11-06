// <copyright file="MuunOptionDefinition.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Muuns;

/// <summary>
/// The option of the Muuns of an option group, for a range of character levels (MuunOption.xml).
/// </summary>
public class MuunOptionDefinition
{
    /// <summary>
    /// Gets or sets the group which the Muuns refer to (<see cref="MuunDefinition.OptionGroup"/>).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunOptionDefinition_Group_Name))]
    public short Group { get; set; }

    /// <summary>
    /// Gets or sets the type of the option.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunOptionDefinition_Type_Name))]
    public MuunOptionType Type { get; set; }

    /// <summary>
    /// Gets or sets the lowest total level of the character for which this row applies.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunOptionDefinition_MinimumLevel_Name))]
    public int MinimumLevel { get; set; } = 1;

    /// <summary>
    /// Gets or sets the highest total level of the character for which this row applies; -1 for none.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunOptionDefinition_MaximumLevel_Name))]
    public int MaximumLevel { get; set; } = -1;

    /// <summary>
    /// Gets or sets the values: index 0 is the value of the evolved Muun, 1 to 5 the values of the Muun levels.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunOptionDefinition_Values_Name))]
    public List<int> Values { get; set; } = [];

    /// <summary>
    /// Gets or sets the condition before the evolution.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunOptionDefinition_Condition_Name))]
    public MuunCondition Condition { get; set; } = new();

    /// <summary>
    /// Gets or sets the condition of the evolved Muun.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunOptionDefinition_EvolvedCondition_Name))]
    public MuunCondition EvolvedCondition { get; set; } = new();

    /// <summary>
    /// Gets or sets the name of the option as the client shows it, with <c>%d</c> for the value.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunOptionDefinition_Name_Name))]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Determines whether the row applies to a total level of a character.
    /// </summary>
    /// <param name="totalLevel">The total level.</param>
    /// <returns><c>true</c>, if it applies.</returns>
    public bool AppliesTo(int totalLevel) => totalLevel >= this.MinimumLevel && (this.MaximumLevel < 0 || totalLevel <= this.MaximumLevel);

    /// <summary>
    /// Gets the value for a Muun level, or the evolved value.
    /// </summary>
    /// <param name="level">The level of the Muun, 1 to 5.</param>
    /// <param name="isEvolved">If set to <c>true</c>, the Muun is evolved.</param>
    /// <returns>The value.</returns>
    public int GetValue(int level, bool isEvolved)
    {
        var index = isEvolved ? 0 : Math.Clamp(level, 1, MuunDefinition.HighestLevel);
        return index < this.Values.Count ? this.Values[index] : 0;
    }
}
