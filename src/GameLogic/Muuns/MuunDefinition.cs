// <copyright file="MuunDefinition.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Muuns;

/// <summary>
/// A Muun: an item of the Muun item group with its option, its additional option and its evolution (MuunInfo.xml).
/// </summary>
public class MuunDefinition
{
    /// <summary>
    /// The highest level of any Muun.
    /// </summary>
    internal const byte HighestLevel = 5;

    /// <summary>
    /// Gets or sets the number of the item in the Muun item group.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunDefinition_Number_Name))]
    public short Number { get; set; }

    /// <summary>
    /// Gets or sets the kind.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunDefinition_Kind_Name))]
    public MuunKind Kind { get; set; } = MuunKind.Normal;

    /// <summary>
    /// Gets or sets the grade.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunDefinition_Grade_Name))]
    public MuunGrade Grade { get; set; }

    /// <summary>
    /// Gets or sets the rank, 1 to 4: the Muun reaches the level rank + 1 (at most 5).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunDefinition_Rank_Name))]
    [Range(1, 6)]
    public byte Rank { get; set; } = 1;

    /// <summary>
    /// Gets or sets the group of its option (<see cref="MuunOptionDefinition.Group"/>).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunDefinition_OptionGroup_Name))]
    public short OptionGroup { get; set; }

    /// <summary>
    /// Gets or sets the type of its additional option (<see cref="MuunAddOptionDefinition.Type"/>).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunDefinition_AddOptionType_Name))]
    public short AddOptionType { get; set; }

    /// <summary>
    /// Gets or sets the days for which the additional option applies, from the moment the Muun first enters a Muun
    /// inventory (<see cref="Item.TimedOptionExpiresAt"/>); 0 for never.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunDefinition_AddOptionDays_Name))]
    public int AddOptionDays { get; set; }

    /// <summary>
    /// Gets or sets the number of the evolved Muun; <c>null</c>, if it doesn't evolve.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunDefinition_EvolvesTo_Name))]
    public short? EvolvesTo { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the Muun is the evolved form of another one.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunDefinition_IsEvolved_Name))]
    public bool IsEvolved { get; set; }

    /// <summary>
    /// Gets the highest level of the Muun.
    /// </summary>
    public byte MaximumLevel => (byte)Math.Clamp(this.Rank + 1, 1, HighestLevel);
}
