// <copyright file="SeasonPassClaimResult.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.SeasonPass;

/// <summary>
/// The result of claiming a reward of the season pass, as the Babylon client numbers it.
/// </summary>
public enum SeasonPassClaimResult
{
    /// <summary>
    /// The reward was claimed.
    /// </summary>
    Claimed = 0,

    /// <summary>
    /// The level is not reached.
    /// </summary>
    LevelNotReached = 1,

    /// <summary>
    /// The reward was already claimed.
    /// </summary>
    AlreadyClaimed = 2,

    /// <summary>
    /// The pass is not owned.
    /// </summary>
    PassNotOwned = 3,

    /// <summary>
    /// The level has no reward in the pass.
    /// </summary>
    NoReward = 4,

    /// <summary>
    /// The reward could not be delivered.
    /// </summary>
    NotDelivered = 5,

    /// <summary>
    /// The season pass is off, or no season runs.
    /// </summary>
    Off = 6,
}
