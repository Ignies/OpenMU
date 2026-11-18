// <copyright file="ItemCustomizationAnswer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ItemCustomization;

using MUnique.OpenMU.GameLogic.Views.ItemCustomization;

/// <summary>
/// An answer to the asked places (Readme.md "Changes"): the customised items, and every item there as told, customised
/// or not, so a first record (a smith's recolour of a plain armour) reaches the players looking at it at once.
/// </summary>
public static class ItemCustomizationAnswer
{
    /// <summary>
    /// Builds an answer.
    /// </summary>
    /// <param name="items">The items of the asked places.</param>
    /// <param name="records">The records of those items that have one.</param>
    /// <returns>The entries to send and the places to watch.</returns>
    public static (List<ItemCustomizationEntryData> Answer, List<(Guid ItemId, ItemCustomizationStorage Storage, ushort ObjectId)> Watched) Build(
        IReadOnlyList<(ItemCustomizationStorage Storage, ushort ObjectId, byte Slot, Guid ItemId)> items,
        IReadOnlyDictionary<Guid, DataModel.Entities.ItemCustomization> records)
    {
        var answer = new List<ItemCustomizationEntryData>();
        var watched = new List<(Guid, ItemCustomizationStorage, ushort)>(items.Count);
        foreach (var item in items)
        {
            if (records.TryGetValue(item.ItemId, out var record))
            {
                answer.Add(new ItemCustomizationEntryData(item.Storage, item.ObjectId, item.Slot, record));
            }

            watched.Add((item.ItemId, item.Storage, item.ObjectId));
        }

        return (answer, watched);
    }
}
