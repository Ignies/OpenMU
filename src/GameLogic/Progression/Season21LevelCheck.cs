// <copyright file="Season21LevelCheck.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Progression;

using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.PlayerActions;

/// <summary>
/// Checks a Season 21 total level, like the map check: mapped by the progression mode when it's on.
/// </summary>
public static class Season21LevelCheck
{
    /// <summary>
    /// Determines whether a player meets a Season 21 total level.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="season21Level">The Season 21 total level.</param>
    /// <returns><c>true</c>, if the player meets it.</returns>
    public static bool Meets(Player player, int season21Level)
    {
        if (ProgressionFeaturePlugIn.GetWorld(player.GameContext) is { } world)
        {
            return GameMapDefinitionExtensions.GetMissingAttribute(player, ProgressionMapping.GetRequirement(season21Level, world.Configuration)) is null;
        }

        var attributes = player.Attributes;
        return (attributes?[Stats.Level] ?? 0) + (attributes?[Stats.MasterLevel] ?? 0) >= player.GameContext.Configuration.GetReachableTotalLevel(season21Level);
    }
}
