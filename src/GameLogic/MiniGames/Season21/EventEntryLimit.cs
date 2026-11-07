// <copyright file="EventEntryLimit.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// How often a character may enter an event a day (S21Emu <c>Event/EventEntryLimit.dat</c>).
/// </summary>
public class EventEntryLimit
{
    /// <summary>
    /// Gets or sets the event.
    /// </summary>
    public MiniGameType Type { get; set; }

    /// <summary>
    /// Gets or sets the entries a day.
    /// </summary>
    public int Limit { get; set; }
}
