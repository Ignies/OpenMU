// <copyright file="EventContributionPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Rewards;

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.MiniGames;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The event contribution of Season 21: kills in an event give points, and at its end every player with enough points
/// gets a Mastery Box into the gremory case (into the inventory when the case isn't available).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.EventContributionPlugIn_Name), Description = nameof(PlugInResources.EventContributionPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("6B92D4E1-0A3C-4F57-9E18-C5D7A2B4F830")]
public class EventContributionPlugIn : IFeaturePlugIn, IDisabledByDefault, ISupportCustomConfiguration<EventContributionConfiguration>, ISupportDefaultCustomConfiguration,
    IAttackableGotKilledPlugIn, IMiniGameEndedPlugIn, IMiniGameObjectiveReachedPlugIn
{
    private readonly ConditionalWeakTable<object, ConcurrentDictionary<Player, int>> _points = new();

    /// <inheritdoc />
    public EventContributionConfiguration? Configuration { get; set; }

    /// <inheritdoc />
    public object CreateDefaultConfig() => new EventContributionConfiguration();

    /// <summary>
    /// Gets the points of a player in a running event.
    /// </summary>
    /// <param name="eventKey">The event, e.g. its <see cref="MiniGameContext"/>.</param>
    /// <param name="player">The player.</param>
    /// <returns>The points.</returns>
    public int GetPoints(object eventKey, Player player)
        => this._points.TryGetValue(eventKey, out var points) && points.TryGetValue(player, out var value) ? value : 0;

    /// <summary>
    /// Gets the box for the points, if any.
    /// </summary>
    /// <param name="points">The points.</param>
    /// <returns>The box, or <c>null</c> when the points give none.</returns>
    public ContributionBox? GetBox(int points)
        => this.Configuration?.Boxes.FirstOrDefault(b => points >= b.MinimumPoints && (b.MaximumPoints <= 0 || points <= b.MaximumPoints));

    /// <inheritdoc />
    public ValueTask AttackableGotKilledAsync(IAttackable killed, IAttacker? killer)
    {
        if (killer is Player { CurrentMiniGame: { } miniGame } player)
        {
            if (killed is Player)
            {
                this.AddPlayerKill(miniGame, miniGame.Definition.Type, player);
            }
            else if (killed is NonPlayerCharacter npc)
            {
                this.AddMonsterKill(miniGame, miniGame.Definition.Type, player, npc.Definition.Number, DateTime.Now.DayOfWeek);
            }
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Adds the points of a killed monster.
    /// </summary>
    /// <param name="eventKey">The event, e.g. its <see cref="MiniGameContext"/>.</param>
    /// <param name="type">The kind of event.</param>
    /// <param name="killer">The killer.</param>
    /// <param name="monsterNumber">The number of the killed monster.</param>
    /// <param name="day">The day of the week.</param>
    public void AddMonsterKill(object eventKey, MiniGameType type, Player killer, short monsterNumber, DayOfWeek day)
    {
        if (this.GetRule(type) is not { } rule)
        {
            return;
        }

        var points = rule.SpecialMonsters.FirstOrDefault(m => m.MonsterNumber == monsterNumber)?.Points
                     ?? (day == DayOfWeek.Sunday && rule.SundayMonsterKillPoints > 0 ? rule.SundayMonsterKillPoints : rule.MonsterKillPoints);
        this.Add(eventKey, killer, points);
    }

    /// <summary>
    /// Adds the points of a killed player.
    /// </summary>
    /// <param name="eventKey">The event, e.g. its <see cref="MiniGameContext"/>.</param>
    /// <param name="type">The kind of event.</param>
    /// <param name="killer">The killer.</param>
    public void AddPlayerKill(object eventKey, MiniGameType type, Player killer)
    {
        if (this.GetRule(type) is { } rule)
        {
            this.Add(eventKey, killer, rule.PlayerKillPoints);
        }
    }

    /// <inheritdoc />
    public ValueTask ObjectiveReachedAsync(MiniGameContext miniGame, Player player, MiniGameObjective objective, int value)
    {
        this.AddObjective(miniGame, miniGame.Definition.Type, player, objective, value);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Adds the points of a reached objective: a rank at the end of the event, a taken or delivered relic.
    /// </summary>
    /// <param name="eventKey">The event, e.g. its <see cref="MiniGameContext"/>.</param>
    /// <param name="type">The kind of event.</param>
    /// <param name="player">The player.</param>
    /// <param name="objective">The objective.</param>
    /// <param name="value">The value of the objective, e.g. the rank.</param>
    public void AddObjective(object eventKey, MiniGameType type, Player player, MiniGameObjective objective, int value)
    {
        if (this.GetRule(type) is not { } rule)
        {
            return;
        }

        var points = objective switch
        {
            MiniGameObjective.Rank when value >= 1 && value <= rule.RankPoints.Count => rule.RankPoints[value - 1],
            MiniGameObjective.RelicClaimed => rule.RelicClaimPoints,
            MiniGameObjective.RelicScored => rule.RelicScorePoints,
            _ => 0,
        };
        this.Add(eventKey, player, points);
    }

    /// <inheritdoc />
    public async ValueTask MiniGameEndedAsync(MiniGameContext miniGame, Player? winner, IReadOnlyCollection<Player> finishers)
    {
        await this.EndAsync(miniGame, miniGame.Definition.Type, winner).ConfigureAwait(false);
    }

    /// <summary>
    /// Ends the event: the winner gets its points, every player with points its box.
    /// </summary>
    /// <param name="eventKey">The event, e.g. its <see cref="MiniGameContext"/>.</param>
    /// <param name="type">The kind of event.</param>
    /// <param name="winner">The winner, if any.</param>
    /// <returns>The number of delivered boxes.</returns>
    public async ValueTask<int> EndAsync(object eventKey, MiniGameType type, Player? winner)
    {
        if (winner is not null && this.GetRule(type) is { } rule)
        {
            this.Add(eventKey, winner, rule.WinnerPoints);
        }

        if (!this._points.TryGetValue(eventKey, out var points))
        {
            return 0;
        }

        this._points.Remove(eventKey);
        var delivered = 0;
        foreach (var (player, value) in points)
        {
            if (this.GetBox(value) is { } box
                && player.PlayerState.CurrentState != PlayerState.Disconnected
                && await RewardDelivery.DeliverAsync(player, box).ConfigureAwait(false))
            {
                delivered++;
            }
        }

        return delivered;
    }

    private EventContributionRule? GetRule(MiniGameType type) => this.Configuration?.Rules.FirstOrDefault(r => r.Type == type);

    private void Add(object eventKey, Player player, int points)
    {
        if (points <= 0)
        {
            return;
        }

        this._points.GetOrCreateValue(eventKey).AddOrUpdate(player, points, (_, current) => current + points);
    }
}
