// <copyright file="ProgressionMoveExtensions.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// The effective requirements and costs of warp list entries and gates, with the progression of the Season 21 maps.
/// </summary>
public static class ProgressionMoveExtensions
{
    /// <summary>
    /// Gets the normal level which a warp list entry requires.
    /// </summary>
    /// <param name="warp">The warp list entry.</param>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The level requirement.</returns>
    public static int GetLevelRequirement(this WarpInfo warp, IGameContext gameContext)
    {
        return ProgressionFeaturePlugIn.GetWorld(gameContext)?.GetMoveLevelRequirement(warp.Gate?.Map, warp.LevelRequirement) ?? warp.LevelRequirement;
    }

    /// <summary>
    /// Gets the zen which a warp list entry costs.
    /// </summary>
    /// <param name="warp">The warp list entry.</param>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The zen.</returns>
    public static int GetCosts(this WarpInfo warp, IGameContext gameContext)
    {
        return ProgressionFeaturePlugIn.GetWorld(gameContext)?.GetWarpCosts(warp) ?? warp.Costs;
    }

    /// <summary>
    /// Gets the normal level which a gate requires.
    /// </summary>
    /// <param name="gate">The gate.</param>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The level requirement.</returns>
    public static int GetLevelRequirement(this EnterGate gate, IGameContext gameContext)
    {
        return ProgressionFeaturePlugIn.GetWorld(gameContext)?.GetMoveLevelRequirement(gate.TargetGate?.Map, gate.LevelRequirement) ?? gate.LevelRequirement;
    }
}
