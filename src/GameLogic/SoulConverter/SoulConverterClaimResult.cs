// <copyright file="SoulConverterClaimResult.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.SoulConverter;

/// <summary>
/// The result of claiming the reward of a converter, as the Babylon client numbers it.
/// </summary>
public enum SoulConverterClaimResult
{
    /// <summary>
    /// The reward was delivered.
    /// </summary>
    Rewarded = 0,

    /// <summary>
    /// The missions are not done.
    /// </summary>
    MissionsNotDone = 1,

    /// <summary>
    /// The character has no active converter.
    /// </summary>
    NoConverter = 2,

    /// <summary>
    /// The tier doesn't offer the reward kind.
    /// </summary>
    KindNotOffered = 3,

    /// <summary>
    /// The feature is off.
    /// </summary>
    Off = 4,

    /// <summary>
    /// The reward could not be delivered.
    /// </summary>
    NotDelivered = 5,
}
