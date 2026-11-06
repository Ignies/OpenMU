// <copyright file="FourthWingCrafting.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Craftings;

using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.PlayerActions.Items;
using MUnique.OpenMU.GameLogic.Views.NPC;
using MUnique.OpenMU.GameLogic.WideItems;

/// <summary>
/// The creation of the fourth wings (Season 21): of the result items, the player gets the wings which its class can
/// wear, with grade options rolled by the grade rates of <see cref="Season21ItemsFeaturePlugIn"/>.
/// </summary>
public class FourthWingCrafting : SimpleItemCraftingHandler
{
    private readonly SimpleCraftingSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="FourthWingCrafting"/> class.
    /// </summary>
    /// <param name="settings">The settings.</param>
    public FourthWingCrafting(SimpleCraftingSettings settings)
        : base(settings)
    {
        this._settings = settings;
    }

    /// <inheritdoc />
    public override CraftingResult? TryGetRequiredItems(Player player, out IList<CraftingRequiredItemLink> items, out byte successRate)
    {
        var result = base.TryGetRequiredItems(player, out items, out successRate);
        if (result is not null)
        {
            return result;
        }

        if (this.GetWings(player) is not { } wings)
        {
            return CraftingResult.CharacterClassTooLow;
        }

        return player.CanSee(wings) ? null : CraftingResult.IncorrectMixItems;
    }

    /// <inheritdoc />
    protected override async ValueTask<List<Item>> CreateOrModifyResultItemsAsync(IList<CraftingRequiredItemLink> requiredItems, Player player, byte socketSlot, byte successRate)
    {
        var wings = player.PersistenceContext.CreateNew<Item>();
        wings.Definition = this.GetWings(player) ?? throw new InvalidOperationException("The class of the player wears none of the result wings.");
        WingGrades.Roll(wings, Season21ItemsFeaturePlugIn.GetConfiguration(player.GameContext)?.GradeRates, Rand.GetRandomizer(), () => player.PersistenceContext.CreateNew<ItemOptionLink>());
        wings.Durability = wings.GetMaximumDurabilityOfOnePiece();
        await player.TemporaryStorage!.AddItemAsync(wings).ConfigureAwait(false);
        return [wings];
    }

    private ItemDefinition? GetWings(Player player)
        => player.SelectedCharacter?.CharacterClass is { } characterClass
            ? this._settings.ResultItems.Select(r => r.ItemDefinition).FirstOrDefault(d => d?.QualifiedCharacters.Contains(characterClass) is true)
            : null;
}
