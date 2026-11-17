// <copyright file="ItemCustomizationFeaturePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ItemCustomization;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.Views.ItemCustomization;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The item customisation channel: which items of the places a player sees carry a look, a name or a lore
/// (Readme.md). Off by default; a world turns it on with its system (Admin > Systems, "Item looks and names").
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.ItemCustomizationFeaturePlugIn_Name), Description = nameof(PlugInResources.ItemCustomizationFeaturePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("B62F3769-D9C9-4861-AFE1-4107BC757189")]
public class ItemCustomizationFeaturePlugIn : IFeaturePlugIn, IDisabledByDefault
{
    /// <summary>
    /// The feature id (<see cref="ForkInfo.Features"/>, <see cref="ForkFeatures"/>).
    /// </summary>
    public const string FeatureId = "item_custom";

    /// <summary>
    /// The most places a request asks about.
    /// </summary>
    public const int MaximumAsks = 32;

    private const int RequestsPerSecond = 4;

    private static readonly long HitTime = (long)TimeSpan.FromMinutes(5).TotalMilliseconds;

    /// <summary>A delivery writes the record right after it makes the item, so a miss is kept only briefly.</summary>
    private static readonly long MissTime = (long)TimeSpan.FromSeconds(30).TotalMilliseconds;

    private readonly ConditionalWeakTable<Player, StrongBox<(long Start, int Count)>> _windows = new();

    private readonly ItemCustomizationReportGate _reports = new();

    private readonly ItemCustomizationWatchers<Player> _watchers = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemCustomizationFeaturePlugIn"/> class.
    /// </summary>
    public ItemCustomizationFeaturePlugIn() => ItemCustomizationCache.Forgotten += this.OnForgotten;

    /// <summary>
    /// Gets the active plugin of a game context.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The plugin, or null when the channel is off.</returns>
    public static ItemCustomizationFeaturePlugIn? Get(IGameContext gameContext) =>
        gameContext.FeaturePlugIns.GetPlugIn<ItemCustomizationFeaturePlugIn>();

    /// <summary>
    /// Answers a request: the customised items of the asked places. At most <see cref="RequestsPerSecond"/> a second per
    /// player; the rest are dropped (the client gives up on an answer after 3 s and asks again).
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="asks">The asked places.</param>
    public async ValueTask HandleRequestAsync(Player player, IReadOnlyList<(ItemCustomizationStorage Storage, ushort ObjectId)> asks)
    {
        if (!this.Allow(player))
        {
            return;
        }

        var answer = await this.AnswerAsync(player, asks.Take(MaximumAsks)).ConfigureAwait(false);
        await player.InvokeViewPlugInAsync<IItemCustomizationViewPlugIn>(p => p.SendCustomizationsAsync(answer)).ConfigureAwait(false);
    }

    /// <summary>
    /// Answers a detail request: the lore of one item.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="storage">The storage.</param>
    /// <param name="objectId">The object id.</param>
    /// <param name="slot">The slot.</param>
    public async ValueTask HandleDetailAsync(Player player, ItemCustomizationStorage storage, ushort objectId, byte slot)
    {
        if (!this.Allow(player)
            || await ItemCustomizationCollector.ItemIdAsync(player, storage, objectId, slot).ConfigureAwait(false) is not { } itemId)
        {
            return;
        }

        var customizations = await this.GetAsync(player, [itemId]).ConfigureAwait(false);
        if (customizations.TryGetValue(itemId, out var customization))
        {
            var entry = new ItemCustomizationEntryData(storage, objectId, slot, customization);
            await player.InvokeViewPlugInAsync<IItemCustomizationViewPlugIn>(p => p.SendDetailAsync(entry)).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Answers a report (EE 37): the item's name and lore as they are now go into a report for the website's
    /// moderation, once per reporter, item and version (<see cref="ItemCustomizationReportGate"/>).
    /// </summary>
    /// <param name="player">The reporting player.</param>
    /// <param name="storage">The storage.</param>
    /// <param name="objectId">The object id.</param>
    /// <param name="slot">The slot.</param>
    public async ValueTask HandleReportAsync(Player player, ItemCustomizationStorage storage, ushort objectId, byte slot)
    {
        var result = await this.ReportAsync(player, storage, objectId, slot).ConfigureAwait(false);
        await player.InvokeViewPlugInAsync<IItemCustomizationViewPlugIn>(p => p.SendReportResultAsync(result)).ConfigureAwait(false);
    }

    /// <summary>
    /// Tells everyone who was told about a changed item again, at once (EE 39): the place they asked about, as it is now.
    /// </summary>
    /// <param name="itemId">The changed item.</param>
    private void OnForgotten(Guid itemId)
    {
        var places = this._watchers.Take(itemId);
        if (places.Count > 0)
        {
            _ = Task.Run(() => this.PushAsync(places));
        }
    }

    private async Task PushAsync(IReadOnlyList<(Player Watcher, ItemCustomizationStorage Storage, ushort ObjectId)> places)
    {
        foreach (var (player, storage, objectId) in places)
        {
            try
            {
                if (player.PlayerState.CurrentState != PlayerState.EnteredWorld)
                {
                    continue;
                }

                var answer = await this.AnswerAsync(player, [(storage, objectId)]).ConfigureAwait(false);
                await player.InvokeViewPlugInAsync<IItemCustomizationViewPlugIn>(p => p.SendChangedAsync(answer)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                player.Logger.LogDebug(ex, "Pushing a changed item customisation to {Player} failed.", player);
            }
        }
    }

    /// <summary>The customised items of the asked places, each noted as told to the player.</summary>
    private async ValueTask<List<ItemCustomizationEntryData>> AnswerAsync(Player player, IEnumerable<(ItemCustomizationStorage Storage, ushort ObjectId)> asks)
    {
        var items = await ItemCustomizationCollector.CollectAsync(player, asks).ConfigureAwait(false);
        var customizations = await this.GetAsync(player, items.Select(i => i.ItemId).Distinct().ToList()).ConfigureAwait(false);
        var answer = new List<ItemCustomizationEntryData>();
        foreach (var item in items)
        {
            if (customizations.TryGetValue(item.ItemId, out var customization))
            {
                answer.Add(new ItemCustomizationEntryData(item.Storage, item.ObjectId, item.Slot, customization));
                this._watchers.Add(item.ItemId, player, item.Storage, item.ObjectId);
            }
        }

        return answer;
    }

    private async ValueTask<ItemCustomizationReportResult> ReportAsync(Player player, ItemCustomizationStorage storage, ushort objectId, byte slot)
    {
        if (player.SelectedCharacter?.Name is not { Length: > 0 } reporter
            || await ItemCustomizationCollector.ItemIdAsync(player, storage, objectId, slot).ConfigureAwait(false) is not { } itemId
            || (await this.GetAsync(player, [itemId]).ConfigureAwait(false)).GetValueOrDefault(itemId) is not { } customization
            || (string.IsNullOrWhiteSpace(customization.Name) && string.IsNullOrWhiteSpace(customization.Lore)))
        {
            return ItemCustomizationReportResult.NotFound;
        }

        var result = this._reports.Try(reporter, itemId, customization.Version, Environment.TickCount64);
        if (result != ItemCustomizationReportResult.Sent)
        {
            return result;
        }

        try
        {
            using var context = player.GameContext.PersistenceContextProvider.CreateNewContext();
            var report = context.CreateNew<DataModel.Entities.ItemCustomizationReport>();
            report.ItemId = itemId;
            report.Reporter = reporter;
            report.Version = customization.Version;
            report.Name = customization.Name;
            report.Lore = customization.Lore;
            report.CreatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync().ConfigureAwait(false);
            return ItemCustomizationReportResult.Sent;
        }
        catch (Exception ex)
        {
            // The unique index: this reporter reported this version before the server last started.
            player.Logger.LogDebug(ex, "Report of item {ItemId} by {Reporter} not stored.", itemId, reporter);
            return ItemCustomizationReportResult.Already;
        }
    }

    private async ValueTask<Dictionary<Guid, DataModel.Entities.ItemCustomization>> GetAsync(Player player, IReadOnlyList<Guid> itemIds)
    {
        var now = Environment.TickCount64;
        var found = new Dictionary<Guid, DataModel.Entities.ItemCustomization>();
        var missing = new List<Guid>();
        foreach (var id in itemIds)
        {
            if (ItemCustomizationCache.TryGet(id, now, out var cached))
            {
                if (cached is { } hit)
                {
                    found[id] = hit;
                }
            }
            else
            {
                missing.Add(id);
            }
        }

        if (missing.Count > 0)
        {
            var loaded = (await player.PersistenceContext.GetItemCustomizationsAsync(missing).ConfigureAwait(false)).ToDictionary(c => c.ItemId);
            foreach (var id in missing)
            {
                var customization = loaded.GetValueOrDefault(id);
                ItemCustomizationCache.Store(id, customization, now + (customization is null ? MissTime : HitTime));
                if (customization is not null)
                {
                    found[id] = customization;
                }
            }
        }

        return found;
    }

    private bool Allow(Player player)
    {
        var window = this._windows.GetValue(player, _ => new StrongBox<(long, int)>((0, 0)));
        var now = Environment.TickCount64;
        lock (window)
        {
            window.Value = now - window.Value.Start >= 1000 ? (now, 1) : (window.Value.Start, window.Value.Count + 1);
            return window.Value.Count <= RequestsPerSecond;
        }
    }
}
