// <copyright file="ItemCustomizationAnswerTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.GameLogic.ItemCustomization;

/// <summary>
/// An answer lists the customised items of the asked places, and notes every item there as told, customised or not: a
/// first record (a smith's recolour of a plain armour) then reaches the players looking at it at once.
/// </summary>
[TestFixture]
public class ItemCustomizationAnswerTest
{
    /// <summary>
    /// A plain item is not in the answer but is watched.
    /// </summary>
    [Test]
    public void APlainItemIsWatchedToo()
    {
        var named = Guid.NewGuid();
        var plain = Guid.NewGuid();
        var items = new List<(ItemCustomizationStorage Storage, ushort ObjectId, byte Slot, Guid ItemId)>
        {
            (ItemCustomizationStorage.Worn, 0x5FD, 3, plain),
            (ItemCustomizationStorage.Worn, 0x5FD, 7, named),
        };
        var records = new Dictionary<Guid, DataModel.Entities.ItemCustomization> { [named] = new() { ItemId = named, Name = "Wings", Version = 1 } };

        var (answer, watched) = ItemCustomizationAnswer.Build(items, records);

        Assert.That(answer.Select(e => e.Slot), Is.EqualTo(new byte[] { 7 }));
        Assert.That(watched.Select(w => w.ItemId), Is.EquivalentTo(new[] { plain, named }));
    }
}
