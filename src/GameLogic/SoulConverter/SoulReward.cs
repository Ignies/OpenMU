// <copyright file="SoulReward.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.SoulConverter;

/// <summary>
/// A reward of a converter tier: every draw is rolled when the player claims it.
/// </summary>
public class SoulReward
{
    /// <summary>
    /// Gets or sets the kind.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SoulReward_Kind_Name))]
    public SoulRewardKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the draws.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SoulReward_Draws_Name))]
    public List<SoulRewardDraw> Draws { get; set; } = [];

    /// <inheritdoc />
    public override string ToString() => this.Kind.ToString();
}
