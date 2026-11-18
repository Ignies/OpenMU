// <copyright file="StarSmithingFeaturePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using MUnique.OpenMU.GameLogic.ItemCustomization;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.TextModeration;
using MUnique.OpenMU.GameLogic.Views.Inventory;
using MUnique.OpenMU.GameLogic.Views.StarSmithing;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Star Smithing (Readme.md): with the Star Smith's Hammer in a hand, a smith recolours, names, writes the lore of and
/// locks the items of the inventory. Off by default; a world turns it on with its system (Admin > Systems).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.StarSmithingFeaturePlugIn_Name), Description = nameof(PlugInResources.StarSmithingFeaturePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("27031977-58F3-4B43-A2EE-524CB1F01D4D")]
public class StarSmithingFeaturePlugIn : IFeaturePlugIn, IDisabledByDefault, ISupportCustomConfiguration<StarSmithingConfiguration>, ISupportDefaultCustomConfiguration, IObjectAddedToMapPlugIn
{
    /// <summary>
    /// The feature id (<see cref="ForkInfo.Features"/>, <see cref="ForkFeatures"/>).
    /// </summary>
    public const string FeatureId = "star_smithing";

    /// <summary>The look bytes a smith may set: the colours (0-13); the weapon effects (14-30) come with recipes.</summary>
    private const int ColourBytes = 14;

    private readonly ConditionalWeakTable<Player, object> _watched = new();

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    public StarSmithingConfiguration? Configuration { get; set; }

    private StarSmithingConfiguration Settings => this.Configuration ??= new StarSmithingConfiguration();

    /// <summary>
    /// Gets the active plugin of a game context.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The plugin, or null when Star Smithing is off.</returns>
    public static StarSmithingFeaturePlugIn? Get(IGameContext gameContext) => gameContext.FeaturePlugIns.GetPlugIn<StarSmithingFeaturePlugIn>();

    /// <inheritdoc />
    public object CreateDefaultConfig() => new StarSmithingConfiguration();

    /// <inheritdoc />
    public async ValueTask ObjectAddedToMapAsync(GameMap map, ILocateable addedObject)
    {
        if (addedObject is not Player { Inventory: { } inventory } player)
        {
            return;
        }

        if (!this._watched.TryGetValue(player, out _))
        {
            this._watched.Add(player, new object());
            inventory.EquippedItemsChanged += async args =>
            {
                if (this.IsHammer(args.Item))
                {
                    await this.SendStateAsync(player).ConfigureAwait(false);
                }
            };
        }

        await this.SendStateAsync(player).ConfigureAwait(false);
    }

    /// <summary>
    /// Runs a smithing action on an item of the inventory and answers it (EE 97, then EE 91).
    /// </summary>
    /// <param name="player">The smith.</param>
    /// <param name="action">The action.</param>
    /// <param name="slot">The inventory slot of the item.</param>
    /// <param name="payload">The look bytes, the name or the lore (UTF-8, zero padded); empty for a lock.</param>
    public async ValueTask HandleAsync(Player player, StarSmithAction action, byte slot, ReadOnlyMemory<byte> payload)
    {
        var zenCost = action == StarSmithAction.Recolor ? this.Settings.RecolorZen : 0;
        var (result, refusal) = await this.RunAsync(player, action, slot, payload, zenCost).ConfigureAwait(false);
        await player.InvokeViewPlugInAsync<IStarSmithingViewPlugIn>(p => p.SendResultAsync(action, result, refusal, (uint)Math.Max(0, zenCost))).ConfigureAwait(false);
        await this.SendStateAsync(player).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends the smith's state (EE 91).
    /// </summary>
    /// <param name="player">The player.</param>
    public async ValueTask SendStateAsync(Player player)
    {
        var hammer = this.HammerOf(player);
        var level = Math.Clamp((int)(player.SelectedCharacter?.StarSmithLevel ?? 1), 1, StarSmithLevels.Maximum);
        var state = new StarSmithState(
            hammer is not null,
            hammer is not null && hammer.Durability < this.Settings.DurabilityPerAction,
            level,
            player.SelectedCharacter?.StarSmithExperience ?? 0,
            StarSmithLevels.ToNext(level),
            (int)(hammer?.Durability ?? 0));
        await player.InvokeViewPlugInAsync<IStarSmithingViewPlugIn>(p => p.SendStateAsync(state)).ConfigureAwait(false);
    }

    /// <summary>
    /// Reads a zero padded UTF-8 text field.
    /// </summary>
    /// <param name="bytes">The field.</param>
    /// <returns>The text.</returns>
    internal static string TextOf(ReadOnlySpan<byte> bytes)
    {
        var end = bytes.IndexOf((byte)0);
        return Encoding.UTF8.GetString(end < 0 ? bytes : bytes[..end]);
    }

    private static bool IsPlaceAllowed(Player player) =>
        player.TradingPartner is null && player.DuelRoom is null && player.CurrentMiniGame is null && player.ShopStorage?.StoreOpen != true;

    /// <summary>The look JSON of a smith's recolour: its colours only, never a weapon effect; null for the item's own colours.</summary>
    private static string? Recolour(ReadOnlySpan<byte> payload)
    {
        Span<byte> look = stackalloc byte[ItemLookBytes.Length];
        payload[..Math.Min(ColourBytes, payload.Length)].CopyTo(look);
        return ItemLookBytes.TryRead(look);
    }

    private static async ValueTask SaveAsync(Player player, Guid itemId, DataModel.Entities.ItemCustomization? existing, Action<DataModel.Entities.ItemCustomization> change)
    {
        using var context = player.GameContext.PersistenceContextProvider.CreateNewContext();
        var record = existing is null
            ? context.CreateNew<DataModel.Entities.ItemCustomization>()
            : await context.GetByIdAsync<DataModel.Entities.ItemCustomization>(existing.Id).ConfigureAwait(false);
        if (record is null)
        {
            return;
        }

        if (existing is null)
        {
            record.ItemId = itemId;
        }

        change(record);
        record.Version++;
        var by = "smith:" + player.SelectedCharacter?.Name;
        record.CreatedBy = by.Length <= 64 ? by : by[..64];
        record.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync().ConfigureAwait(false);
        ItemCustomizationCache.Forget(itemId);
    }

    private async ValueTask<(StarSmithResultCode Result, byte Refusal)> RunAsync(Player player, StarSmithAction action, byte slot, ReadOnlyMemory<byte> payload, int zenCost)
    {
        var hammer = this.HammerOf(player);
        var item = player.Inventory?.GetItem(slot);
        var itemId = item is MUnique.OpenMU.Persistence.IIdentifiable { Id: var id } && id != Guid.Empty && !ReferenceEquals(item, hammer) ? id : Guid.Empty;
        var record = itemId == Guid.Empty ? null : (await player.PersistenceContext.GetItemCustomizationsAsync([itemId]).ConfigureAwait(false)).FirstOrDefault();
        var check = new StarSmithCheck(
            FeatureOn: true,
            HammerHeld: hammer is not null,
            HammerDurability: (int)(hammer?.Durability ?? 0),
            DurabilityCost: this.Settings.DurabilityPerAction,
            PlaceAllowed: IsPlaceAllowed(player),
            ItemFound: itemId != Guid.Empty,
            ItemLocked: record?.Locked ?? false,
            Zen: player.Money,
            ZenCost: zenCost);
        var result = StarSmithRules.Check(action, check);
        if (result != StarSmithResultCode.Done)
        {
            return (result, 0);
        }

        string? text = null;
        if (action is StarSmithAction.Rename or StarSmithAction.Lore && TextOf(payload.Span) is { Length: > 0 } written)
        {
            var verdict = TextModerationPlugIn.Check(player.GameContext, action == StarSmithAction.Rename ? TextKind.Name : TextKind.Lore, written);
            if (!verdict.Ok)
            {
                return (StarSmithResultCode.TextRefused, (byte)verdict.Refusal);
            }

            text = verdict.Text;
        }

        if (action == StarSmithAction.Recolor && !player.TryRemoveMoney(zenCost))
        {
            return (StarSmithResultCode.NotEnoughZen, 0);
        }

        var look = action == StarSmithAction.Recolor ? Recolour(payload.Span) : null;
        await SaveAsync(player, itemId, record, customization =>
        {
            switch (action)
            {
                case StarSmithAction.Recolor:
                    customization.Look = look;
                    break;
                case StarSmithAction.Rename:
                    customization.Name = text;
                    break;
                case StarSmithAction.Lore:
                    customization.Lore = text;
                    break;
                default:
                    customization.Locked = true;
                    customization.LockedAt = DateTime.UtcNow;
                    break;
            }
        }).ConfigureAwait(false);

        if (StarSmithRules.WearsHammer(action) && hammer is not null)
        {
            hammer.Durability = Math.Max(0, hammer.Durability - this.Settings.DurabilityPerAction);
            await player.InvokeViewPlugInAsync<IItemDurabilityChangedPlugIn>(p => p.ItemDurabilityChangedAsync(hammer, false)).ConfigureAwait(false);
        }

        return (StarSmithResultCode.Done, 0);
    }

    private Item? HammerOf(Player player)
    {
        if (player.Inventory is not { } inventory)
        {
            return null;
        }

        foreach (var hand in new byte[] { InventoryConstants.LeftHandSlot, InventoryConstants.RightHandSlot })
        {
            if (inventory.GetItem(hand) is { } item && this.IsHammer(item))
            {
                return item;
            }
        }

        return null;
    }

    private bool IsHammer(Item? item) =>
        item?.Definition is { } definition && definition.Group == this.Settings.HammerGroup && definition.Number == this.Settings.HammerNumber;
}
