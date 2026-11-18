// <copyright file="ItemCustomizationReport.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.DataModel.Entities;

/// <summary>
/// A player's report of a customised item's name or lore (GameLogic/TextModeration/Readme.md), with the text as it was
/// reported. The website's Moderation page lists them; an admin resets the text with a reason.
/// </summary>
[AggregateRoot]
public class ItemCustomizationReport
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the persistent identifier of the reported item.
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Gets or sets the name of the reporting character.
    /// </summary>
    public string Reporter { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the item's customisation that was reported.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Gets or sets the name as it was reported.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the lore as it was reported.
    /// </summary>
    public string? Lore { get; set; }

    /// <summary>
    /// Gets or sets when it was reported.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
