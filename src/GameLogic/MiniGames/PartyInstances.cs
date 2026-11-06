// <copyright file="PartyInstances.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames;

using MUnique.OpenMU.GameLogic.PlayerActions.MiniGames;

/// <summary>
/// Finds and enters the party instances of Season 21 (<see cref="MiniGameType.PartyInstance"/>).
/// </summary>
public static class PartyInstances
{
    /// <summary>
    /// The ticket slot which says "no ticket" to <see cref="EnterMiniGameAction"/>.
    /// </summary>
    private const byte NoTicket = 0xFF;

    /// <summary>
    /// Gets the available party instance whose entrance is on the map, if the world has one.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <param name="map">The map.</param>
    /// <returns>The definition, or <c>null</c>.</returns>
    public static MiniGameDefinition? FindForMap(IGameContext gameContext, GameMapDefinition map)
        => gameContext.Configuration.MiniGameDefinitions.FirstOrDefault(d => d.Type == MiniGameType.PartyInstance && Equals(d.Entrance?.Map, map) && d.IsAvailable());

    /// <summary>
    /// Lets the player enter the instance of its party (a new one when the party has none running), with the usual
    /// entry rules of a mini game.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="definition">The definition of the instance.</param>
    public static ValueTask EnterAsync(Player player, MiniGameDefinition definition)
        => new EnterMiniGameAction().TryEnterMiniGameAsync(player, definition.Type, definition.GameLevel, NoTicket);
}
