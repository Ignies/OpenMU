// <copyright file="MiniGameDefinitionExtensions.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.MiniGames;

using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// Extensions regarding the <see cref="MiniGameDefinition"/>s.
/// </summary>
public static class MiniGameDefinitionExtensions
{
    /// <summary>
    /// Gets the suitable mini game definition for the player and mini game type.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="miniGameType">Type of the mini game.</param>
    /// <param name="eventLevel">The event level.</param>
    /// <returns>The suitable mini game definition.</returns>
    public static MiniGameDefinition? GetSuitableMiniGameDefinition(this Player player, MiniGameType miniGameType, byte eventLevel)
    {
        var definitions = player.GameContext.Configuration.MiniGameDefinitions
            .Where(def => def.Type == miniGameType && def.IsAvailable())
            .ToList();
        return definitions
                   .OrderByDescending(def => def.GameLevel)
                   .FirstOrDefault(def => def.IsInLevelRange(player))
               ?? definitions.FirstOrDefault(def => def.GameLevel == eventLevel);
    }

    /// <summary>
    /// Determines whether the mini game is available in the game, which requires that
    /// it's active, and that it has an entrance map which is active, too.
    /// </summary>
    /// <param name="definition">The definition of the mini game.</param>
    /// <returns><c>true</c> if the mini game is available; otherwise, <c>false</c>.</returns>
    public static bool IsAvailable(this MiniGameDefinition definition)
    {
        return definition.IsActive && definition.Entrance?.Map is { IsActive: true };
    }

    /// <summary>
    /// Determines whether the specified player is in level range of the mini game.
    /// </summary>
    /// <param name="definition">The definition of the mini game.</param>
    /// <param name="player">The player.</param>
    /// <returns><c>true</c> if the specified player is in level range; otherwise, <c>false</c>.</returns>
    public static bool IsInLevelRange(this MiniGameDefinition definition, Player player)
    {
        if (definition.RequiresMasterClass && player.SelectedCharacter?.CharacterClass?.IsMasterClass is not true)
        {
            return false;
        }

        var isSpecialCharacter = player.SelectedCharacter!.IsSpecialCharacter();
        var level = definition.GetBracketLevel(player, isSpecialCharacter);
        if (isSpecialCharacter)
        {
            return level >= definition.MinimumSpecialCharacterLevel
                   && level <= definition.MaximumSpecialCharacterLevel;
        }

        return level >= definition.MinimumCharacterLevel
               && level <= definition.MaximumCharacterLevel;
    }

    /// <summary>
    /// Gets the level which the level bracket of the mini game checks for a player, see
    /// <see cref="GetBracketLevel(MiniGameDefinition, int, int, short, bool)"/>.
    /// </summary>
    /// <param name="definition">The definition of the mini game.</param>
    /// <param name="player">The player.</param>
    /// <param name="isSpecialCharacter">Whether the character is a special character, which has its own bracket.</param>
    /// <returns>The level which the bracket checks.</returns>
    public static int GetBracketLevel(this MiniGameDefinition definition, Player player, bool isSpecialCharacter)
    {
        var attributes = player.Attributes;
        return definition.GetBracketLevel(
            (int)(attributes?[Stats.Level] ?? 0),
            (int)(attributes?[Stats.MasterLevel] ?? 0),
            player.GameContext.Configuration.MaximumLevel,
            isSpecialCharacter);
    }

    /// <summary>
    /// Gets the level which the level bracket of the mini game checks for a character: the normal level for a
    /// bracket within the maximum normal level (Season 6), the total level (level and master level) for a bracket
    /// which reaches above it (the grades of Season 21).
    /// </summary>
    /// <param name="definition">The definition of the mini game.</param>
    /// <param name="level">The normal level of the character.</param>
    /// <param name="masterLevel">The master level of the character.</param>
    /// <param name="maximumLevel">The maximum normal level of the game configuration.</param>
    /// <param name="isSpecialCharacter">Whether the character is a special character, which has its own bracket.</param>
    /// <returns>The level which the bracket checks.</returns>
    public static int GetBracketLevel(this MiniGameDefinition definition, int level, int masterLevel, short maximumLevel, bool isSpecialCharacter = false)
    {
        var maximum = isSpecialCharacter ? definition.MaximumSpecialCharacterLevel : definition.MaximumCharacterLevel;
        return maximum > maximumLevel ? level + masterLevel : level;
    }
}
