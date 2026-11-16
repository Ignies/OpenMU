// <copyright file="IItemCustomizationViewPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.ItemCustomization;

using MUnique.OpenMU.GameLogic.ItemCustomization;

/// <summary>
/// One customised item of an answer.
/// </summary>
/// <param name="Storage">The storage, as asked.</param>
/// <param name="ObjectId">The object id, as asked.</param>
/// <param name="Slot">The slot of the item.</param>
/// <param name="Customization">The customisation.</param>
public record ItemCustomizationEntryData(ItemCustomizationStorage Storage, ushort ObjectId, byte Slot, DataModel.Entities.ItemCustomization Customization);

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
}
