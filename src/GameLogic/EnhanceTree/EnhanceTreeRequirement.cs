// <copyright file="EnhanceTreeRequirement.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EnhanceTree;

/// <summary>
/// A skill which a node of the skill enhancement tree requires, with the points it needs.
/// </summary>
public class EnhanceTreeRequirement
{
    /// <summary>
    /// Gets or sets the number of the required skill.
    /// </summary>
    public short Skill { get; set; }

    /// <summary>
    /// Gets or sets the points the required skill needs; <c>-1</c> when it only has to be learned.
    /// </summary>
    public short Points { get; set; }
}
