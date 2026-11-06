// <copyright file="ISoulConverterViewPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.World;

using MUnique.OpenMU.GameLogic.SoulConverter;

/// <summary>
/// Interface of a view whose implementation shows the monster soul converter.
/// </summary>
public interface ISoulConverterViewPlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows the state of the converter.
    /// </summary>
    /// <param name="tier">The tier of the active converter; 0 when none is active.</param>
    /// <param name="activationsLeft">The converters the character may still activate this week.</param>
    /// <param name="expiresAt">The point in time at which the active converter expires; <c>null</c> when none is active.</param>
    /// <param name="missions">The missions of the active converter.</param>
    ValueTask ShowSoulConverterStateAsync(byte tier, int activationsLeft, DateTime? expiresAt, IReadOnlyList<SoulMissionProgress> missions);

    /// <summary>
    /// Shows the result of a claimed reward.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="kind">The reward kind.</param>
    ValueTask ShowSoulConverterResultAsync(SoulConverterClaimResult result, SoulRewardKind kind);
}
