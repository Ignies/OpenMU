// <copyright file="Season21EventsFeaturePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlayerActions.MiniGames;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Holds the definitions of the events of Season 21 which run as mini games (Tormented Square, Labyrinth of
/// Dimensions, Protector of Acheron, Boss Battle Together), and lets the players enter them at their NPCs
/// (<see cref="Season21EventsConfiguration.Entrances"/>). Each scheduled event is switched by its start plugin.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.Season21EventsFeaturePlugIn_Name), Description = nameof(PlugInResources.Season21EventsFeaturePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("EF98A1E3-573C-4F97-BA98-72C2AF87D685")]
public class Season21EventsFeaturePlugIn : IFeaturePlugIn, ISupportCustomConfiguration<Season21EventsConfiguration>, ISupportDefaultCustomConfiguration, IPlayerTalkToNpcPlugIn
{
    /// <summary>
    /// The ticket slot which says "no ticket".
    /// </summary>
    private const byte NoTicket = 0xFF;

    private Season21EventsConfiguration? _configuration;

    /// <inheritdoc />
    public Season21EventsConfiguration? Configuration
    {
        get => this._configuration;
        set
        {
            this._configuration = value;
            this.Entries = new EventEntryCounter(value?.EntryLimitStateFile);
        }
    }

    /// <summary>
    /// Gets the counts of the event entries, kept in <see cref="Season21EventsConfiguration.EntryLimitStateFile"/>.
    /// </summary>
    public EventEntryCounter Entries { get; private set; } = new();

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// Gets the configuration of the events of a game, or an empty one.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The configuration.</returns>
    public static Season21EventsConfiguration GetConfiguration(IGameContext gameContext)
        => gameContext.FeaturePlugIns.GetPlugIn<Season21EventsFeaturePlugIn>()?.Configuration ?? new Season21EventsConfiguration();

    /// <summary>
    /// Gets a value indicating whether the player's character entered an event as often as the daily limit allows.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="type">The event.</param>
    /// <returns><c>true</c>, if the limit is reached.</returns>
    public static bool IsDailyLimitReached(Player player, MiniGameType type)
    {
        return player.GameContext.FeaturePlugIns.GetPlugIn<Season21EventsFeaturePlugIn>() is { Configuration: { } configuration } plugIn
               && player.SelectedCharacter is { } character
               && plugIn.Entries.IsLimitReached(character.Id, type, configuration.DailyEntryLimits, Today);
    }

    /// <summary>
    /// Counts an entry of the player's character into an event.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="type">The event.</param>
    public static void CountEntry(Player player, MiniGameType type)
    {
        if (player.GameContext.FeaturePlugIns.GetPlugIn<Season21EventsFeaturePlugIn>() is { Configuration.DailyEntryLimits.Count: > 0 } plugIn
            && player.SelectedCharacter is { } character)
        {
            plugIn.Entries.Count(character.Id, type, Today, player.Logger);
        }
    }

    /// <inheritdoc />
    public object CreateDefaultConfig() => new Season21EventsConfiguration();

    /// <inheritdoc />
    public async ValueTask PlayerTalksToNpcAsync(Player player, NonPlayerCharacter npc, NpcTalkEventArgs eventArgs)
    {
        if (this.Configuration?.Entrances.FirstOrDefault(e => e.NpcNumber == npc.Definition.Number) is not { } entrance)
        {
            return;
        }

        // The entry warps the player, which changes its state itself, so the talk action must leave it alone.
        eventArgs.HasBeenHandled = true;
        eventArgs.LeavesDialogOpen = true;
        await new EnterMiniGameAction().TryEnterMiniGameAsync(player, entrance.Type, entrance.GameLevel, NoTicket).ConfigureAwait(false);
        player.OpenedNpc = null;
    }
}
