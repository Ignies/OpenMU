// <copyright file="SeedSphereEnhanceCrafting.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Craftings;

using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.PlayerActions.Items;
using MUnique.OpenMU.GameLogic.Views.NPC;
using MUnique.OpenMU.GameLogic.WideItems;

/// <summary>
/// Crafting which enhances a seed sphere to the next sphere level (Season 21, up to level 10), keeping its element
/// and its option. The required seed spheres, runes and money come from the crafting settings.
/// </summary>
public class SeedSphereEnhanceCrafting : SimpleItemCraftingHandler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SeedSphereEnhanceCrafting"/> class.
    /// </summary>
    /// <param name="settings">The settings.</param>
    public SeedSphereEnhanceCrafting(SimpleCraftingSettings settings)
        : base(settings)
    {
    }

    /// <summary>
    /// Gets the reference to the seed sphere which must be specified in the <see cref="ItemCraftingRequiredItem.Reference"/>.
    /// </summary>
    public static byte SeedSphereReference { get; } = 0x77;

    /// <inheritdoc />
    public override CraftingResult? TryGetRequiredItems(Player player, out IList<CraftingRequiredItemLink> items, out byte successRate)
    {
        var result = base.TryGetRequiredItems(player, out items, out successRate);
        if (result != default)
        {
            return result;
        }

        var seedSphere = items.Single(i => i.ItemRequirement.Reference == SeedSphereReference).Items.Single();
        if (GetNextDefinition(player, seedSphere) is not { } next || !player.CanSee(next))
        {
            return CraftingResult.IncorrectMixItems;
        }

        return null;
    }

    /// <inheritdoc />
    protected override async ValueTask<List<Item>> CreateOrModifyResultItemsAsync(IList<CraftingRequiredItemLink> requiredItems, Player player, byte socketSlot, byte successRate)
    {
        var seedSphere = requiredItems.Single(i => i.ItemRequirement.Reference == SeedSphereReference).Items.Single();
        var result = player.PersistenceContext.CreateNew<Item>();
        result.Definition = GetNextDefinition(player, seedSphere) ?? throw new InvalidOperationException($"The seed sphere {seedSphere} has no next level.");
        result.Level = seedSphere.Level; // The level defines the kind of option
        result.Durability = 1;
        await player.TemporaryStorage!.AddItemAsync(result).ConfigureAwait(false);
        return [result];
    }

    private static ItemDefinition? GetNextDefinition(Player player, Item seedSphere)
    {
        seedSphere.ThrowNotInitializedProperty(seedSphere.Definition is null, nameof(seedSphere.Definition));
        var level = MountSeedSphereCrafting.GetSphereLevel(seedSphere.Definition);
        var element = seedSphere.Definition.Number >= MountSeedSphereCrafting.WideSeedSphereNumberStart
            ? (seedSphere.Definition.Number - MountSeedSphereCrafting.WideSeedSphereNumberStart) % SeedSphereCrafting.SeedTypes
            : (seedSphere.Definition.Number - SeedSphereCrafting.SeedSphereNumberStart) % SeedSphereCrafting.SeedTypes;
        var number = MountSeedSphereCrafting.GetSeedSphereNumber(level + 1, element);
        return player.GameContext.Configuration.Items.FirstOrDefault(i => i.Group == seedSphere.Definition.Group && i.Number == number);
    }
}
