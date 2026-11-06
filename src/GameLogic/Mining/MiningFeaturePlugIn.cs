// <copyright file="MiningFeaturePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Mining;

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Rewards;
using MUnique.OpenMU.GameLogic.Views.Inventory;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The mining of Season 21 with the Pandora Pick: a character mines the spots in the safe zones stage by stage, may stop
/// and collect after each stage, and loses what the spot gave so far when a stage fails.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.MiningFeaturePlugIn_Name), Description = nameof(PlugInResources.MiningFeaturePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("C3A85E21-7B4D-4F6A-9E12-5D8B0F3A6C47")]
public class MiningFeaturePlugIn : IFeaturePlugIn, ISupportCustomConfiguration<MiningConfiguration>, ISupportDefaultCustomConfiguration, IDisabledByDefault, IPlayerTalkToNpcPlugIn
{
    private const int RateBasis = 10000;

    private readonly ConditionalWeakTable<Player, MiningProgress> _progress = new();

    private readonly ConcurrentDictionary<NonPlayerCharacter, Player> _miners = new();

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    public MiningConfiguration? Configuration { get; set; }

    /// <summary>
    /// Gets the active plugin of a game context.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The plugin, or <c>null</c> when the feature is off.</returns>
    public static MiningFeaturePlugIn? Get(IGameContext gameContext) => gameContext.FeaturePlugIns.GetPlugIn<MiningFeaturePlugIn>();

    /// <inheritdoc />
    public object CreateDefaultConfig() => new MiningConfiguration();

    /// <summary>
    /// Mines the next stage of a spot. After the last stage, the jewels are collected.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="spot">The spot.</param>
    /// <returns>The result.</returns>
    public async ValueTask<MiningResult> MineAsync(Player player, NonPlayerCharacter spot)
    {
        var (result, stage, count, bonus) = await this.TryMineAsync(player, spot).ConfigureAwait(false);
        await ShowAsync(player, spot, result, stage, count, bonus).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Stops mining a spot and collects what it gave so far.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="spot">The spot.</param>
    /// <returns>The result.</returns>
    public async ValueTask<MiningResult> CollectAsync(Player player, NonPlayerCharacter spot)
    {
        var (result, stage, count) = await this.TryCollectAsync(player, spot).ConfigureAwait(false);
        await ShowAsync(player, spot, result, stage, count, 0).ConfigureAwait(false);
        return result;
    }

    /// <inheritdoc />
    public async ValueTask PlayerTalksToNpcAsync(Player player, NonPlayerCharacter npc, NpcTalkEventArgs eventArgs)
    {
        if (this.Configuration is not { } configuration || FindSpot(configuration, npc) is null)
        {
            return;
        }

        eventArgs.HasBeenHandled = true;
        if (player.ViewPlugIns.GetPlugIn<IMiningViewPlugIn>() is not { } view)
        {
            await player.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.MiningNeedsBabylonClient)).ConfigureAwait(false);
            return;
        }

        var progress = this._progress.GetOrCreateValue(player);
        var stage = progress.Spot == npc ? progress.Stage : 0;
        await view.ShowMiningResultAsync(npc, MiningResult.Opened, (byte)stage, stage > 0 ? FindSpot(configuration, npc)!.Stages[stage - 1].ItemCount : 0, 0).ConfigureAwait(false);
    }

    private static MiningSpotDefinition? FindSpot(MiningConfiguration configuration, NonPlayerCharacter npc)
        => configuration.Spots.FirstOrDefault(s => s.NpcNumber == npc.Definition.Number && s.Stages.Count > 0);

    private static bool Rolls(int rate) => rate > 0 && Rand.NextInt(0, RateBasis) < rate;

    private static Item? FindPickaxe(Player player, MiningConfiguration configuration)
    {
        foreach (var slot in new[] { InventoryConstants.LeftHandSlot, InventoryConstants.RightHandSlot })
        {
            if (player.Inventory?.GetItem(slot) is { Definition: { } definition } item
                && definition.Group == configuration.PickaxeGroup
                && definition.Number == configuration.PickaxeNumber)
            {
                return item;
            }
        }

        return null;
    }

    private static async ValueTask ShowAsync(Player player, NonPlayerCharacter spot, MiningResult result, int stage, int count, int bonus)
    {
        if (player.ViewPlugIns.GetPlugIn<IMiningViewPlugIn>() is { } view)
        {
            await view.ShowMiningResultAsync(spot, result, (byte)stage, count, bonus).ConfigureAwait(false);
        }
    }

    private static bool IsInReach(Player player, NonPlayerCharacter spot, MiningConfiguration configuration)
        => spot.CurrentMap == player.CurrentMap && player.IsInRange(spot.Position, configuration.MaximumDistance);

    private bool IsOccupied(Player player, NonPlayerCharacter spot, MiningConfiguration configuration)
    {
        if (!configuration.OneMinerPerSpot
            || !this._miners.TryGetValue(spot, out var miner)
            || miner == player)
        {
            return false;
        }

        if (this._progress.TryGetValue(miner, out var progress) && progress.Spot == spot && progress.Stage > 0 && IsInReach(miner, spot, configuration))
        {
            return true;
        }

        this._miners.TryRemove(new KeyValuePair<NonPlayerCharacter, Player>(spot, miner));
        return false;
    }

    private void Release(Player player, MiningProgress progress)
    {
        if (progress.Spot is { } spot)
        {
            this._miners.TryRemove(new KeyValuePair<NonPlayerCharacter, Player>(spot, player));
        }

        progress.Spot = null;
        progress.Stage = 0;
    }

    private async ValueTask<(MiningResult Result, int Stage, int Count, int Bonus)> TryMineAsync(Player player, NonPlayerCharacter spot)
    {
        if (this.Configuration is not { } configuration)
        {
            return (MiningResult.Off, 0, 0, 0);
        }

        if (FindSpot(configuration, spot) is not { } definition || !IsInReach(player, spot, configuration))
        {
            return (MiningResult.NoSpot, 0, 0, 0);
        }

        if (this.IsOccupied(player, spot, configuration))
        {
            return (MiningResult.Occupied, 0, 0, 0);
        }

        var progress = this._progress.GetOrCreateValue(player);
        if (progress.Spot != spot)
        {
            this.Release(player, progress);
            progress.Spot = spot;
        }

        var stage = definition.Stages[Math.Min(progress.Stage, definition.Stages.Count - 1)];
        if (FindPickaxe(player, configuration) is not { } pickaxe)
        {
            return (MiningResult.NoPickaxe, progress.Stage, 0, 0);
        }

        if (pickaxe.Durability < stage.DurabilityCost)
        {
            return (MiningResult.PickaxeWorn, progress.Stage, 0, 0);
        }

        pickaxe.Durability -= stage.DurabilityCost;
        await player.InvokeViewPlugInAsync<IItemDurabilityChangedPlugIn>(p => p.ItemDurabilityChangedAsync(pickaxe, false)).ConfigureAwait(false);
        this._miners[spot] = player;
        if (!Rolls(stage.SuccessRate))
        {
            var failed = progress.Stage + 1;
            this.Release(player, progress);
            return (MiningResult.Failed, failed, 0, 0);
        }

        progress.Stage++;
        if (progress.Stage < definition.Stages.Count)
        {
            return (MiningResult.Succeeded, progress.Stage, stage.ItemCount, 0);
        }

        var reached = progress.Stage;
        var bonus = Rolls(stage.BonusRate) ? stage.BonusCount : 0;
        this.Release(player, progress);
        await this.DeliverAsync(player, configuration, stage.ItemCount + bonus).ConfigureAwait(false);
        return (MiningResult.Collected, reached, stage.ItemCount, bonus);
    }

    private async ValueTask<(MiningResult Result, int Stage, int Count)> TryCollectAsync(Player player, NonPlayerCharacter spot)
    {
        if (this.Configuration is not { } configuration)
        {
            return (MiningResult.Off, 0, 0);
        }

        if (FindSpot(configuration, spot) is not { } definition
            || !this._progress.TryGetValue(player, out var progress)
            || progress.Spot != spot
            || progress.Stage == 0)
        {
            return (MiningResult.NoSpot, 0, 0);
        }

        var reached = progress.Stage;
        var count = definition.Stages[reached - 1].ItemCount;
        this.Release(player, progress);
        await this.DeliverAsync(player, configuration, count).ConfigureAwait(false);
        return (MiningResult.Collected, reached, count);
    }

    private async ValueTask DeliverAsync(Player player, MiningConfiguration configuration, int count)
    {
        foreach (var bundle in configuration.Bundles.Where(b => b.JewelCount > 0).OrderByDescending(b => b.JewelCount))
        {
            for (; count >= bundle.JewelCount; count -= bundle.JewelCount)
            {
                await RewardDelivery.DeliverAsync(player, new RewardItem { Group = bundle.ItemGroup, Number = bundle.ItemNumber, Level = bundle.ItemLevel }, true).ConfigureAwait(false);
            }
        }

        for (; count > 0; count--)
        {
            await RewardDelivery.DeliverAsync(player, new RewardItem { Group = configuration.JewelGroup, Number = configuration.JewelNumber }, true).ConfigureAwait(false);
        }
    }

    private sealed class MiningProgress
    {
        public NonPlayerCharacter? Spot { get; set; }

        public int Stage { get; set; }
    }
}
