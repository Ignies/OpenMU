// <copyright file="MiningResult.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Mining;

/// <summary>
/// The result of a mining request, as the Babylon client shows it.
/// </summary>
public enum MiningResult
{
    /// <summary>
    /// The player talked to the spot; the mining window opens.
    /// </summary>
    Opened,

    /// <summary>
    /// The stage was mined; its reward is pending.
    /// </summary>
    Succeeded,

    /// <summary>
    /// The stage failed; the pending reward is lost.
    /// </summary>
    Failed,

    /// <summary>
    /// The pending reward was collected.
    /// </summary>
    Collected,

    /// <summary>
    /// The character holds no mining pickaxe.
    /// </summary>
    NoPickaxe,

    /// <summary>
    /// The pickaxe has not enough durability left for the stage.
    /// </summary>
    PickaxeWorn,

    /// <summary>
    /// No mining spot is in reach.
    /// </summary>
    NoSpot,

    /// <summary>
    /// Another player mines the spot.
    /// </summary>
    Occupied,

    /// <summary>
    /// Mining is off.
    /// </summary>
    Off,
}
