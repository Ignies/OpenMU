// <copyright file="SeasonPassReward.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.SeasonPass;

/// <summary>
/// A reward of the season pass (<c>Reward</c>): a buff (<see cref="EffectIndex"/>) or an item.
/// </summary>
public class SeasonPassReward
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassReward_Id_Name))]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the buff of the reward (<see cref="SeasonPassBuff.EffectIndex"/>); -1 for an item.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassReward_EffectIndex_Name))]
    public int EffectIndex { get; set; } = -1;

    /// <summary>
    /// Gets or sets the group of the item.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassReward_ItemGroup_Name))]
    public byte ItemGroup { get; set; }

    /// <summary>
    /// Gets or sets the number of the item.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassReward_ItemNumber_Name))]
    public short ItemNumber { get; set; }

    /// <summary>
    /// Gets or sets the count (durability) of the item; 0 for the one of its definition.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassReward_Count_Name))]
    public byte Count { get; set; }

    /// <summary>
    /// Gets or sets the name of the source.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassReward_Name_Name))]
    public string Name { get; set; } = string.Empty;

    /// <inheritdoc />
    public override string ToString() => this.Name;
}
