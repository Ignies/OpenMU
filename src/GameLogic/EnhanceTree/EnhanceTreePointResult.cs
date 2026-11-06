// <copyright file="EnhanceTreePointResult.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EnhanceTree;

/// <summary>
/// The result of adding a point to the skill enhancement tree. The numbers are the result byte of the Babylon packet.
/// </summary>
public enum EnhanceTreePointResult
{
    /// <summary>
    /// The point was added.
    /// </summary>
    Added = 0,

    /// <summary>
    /// The tree is off, or the character's class has no tree (before the fourth evolution).
    /// </summary>
    NotAvailable = 1,

    /// <summary>
    /// The skill is no node of the class's tree.
    /// </summary>
    UnknownNode = 2,

    /// <summary>
    /// A required skill is missing or below its points, or no stage of a passive is unlocked.
    /// </summary>
    RequirementMissing = 3,

    /// <summary>
    /// Not enough free points.
    /// </summary>
    NotEnoughPoints = 4,

    /// <summary>
    /// The node is at its maximum.
    /// </summary>
    MaximumReached = 5,
}
