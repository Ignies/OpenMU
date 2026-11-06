// <copyright file="PlayerEvolutionExtensions.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Classes;

using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// Extensions which read the <see cref="EvolutionRules"/> for a player in its world.
/// </summary>
public static class PlayerEvolutionExtensions
{
    /// <summary>
    /// Determines whether the player meets the level rule of an evolution in the progression mode of its world.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="evolution">The evolution.</param>
    /// <returns><c>true</c>, if the player meets the rule.</returns>
    public static bool MeetsEvolutionRule(this Player player, ClassEvolution evolution)
    {
        return player.Attributes is { } attributes
               && EvolutionRules.IsMet(
                   evolution,
                   (int)attributes[Stats.Level],
                   (int)attributes[Stats.MasterLevel],
                   (int)attributes[Stats.Resets],
                   ProgressionFeaturePlugIn.GetWorld(player.GameContext)?.Configuration);
    }

    /// <summary>
    /// Gets the level which an evolution asks for in the progression mode of the player's world.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="evolution">The evolution.</param>
    /// <returns>The level and the key it's compared with.</returns>
    public static (int Level, ProgressionLevelKey Key) GetEvolutionRequirement(this Player player, ClassEvolution evolution)
    {
        var configuration = ProgressionFeaturePlugIn.GetWorld(player.GameContext)?.Configuration;
        return (EvolutionRules.GetRequiredLevel(evolution, configuration), EvolutionRules.GetLevelKey(configuration));
    }
}
