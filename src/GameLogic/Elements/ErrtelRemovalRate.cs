// <copyright file="ErrtelRemovalRate.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Elements;

/// <summary>
/// The chances to take an errtel of one level out of a pentagram, by the rank of the errtel.
/// </summary>
public class ErrtelRemovalRate
{
    /// <summary>
    /// Gets or sets the level of the errtel (the level of its highest rank option).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ErrtelRemovalRate_Level_Name))]
    public int Level { get; set; }

    /// <summary>
    /// Gets or sets the chances in 1/10000 by rank: the first for rank 1, the second for rank 2, and so on.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ErrtelRemovalRate_Rates_Name))]
    public List<int> Rates { get; set; } = [];

    /// <inheritdoc />
    public override string ToString() => $"Level {this.Level}: {string.Join(", ", this.Rates)}";
}
