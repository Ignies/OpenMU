// <copyright file="ItemExpiryPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ItemExpiry;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.GremoryCase;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Removes the expired items (<see cref="Item.ExpiresAt"/>) of the players in the world: inventory, vault and gremory cases.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.ItemExpiryPlugIn_Name), Description = nameof(PlugInResources.ItemExpiryPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("8E3F2A71-5C0D-4B96-A1E7-3D58C9F04B26")]
public class ItemExpiryPlugIn : IFeaturePlugIn, IPeriodicTaskPlugIn, IDisabledByDefault, ISupportCustomConfiguration<ItemExpiryConfiguration>, ISupportDefaultCustomConfiguration
{
    /// <summary>
    /// The feature id of the fork marker.
    /// </summary>
    public const string FeatureId = "item_expiry";

    private DateTime _nextRunUtc = DateTime.UtcNow;

    /// <inheritdoc />
    public ItemExpiryConfiguration? Configuration { get; set; }

    /// <inheritdoc />
    public async ValueTask ExecuteTaskAsync(GameContext gameContext)
    {
        var now = DateTime.UtcNow;
        if (now < this._nextRunUtc)
        {
            return;
        }

        this._nextRunUtc = now + (this.Configuration ?? new ItemExpiryConfiguration()).Interval;
        foreach (var player in await gameContext.GetPlayersAsync().ConfigureAwait(false))
        {
            if (player.PlayerState.CurrentState != PlayerState.EnteredWorld)
            {
                continue;
            }

            try
            {
                await player.RemoveExpiredItemsAsync(now).ConfigureAwait(false);
                await GremoryCaseService.RemoveExpiredAsync(player, now).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                player.Logger.LogError(ex, "Error when removing the expired items of player '{player}'.", player);
            }
        }
    }

    /// <inheritdoc />
    public void ForceStart() => this._nextRunUtc = DateTime.UtcNow;

    /// <inheritdoc />
    public object CreateDefaultConfig() => new ItemExpiryConfiguration();
}
