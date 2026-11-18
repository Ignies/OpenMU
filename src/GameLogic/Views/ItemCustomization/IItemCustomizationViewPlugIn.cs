// <copyright file="IItemCustomizationViewPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.ItemCustomization;

using MUnique.OpenMU.GameLogic.ItemCustomization;

/// <summary>
/// The view of the item customisation channel (EE 33-36, GameLogic/ItemCustomization/Readme.md).
/// </summary>
public interface IItemCustomizationViewPlugIn : IViewPlugIn
{
    /// <summary>
    /// Sends the customised items of the asked places.
    /// </summary>
    /// <param name="entries">The customised items.</param>
    ValueTask SendCustomizationsAsync(IReadOnlyList<ItemCustomizationEntryData> entries);

    /// <summary>
    /// Sends the lore of a customised item.
    /// </summary>
    /// <param name="entry">The item.</param>
    ValueTask SendDetailAsync(ItemCustomizationEntryData entry);

    /// <summary>
    /// Pushes the places whose customisations changed, unasked (EE 39).
    /// </summary>
    /// <param name="entries">The customised items of those places.</param>
    ValueTask SendChangedAsync(IReadOnlyList<ItemCustomizationEntryData> entries);

    /// <summary>
    /// Answers a report.
    /// </summary>
    /// <param name="result">The result.</param>
    ValueTask SendReportResultAsync(ItemCustomizationReportResult result);
}
