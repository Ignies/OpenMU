// <copyright file="MuunAddOptionDefinition.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Muuns;

/// <summary>
/// The additional option of the Muuns of a type: it raises their option for the first days (MuunAddOption.xml).
/// </summary>
public class MuunAddOptionDefinition
{
    /// <summary>
    /// Gets or sets the type which the Muuns refer to (<see cref="MuunDefinition.AddOptionType"/>).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunAddOptionDefinition_Type_Name))]
    public short Type { get; set; }

    /// <summary>
    /// Gets or sets the lowest total level of the character for which this row applies.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunAddOptionDefinition_MinimumLevel_Name))]
    public int MinimumLevel { get; set; } = 1;

    /// <summary>
    /// Gets or sets the highest total level of the character for which this row applies; -1 for none.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunAddOptionDefinition_MaximumLevel_Name))]
    public int MaximumLevel { get; set; } = -1;

    /// <summary>
    /// Gets or sets how it changes the option.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunAddOptionDefinition_Kind_Name))]
    public MuunAddOptionKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the values: index 0 for the evolved Muun, 1 to 5 for the Muun levels.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunAddOptionDefinition_Values_Name))]
    public List<int> Values { get; set; } = [];

    /// <summary>
    /// Determines whether the row applies to a total level of a character.
    /// </summary>
    /// <param name="totalLevel">The total level.</param>
    /// <returns><c>true</c>, if it applies.</returns>
    public bool AppliesTo(int totalLevel) => totalLevel >= this.MinimumLevel && (this.MaximumLevel < 0 || totalLevel <= this.MaximumLevel);

    /// <summary>
    /// Applies the additional option to the value of an option.
    /// </summary>
    /// <param name="value">The value of the option.</param>
    /// <param name="level">The level of the Muun.</param>
    /// <param name="isEvolved">If set to <c>true</c>, the Muun is evolved.</param>
    /// <returns>The raised value.</returns>
    public int Apply(int value, int level, bool isEvolved)
    {
        var amount = this.GetAmount(level, isEvolved);
        return this.Kind switch
        {
            MuunAddOptionKind.Multiplier when amount > 0 => value * amount,
            MuunAddOptionKind.Bonus => value + amount,
            _ => value,
        };
    }

    /// <summary>
    /// Gets the seconds by which the additional option reduces the cooldown of the attack skill of the Muun.
    /// </summary>
    /// <param name="level">The level of the Muun.</param>
    /// <param name="isEvolved">If set to <c>true</c>, the Muun is evolved.</param>
    /// <returns>The seconds; 0 for another kind.</returns>
    public int GetCooldownReduction(int level, bool isEvolved)
        => this.Kind == MuunAddOptionKind.SkillCooldownReduction ? Math.Max(0, this.GetAmount(level, isEvolved)) : 0;

    private int GetAmount(int level, bool isEvolved)
    {
        var index = isEvolved ? 0 : Math.Clamp(level, 1, MuunDefinition.HighestLevel);
        return index < this.Values.Count ? this.Values[index] : 0;
    }
}
