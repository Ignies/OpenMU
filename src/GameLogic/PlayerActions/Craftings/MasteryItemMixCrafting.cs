// <copyright file="MasteryItemMixCrafting.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Craftings;

using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.PlayerActions.Items;
using MUnique.OpenMU.GameLogic.Views.NPC;
using MUnique.OpenMU.GameLogic.WideItems;
using MUnique.OpenMU.Persistence;

/// <summary>
/// The mastery item mixes of Season 21 (unsealing, tier upgrades, the mastery jewelry): one crafting which finds the
/// mix by the items in the chaos machine, in the mix table of <see cref="Season21ItemsFeaturePlugIn"/>.
/// </summary>
public class MasteryItemMixCrafting : BaseItemCraftingHandler
{
    private const int GroupSize = 512;
    private const int OptionValuePerLevel = 4;
    private const int MaximumNewOptionLevel = 4;

    /// <inheritdoc />
    public override CraftingResult? TryGetRequiredItems(Player player, out IList<CraftingRequiredItemLink> items, out byte successRateByItems)
    {
        items = [];
        successRateByItems = 0;
        var storage = player.TemporaryStorage?.Items.ToList() ?? [];
        if (storage.Count == 0 || Season21ItemsFeaturePlugIn.GetConfiguration(player.GameContext) is not { } configuration)
        {
            return CraftingResult.IncorrectMixItems;
        }

        foreach (var recipe in configuration.MasteryMixes)
        {
            if (Match(recipe, storage) is { } links
                && player.GameContext.Configuration.Items.FirstOrDefault(d => IndexOf(d) == recipe.ResultIndex) is { } result
                && player.CanSee(result))
            {
                items = links;
                successRateByItems = recipe.SuccessRate;
                return null;
            }
        }

        return CraftingResult.IncorrectMixItems;
    }

    /// <inheritdoc />
    protected override int GetPrice(byte successRate, IList<CraftingRequiredItemLink> requiredItems)
        => requiredItems.Select(l => l.ItemRequirement).OfType<MixRequirement>().FirstOrDefault()?.Recipe.Money ?? 0;

    /// <inheritdoc />
    protected override async ValueTask<List<Item>> CreateOrModifyResultItemsAsync(IList<CraftingRequiredItemLink> requiredItems, Player player, byte socketSlot, byte successRate)
    {
        var mainLink = requiredItems.First(l => l.ItemRequirement is MixRequirement { IsMain: true });
        var recipe = ((MixRequirement)mainLink.ItemRequirement).Recipe;
        var main = mainLink.Items.First();
        var result = player.PersistenceContext.CreateNew<Item>();
        result.Definition = player.GameContext.Configuration.Items.First(d => IndexOf(d) == recipe.ResultIndex);
        result.Level = recipe.Keep.HasFlag(MasteryMixKeep.CopyAll) ? main.Level : recipe.ResultLevel;
        result.HasSkill = result.Definition.Skill is not null
                          && (recipe.SkillPercent is { } skillPercent ? Rand.NextRandomBool(skillPercent) : main.HasSkill || !recipe.Keep.HasFlag(MasteryMixKeep.CopyAll));
        result.SocketCount = Math.Min(recipe.SocketWeights.Count > 0 ? RollSockets(recipe.SocketWeights) : main.SocketCount, result.Definition.MaximumSockets);
        CopyOptions(main, result, recipe.Keep, player.PersistenceContext);
        AddNewOptions(result, recipe.Keep, player.PersistenceContext);
        AddLuck(result, recipe.LuckPercent, player.PersistenceContext);
        result.Durability = result.GetMaximumDurabilityOfOnePiece();

        await player.TemporaryStorage!.RemoveItemAsync(main).ConfigureAwait(false);
        await player.PersistenceContext.DeleteAsync(main).ConfigureAwait(false);
        await player.TemporaryStorage.AddItemAsync(result).ConfigureAwait(false);
        return [result];
    }

    private static int IndexOf(ItemDefinition definition) => (definition.Group * GroupSize) + definition.Number;

    private static int CountOf(Item item) => item.IsStackable() ? (int)item.Durability : 1;

    private static int OptionValueOf(Item item)
        => (item.ItemOptions.FirstOrDefault(o => o.ItemOption?.OptionType == ItemOptionTypes.Option)?.Level ?? 0) * OptionValuePerLevel;

    private static bool Matches(Item item, MasteryMixMaterial material)
        => item.Definition is { } definition
           && IndexOf(definition) >= material.IndexMin && IndexOf(definition) <= material.IndexMax
           && item.Level >= material.LevelMin && item.Level <= material.LevelMax
           && OptionValueOf(item) >= material.OptionMin && OptionValueOf(item) <= material.OptionMax
           && (material.EmptySocketsMin == 0
               || (item.SocketCount >= material.EmptySocketsMin && item.ItemOptions.All(o => o.ItemOption?.OptionType != ItemOptionTypes.SocketOption)));

    private static int RollSockets(List<int> weights)
    {
        var roll = Rand.NextInt(0, Math.Max(weights.Sum(), 1));
        for (var i = 0; i < weights.Count; i++)
        {
            roll -= weights[i];
            if (roll < 0)
            {
                return i + 1;
            }
        }

        return weights.Count;
    }

    private static void AddLuck(Item result, byte luckPercent, IContext context)
    {
        if (luckPercent > 0
            && result.ItemOptions.All(l => l.ItemOption?.OptionType != ItemOptionTypes.Luck)
            && result.Definition!.PossibleItemOptions.SelectMany(o => o.PossibleOptions).FirstOrDefault(p => p.OptionType == ItemOptionTypes.Luck) is { } luck
            && Rand.NextRandomBool(luckPercent))
        {
            AddLink(result, luck, 0, context);
        }
    }

    private static List<CraftingRequiredItemLink>? Match(MasteryMixRecipe recipe, List<Item> storage)
    {
        var remaining = storage.ToList();
        var links = new List<CraftingRequiredItemLink>(recipe.Materials.Count);
        for (var i = 0; i < recipe.Materials.Count; i++)
        {
            var material = recipe.Materials[i];
            var found = remaining.Where(item => Matches(item, material)).ToList();
            var count = found.Sum(CountOf);
            if (count < material.CountMin || count > material.CountMax)
            {
                return null;
            }

            found.ForEach(item => remaining.Remove(item));
            links.Add(new CraftingRequiredItemLink(found, new MixRequirement(recipe, i == 0)));
        }

        return remaining.Count == 0 ? links : null;
    }

    private static void CopyOptions(Item main, Item result, MasteryMixKeep keep, IContext context)
    {
        var possible = result.Definition!.PossibleItemOptions.SelectMany(o => o.PossibleOptions).ToList();
        foreach (var link in main.ItemOptions.ToList())
        {
            if (link.ItemOption is not { OptionType: { } type } option || !IsKept(type, keep))
            {
                continue;
            }

            if (possible.FirstOrDefault(p => p.OptionType == type && p.Number == option.Number && p.SubOptionType == option.SubOptionType) is { } target)
            {
                var copy = context.CreateNew<ItemOptionLink>();
                copy.ItemOption = target;
                copy.Level = link.Level;
                copy.Index = link.Index;
                result.ItemOptions.Add(copy);
            }
        }
    }

    private static bool IsKept(ItemOptionType type, MasteryMixKeep keep)
        => keep.HasFlag(MasteryMixKeep.CopyAll)
           || (keep.HasFlag(MasteryMixKeep.Excellent) && (type == ItemOptionTypes.Excellent || type == Season21ItemOptionTypes.Mastery))
           || (keep.HasFlag(MasteryMixKeep.Luck) && type == ItemOptionTypes.Luck)
           || (keep.HasFlag(MasteryMixKeep.SetOptions) && (type == Season21ItemOptionTypes.MasteryBonus || type == ItemOptionTypes.AncientBonus));

    private static void AddNewOptions(Item result, MasteryMixKeep keep, IContext context)
    {
        if (keep.HasFlag(MasteryMixKeep.NewSetOption))
        {
            foreach (var definition in result.Definition!.PossibleItemOptions.Where(o => o.AddsRandomly
                         && o.PossibleOptions.Any(p => p.OptionType == Season21ItemOptionTypes.Mastery || p.OptionType == Season21ItemOptionTypes.MasteryBonus)))
            {
                if (result.ItemOptions.Any(l => definition.PossibleOptions.Contains(l.ItemOption!)) || !Rand.NextRandomBool(definition.AddChance))
                {
                    continue;
                }

                var option = definition.PossibleOptions.SelectRandom()!;
                AddLink(result, option, option.LevelDependentOptions.Count > 0 ? option.LevelDependentOptions.Select(l => l.Level).SelectRandom() : 0, context);
            }
        }

        if (keep.HasFlag(MasteryMixKeep.NewOption)
            && result.ItemOptions.All(l => l.ItemOption?.OptionType != ItemOptionTypes.Option)
            && result.Definition!.PossibleItemOptions.SelectMany(o => o.PossibleOptions).FirstOrDefault(p => p.OptionType == ItemOptionTypes.Option) is { } normal)
        {
            AddLink(result, normal, Rand.NextInt(1, MaximumNewOptionLevel + 1), context);
        }
    }

    private static void AddLink(Item item, IncreasableItemOption option, int level, IContext context)
    {
        var link = context.CreateNew<ItemOptionLink>();
        link.ItemOption = option;
        link.Level = level;
        item.ItemOptions.Add(link);
    }

    /// <summary>
    /// A material of a mix, as the base crafting handler sees it: the main material stays when the mix fails and is
    /// replaced by the result when it succeeds; the other materials are used up.
    /// </summary>
    private sealed class MixRequirement : ItemCraftingRequiredItem
    {
        public MixRequirement(MasteryMixRecipe recipe, bool isMain)
        {
            this.Recipe = recipe;
            this.IsMain = isMain;
            this.PossibleItems = new List<ItemDefinition>();
            this.RequiredItemOptions = new List<ItemOptionType>();
            this.SuccessResult = isMain ? MixResult.StaysAsIs : MixResult.Disappear;
            this.FailResult = isMain ? MixResult.StaysAsIs : MixResult.Disappear;
        }

        public MasteryMixRecipe Recipe { get; }

        public bool IsMain { get; }
    }
}
