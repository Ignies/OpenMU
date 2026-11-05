// <copyright file="LuckyItemWearPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.Views.Inventory;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Wears down the lucky items (<see cref="LuckyItemExtensions.IsLuckyItem"/>) which players have equipped: they lose a
/// point of durability each time they were worn for the configured interval. Only a jewel of extension repairs them.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.LuckyItemWearPlugIn_Name), Description = nameof(PlugInResources.LuckyItemWearPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("E7833453-F292-4880-908A-453BDD418D70")]
public class LuckyItemWearPlugIn : IPeriodicTaskPlugIn, ISupportCustomConfiguration<LuckyItemWearConfiguration>, ISupportDefaultCustomConfiguration
{
    /// <summary>
    /// The time between the checks of the players.
    /// </summary>
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

    private readonly ConditionalWeakTable<Player, StrongBox<DateTime>> _wornSince = new();

    private DateTime _nextRunUtc = DateTime.UtcNow;

    /// <inheritdoc />
    public LuckyItemWearConfiguration? Configuration { get; set; }

    /// <inheritdoc />
    public async ValueTask ExecuteTaskAsync(GameContext gameContext)
    {
        var now = DateTime.UtcNow;
        if (now < this._nextRunUtc)
        {
            return;
        }

        this._nextRunUtc = now + CheckInterval;
        var configuration = this.Configuration ??= new LuckyItemWearConfiguration();
        foreach (var player in await gameContext.GetPlayersAsync().ConfigureAwait(false))
        {
            if (player.PlayerState.CurrentState != PlayerState.EnteredWorld)
            {
                this._wornSince.Remove(player);
                continue;
            }

            var wornSince = this._wornSince.GetValue(player, _ => new StrongBox<DateTime>(now));
            if (now - wornSince.Value < configuration.Interval)
            {
                continue;
            }

            wornSince.Value = now;
            await WearAsync(player).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public void ForceStart()
    {
        this._nextRunUtc = DateTime.UtcNow;
    }

    /// <inheritdoc />
    public object CreateDefaultConfig()
    {
        return new LuckyItemWearConfiguration();
    }

    /// <summary>
    /// Takes a point of durability from each equipped lucky item of the player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The task.</returns>
    internal static async ValueTask WearAsync(Player player)
    {
        var configuration = player.GameContext.Configuration;
        var items = player.Inventory?.EquippedItems
            .Where(item => item.Durability > 0 && item.Definition?.IsLuckyItem(configuration) is true)
            .ToList() ?? [];
        foreach (var item in items)
        {
            item.Durability = Math.Max(0, item.Durability - 1);
            await player.InvokeViewPlugInAsync<IItemDurabilityChangedPlugIn>(p => p.ItemDurabilityChangedAsync(item, false)).ConfigureAwait(false);
        }
    }
}
