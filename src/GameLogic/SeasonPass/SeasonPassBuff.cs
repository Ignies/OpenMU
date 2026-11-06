// <copyright file="SeasonPassBuff.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.SeasonPass;

/// <summary>
/// A buff of the season pass (<c>BuffEffect</c>): it raises an attribute until the season ends, by the value of the
/// highest level at which it was claimed.
/// </summary>
public class SeasonPassBuff
{
    /// <summary>
    /// Gets or sets the effect index.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassBuff_EffectIndex_Name))]
    public int EffectIndex { get; set; }

    /// <summary>
    /// Gets or sets the id of the attribute (<see cref="Attributes.Stats"/>) the buff raises; <c>null</c> when the effect has no
    /// attribute of this server yet.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassBuff_AttributeId_Name))]
    public Guid? AttributeId { get; set; }

    /// <summary>
    /// Gets or sets the values by the level of the claim, from level 1.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassBuff_Values_Name))]
    public List<int> Values { get; set; } = [];

    /// <summary>
    /// Gets or sets the name of the source.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassBuff_Name_Name))]
    public string Name { get; set; } = string.Empty;

    /// <inheritdoc />
    public override string ToString() => this.Name;
}
