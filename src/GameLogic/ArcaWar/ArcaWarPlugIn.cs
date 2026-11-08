// <copyright file="ArcaWarPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ArcaWar;

using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The Arca War of Season 21: a guild war over three obelisks on Acheron and Deventer (maps 92 and 96), by its schedule.
/// The guilds holding obelisks at the end keep the buffs of their elements for a week (<see cref="ArcaWarState"/>).
/// </summary>
/// <remarks>
/// During the war, on its maps: members of one guild can't hurt each other (unless friendly fire is on), players of other
/// guilds fight without the player killer penalty, and players outside the war can't fight the ones in it.
/// </remarks>
[PlugIn]
[Display(Name = nameof(PlugInResources.ArcaWarPlugIn_Name), Description = nameof(PlugInResources.ArcaWarPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("2F8D4A61-9C3B-4E7A-B5D2-71E0C4A8F936")]
public sealed class ArcaWarPlugIn : IFeaturePlugIn, IPeriodicTaskPlugIn, ISupportCustomConfiguration<ArcaWarConfiguration>, ISupportDefaultCustomConfiguration, IDisabledByDefault,
    IAttackableGotKilledPlugIn, IPlayerStateChangedPlugIn, IPlayerAttackRestrictionPlugIn, IPlayerKillPenaltyExemptionPlugIn,
    IPlayerDamageFactorPlugIn, IObjectAddedToMapPlugIn, IObjectRemovedFromMapPlugIn
{
    /// <summary>
    /// The fork feature id.
    /// </summary>
    public const string FeatureId = "arca_war";

    private readonly ConcurrentDictionary<IGameContext, ArcaWarContext> _contexts = new();

    private readonly ConcurrentDictionary<IGameContext, int> _runningTicks = new();

    /// <inheritdoc />
    public ArcaWarConfiguration? Configuration { get; set; }

    /// <summary>
    /// Gets or sets the clock; tests replace it.
    /// </summary>
    internal Func<DateTime> UtcNow { get; set; } = () => DateTime.UtcNow;

    private ArcaWarConfiguration Settings => this.Configuration ??= new ArcaWarConfiguration();

    /// <summary>
    /// Gets the active plugin of a game context.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The plugin, or <c>null</c> when the feature is off.</returns>
    public static ArcaWarPlugIn? Get(IGameContext gameContext) => gameContext.FeaturePlugIns.GetPlugIn<ArcaWarPlugIn>();

    /// <summary>
    /// Gets the war of a game context.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The war.</returns>
    public ArcaWarContext GetContext(IGameContext gameContext)
        => this._contexts.GetOrAdd(gameContext, context => new ArcaWarContext(context, this.Settings, () => this.UtcNow()));

    /// <inheritdoc />
    public ValueTask ExecuteTaskAsync(GameContext gameContext) => this.TickAsync(gameContext);

    /// <inheritdoc />
    public void ForceStart()
    {
        foreach (var context in this._contexts.Values)
        {
            context.ForceStart();
        }
    }

    /// <inheritdoc />
    public object CreateDefaultConfig() => new ArcaWarConfiguration();

    /// <summary>
    /// Opens the war of a game context at the next tick, or ends its current stage then (the game master command).
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    public void Advance(IGameContext gameContext) => this.GetContext(gameContext).Advance();

    /// <summary>
    /// Handles a request of a player and shows its result.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="kind">The kind of the request.</param>
    /// <returns>The result.</returns>
    public async ValueTask<ArcaWarRequestResult> RequestAsync(Player player, ArcaWarRequestKind kind)
    {
        var result = await this.GetContext(player.GameContext).RequestAsync(player, kind).ConfigureAwait(false);
        await player.InvokeViewPlugInAsync<IArcaWarViewPlugIn>(p => p.ShowRequestResultAsync(kind, result)).ConfigureAwait(false);
        return result;
    }

    /// <inheritdoc />
    public ValueTask AttackableGotKilledAsync(IAttackable killed, IAttacker? killer)
        => ((killed as Player) ?? ArcaWarContext.PlayerOf(killer))?.GameContext is { } gameContext && this._contexts.TryGetValue(gameContext, out var context)
            ? context.OnKilledAsync(killed, killer)
            : ValueTask.CompletedTask;

    /// <inheritdoc />
    public ValueTask PlayerStateChangedAsync(Player player, State previousState, State currentState)
        => this._contexts.TryGetValue(player.GameContext, out var context) ? context.OnPlayerStateChangedAsync(player, previousState, currentState) : ValueTask.CompletedTask;

    /// <inheritdoc />
    public void CheckAttack(Player attacker, Player defender, CancelEventArgs eventArgs)
    {
        if (this._contexts.TryGetValue(attacker.GameContext, out var context) && context.PreventsAttack(attacker, defender))
        {
            eventArgs.Cancel = true;
        }
    }

    /// <inheritdoc />
    public void CheckExemption(Player attacker, Player defender, PlayerKillPenaltyExemptionArgs eventArgs)
    {
        if (this._contexts.TryGetValue(attacker.GameContext, out var context) && context.ExemptsFromPenalty(attacker, defender))
        {
            eventArgs.IsExempted = true;
        }
    }

    /// <inheritdoc />
    public void ModifyDamageFactor(Player attacker, Player defender, PlayerDamageFactorArgs args)
    {
        if (this._contexts.TryGetValue(attacker.GameContext, out var context))
        {
            args.Factor *= context.GetDamageFactor(attacker, defender);
        }
    }

    /// <inheritdoc />
    public ValueTask ObjectAddedToMapAsync(GameMap map, ILocateable addedObject)
        => addedObject is Player player && this._contexts.TryGetValue(player.GameContext, out var context)
            ? context.UpdateBonusAsync(player, map, true)
            : ValueTask.CompletedTask;

    /// <inheritdoc />
    public ValueTask ObjectRemovedFromMapAsync(GameMap map, ILocateable removedObject)
        => removedObject is Player player && this._contexts.TryGetValue(player.GameContext, out var context)
            ? context.UpdateBonusAsync(player, map, false)
            : ValueTask.CompletedTask;

    /// <summary>
    /// Advances the war of a game context.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>A task.</returns>
    internal async ValueTask TickAsync(IGameContext gameContext)
    {
        if (!this._runningTicks.TryAdd(gameContext, 1) && !this._runningTicks.TryUpdate(gameContext, 1, 0))
        {
            return;
        }

        try
        {
            var context = this.GetContext(gameContext);
            context.Configuration = this.Settings;
            await context.TickAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            gameContext.LoggerFactory.CreateLogger<ArcaWarPlugIn>().LogError(ex, "Unexpected error in the Arca War.");
        }
        finally
        {
            this._runningTicks[gameContext] = 0;
        }
    }
}
