// <copyright file="ElementalUpgradeCrafting.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Craftings;

using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.PlayerActions.Items;

/// <summary>
/// The upgrade of a pentagram or an errtel to its mastery item (e.g. Antonia's Sword to Antonia's Sword [I], Errtel
/// of Anger to Errtel of Anger [I]; S21Emu ElementalUpgrade.xml, PentagramUpgradeMix and ErrtelUpgradeMix): the item
/// becomes the result item and keeps its element, sockets, errtels and rank options.
/// </summary>
public class ElementalUpgradeCrafting : SimpleItemCraftingHandler
{
    private readonly SimpleCraftingSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementalUpgradeCrafting"/> class.
    /// </summary>
    /// <param name="settings">The settings.</param>
    public ElementalUpgradeCrafting(SimpleCraftingSettings settings)
        : base(settings)
    {
        this._settings = settings;
    }

    /// <summary>
    /// Gets the reference of the upgraded item.
    /// </summary>
    public static byte MainItemReference { get; } = 0x78;

    /// <inheritdoc />
    protected override ValueTask<List<Item>> CreateOrModifyResultItemsAsync(IList<CraftingRequiredItemLink> requiredItems, Player player, byte socketSlot, byte successRate)
    {
        var item = requiredItems.Single(i => i.ItemRequirement.Reference == MainItemReference).Items.Single();
        var result = this._settings.ResultItems.FirstOrDefault()?.ItemDefinition
                     ?? throw new InvalidOperationException($"The crafting {this.GetType().Name} has no result item.");
        var options = result.PossibleItemOptions.SelectMany(o => o.PossibleOptions).ToList();
        foreach (var link in item.ItemOptions.Where(o => o.ItemOption?.OptionType == Season21ItemOptionTypes.ErrtelRank))
        {
            // The rank options of an errtel are the result's own options of the same number.
            if (item.Definition!.PossibleItemOptions.Any(d => d.PossibleOptions.Contains(link.ItemOption!))
                && options.FirstOrDefault(o => o.OptionType == link.ItemOption!.OptionType && o.Number == link.ItemOption.Number) is { } option)
            {
                link.ItemOption = option;
            }
        }

        item.Definition = result;
        return ValueTask.FromResult(new List<Item> { item });
    }
}
