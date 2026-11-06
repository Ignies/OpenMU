// <copyright file="EventEntrance.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// An NPC which lets the player enter a Season 21 event, e.g. the Portal of Dimension of the Instance Zone.
/// </summary>
public class EventEntrance
{
    /// <summary>
    /// Gets or sets the number of the NPC.
    /// </summary>
    public short NpcNumber { get; set; }

    /// <summary>
    /// Gets or sets the type of the event.
    /// </summary>
    public MiniGameType Type { get; set; }

    /// <summary>
    /// Gets or sets the game level of the event.
    /// </summary>
    public byte GameLevel { get; set; } = 1;
}
