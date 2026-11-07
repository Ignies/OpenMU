// <copyright file="AbilityCoreService.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.AbilityCore;

using System.Runtime.CompilerServices;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.Progression;
using MUnique.OpenMU.GameLogic.Views.AbilityCore;
using MUnique.OpenMU.GameLogic.Views.Inventory;

/// <summary>
/// The Ability Enhancement Core of a player: its storage, the effects of its cards and the actions of its window.
/// See Readme.md.
/// </summary>
public static class AbilityCoreService
{
    /// <summary>
    /// The inventory slot of a request which lets the server pick: the first free slot, or the card in the core.
    /// </summary>
    public const byte AnySlot = 0xFF;

    private static readonly ConditionalWeakTable<Player, CoreState> States = new();

    /// <summary>
    /// Gets the core storage of the character of the player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="create">If set to <c>true</c>, a missing storage is created.</param>
    /// <returns>The storage, if it exists or was created.</returns>
    public static ItemStorage? GetStorage(Player player, bool create)
        => player.SelectedCharacter is { } character
            ? character.AbilityCore ??= create ? player.PersistenceContext.CreateNew<ItemStorage>() : null
            : null;

    /// <summary>
    /// Gets the open slots of the player's character, bit n for the slot n.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The bits.</returns>
    public static uint GetOpenSlots(Player player)
        => GetConfiguration(player) is { } configuration ? GetOpenSlots(player, configuration) : 0;

    /// <summary>
    /// Gets the slots whose card takes effect, bit n for the slot n.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The bits.</returns>
    public static uint GetActiveSlots(Player player)
        => GetConfiguration(player) is { } configuration
            ? GetActiveCards(player, configuration).Aggregate(0u, (bits, entry) => bits | (1u << entry.Item.ItemSlot))
            : 0;

    /// <summary>
    /// Shows the core to the player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The task.</returns>
    public static async ValueTask ShowAsync(Player player)
    {
        if (GetConfiguration(player) is not { } configuration || player.ViewPlugIns.GetPlugIn<IAbilityCoreViewPlugIn>() is not { } view)
        {
            return;
        }

        var cards = GetStorage(player, false)?.Items.Where(i => i.ItemSlot < AbilityCoreSlots.Count).ToList() ?? [];
        await view.ShowAbilityCoreAsync(GetOpenSlots(player, configuration), GetActiveSlots(player), cards).ConfigureAwait(false);
    }

    /// <summary>
    /// Applies the options and skills of the cards which take effect, and takes away those which no longer do.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The task.</returns>
    public static async ValueTask RefreshAsync(Player player)
    {
        var state = States.GetValue(player, _ => new CoreState());
        var configuration = GetConfiguration(player);
        var active = configuration is null ? [] : GetActiveCards(player, configuration);
        foreach (var powerUp in state.PowerUps)
        {
            powerUp.Dispose();
        }

        state.PowerUps.Clear();
        if (player.Attributes is { } attributes && GetBaseClass(player) is { } baseClass)
        {
            foreach (var (item, card) in active)
            {
                AddPowerUps(state.PowerUps, item, card, baseClass, attributes);
            }
        }

        var skills = active.Where(e => e.Card.Skill > 0).Select(e => e.Card.Skill).ToHashSet();
        if (player.SkillList is not { } skillList)
        {
            return;
        }

        foreach (var removed in state.Skills.Except(skills).ToList())
        {
            await skillList.RemoveItemSkillAsync((ushort)removed).ConfigureAwait(false);
            state.Skills.Remove(removed);
        }

        foreach (var added in skills.Except(state.Skills).ToList())
        {
            if (player.GameContext.Configuration.Skills.FirstOrDefault(s => s.Number == added) is { } skill)
            {
                await skillList.AddItemSkillAsync(skill).ConfigureAwait(false);
                state.Skills.Add(added);
            }
        }
    }

    /// <summary>
    /// Does an action of the core window and shows its result; after a success, the effects and the core are updated.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="action">The action.</param>
    /// <param name="coreSlot">The slot of the core.</param>
    /// <param name="inventorySlot">The slot of the inventory, or <see cref="AnySlot"/>.</param>
    /// <returns>The result.</returns>
    public static async ValueTask<AbilityCoreResult> RequestAsync(Player player, AbilityCoreAction action, byte coreSlot, byte inventorySlot)
    {
        (AbilityCoreResult Result, Item? Item) outcome = (AbilityCoreResult.Disabled, null);
        if (GetConfiguration(player) is { } configuration && player.Inventory is not null)
        {
            outcome = player.SelectedCharacter?.CharacterClass?.GetEvolution() != ClassEvolution.Fifth
                ? (AbilityCoreResult.NotFifthEvolution, null)
                : action switch
                {
                    AbilityCoreAction.Insert => await InsertAsync(player, configuration, coreSlot, inventorySlot).ConfigureAwait(false),
                    AbilityCoreAction.Remove => await RemoveAsync(player, coreSlot, inventorySlot).ConfigureAwait(false),
                    AbilityCoreAction.LevelUp => await LevelUpAsync(player, configuration, coreSlot, inventorySlot).ConfigureAwait(false),
                    AbilityCoreAction.Reduce => await ReduceAsync(player, configuration, inventorySlot).ConfigureAwait(false),
                    _ => (AbilityCoreResult.NotACard, null),
                };
        }

        var (result, item) = outcome;
        await player.InvokeViewPlugInAsync<IAbilityCoreViewPlugIn>(p => p.AbilityCoreResultAsync(action, result, coreSlot, inventorySlot, item)).ConfigureAwait(false);
        if (result == AbilityCoreResult.Success)
        {
            await RefreshAsync(player).ConfigureAwait(false);
            await ShowAsync(player).ConfigureAwait(false);
        }

        return result;
    }

    private static AbilityCoreConfiguration? GetConfiguration(Player player) => AbilityCoreFeaturePlugIn.Get(player.GameContext)?.GetConfiguration();

    private static BaseClassCode? GetBaseClass(Player player)
        => player.SelectedCharacter?.CharacterClass is { } characterClass ? ClassCodes.GetBaseClass(characterClass.Number) : null;

    private static uint GetOpenSlots(Player player, AbilityCoreConfiguration configuration)
    {
        if (player.SelectedCharacter?.CharacterClass is not { } characterClass || player.Attributes is not { } attributes)
        {
            return 0;
        }

        return AbilityCoreRules.GetOpenSlots(
            characterClass.GetEvolution(),
            (int)attributes[Stats.Level],
            (int)attributes[Stats.MasterLevel],
            (int)attributes[Stats.Resets],
            ProgressionFeaturePlugIn.GetWorld(player.GameContext)?.Configuration,
            configuration);
    }

    private static List<(Item Item, AbilityCardDefinition Card)> GetActiveCards(Player player, AbilityCoreConfiguration configuration)
    {
        if (GetStorage(player, false) is not { } storage || GetBaseClass(player) is not { } baseClass)
        {
            return [];
        }

        var open = GetOpenSlots(player, configuration);
        var fitting = new List<(Item Item, AbilityCardDefinition Card)>();
        foreach (var item in storage.Items)
        {
            if (item.ItemSlot < AbilityCoreSlots.Count
                && (open & (1u << item.ItemSlot)) != 0
                && configuration.FindCard(item) is { } card
                && CheckFit(player, configuration, card, item.ItemSlot, baseClass) == AbilityCoreResult.Success)
            {
                fitting.Add((item, card));
            }
        }

        return fitting.Where(e => e.Card.Kind != AbilityCardKind.Elite || CheckRow(fitting, e.Item.ItemSlot, e.Item.Level) == AbilityCoreResult.Success).ToList();
    }

    private static AbilityCoreResult CheckFit(Player player, AbilityCoreConfiguration configuration, AbilityCardDefinition card, byte slot, BaseClassCode baseClass)
    {
        if (card.Kind != AbilityCoreSlots.GetKind(slot)
            || !card.IsFor(baseClass)
            || (configuration.RowsMatchColor && card.Kind is AbilityCardKind.Option or AbilityCardKind.Elite && (int)card.Color != (AbilityCoreSlots.GetRow(slot) + 1)))
        {
            return AbilityCoreResult.WrongSlot;
        }

        if (card.Kind == AbilityCardKind.Skill && card.RequiredSkill > 0 && player.SkillList?.ContainsSkill((ushort)card.RequiredSkill) != true)
        {
            return AbilityCoreResult.RequirementMissing;
        }

        return AbilityCoreResult.Success;
    }

    /// <summary>
    /// Checks the row of an elite slot: its five option slots hold fitting cards, none below the elite's level.
    /// </summary>
    private static AbilityCoreResult CheckRow(IReadOnlyCollection<(Item Item, AbilityCardDefinition Card)> fitting, byte eliteSlot, byte eliteLevel)
    {
        var row = AbilityCoreSlots.GetOptionSlots(AbilityCoreSlots.GetRow(eliteSlot)).ToHashSet();
        var options = fitting.Where(e => row.Contains(e.Item.ItemSlot)).Select(e => e.Item.Level).ToList();
        if (options.Count < AbilityCoreSlots.OptionsPerRow)
        {
            return AbilityCoreResult.RequirementMissing;
        }

        return eliteLevel > options.Min() ? AbilityCoreResult.EliteLevelTooHigh : AbilityCoreResult.Success;
    }

    private static void AddPowerUps(List<PowerUpWrapper> powerUps, Item item, AbilityCardDefinition card, BaseClassCode baseClass, AttributeSystem attributes)
    {
        foreach (var link in item.ItemOptions)
        {
            if (link.ItemOption is not IncreasableItemOption option
                || option.OptionType != Season21ItemOptionTypes.AbilityCardOption
                || (card.Options.FirstOrDefault(o => o.Number == option.Number) is { } cardOption && !cardOption.AppliesTo(baseClass)))
            {
                continue;
            }

            var powerUp = option.LevelDependentOptions.FirstOrDefault(l => l.Level == item.Level)?.PowerUpDefinition ?? option.PowerUpDefinition;
            if (powerUp?.Boost is not null && powerUp.TargetAttribute is not null)
            {
                powerUps.AddRange(PowerUpWrapper.CreateByPowerUpDefinition(powerUp, attributes));
            }
        }
    }

    private static async ValueTask<(AbilityCoreResult Result, Item? Item)> InsertAsync(Player player, AbilityCoreConfiguration configuration, byte coreSlot, byte inventorySlot)
    {
        if (GetBagItem(player, inventorySlot) is not { } item)
        {
            return (AbilityCoreResult.ItemNotFound, null);
        }

        if (configuration.FindCard(item) is not { } card)
        {
            return (AbilityCoreResult.NotACard, null);
        }

        if (coreSlot >= AbilityCoreSlots.Count || (GetOpenSlots(player, configuration) & (1u << coreSlot)) == 0)
        {
            return (AbilityCoreResult.SlotLocked, null);
        }

        var fit = CheckFit(player, configuration, card, coreSlot, GetBaseClass(player)!.Value);
        if (fit != AbilityCoreResult.Success)
        {
            return (fit, null);
        }

        var storage = GetStorage(player, true)!;
        if (storage.Items.Any(i => i.ItemSlot == coreSlot))
        {
            return (AbilityCoreResult.SlotOccupied, null);
        }

        if (card.Kind == AbilityCardKind.Elite
            && CheckRow(GetActiveCards(player, configuration), coreSlot, item.Level) is var row and not AbilityCoreResult.Success)
        {
            return (row, null);
        }

        await player.Inventory!.RemoveItemAsync(item).ConfigureAwait(false);
        await player.InvokeViewPlugInAsync<IItemRemovedPlugIn>(p => p.RemoveItemAsync(inventorySlot)).ConfigureAwait(false);
        item.ItemSlot = coreSlot;
        storage.Items.Add(item);
        return (AbilityCoreResult.Success, item);
    }

    private static async ValueTask<(AbilityCoreResult Result, Item? Item)> RemoveAsync(Player player, byte coreSlot, byte inventorySlot)
    {
        if (GetStorage(player, false) is not { } storage || storage.Items.FirstOrDefault(i => i.ItemSlot == coreSlot) is not { } item)
        {
            return (AbilityCoreResult.ItemNotFound, null);
        }

        var inventory = player.Inventory!;
        if ((inventorySlot == AnySlot ? inventory.CheckInvSpace(item) : inventorySlot) is not { } target)
        {
            return (AbilityCoreResult.InventoryFull, null);
        }

        storage.Items.Remove(item);
        if (!await inventory.AddItemAsync(target, item).ConfigureAwait(false))
        {
            item.ItemSlot = coreSlot;
            storage.Items.Add(item);
            return (AbilityCoreResult.InventoryFull, null);
        }

        await player.InvokeViewPlugInAsync<IItemAppearPlugIn>(p => p.ItemAppearAsync(item)).ConfigureAwait(false);
        return (AbilityCoreResult.Success, item);
    }

    private static async ValueTask<(AbilityCoreResult Result, Item? Item)> LevelUpAsync(Player player, AbilityCoreConfiguration configuration, byte coreSlot, byte inventorySlot)
    {
        var inCore = inventorySlot == AnySlot;
        var item = inCore ? GetStorage(player, false)?.Items.FirstOrDefault(i => i.ItemSlot == coreSlot) : GetBagItem(player, inventorySlot);
        if (item is null)
        {
            return (AbilityCoreResult.ItemNotFound, null);
        }

        if (configuration.FindCard(item) is not { Kind: not AbilityCardKind.Extra } card)
        {
            return (AbilityCoreResult.NotACard, null);
        }

        if (item.Level >= configuration.MaximumCardLevel
            || configuration.LevelUps.FirstOrDefault(l => l.Kind == card.Kind && l.Level == item.Level) is not { } price)
        {
            return (AbilityCoreResult.MaximumLevel, null);
        }

        if (inCore && card.Kind == AbilityCardKind.Elite
            && CheckRow(GetActiveCards(player, configuration), item.ItemSlot, (byte)(item.Level + 1)) is var row and not AbilityCoreResult.Success)
        {
            return (row, null);
        }

        var inventory = player.Inventory!;
        var materials = GetBagItems(player)
            .Where(i => i != item && i.Definition == item.Definition)
            .OrderBy(i => i.Level)
            .Take(price.Cards)
            .ToList();
        if (materials.Count < price.Cards)
        {
            return (AbilityCoreResult.NotEnoughCards, null);
        }

        var stones = GetBagItems(player).Where(i => i.Definition?.Group == price.StoneGroup && i.Definition.Number == price.StoneNumber).ToList();
        if (stones.Sum(s => s.Durability) < price.Stones)
        {
            return (AbilityCoreResult.NotEnoughStones, null);
        }

        if (!player.TryRemoveMoney(price.Money))
        {
            return (AbilityCoreResult.NotEnoughMoney, null);
        }

        foreach (var material in materials)
        {
            await DeleteFromInventoryAsync(player, material).ConfigureAwait(false);
        }

        await ConsumeAsync(player, stones, price.Stones).ConfigureAwait(false);
        item.Level++;
        if (!inCore)
        {
            await player.InvokeViewPlugInAsync<IItemUpgradedPlugIn>(p => p.ItemUpgradedAsync(item)).ConfigureAwait(false);
        }

        return (AbilityCoreResult.Success, item);
    }

    private static async ValueTask<(AbilityCoreResult Result, Item? Item)> ReduceAsync(Player player, AbilityCoreConfiguration configuration, byte inventorySlot)
    {
        if (GetBagItem(player, inventorySlot) is not { } item)
        {
            return (AbilityCoreResult.ItemNotFound, null);
        }

        if (configuration.FindCard(item) is not { } card
            || configuration.Reductions.FirstOrDefault(r => r.Kind == card.Kind) is not { } reduction
            || player.GameContext.Configuration.Items.FirstOrDefault(d => d.Group == reduction.StoneGroup && d.Number == reduction.StoneNumber) is not { } stoneDefinition)
        {
            return (AbilityCoreResult.NotACard, null);
        }

        if (player.Money < reduction.Money)
        {
            return (AbilityCoreResult.NotEnoughMoney, null);
        }

        var inventory = player.Inventory!;
        var stone = player.PersistenceContext.CreateNew<Item>();
        stone.Definition = stoneDefinition;
        stone.Durability = Math.Max(1, reduction.Stones);
        await inventory.RemoveItemAsync(item).ConfigureAwait(false);
        if (inventory.CheckInvSpace(stone) is not { } slot || !await inventory.AddItemAsync(slot, stone).ConfigureAwait(false))
        {
            await inventory.AddItemAsync(inventorySlot, item).ConfigureAwait(false);
            await player.PersistenceContext.DeleteAsync(stone).ConfigureAwait(false);
            return (AbilityCoreResult.InventoryFull, null);
        }

        player.TryRemoveMoney(reduction.Money);
        await player.InvokeViewPlugInAsync<IItemRemovedPlugIn>(p => p.RemoveItemAsync(inventorySlot)).ConfigureAwait(false);
        await player.PersistenceContext.DeleteAsync(item).ConfigureAwait(false);
        await player.InvokeViewPlugInAsync<IItemAppearPlugIn>(p => p.ItemAppearAsync(stone)).ConfigureAwait(false);
        return (AbilityCoreResult.Success, stone);
    }

    private static IEnumerable<Item> GetBagItems(Player player)
        => player.Inventory?.Items.Where(i => !InventoryConstants.IsEquippedSlot(i.ItemSlot)) ?? [];

    private static Item? GetBagItem(Player player, byte slot)
        => slot == AnySlot || InventoryConstants.IsEquippedSlot(slot) ? null : player.Inventory?.GetItem(slot);

    private static async ValueTask ConsumeAsync(Player player, List<Item> stones, int amount)
    {
        foreach (var stone in stones)
        {
            if (amount <= 0)
            {
                return;
            }

            var taken = Math.Min(amount, (int)stone.Durability);
            amount -= taken;
            stone.Durability -= taken;
            if (stone.Durability <= 0)
            {
                await DeleteFromInventoryAsync(player, stone).ConfigureAwait(false);
            }
            else
            {
                await player.InvokeViewPlugInAsync<IItemDurabilityChangedPlugIn>(p => p.ItemDurabilityChangedAsync(stone, true)).ConfigureAwait(false);
            }
        }
    }

    private static async ValueTask DeleteFromInventoryAsync(Player player, Item item)
    {
        var slot = item.ItemSlot;
        await player.Inventory!.RemoveItemAsync(item).ConfigureAwait(false);
        await player.InvokeViewPlugInAsync<IItemRemovedPlugIn>(p => p.RemoveItemAsync(slot)).ConfigureAwait(false);
        await player.PersistenceContext.DeleteAsync(item).ConfigureAwait(false);
    }

    private sealed class CoreState
    {
        public List<PowerUpWrapper> PowerUps { get; } = [];

        public HashSet<short> Skills { get; } = [];
    }
}
