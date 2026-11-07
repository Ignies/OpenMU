// <copyright file="Season21EventsConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The configuration of the events of Season 21 which run as mini games. The definitions are set by the update of each
/// event; an event without a definition has nothing to play.
/// </summary>
public class Season21EventsConfiguration
{
    /// <summary>
    /// Gets or sets the Tormented Square.
    /// </summary>
    public RoundsEventDefinition? TormentedSquare { get; set; }

    /// <summary>
    /// Gets or sets the Labyrinth of Dimensions.
    /// </summary>
    public RoundsEventDefinition? Labyrinth { get; set; }

    /// <summary>
    /// Gets or sets the Protector of Acheron, by its game level (one map each).
    /// </summary>
    public List<RoundsEventDefinition> AcheronGuardian { get; set; } = [];

    /// <summary>
    /// Gets or sets the Boss Battle Together.
    /// </summary>
    public BossBattleDefinition? BossBattle { get; set; }

    /// <summary>
    /// Gets or sets the NPCs which let the players enter an event (the Season 6 client has no event window).
    /// </summary>
    public List<EventEntrance> Entrances { get; set; } = [];

    /// <summary>
    /// Gets or sets how often a character may enter an event a day; an event without a limit (or with 0) has none. The
    /// event rules update sets the ones of S21Emu <c>Event/EventEntryLimit.dat</c>.
    /// </summary>
    public List<EventEntryLimit> DailyEntryLimits { get; set; } = [];

    /// <summary>
    /// Gets or sets the state file of the daily entry counts, relative to the server directory; an empty path keeps them
    /// in memory.
    /// </summary>
    public string EntryLimitStateFile { get; set; } = "state/event_entries.json";

    /// <summary>
    /// Gets the definition of a rounds event.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="gameLevel">The game level.</param>
    /// <returns>The definition, or <c>null</c>.</returns>
    public RoundsEventDefinition? GetRounds(MiniGameType type, byte gameLevel) => type switch
    {
        MiniGameType.TormentedSquare => this.TormentedSquare,
        MiniGameType.LabyrinthOfDimensions => this.Labyrinth,
        MiniGameType.AcheronGuardian => this.AcheronGuardian.ElementAtOrDefault(gameLevel - 1),
        _ => null,
    };
}
