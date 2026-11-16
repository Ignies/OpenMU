// <copyright file="ItemCustomization.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.DataModel.Entities;

/// <summary>
/// The customisation of one item: its look, name, lore and rarity (GameLogic/ItemCustomization/Readme.md).
/// </summary>
/// <remarks>
/// It's kept in its own table, keyed by the item's persistent id, so an item keeps it wherever it moves
/// (inventory, vault, trade, shop) and loses it with the item.
/// </remarks>
[AggregateRoot]
public class ItemCustomization
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the persistent identifier of the item.
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Gets or sets the custom name, or null to show the item's own name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the lore, at most three lines, or null.
    /// </summary>
    public string? Lore { get; set; }

    /// <summary>
    /// Gets or sets the rarity, from 0 (Common) to 5 (Mythic).
    /// </summary>
    public byte Rarity { get; set; }

    /// <summary>
    /// Gets or sets the recolour look as JSON (the site's item look), or null for the item's own colours.
    /// </summary>
    public string? Look { get; set; }

    /// <summary>
    /// Gets or sets the version, which goes up on every change; clients keep the lore by it.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Gets or sets who made the customisation (an account, a delivery, a smith).
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the time of the last change.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
