// <copyright file="StarSmithingFeaturePlugIn.Forge.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

using System.Security.Cryptography;
using MUnique.OpenMU.GameLogic.ItemCustomization;
using MUnique.OpenMU.GameLogic.Views.Inventory;
using MUnique.OpenMU.GameLogic.Views.StarSmithing;

/// <summary>
/// The Constellation Forge on the wire (Readme.md "Constellation Forge", "Wire"): a start (EE 99) checks the smith, the
/// item and the slots, deals the sky from a seed of the server's own, spends the ingredients, the catalysts and the
/// hammer's durability, and answers with EE 9D; strikes (EE 9A) are stamped with the server's clock and answered with
/// EE 9E; a seal (EE 9B), a crack, an abandon (EE 9C) or the idle timeout ends the attempt with EE 9F and applies it.
/// </summary>
public partial class StarSmithingFeaturePlugIn
{
    /// <summary>An unused slot of EE 99.</summary>
    private const byte NoSlot = 0xFF;

    /// <summary>The most ingredient slots of EE 99.</summary>
    private const int MaximumIngredients = 10;

    /// <summary>The most catalyst slots of EE 99.</summary>
    private const int MaximumCatalysts = 4;

    /// <summary>The smith experience a sealed attunement (no recipe) gives, so experimenting is never a dead end.</summary>
    private const int AttunementExperience = 10;

    /// <summary>The share of a recipe's experience a crack gives, in percent (design section 3, step 6).</summary>
    private const int CrackExperiencePercent = 30;

    /// <summary>Gets or sets the server clock in milliseconds; tests replace it.</summary>
    internal Func<long> Clock { get; set; } = () => Environment.TickCount64;

    /// <summary>Gets or sets where a new attempt's seed comes from: a cryptographic random number; tests replace it.</summary>
    internal Func<long> NewSeed { get; set; } = () => BitConverter.ToInt64(RandomNumberGenerator.GetBytes(sizeof(long)));

    private StarSmithBeat ForgeBeat => new(this.Settings.BeatIntervalMs, this.Settings.BeatToleranceMs, this.Settings.RoundTripCapMs);

    private int IdleTimeoutMs => Math.Max(1, this.Settings.ForgeIdleTimeoutSeconds) * 1000;

    /// <summary>
    /// Starts a recipe attempt and answers with the sky (EE 9D).
    /// </summary>
    /// <param name="player">The smith.</param>
    /// <param name="targetSlot">The inventory slot of the item worked on.</param>
    /// <param name="ingredientSlots">The inventory slots of the ingredients (0xFF unused).</param>
    /// <param name="catalystSlots">The inventory slots of the catalysts (0xFF unused).</param>
    public async ValueTask StartForgeAsync(Player player, byte targetSlot, IReadOnlyList<byte> ingredientSlots, IReadOnlyList<byte> catalystSlots)
    {
        var (sky, attempt) = await this.TryStartForgeAsync(player, targetSlot, ingredientSlots, catalystSlots).ConfigureAwait(false);
        await player.InvokeViewPlugInAsync<IStarSmithingViewPlugIn>(p => p.SendForgeSkyAsync(sky)).ConfigureAwait(false);
        if (attempt is not null)
        {
            await this.SendStateAsync(player).ConfigureAwait(false);
            _ = Task.Run(() => this.WatchIdleAsync(player, attempt));
        }
    }

    /// <summary>
    /// Strikes a star of the running attempt at the server's time and answers it (EE 9E); a crack ends the attempt.
    /// </summary>
    /// <param name="player">The smith.</param>
    /// <param name="star">The star's index.</param>
    public async ValueTask StrikeForgeAsync(Player player, byte star)
    {
        if (player.StarSmithForge is not { } attempt)
        {
            var none = new StarSmithStrike(StarSmithStrikeResult.NotDrafting, StarSmithTiming.Late, default);
            await player.InvokeViewPlugInAsync<IStarSmithingViewPlugIn>(p => p.SendForgeStrikeAsync(star, none, 0, [])).ConfigureAwait(false);
            return;
        }

        var strike = attempt.Strike(star, this.Clock());
        var left = attempt.Forge.StrikesLeft;
        var guidance = attempt.Guidance();
        await player.InvokeViewPlugInAsync<IStarSmithingViewPlugIn>(p => p.SendForgeStrikeAsync(star, strike, left, guidance)).ConfigureAwait(false);
        if (strike.Result == StarSmithStrikeResult.Cracked && attempt.Forge.Result is { } result)
        {
            await this.EndForgeAsync(player, attempt, StarSmithForgeOutcome.Cracked, result).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Seals the running attempt and applies it; refused (EE 9F outcome 4) without one or before the first strike.
    /// </summary>
    /// <param name="player">The smith.</param>
    public async ValueTask SealForgeAsync(Player player)
    {
        if (player.StarSmithForge is { } attempt && attempt.Seal() is { } result)
        {
            await this.EndForgeAsync(player, attempt, StarSmithForgeOutcome.Sealed, result).ConfigureAwait(false);
            return;
        }

        await player.InvokeViewPlugInAsync<IStarSmithingViewPlugIn>(p => p.SendForgeResultAsync(StarSmithForgeOutcome.Refused, null)).ConfigureAwait(false);
    }

    /// <summary>
    /// Abandons the running attempt: the ingredients are lost, the item stays.
    /// </summary>
    /// <param name="player">The smith.</param>
    public async ValueTask AbandonForgeAsync(Player player)
    {
        if (player.StarSmithForge is { } attempt && attempt.Abandon() is { } result)
        {
            await this.EndForgeAsync(player, attempt, StarSmithForgeOutcome.Abandoned, result).ConfigureAwait(false);
            return;
        }

        await player.InvokeViewPlugInAsync<IStarSmithingViewPlugIn>(p => p.SendForgeResultAsync(StarSmithForgeOutcome.Refused, null)).ConfigureAwait(false);
    }

    /// <summary>
    /// Ends the running attempt like an abandon when it had no strike for the idle timeout.
    /// </summary>
    /// <param name="player">The smith.</param>
    internal async ValueTask TimeOutIdleForgeAsync(Player player)
    {
        if (player.StarSmithForge is { } attempt && attempt.IsIdle(this.Clock(), this.IdleTimeoutMs) && attempt.Abandon() is { } result)
        {
            await this.EndForgeAsync(player, attempt, StarSmithForgeOutcome.TimedOut, result).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// The smith's Guidebook rows for EE 90: the found recipes in full, by number (a stored recipe the world no longer
    /// has is left out), then a sealed row with its hint only for every recipe not found yet, by number.
    /// </summary>
    /// <param name="player">The smith.</param>
    /// <returns>The rows.</returns>
    internal IReadOnlyList<StarSmithGuidebookEntry> GuidebookOf(Player player)
    {
        var recipes = this.Settings.Recipes.OrderBy(r => r.Number).ToList();
        var found = StarSmithGuidebook.Parse(player.SelectedCharacter?.StarSmithGuidebook);
        var seen = StarSmithGuidebook.Seen(player.SelectedCharacter?.StarSmithGuidebook);
        var rows = recipes
            .Where(r => found.ContainsKey(r.Number))
            .Select(r => StarSmithGuidebookEntry.Found(r.Number, found[r.Number], r.Name) with
            {
                ShapeLength = r.Shape?.Stars.Count ?? 0,
                Ingredients = r.Ingredients,
                BoostsSeen = BoostsSeen(r, seen.GetValueOrDefault(r.Number)),
            })
            .ToList();
        rows.AddRange(recipes.Where(r => !found.ContainsKey(r.Number)).Select(r => r.HintKind switch
        {
            StarSmithHintKind.Riddle => StarSmithGuidebookEntry.Sealed(StarSmithHintKind.Riddle, r.Hint, 0, 0),
            StarSmithHintKind.Ingredient => StarSmithGuidebookEntry.Sealed(StarSmithHintKind.Ingredient, string.Empty, r.HintGroup, r.HintNumber),
            _ => StarSmithGuidebookEntry.Sealed(StarSmithHintKind.None, string.Empty, 0, 0),
        }));
        return rows;
    }

    /// <summary>
    /// What a seal's rolls are multiplied by: the shape's counted extension and the balance's scaling of the smith level.
    /// </summary>
    private double MagnitudeOf(StarSmithRecipe recipe, StarSmithForgeResult result, int smithLevel)
        => (1 + (result.Extension * (recipe.Shape?.ExtensionPercent ?? 0) / 100.0)) * this.Settings.Balance.LevelFactor(smithLevel);

    /// <summary>
    /// Per stat, the range (unscaled, times 100) of a recipe's outcomes whose bit is in the seen mask.
    /// </summary>
    private static IReadOnlyList<StarSmithBoostSeen> BoostsSeen(StarSmithRecipe recipe, int seen)
        => recipe.Outcomes
            .Where((_, index) => index < 31 && (seen & (1 << index)) != 0)
            .SelectMany(o => o.Boosts)
            .GroupBy(b => b.Stat)
            .OrderBy(g => g.Key)
            .Select(g => new StarSmithBoostSeen(g.Key, (int)Math.Round(g.Min(b => Math.Min(b.Minimum, b.Maximum)) * 100), (int)Math.Round(g.Max(b => Math.Max(b.Minimum, b.Maximum)) * 100)))
            .ToList();

    /// <summary>
    /// What a recipe's target rule reads of an item: its kind, level and excellent option.
    /// </summary>
    private static StarSmithTarget TargetOf(Item item)
    {
        var definition = item.Definition!;
        var slots = definition.ItemSlot?.ItemSlots ?? [];
        var kind = definition.Group switch
        {
            <= 5 => StarSmithTargetKind.Weapon,
            <= 11 => StarSmithTargetKind.Armor,
            _ when definition.IsWing() || slots.Contains(InventoryConstants.WingsSlot) => StarSmithTargetKind.Wings,
            _ when slots.Contains(InventoryConstants.PendantSlot) => StarSmithTargetKind.Pendant,
            _ when slots.Contains(InventoryConstants.Ring1Slot) || slots.Contains(InventoryConstants.Ring2Slot) => StarSmithTargetKind.Ring,
            _ => StarSmithTargetKind.None,
        };
        return new StarSmithTarget(kind, item.Level, item.IsExcellent());
    }

    private static async ValueTask DestroyAsync(Player player, Item item, Guid itemId)
    {
        if (player.Inventory is not { } inventory)
        {
            return;
        }

        var slot = item.ItemSlot;
        await inventory.RemoveItemAsync(item).ConfigureAwait(false);
        await player.PersistenceContext.DeleteAsync(item).ConfigureAwait(false);
        await player.InvokeViewPlugInAsync<IItemRemovedPlugIn>(p => p.RemoveItemAsync(slot)).ConfigureAwait(false);
        ItemCustomizationCache.Forget(itemId);
        StarSmithBoostBook.Forget(itemId);
    }

    /// <summary>The recipes in the smith's Guidebook, which guide the path and are no first completion.</summary>
    private static IReadOnlySet<short> KnownRecipes(Player player)
        => StarSmithGuidebook.Parse(player.SelectedCharacter?.StarSmithGuidebook).Keys.ToHashSet();

    /// <summary>
    /// The smith's reward of an ended attempt (design section 3, step 6): a seal gives the recipe's experience (an
    /// attunement a little), doubled on the first completion, which also writes the recipe into the Guidebook; a seal of
    /// a known recipe raises its made count; a crack gives 30% of the recipe it was on; an abandon or timeout nothing.
    /// </summary>
    private static void Reward(Player player, StarSmithForgeOutcome outcome, StarSmithForgeResult result, int rolledOutcome, StarSmithCurve levelCurve)
    {
        if (player.SelectedCharacter is not { } character)
        {
            return;
        }

        var earned = outcome switch
        {
            StarSmithForgeOutcome.Sealed => (result.Recipe?.Experience ?? AttunementExperience) * (result.FirstCompletion ? 2 : 1),
            StarSmithForgeOutcome.Cracked => (result.Recipe?.Experience ?? 0) * CrackExperiencePercent / 100,
            _ => 0,
        };
        if (outcome == StarSmithForgeOutcome.Sealed && result.Recipe is { } recipe)
        {
            character.StarSmithGuidebook = StarSmithGuidebook.Record(character.StarSmithGuidebook, recipe.Number, rolledOutcome);
        }

        var (level, experience) = StarSmithLevels.Add(character.StarSmithLevel, character.StarSmithExperience, Math.Max(0, earned), levelCurve);
        character.StarSmithLevel = (short)level;
        character.StarSmithExperience = experience;
    }

    private async ValueTask<(StarSmithForgeSkyMessage Sky, StarSmithForgeAttempt? Attempt)> TryStartForgeAsync(Player player, byte targetSlot, IReadOnlyList<byte> ingredientSlots, IReadOnlyList<byte> catalystSlots)
    {
        if (player.StarSmithForge is not null)
        {
            return (StarSmithForgeSkyMessage.Refused(StarSmithResultCode.AttemptRunning, targetSlot), null);
        }

        var hammer = this.HammerOf(player);
        var item = player.Inventory?.GetItem(targetSlot);
        var itemId = item is MUnique.OpenMU.Persistence.IIdentifiable { Id: var id } && id != Guid.Empty && !ReferenceEquals(item, hammer) && item.Definition is not null ? id : Guid.Empty;
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
            ZenCost: 0);
        var result = StarSmithRules.Check(StarSmithAction.Forge, check);
        if (result != StarSmithResultCode.Done)
        {
            return (StarSmithForgeSkyMessage.Refused(result, targetSlot), null);
        }

        var named = new HashSet<byte> { targetSlot };
        var ingredients = this.SlotItems(player, ingredientSlots, MaximumIngredients, named, hammer);
        var catalysts = this.SlotItems(player, catalystSlots, MaximumCatalysts, named, hammer);
        if (ingredients is null || catalysts is null)
        {
            return (StarSmithForgeSkyMessage.Refused(StarSmithResultCode.WrongSlot, targetSlot), null);
        }

        // Catalysts (jewels) never count as ingredients.
        if (ingredients.Count == 0)
        {
            return (StarSmithForgeSkyMessage.Refused(StarSmithResultCode.NoIngredients, targetSlot), null);
        }

        var target = TargetOf(item!);
        var counts = ingredients
            .GroupBy(i => ((byte)i.Definition!.Group, i.Definition.Number))
            .Select(g => (g.Key.Item1, g.Key.Number, g.Count()))
            .ToList();
        // No fitting recipe is no refusal: the sky has no shape and a seal attunes (design section 3).
        var candidates = StarSmithRecipes.Candidates(this.Settings.Recipes, target, counts);

        var level = Math.Clamp((int)(player.SelectedCharacter?.StarSmithLevel ?? 1), 1, StarSmithLevels.Maximum);
        var beat = this.ForgeBeat;

        // Measured by the server's own beat ping (EE F3/F4); a client claim is never read. The beat caps it.
        var roundTripMs = this.RoundTripOf(player).RoundTripMs;
        var attempt = StarSmithForgeAttempt.Start(
            this.NewSeed(),
            candidates,
            target,
            level,
            ingredients.Count,
            catalysts.Select(c => ((byte)c.Definition!.Group, c.Definition.Number)).ToList(),
            this.Settings.Forge,
            beat,
            this.Clock(),
            roundTripMs,
            KnownRecipes(player),
            targetSlot,
            itemId);
        attempt.AttunementStat = StarSmithOutcomes.AttunementStat(ingredients.Select(i => (byte)i.Definition!.Group));
        player.StarSmithForge = attempt;

        // Spent at the start, so leaving (or losing the connection) can never keep them.
        foreach (var spent in ingredients.Concat(catalysts))
        {
            await SpendOneAsync(player, spent).ConfigureAwait(false);
        }

        if (hammer is not null)
        {
            hammer.Durability = Math.Max(0, hammer.Durability - this.Settings.DurabilityPerAction);
            await player.InvokeViewPlugInAsync<IItemDurabilityChangedPlugIn>(p => p.ItemDurabilityChangedAsync(hammer, false)).ConfigureAwait(false);
        }

        var firstBeatMs = (int)Math.Clamp(attempt.StartMs + beat.IntervalMs - this.Clock(), 0, beat.IntervalMs);
        return (StarSmithForgeSkyMessage.From(attempt.Sky, targetSlot, beat, firstBeatMs), attempt);
    }

    /// <summary>
    /// The items in the named slots, or null when one is empty, named twice, the hammer or the item worked on.
    /// </summary>
    private List<Item>? SlotItems(Player player, IReadOnlyList<byte> slots, int maximum, HashSet<byte> named, Item? hammer)
    {
        var items = new List<Item>();
        foreach (var slot in slots.Take(maximum).Where(s => s != NoSlot))
        {
            if (!named.Add(slot) || player.Inventory?.GetItem(slot) is not { Definition: not null } found || ReferenceEquals(found, hammer))
            {
                return null;
            }

            items.Add(found);
        }

        return items;
    }

    /// <summary>
    /// Ends an attempt once: applies it to the item, answers EE 9F and sends the smith's state. A seal writes the
    /// rarity and shows the hammering to everyone who sees the smith; a crack applies the recipe's fate; an abandon or a
    /// timeout leaves the item as it is (the ingredients were spent at the start).
    /// </summary>
    private async ValueTask EndForgeAsync(Player player, StarSmithForgeAttempt attempt, StarSmithForgeOutcome outcome, StarSmithForgeResult result)
    {
        if (!ReferenceEquals(player.StarSmithForge, attempt))
        {
            return;
        }

        player.StarSmithForge = null;
        var item = player.Inventory?.Items.FirstOrDefault(i => i is MUnique.OpenMU.Persistence.IIdentifiable { Id: var id } && id == attempt.ItemId);
        var existing = (await player.PersistenceContext.GetItemCustomizationsAsync([attempt.ItemId]).ConfigureAwait(false)).FirstOrDefault();
        StarSmithRoll? roll = null;
        switch (outcome)
        {
            case StarSmithForgeOutcome.Sealed:
                var smithLevel = (short)Math.Clamp((int)(player.SelectedCharacter?.StarSmithLevel ?? 1), 1, StarSmithLevels.Maximum);
                roll = result.Recipe is { } sealedRecipe
                    ? StarSmithOutcomes.Roll(sealedRecipe, result.Rarity, this.MagnitudeOf(sealedRecipe, result, smithLevel), attempt.Seed, this.Settings.Balance)
                    : StarSmithOutcomes.Roll(StarSmithOutcomes.Attunement(attempt.AttunementStat), result.Rarity, this.Settings.Balance.LevelFactor(smithLevel), attempt.Seed, this.Settings.Balance);
                await SaveAsync(player, attempt.ItemId, existing, c =>
                {
                    c.Rarity = (byte)result.Rarity;
                    c.Boosts = StarSmithBoosts.Format(roll?.Boosts ?? []);
                    c.SmithLevel = smithLevel;
                }).ConfigureAwait(false);
                ApplyBoosts(player, item, attempt.ItemId, roll?.Boosts ?? []);
                await player.ForEachWorldObserverAsync<IStarSmithingViewPlugIn>(p => p.SendAnimationAsync(player, StarSmithAction.Forge, Strikes), true).ConfigureAwait(false);
                break;
            case StarSmithForgeOutcome.Cracked when result.Fail == StarSmithOnFail.ItemDowngraded && existing is not null:
                await SaveAsync(player, attempt.ItemId, existing, c =>
                {
                    c.Name = null;
                    c.Lore = null;
                    c.Look = null;
                    c.Rarity = 0;
                    c.Boosts = null;
                    c.SmithLevel = 0;
                }).ConfigureAwait(false);
                ApplyBoosts(player, item, attempt.ItemId, []);
                break;
            case StarSmithForgeOutcome.Cracked when result.Fail == StarSmithOnFail.ItemDestroyed && item is not null:
                await DestroyAsync(player, item, attempt.ItemId).ConfigureAwait(false);
                break;
            default:
                break;
        }

        Reward(player, outcome, result, roll?.OutcomeIndex ?? -1, this.Settings.Balance.LevelCurve);
        await player.InvokeViewPlugInAsync<IStarSmithingViewPlugIn>(p => p.SendForgeResultAsync(outcome, result)).ConfigureAwait(false);
        await this.SendStateAsync(player).ConfigureAwait(false);
    }

    /// <summary>
    /// Waits for the attempt's idle timeout and ends it then, unless strikes kept it going or it ended before.
    /// </summary>
    private async Task WatchIdleAsync(Player player, StarSmithForgeAttempt attempt)
    {
        try
        {
            while (ReferenceEquals(player.StarSmithForge, attempt))
            {
                var wait = attempt.LastActivityMs + this.IdleTimeoutMs - this.Clock();
                if (wait > 0)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(Math.Min(wait, this.IdleTimeoutMs))).ConfigureAwait(false);
                    continue;
                }

                await this.TimeOutIdleForgeAsync(player).ConfigureAwait(false);
                return;
            }
        }
        catch (Exception ex)
        {
            player.Logger.LogError(ex, "The idle timeout of a Constellation Forge attempt failed for {Player}.", player.Name);
        }
    }
}
