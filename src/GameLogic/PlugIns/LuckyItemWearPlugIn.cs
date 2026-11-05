// <copyright file="LuckyItemWearPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Wears down the lucky items (<see cref="LuckyItemExtensions.IsLuckyItem"/>) which players have equipped: each piece
/// loses a point of durability each time it was worn for the configured interval, counted per piece and only while
/// it's equipped. Only a jewel of extension repairs them.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.LuckyItemWearPlugIn_Name), Description = nameof(PlugInResources.LuckyItemWearPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("E7833453-F292-4880-908A-453BDD418D70")]
public class LuckyItemWearPlugIn : IPeriodicTaskPlugIn, ISupportCustomConfiguration<LuckyItemWearConfiguration>, ISupportDefaultCustomConfiguration
{
    /// <summary>
    /// The time between the checks of the players, which is also the shortest interval of the wear.
    /// </summary>
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

    private readonly ConditionalWeakTable<Item, StrongBox<TimeSpan>> _wornTime = new();

    private DateTime _lastRunUtc = DateTime.UtcNow;

    /// <inheritdoc />
    public LuckyItemWearConfiguration? Configuration { get; set; }

    /// <inheritdoc />
    public async ValueTask ExecuteTaskAsync(GameContext gameContext)
    {
        var now = DateTime.UtcNow;
        var elapsed = now - this._lastRunUtc;
        if (elapsed < CheckInterval)
        {
            return;
        }

        this._lastRunUtc = now;
        elapsed = elapsed > CheckInterval * 2 ? CheckInterval : elapsed;
        foreach (var player in await gameContext.GetPlayersAsync().ConfigureAwait(false))
        {
            if (player.PlayerState.CurrentState != PlayerState.EnteredWorld)
            {
                continue;
            }

            try
            {
                await this.WearAsync(player, elapsed).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                player.Logger.LogError(ex, "Couldn't wear the lucky items of {player}.", player);
            }
        }
    }

    /// <inheritdoc />
    public void ForceStart()
    {
        this._lastRunUtc = DateTime.UtcNow - CheckInterval;
    }

    /// <inheritdoc />
    public object CreateDefaultConfig()
    {
        return new LuckyItemWearConfiguration();
    }

    /// <summary>
    /// Adds the elapsed time to each equipped lucky item of the player, and takes a point of durability from each
    /// one which was worn for the interval, through <see cref="Player.DecreaseItemDurabilityAsync"/>, so the stats
    /// are recalculated when a piece breaks.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="elapsed">The time since the last call.</param>
    /// <returns>The task.</returns>
    internal async ValueTask WearAsync(Player player, TimeSpan elapsed)
    {
        var interval = this.Configuration?.Interval ?? new LuckyItemWearConfiguration().Interval;
        interval = interval < CheckInterval ? CheckInterval : interval;
        var configuration = player.GameContext.Configuration;
        var items = player.Inventory?.EquippedItems
            .Where(item => item.Durability > 0 && item.Definition?.IsLuckyItem(configuration) is true)
            .ToList() ?? [];
        foreach (var item in items)
        {
            var worn = this._wornTime.GetValue(item, _ => new StrongBox<TimeSpan>(TimeSpan.Zero));
            worn.Value += elapsed;
            if (worn.Value < interval)
            {
                continue;
            }

            worn.Value -= interval;
            await player.DecreaseItemDurabilityAsync(item, 1).ConfigureAwait(false);
        }
    }
}
