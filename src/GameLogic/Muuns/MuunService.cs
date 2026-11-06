// <copyright file="MuunService.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Muuns;

using System.Runtime.CompilerServices;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Views.Inventory;
using MUnique.OpenMU.GameLogic.Views.Muuns;
using MUnique.OpenMU.Persistence;

/// <summary>
/// The Muun system of a player: moves between the inventory and the Muun inventory, the options of the equipped Muuns,
/// level up, evolution, the energy converter, health, the exchange and riding.
/// </summary>
/// <remarks>
/// Only a player whose client shows the Muuns (<see cref="IMuunViewPlugIn"/>, the Babylon client) has the system; a
/// Season 6 client never sees a Muun (they are wide items) and gets "off" for every request.
/// </remarks>
public static class MuunService
{
    private static readonly ConditionalWeakTable<Player, MuunState> States = new();

    private static readonly byte[] EquippedSlots = [MuunSlots.Main, MuunSlots.Sub];

    /// <summary>
    /// Determines whether the player has the Muun system: it is on and the client shows it.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns><c>true</c>, if the player has the system.</returns>
    public static bool IsAvailable(Player player)
        => MuunFeaturePlugIn.Get(player.GameContext) is not null && player.ViewPlugIns.GetPlugIn<IMuunViewPlugIn>() is not null;

    /// <summary>
    /// Gets the Muun inventory of the character of the player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="create">If set to <c>true</c>, a missing storage is created.</param>
    /// <returns>The storage, if it exists or was created.</returns>
    public static ItemStorage? GetStorage(Player player, bool create)
        => player.SelectedCharacter is { } character
            ? character.MuunInventory ??= create ? player.PersistenceContext.CreateNew<ItemStorage>() : null
            : null;

    /// <summary>
    /// Gets the item of a slot of a storage.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="storage">The storage.</param>
    /// <param name="slot">The slot.</param>
    /// <returns>The item.</returns>
    public static Item? GetItem(Player player, MuunStorageKind storage, byte slot)
        => storage == MuunStorageKind.Inventory
            ? player.Inventory?.GetItem(slot)
            : GetStorage(player, false)?.Items.FirstOrDefault(i => i.ItemSlot == slot);

    /// <summary>
    /// Gets the Muun of the main slot.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The Muun of the main slot.</returns>
    public static Item? GetMainMuun(Player player) => GetItem(player, MuunStorageKind.MuunInventory, MuunSlots.Main);

    /// <summary>
    /// Determines whether the player rides the Muun of its main slot.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns><c>true</c>, if the player rides.</returns>
    public static bool IsRiding(Player player) => States.TryGetValue(player, out var state) && state.IsRiding;

    /// <summary>
    /// Gets the options of the equipped Muuns as they stand now.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The options.</returns>
    public static IReadOnlyList<MuunActiveOption> GetOptions(Player player)
        => States.TryGetValue(player, out var state) ? state.Options : [];

    /// <summary>
    /// Shows the Muun inventory, the options and the Muun appearance to the player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The task.</returns>
    public static async ValueTask ShowAsync(Player player)
    {
        if (MuunFeaturePlugIn.Get(player.GameContext) is not { } plugIn || player.ViewPlugIns.GetPlugIn<IMuunViewPlugIn>() is not { } view)
        {
            return;
        }

        var items = GetStorage(player, false)?.Items.OrderBy(i => i.ItemSlot).ToList() ?? [];
        await view.ShowMuunInventoryAsync(items).ConfigureAwait(false);
        if (IsRiding(player) && !CanRideHere(player, plugIn.GetConfiguration()))
        {
            GetState(player).IsRiding = false;
        }

        await RefreshAsync(player, true).ConfigureAwait(false);
        await ShowAppearanceAsync(player).ConfigureAwait(false);
    }

    /// <summary>
    /// Evaluates the options of the equipped Muuns and applies them to the player. The options are sent when they
    /// changed, or always when <paramref name="force"/> is set.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="force">If set to <c>true</c>, the options are sent even if they didn't change.</param>
    /// <returns>The task.</returns>
    public static async ValueTask RefreshAsync(Player player, bool force)
    {
        var state = GetState(player);
        var plugIn = MuunFeaturePlugIn.Get(player.GameContext);
        var options = new List<MuunActiveOption>(2);
        var elements = new List<(AttributeDefinition Attribute, IElement Element)>(2);
        if (plugIn is not null && player.Attributes is { } attributes && GetStorage(player, false) is { } storage)
        {
            var now = DateTime.UtcNow;
            var totalLevel = Math.Max(1, (int)attributes[Stats.TotalLevel]);
            var map = player.CurrentMap?.MapId ?? -1;
            foreach (var slot in EquippedSlots)
            {
                if (storage.Items.FirstOrDefault(i => i.ItemSlot == slot) is { } item
                    && plugIn.GetMuun(item) is { } muun
                    && plugIn.GetOption(muun, totalLevel) is { } option)
                {
                    var level = GetLevel(item, muun);
                    var value = option.GetValue(level, muun.IsEvolved);
                    var addOption = item.TimedOptionExpiresAt > now ? plugIn.GetAddOption(muun, totalLevel) : null;
                    if (addOption is not null)
                    {
                        value = addOption.Apply(value, level, muun.IsEvolved);
                    }

                    var condition = muun.IsEvolved ? option.EvolvedCondition : option.Condition;
                    var isActive = item.Durability > 0 && value != 0 && condition.IsMet(map, totalLevel, now);
                    options.Add(new MuunActiveOption(slot, option.Type, value, isActive, addOption is not null));
                    if (isActive && MuunEffects.GetEffect(option.Type, muun.IsEvolved) is { } effect)
                    {
                        elements.Add((effect.Attribute, new SimpleElement(value * effect.Factor, AggregateType.AddRaw)));
                    }
                }
            }
        }

        bool changed;

        // The periodic check and the player's own actions both refresh; the swap of the elements is one step.
        lock (state)
        {
            foreach (var (attribute, element) in state.Elements)
            {
                player.Attributes?.RemoveElement(element, attribute);
            }

            foreach (var (attribute, element) in elements)
            {
                player.Attributes?.AddElement(element, attribute);
            }

            state.Elements = elements;
            changed = !state.Options.SequenceEqual(options);
            state.Options = options;
        }

        if (!force && !changed)
        {
            return;
        }

        await player.InvokeViewPlugInAsync<IMuunViewPlugIn>(p => p.ShowMuunOptionsAsync(options)).ConfigureAwait(false);
    }

    /// <summary>
    /// Moves an item between the inventory and the Muun inventory, or within one of them. Only a Muun goes into an
    /// equipped slot, and only items of the Muun item group into the Muun inventory.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="fromStorage">The storage of the source slot.</param>
    /// <param name="fromSlot">The source slot.</param>
    /// <param name="toStorage">The storage of the target slot.</param>
    /// <param name="toSlot">The target slot.</param>
    /// <returns>The result.</returns>
    public static async ValueTask<MuunActionResult> MoveAsync(Player player, MuunStorageKind fromStorage, byte fromSlot, MuunStorageKind toStorage, byte toSlot)
    {
        var result = await TryMoveAsync(player, fromStorage, fromSlot, toStorage, toSlot).ConfigureAwait(false);
        var item = result == MuunActionResult.Success ? GetItem(player, toStorage, toSlot) : null;
        await player.InvokeViewPlugInAsync<IMuunViewPlugIn>(p => p.MuunItemMovedAsync(result, fromStorage, fromSlot, toStorage, toSlot, item)).ConfigureAwait(false);
        if (result == MuunActionResult.Success && (IsEquippedPlace(fromStorage, fromSlot) || IsEquippedPlace(toStorage, toSlot)))
        {
            await OnEquipmentChangedAsync(player).ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Does an action with a source item on a target item: <see cref="MuunAction.LevelUp"/>, <see cref="MuunAction.Evolve"/>,
    /// <see cref="MuunAction.ChargeEnergy"/> or <see cref="MuunAction.Restore"/>. The source item is used up.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="action">The action.</param>
    /// <param name="sourceStorage">The storage of the source item.</param>
    /// <param name="sourceSlot">The slot of the source item.</param>
    /// <param name="targetStorage">The storage of the target item.</param>
    /// <param name="targetSlot">The slot of the target item.</param>
    /// <returns>The result.</returns>
    public static async ValueTask<MuunActionResult> UseItemAsync(Player player, MuunAction action, MuunStorageKind sourceStorage, byte sourceSlot, MuunStorageKind targetStorage, byte targetSlot)
    {
        var result = await TryUseItemAsync(player, action, sourceStorage, sourceSlot, targetStorage, targetSlot).ConfigureAwait(false);
        var target = GetItem(player, targetStorage, targetSlot);
        await player.InvokeViewPlugInAsync<IMuunViewPlugIn>(p => p.MuunActionResultAsync(action, result, targetStorage, targetSlot, target)).ConfigureAwait(false);
        if (result == MuunActionResult.Success && IsEquippedPlace(targetStorage, targetSlot))
        {
            await OnEquipmentChangedAsync(player).ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Exchanges the items of an offer of the exchange NPC, which the player talks to, for its evolution stone. The
    /// stone goes into the bag of the Muun inventory.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="offerIndex">The index of the offer.</param>
    /// <returns>The result.</returns>
    public static async ValueTask<MuunActionResult> ExchangeAsync(Player player, byte offerIndex)
    {
        var (result, stone) = await TryExchangeAsync(player, offerIndex).ConfigureAwait(false);
        await player.InvokeViewPlugInAsync<IMuunViewPlugIn>(p => p.MuunActionResultAsync(MuunAction.Exchange, result, MuunStorageKind.MuunInventory, stone?.ItemSlot ?? 0, stone)).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Mounts or dismounts the Muun of the main slot.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="ride">If set to <c>true</c>, the player mounts; otherwise, dismounts.</param>
    /// <returns>The result.</returns>
    public static async ValueTask<MuunActionResult> RideAsync(Player player, bool ride)
    {
        var result = TryRide(player, ride);
        await player.InvokeViewPlugInAsync<IMuunViewPlugIn>(p => p.MuunActionResultAsync(MuunAction.Ride, result, MuunStorageKind.MuunInventory, MuunSlots.Main, GetMainMuun(player))).ConfigureAwait(false);
        if (result == MuunActionResult.Success)
        {
            await ShowAppearanceAsync(player).ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Lets the equipped Muuns lose health. A Muun without health has no option until it is restored.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="amount">The amount of health.</param>
    /// <returns>The task.</returns>
    public static async ValueTask DecayHealthAsync(Player player, int amount)
    {
        if (GetStorage(player, false) is not { } storage || player.ViewPlugIns.GetPlugIn<IMuunViewPlugIn>() is not { } view)
        {
            return;
        }

        foreach (var item in storage.Items.Where(i => MuunSlots.IsEquipped(i.ItemSlot) && i.Durability > 0).ToList())
        {
            item.Durability = Math.Max(0, item.Durability - amount);
            await view.MuunActionResultAsync(MuunAction.Restore, MuunActionResult.Success, MuunStorageKind.MuunInventory, item.ItemSlot, item).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Gets the level of a Muun item: 1 to its highest level; a hero Muun is at its highest level.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="muun">The Muun.</param>
    /// <returns>The level.</returns>
    public static int GetLevel(Item item, MuunDefinition muun)
        => muun.Kind == MuunKind.Hero ? muun.MaximumLevel : Math.Clamp((int)item.Level, 1, muun.MaximumLevel);

    /// <summary>
    /// Gets the Muun which an evolution stone evolves.
    /// </summary>
    /// <param name="stone">The evolution stone.</param>
    /// <returns>The number of the Muun, or <c>null</c>.</returns>
    public static short? GetEvolutionTarget(Item stone)
        => stone.ItemOptions.FirstOrDefault(o => o.ItemOption?.OptionType == Season21ItemOptionTypes.MuunEvolutionTarget) is { } link ? (short)link.Level : null;

    /// <summary>
    /// Sets the Muun which an evolution stone evolves. The stone's definition needs the option.
    /// </summary>
    /// <param name="stone">The evolution stone.</param>
    /// <param name="muunNumber">The number of the Muun.</param>
    /// <param name="context">The persistence context.</param>
    /// <returns><c>true</c>, if the definition of the stone has the option.</returns>
    public static bool SetEvolutionTarget(Item stone, short muunNumber, IContext context)
    {
        if (stone.Definition?.PossibleItemOptions.SelectMany(o => o.PossibleOptions).FirstOrDefault(o => o.OptionType == Season21ItemOptionTypes.MuunEvolutionTarget) is not { } option)
        {
            return false;
        }

        var link = stone.ItemOptions.FirstOrDefault(o => o.ItemOption == option);
        if (link is null)
        {
            link = context.CreateNew<ItemOptionLink>();
            link.ItemOption = option;
            stone.ItemOptions.Add(link);
        }

        link.Level = muunNumber;
        return true;
    }

    private static MuunState GetState(Player player) => States.GetValue(player, _ => new MuunState());

    private static bool IsEquippedPlace(MuunStorageKind storage, byte slot) => storage == MuunStorageKind.MuunInventory && MuunSlots.IsEquipped(slot);

    private static async ValueTask OnEquipmentChangedAsync(Player player)
    {
        if (IsRiding(player) && (GetMainMuun(player) is not { } main || MuunFeaturePlugIn.Get(player.GameContext)?.GetMuun(main) is not { Kind: MuunKind.Mount }))
        {
            GetState(player).IsRiding = false;
        }

        await RefreshAsync(player, false).ConfigureAwait(false);
        await ShowAppearanceAsync(player).ConfigureAwait(false);
    }

    private static async ValueTask ShowAppearanceAsync(Player player)
    {
        var main = GetMainMuun(player);
        var isRiding = IsRiding(player);
        await player.ForEachWorldObserverAsync<IMuunViewPlugIn>(p => p.ShowMuunAppearanceAsync(player, main, isRiding), true).ConfigureAwait(false);
    }

    private static async ValueTask<MuunActionResult> TryMoveAsync(Player player, MuunStorageKind fromStorage, byte fromSlot, MuunStorageKind toStorage, byte toSlot)
    {
        if (!IsAvailable(player) || MuunFeaturePlugIn.Get(player.GameContext) is not { } plugIn || player.Inventory is not { } inventory)
        {
            return MuunActionResult.Disabled;
        }

        if (GetItem(player, fromStorage, fromSlot) is not { } item)
        {
            return MuunActionResult.ItemNotFound;
        }

        if (fromStorage == toStorage && fromSlot == toSlot)
        {
            return MuunActionResult.Success;
        }

        if (toStorage == MuunStorageKind.MuunInventory)
        {
            if (toSlot >= MuunSlots.Count)
            {
                return MuunActionResult.StorageFull;
            }

            if (item.Definition?.Group != plugIn.GetConfiguration().ItemGroup
                || (MuunSlots.IsEquipped(toSlot) && plugIn.GetMuun(item) is null))
            {
                return MuunActionResult.NotAMuun;
            }

            if (GetItem(player, MuunStorageKind.MuunInventory, toSlot) is not null)
            {
                return MuunActionResult.SlotOccupied;
            }
        }

        var storage = GetStorage(player, true)!;
        if (fromStorage == MuunStorageKind.Inventory)
        {
            if (toStorage == MuunStorageKind.Inventory)
            {
                return MuunActionResult.WrongMaterial;
            }

            await inventory.RemoveItemAsync(item).ConfigureAwait(false);
            await player.InvokeViewPlugInAsync<IItemRemovedPlugIn>(p => p.RemoveItemAsync(fromSlot)).ConfigureAwait(false);
            PutIntoMuunInventory(storage, item, toSlot, plugIn);
            return MuunActionResult.Success;
        }

        if (toStorage == MuunStorageKind.MuunInventory)
        {
            item.ItemSlot = toSlot;
            return MuunActionResult.Success;
        }

        storage.Items.Remove(item);
        if (!await inventory.AddItemAsync(toSlot, item).ConfigureAwait(false))
        {
            item.ItemSlot = fromSlot;
            storage.Items.Add(item);
            return MuunActionResult.SlotOccupied;
        }

        await player.InvokeViewPlugInAsync<IItemAppearPlugIn>(p => p.ItemAppearAsync(item)).ConfigureAwait(false);
        return MuunActionResult.Success;
    }

    private static void PutIntoMuunInventory(ItemStorage storage, Item item, byte slot, MuunFeaturePlugIn plugIn)
    {
        item.ItemSlot = slot;
        if (item.TimedOptionExpiresAt is null && plugIn.GetMuun(item) is { AddOptionDays: > 0 } muun)
        {
            item.TimedOptionExpiresAt = DateTime.UtcNow.AddDays(muun.AddOptionDays);
        }

        storage.Items.Add(item);
    }

    private static async ValueTask<MuunActionResult> TryUseItemAsync(Player player, MuunAction action, MuunStorageKind sourceStorage, byte sourceSlot, MuunStorageKind targetStorage, byte targetSlot)
    {
        if (!IsAvailable(player) || MuunFeaturePlugIn.Get(player.GameContext) is not { } plugIn)
        {
            return MuunActionResult.Disabled;
        }

        var source = GetItem(player, sourceStorage, sourceSlot);
        var target = GetItem(player, targetStorage, targetSlot);
        if (source is null || target is null || source == target)
        {
            return MuunActionResult.ItemNotFound;
        }

        var result = action switch
        {
            MuunAction.LevelUp => LevelUp(plugIn, source, target),
            MuunAction.Evolve => Evolve(player, plugIn, source, target),
            MuunAction.ChargeEnergy => ChargeEnergy(player, plugIn, source, target),
            MuunAction.Restore => Restore(plugIn, source, target),
            _ => MuunActionResult.WrongMaterial,
        };

        if (result != MuunActionResult.Success)
        {
            return result;
        }

        await RemoveAsync(player, sourceStorage, source).ConfigureAwait(false);
        if (targetStorage == MuunStorageKind.Inventory)
        {
            await player.InvokeViewPlugInAsync<IItemRemovedPlugIn>(p => p.RemoveItemAsync(targetSlot)).ConfigureAwait(false);
            await player.InvokeViewPlugInAsync<IItemAppearPlugIn>(p => p.ItemAppearAsync(target)).ConfigureAwait(false);
        }

        return result;
    }

    private static MuunActionResult LevelUp(MuunFeaturePlugIn plugIn, Item material, Item target)
    {
        if (plugIn.GetMuun(target) is not { } muun || muun.Kind == MuunKind.Hero)
        {
            return MuunActionResult.NotAMuun;
        }

        if (material.Definition != target.Definition || material.Level > 1)
        {
            return MuunActionResult.WrongMaterial;
        }

        var level = GetLevel(target, muun);
        if (muun.IsEvolved || level >= muun.MaximumLevel)
        {
            return MuunActionResult.MaximumLevel;
        }

        target.Level = (byte)(level + 1);
        return MuunActionResult.Success;
    }

    private static MuunActionResult Evolve(Player player, MuunFeaturePlugIn plugIn, Item stone, Item target)
    {
        var configuration = plugIn.GetConfiguration();
        if (plugIn.GetMuun(target) is not { } muun)
        {
            return MuunActionResult.NotAMuun;
        }

        if (muun.EvolvesTo is not { } evolvedNumber
            || player.GameContext.Configuration.Items.FirstOrDefault(d => d.Group == configuration.ItemGroup && d.Number == evolvedNumber) is not { } evolved)
        {
            return MuunActionResult.CannotEvolve;
        }

        var fits = stone.Definition?.Group == configuration.ItemGroup
            && ((stone.Definition.Number == configuration.EvolutionStoneNumber && GetEvolutionTarget(stone) == muun.Number)
                || (stone.Definition.Number == configuration.TempestEvolutionStoneNumber && muun.Grade == MuunGrade.Tempest));
        if (!fits)
        {
            return MuunActionResult.WrongMaterial;
        }

        if (GetLevel(target, muun) < muun.MaximumLevel)
        {
            return MuunActionResult.LevelTooLow;
        }

        target.Definition = evolved;
        return MuunActionResult.Success;
    }

    private static MuunActionResult ChargeEnergy(Player player, MuunFeaturePlugIn plugIn, Item source, Item converter)
    {
        var configuration = plugIn.GetConfiguration();
        if (converter.Definition?.Group != configuration.ItemGroup || converter.Definition.Number != configuration.EnergyConverterNumber)
        {
            return MuunActionResult.WrongMaterial;
        }

        int energy;
        if (plugIn.GetMuun(source) is { } muun)
        {
            var level = GetLevel(source, muun);
            energy = (configuration.RankEnergy.FirstOrDefault(r => r.Key == muun.Rank)?.Points ?? 0)
                     + (configuration.LevelEnergy.FirstOrDefault(r => r.Key == level)?.Points ?? 0);
        }
        else if (source.Definition?.Group == configuration.ItemGroup
                 && (source.Definition.Number == configuration.EvolutionStoneNumber || source.Definition.Number == configuration.TempestEvolutionStoneNumber))
        {
            energy = configuration.EvolutionStoneEnergy;
        }
        else
        {
            return MuunActionResult.WrongMaterial;
        }

        var charged = (int)converter.Durability + energy;
        if (charged < configuration.WizardryStoneEnergy)
        {
            converter.Durability = charged;
            return MuunActionResult.Success;
        }

        if (player.GameContext.Configuration.Items.FirstOrDefault(d => d.Group == configuration.ItemGroup && d.Number == configuration.WizardryStoneNumber) is not { } stone)
        {
            return MuunActionResult.ItemNotFound;
        }

        converter.Definition = stone;
        converter.Durability = 1;
        return MuunActionResult.Success;
    }

    private static MuunActionResult Restore(MuunFeaturePlugIn plugIn, Item jewel, Item target)
    {
        if (jewel.Definition is not { } definition || definition.Group != ItemConstants.JewelOfLife.Group || definition.Number != ItemConstants.JewelOfLife.Number)
        {
            return MuunActionResult.WrongMaterial;
        }

        if (plugIn.GetMuun(target) is null)
        {
            return MuunActionResult.NotAMuun;
        }

        // The health of a Muun is its durability, at most the durability of its definition.
        var maximum = Math.Max((byte)1, target.Definition!.Durability);
        if (target.Durability >= maximum)
        {
            return MuunActionResult.MaximumLevel;
        }

        target.Durability = maximum;
        return MuunActionResult.Success;
    }

    private static async ValueTask<(MuunActionResult Result, Item? Stone)> TryExchangeAsync(Player player, byte offerIndex)
    {
        if (!IsAvailable(player) || MuunFeaturePlugIn.Get(player.GameContext) is not { } plugIn)
        {
            return (MuunActionResult.Disabled, null);
        }

        var configuration = plugIn.GetConfiguration();
        if (player.OpenedNpc?.Definition.Number != configuration.ExchangeNpcNumber)
        {
            return (MuunActionResult.NpcNotOpened, null);
        }

        if (offerIndex >= configuration.ExchangeOffers.Count
            || configuration.ExchangeOffers[offerIndex] is not { } offer
            || player.GameContext.Configuration.Items.FirstOrDefault(d => d.Group == offer.ResultGroup && d.Number == offer.ResultNumber) is not { } resultDefinition)
        {
            return (MuunActionResult.ItemNotFound, null);
        }

        var storage = GetStorage(player, true)!;
        var paid = storage.Items.Select(i => (MuunStorageKind.MuunInventory, i))
            .Concat(player.Inventory?.Items.Select(i => (MuunStorageKind.Inventory, i)) ?? [])
            .Where(e => e.i.Definition?.Group == offer.CostGroup && e.i.Definition.Number == offer.CostNumber)
            .Take(offer.CostCount)
            .ToList();
        if (paid.Count < offer.CostCount)
        {
            return (MuunActionResult.NotEnoughItems, null);
        }

        if (FindFreeBagSlot(storage) is not { } slot)
        {
            return (MuunActionResult.StorageFull, null);
        }

        var stone = player.PersistenceContext.CreateNew<Item>();
        stone.Definition = resultDefinition;
        stone.Durability = Math.Max((byte)1, resultDefinition.Durability);
        if (offer.TargetNumber is { } target && !SetEvolutionTarget(stone, target, player.PersistenceContext))
        {
            await player.PersistenceContext.DeleteAsync(stone).ConfigureAwait(false);
            return (MuunActionResult.ItemNotFound, null);
        }

        foreach (var (kind, item) in paid)
        {
            await RemoveAsync(player, kind, item).ConfigureAwait(false);
        }

        stone.ItemSlot = slot;
        storage.Items.Add(stone);
        return (MuunActionResult.Success, stone);
    }

    private static MuunActionResult TryRide(Player player, bool ride)
    {
        if (!IsAvailable(player) || MuunFeaturePlugIn.Get(player.GameContext) is not { } plugIn)
        {
            return MuunActionResult.Disabled;
        }

        var state = GetState(player);
        if (!ride)
        {
            state.IsRiding = false;
            return MuunActionResult.Success;
        }

        if (GetMainMuun(player) is not { } main)
        {
            return MuunActionResult.ItemNotFound;
        }

        if (plugIn.GetMuun(main) is not { Kind: MuunKind.Mount } || main.Durability <= 0)
        {
            return MuunActionResult.NotAMount;
        }

        if (!CanRideHere(player, plugIn.GetConfiguration()))
        {
            return MuunActionResult.NotAllowedHere;
        }

        state.IsRiding = true;
        return MuunActionResult.Success;
    }

    private static bool CanRideHere(Player player, MuunConfiguration configuration)
        => player.CurrentMiniGame?.Definition.Type is not { } type || !configuration.NoRidingMiniGames.Contains(type);

    private static byte? FindFreeBagSlot(ItemStorage storage)
    {
        var used = storage.Items.Select(i => i.ItemSlot).ToHashSet();
        for (var slot = MuunSlots.FirstBagSlot; slot < MuunSlots.Count; slot++)
        {
            if (!used.Contains(slot))
            {
                return slot;
            }
        }

        return null;
    }

    private static async ValueTask RemoveAsync(Player player, MuunStorageKind storageKind, Item item)
    {
        if (storageKind == MuunStorageKind.Inventory)
        {
            var slot = item.ItemSlot;
            await player.Inventory!.RemoveItemAsync(item).ConfigureAwait(false);
            await player.InvokeViewPlugInAsync<IItemRemovedPlugIn>(p => p.RemoveItemAsync(slot)).ConfigureAwait(false);
        }
        else
        {
            GetStorage(player, false)?.Items.Remove(item);
        }

        await player.PersistenceContext.DeleteAsync(item).ConfigureAwait(false);
    }

    private sealed class MuunState
    {
        public List<(AttributeDefinition Attribute, IElement Element)> Elements { get; set; } = [];

        public IReadOnlyList<MuunActiveOption> Options { get; set; } = [];

        public bool IsRiding { get; set; }
    }
}
