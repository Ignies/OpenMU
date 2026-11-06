// <copyright file="EnhanceTreePassiveStage.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EnhanceTree;

/// <summary>
/// A stage of a passive of the skill enhancement tree.
/// </summary>
public class EnhanceTreePassiveStage
{
    /// <summary>
    /// Gets or sets the points spent in the tree which unlock the stage.
    /// </summary>
    public short PointCondition { get; set; }

    /// <summary>
    /// Gets or sets the points one level of the stage costs.
    /// </summary>
    public byte PointsPerLevel { get; set; }

    /// <summary>
    /// Gets or sets the highest level of the passive in this stage.
    /// </summary>
    public byte MaximumLevel { get; set; }

    /// <summary>
    /// Gets or sets the id of the formula of the stage's value.
    /// </summary>
    public short FormulaId { get; set; }
}
