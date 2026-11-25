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
/// locks the items of the inventory, and plays recipe attempts in the Constellation Forge
/// (StarSmithingFeaturePlugIn.Forge.cs). Off by default; a world turns it on with its system (Admin > Systems).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.StarSmithingFeaturePlugIn_Name), Description = nameof(PlugInResources.StarSmithingFeaturePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("27031977-58F3-4B43-A2EE-524CB1F01D4D")]
public partial class StarSmithingFeaturePlugIn : IFeaturePlugIn, IDisabledByDefault, ISupportCustomConfiguration<StarSmithingConfiguration>, ISupportDefaultCustomConfiguration, IObjectAddedToMapPlugIn
{
    /// <summary>
    /// The feature id (<see cref="ForkInfo.Features"/>, <see cref="ForkFeatures"/>).
    /// </summary>
    public const string FeatureId = "star_smithing";

    /// <summary>The look bytes a smith may set: the colours (0-13); the weapon effects (14-30) come with recipes.</summary>
    private const int ColourBytes = 14;

    /// <summary>The hammer strikes of an action (EE 92); clients hold the result until the last one.</summary>
    private const byte Strikes = 3;

    /// <summary>No dye named in a recolour (EE 93).</summary>
    private const byte NoDye = 0xFF;

    /// <summary>The look's flag of a glow (<see cref="ItemLookBytes"/>, byte 1).</summary>
    private const byte GlowFlag = 2;

    /// <summary>The inventories watched for the hammer: a character entering the world gets a new one.</summary>
    private readonly ConditionalWeakTable<IInventoryStorage, object> _watched = new();

    /// <summary>Each smith's measured round trip (the beat ping), gone with the player.</summary>
    private readonly ConditionalWeakTable<Player, StarSmithRoundTrip> _roundTrips = new();

    /// <summary>The configuration already checked by <see cref="StarSmithConfigValidator.Sanitize"/>.</summary>
    private StarSmithingConfiguration? _checked;

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    public StarSmithingConfiguration? Configuration { get; set; }

    private StarSmithingConfiguration Settings
    {
        get
        {
            // A configuration is checked once when it comes in: broken recipes are dropped, a broken balance is the default.
            var settings = this.Configuration ??= new StarSmithingConfiguration();
            if (!ReferenceEquals(settings, this._checked))
            {
                StarSmithConfigValidator.Sanitize(settings);
                this._checked = settings;
            }

            return settings;
        }
    }

    /// <summary>
    /// Gets the active plugin of a game context.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The plugin, or null when Star Smithing is off.</returns>
    public static StarSmithingFeaturePlugIn? Get(IGameContext gameContext) => gameContext.FeaturePlugIns.GetPlugIn<StarSmithingFeaturePlugIn>();

    /// <summary>
    /// Takes a client's answer to the beat ping (EE F4): a sample when it carries the token of the ping out.
    /// </summary>
    /// <param name="player">The smith.</param>
    /// <param name="token">The token it carries.</param>
    public void Pong(Player player, uint token) => this.RoundTripOf(player).Answer(token, this.Clock());

    /// <summary>
    /// The smith's measured round trip.
    /// </summary>
    /// <param name="player">The smith.</param>
    /// <returns>The round trip.</returns>
    internal StarSmithRoundTrip RoundTripOf(Player player) => this._roundTrips.GetValue(player, _ => new StarSmithRoundTrip());

    /// <summary>Gets the balance tables the boosts in play are capped by.</summary>
    public StarSmithBalance Balance => this.Settings.Balance;

    /// <inheritdoc />
    public object CreateDefaultConfig() => new StarSmithingConfiguration();

    /// <summary>
    /// Puts an item's boosts in play: the boost book learns them and, when the item is worn, its power-ups are made
    /// again.
    /// </summary>
    /// <param name="player">The owner.</param>
    /// <param name="item">The item.</param>
    /// <param name="itemId">The item's persistent id.</param>
    /// <param name="boosts">The boosts; none takes them out.</param>
    internal static void ApplyBoosts(Player player, Item? item, Guid itemId, IReadOnlyList<StarSmithBoost> boosts)
    {
        StarSmithBoostBook.Set(itemId, boosts);
        if (item is null || player.Attributes is not { } attributes || player.Inventory?.EquippedItems.Contains(item) != true)
        {
            return;
        }

        if (attributes.ItemPowerUps.Remove(item, out var old))
        {
            foreach (var powerUp in old)
            {
                powerUp.Dispose();
            }
        }

        attributes.ItemPowerUps.Add(item, player.GameContext.ItemPowerUpFactory.GetPowerUps(item, attributes).ToList());
    }

    /// <summary>
    /// Reads the boosts of every item a player carries into the boost book, and makes the worn ones' power-ups again.
    /// </summary>
    private static async ValueTask LoadBoostsAsync(Player player)
    {
        var items = player.Inventory?.Items
            .Select(i => (Item: i, Id: i is MUnique.OpenMU.Persistence.IIdentifiable { Id: var id } ? id : Guid.Empty))
            .Where(i => i.Id != Guid.Empty)
            .ToList() ?? [];
        if (items.Count == 0)
        {
            return;
        }

        var records = (await player.PersistenceContext.GetItemCustomizationsAsync(items.Select(i => i.Id).ToList()).ConfigureAwait(false))
            .Where(r => !string.IsNullOrEmpty(r.Boosts))
            .ToDictionary(r => r.ItemId);
        foreach (var (item, id) in items.Where(i => records.ContainsKey(i.Id)))
        {
            ApplyBoosts(player, item, id, StarSmithBoosts.Parse(records[id].Boosts));
        }
    }

    /// <inheritdoc />
    public async ValueTask ObjectAddedToMapAsync(GameMap map, ILocateable addedObject)
    {
        if (addedObject is not Player { Inventory: { } inventory } player)
        {
            return;
        }

        if (!this._watched.TryGetValue(inventory, out _))
        {
            this._watched.Add(inventory, new object());
            await LoadBoostsAsync(player).ConfigureAwait(false);
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
    /// Runs a smithing action on an item of the inventory and answers it (EE 92 to everyone who sees the smith when done, EE 97, then EE 91).
    /// </summary>
    /// <param name="player">The smith.</param>
    /// <param name="action">The action.</param>
    /// <param name="slot">The inventory slot of the item.</param>
    /// <param name="payload">The look bytes, the name or the lore (UTF-8, zero padded); empty for a lock.</param>
    /// <param name="dyeSlot">For a recolour, the inventory slot of the colour dye spent (0xFF none).</param>
    /// <param name="radiantSlot">For a recolour, the inventory slot of the Radiant Dye spent for a glow (0xFF none).</param>
    public async ValueTask HandleAsync(Player player, StarSmithAction action, byte slot, ReadOnlyMemory<byte> payload, byte dyeSlot = NoDye, byte radiantSlot = NoDye)
    {
        var zenCost = action == StarSmithAction.Recolor ? this.Settings.RecolorZen : 0;
        var (result, refusal) = await this.RunAsync(player, action, slot, payload, zenCost, dyeSlot, radiantSlot).ConfigureAwait(false);
        if (result == StarSmithResultCode.Done)
        {
            await player.ForEachWorldObserverAsync<IStarSmithingViewPlugIn>(p => p.SendAnimationAsync(player, action, Strikes), true).ConfigureAwait(false);
        }

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
            StarSmithLevels.ToNext(level, this.Settings.Balance.LevelCurve),
            (int)(hammer?.Durability ?? 0));
        await player.InvokeViewPlugInAsync<IStarSmithingViewPlugIn>(p => p.SendStateAsync(state)).ConfigureAwait(false);
        if (this.RoundTripOf(player).Send(this.Clock()) is { } token)
        {
            await player.InvokeViewPlugInAsync<IStarSmithingViewPlugIn>(p => p.SendPingAsync(token)).ConfigureAwait(false);
        }

        if (hammer is not null)
        {
            await player.InvokeViewPlugInAsync<IStarSmithingViewPlugIn>(p => p.SendDyesAsync(this.Settings.Dyes)).ConfigureAwait(false);
            var found = this.GuidebookOf(player);
            await player.InvokeViewPlugInAsync<IStarSmithingViewPlugIn>(p => p.SendGuidebookAsync(this.Settings.Recipes.Count, found)).ConfigureAwait(false);
        }
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

    /// <summary>Takes one dye from its stack, or the item when it was the last.</summary>
    private static async ValueTask SpendOneAsync(Player player, Item dye)
    {
        if (dye.Durability > 1)
        {
            dye.Durability--;
            await player.InvokeViewPlugInAsync<IItemDurabilityChangedPlugIn>(p => p.ItemDurabilityChangedAsync(dye, true)).ConfigureAwait(false);
            return;
        }

        if (player.Inventory is not { } inventory)
        {
            return;
        }

        var slot = dye.ItemSlot;
        await inventory.RemoveItemAsync(dye).ConfigureAwait(false);
        await player.PersistenceContext.DeleteAsync(dye).ConfigureAwait(false);
        await player.InvokeViewPlugInAsync<IItemRemovedPlugIn>(p => p.RemoveItemAsync(slot)).ConfigureAwait(false);
    }

    private async ValueTask<(StarSmithResultCode Result, byte Refusal)> RunAsync(Player player, StarSmithAction action, byte slot, ReadOnlyMemory<byte> payload, int zenCost, byte dyeSlot, byte radiantSlot)
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

        // The dyes a recolour spends (Readme.md "Dyes"): none for the original colours, the Radiant Dye only for a glow.
        Item? colourDye = null;
        Item? radiantDye = null;
        if (action == StarSmithAction.Recolor)
        {
            var lookBytes = payload.Span[..Math.Min(ItemLookBytes.Length, payload.Length)];
            colourDye = this.DyeItem(player, dyeSlot, hammer, item);
            radiantDye = this.DyeItem(player, radiantSlot, hammer, item);
            var dyes = StarSmithDyes.Check(lookBytes, this.DyeOf(colourDye), this.DyeOf(radiantDye));
            if (dyes != StarSmithResultCode.Done)
            {
                return (dyes, 0);
            }

            var needsDye = StarSmithDyes.NeedsDye(lookBytes);
            colourDye = needsDye ? colourDye : null;
            radiantDye = needsDye && (lookBytes[1] & GlowFlag) != 0 ? radiantDye : null;
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

        foreach (var dye in new[] { colourDye, radiantDye })
        {
            if (dye is not null)
            {
                await SpendOneAsync(player, dye).ConfigureAwait(false);
            }
        }

        if (StarSmithRules.WearsHammer(action) && hammer is not null)
        {
            hammer.Durability = Math.Max(0, hammer.Durability - this.Settings.DurabilityPerAction);
            await player.InvokeViewPlugInAsync<IItemDurabilityChangedPlugIn>(p => p.ItemDurabilityChangedAsync(hammer, false)).ConfigureAwait(false);
        }

        return (StarSmithResultCode.Done, 0);
    }

    /// <summary>The bag item in a dye slot, if it is a dye (never the hammer or the item being recoloured).</summary>
    private Item? DyeItem(Player player, byte slot, Item? hammer, Item? target) =>
        slot != NoDye && player.Inventory?.GetItem(slot) is { } found && !ReferenceEquals(found, hammer) && !ReferenceEquals(found, target) && this.DyeOf(found) is not null ? found : null;

    private StarSmithDye? DyeOf(Item? item) =>
        item?.Definition is { } definition ? StarSmithDyes.Find(this.Settings.Dyes, (byte)definition.Group, definition.Number) : null;

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
