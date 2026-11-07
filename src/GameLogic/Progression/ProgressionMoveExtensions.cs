// <copyright file="ProgressionMoveExtensions.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Progression;

using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// The effective requirements and costs of warp list entries, gates and items, with the progression of the Season 21 maps.
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
        return GetLevelRequirement(gameContext, warp.Gate?.Map, warp.LevelRequirement, c => c.Season21ClassicWarpLevels, warp.Index);
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
        return GetLevelRequirement(gameContext, gate.TargetGate?.Map, gate.LevelRequirement, c => c.Season21ClassicGateLevels, gate.Number);
    }

    /// <summary>
    /// Gets the level which the level requirement of an item is checked against, when the progression decides it: the
    /// power level (level plus the worth of the stars, capped) in the stars mode.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The level, or <c>null</c> when the normal level applies.</returns>
    public static int? GetItemRequirementLevel(this Player player)
    {
        if (ProgressionFeaturePlugIn.GetWorld(player.GameContext) is not { } world || player.Attributes is not { } attributes)
        {
            return null;
        }

        var key = ProgressionMapping.GetItemLevelRequirementKey(world.Configuration);
        return key == ProgressionLevelKey.Level
            ? null
            : StarRules.GetRequirementLevel(key, (int)attributes[Stats.Level], (int)attributes[Stats.MasterLevel], (int)attributes[Stats.Resets], world.Configuration);
    }

    /// <summary>
    /// Gets the level a move to a map requires: a Season 21 map's from the world, a classic map's by
    /// <see cref="ProgressionConfiguration.ClassicMapEntryLevels"/> when the Season 21 table holds it, else the stored one.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <param name="map">The target map.</param>
    /// <param name="storedRequirement">The stored level requirement.</param>
    /// <param name="table">The Season 21 table of the move kind.</param>
    /// <param name="key">The warp index or gate number.</param>
    /// <returns>The level requirement.</returns>
    private static int GetLevelRequirement(IGameContext gameContext, GameMapDefinition? map, int storedRequirement, Func<ProgressionConfiguration, Dictionary<int, int>> table, int key)
    {
        if (ProgressionFeaturePlugIn.GetWorld(gameContext) is { } world && world.TryGetRequirement(map, out var requirement))
        {
            return requirement.Level;
        }

        return ProgressionFeaturePlugIn.GetConfiguration(gameContext) is { } configuration && table(configuration).TryGetValue(key, out var season21Level)
            ? ProgressionMapping.GetClassicEntryLevel(storedRequirement, season21Level, configuration)
            : storedRequirement;
    }
}
