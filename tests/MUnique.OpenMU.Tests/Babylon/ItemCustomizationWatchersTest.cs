// <copyright file="ItemCustomizationWatchersTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.GameLogic.ItemCustomization;

/// <summary>
/// Who was last told about an item (the places they asked), so a change of its record (a moderator's reset, a new
/// delivery look) reaches them at once (EE 39) instead of on their next ask.
/// </summary>
[TestFixture]
public class ItemCustomizationWatchersTest
{
    private static readonly Guid Item = Guid.NewGuid();

    /// <summary>
    /// Every place an item was answered for is given back once when it changes, each place once.
    /// </summary>
    [Test]
    public void APlaceIsGivenBackOnce()
    {
        var watchers = new ItemCustomizationWatchers<object>();
        var owner = new object();
        var viewer = new object();
        watchers.Add(Item, owner, ItemCustomizationStorage.Inventory, 0x200);
        watchers.Add(Item, owner, ItemCustomizationStorage.Inventory, 0x200);
        watchers.Add(Item, viewer, ItemCustomizationStorage.Worn, 0x5FD);

        var places = watchers.Take(Item);

        Assert.That(places.Select(p => (p.Watcher == owner, p.Storage, p.ObjectId)), Is.EquivalentTo(new[]
        {
            (true, ItemCustomizationStorage.Inventory, (ushort)0x200),
            (false, ItemCustomizationStorage.Worn, (ushort)0x5FD),
        }));
        Assert.That(watchers.Take(Item), Is.Empty);
    }

    /// <summary>
    /// A watcher that is gone (a player who logged off) is not given back.
    /// </summary>
    [Test]
    public void AGoneWatcherIsDropped()
    {
        var watchers = new ItemCustomizationWatchers<object>();
        AddShortLived(watchers);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        Assert.That(watchers.Take(Item), Is.Empty);
    }

    /// <summary>
    /// Forgetting a record tells whoever listens (the game's channel) which item changed.
    /// </summary>
    [Test]
    public void ForgetTellsWhichItemChanged()
    {
        var heard = new List<Guid>();
        var forgotten = typeof(ItemCustomizationCache).GetEvent("Forgotten");
        Assert.That(forgotten, Is.Not.Null);
        Action<Guid> listen = heard.Add;
        forgotten!.AddEventHandler(null, listen);
        try
        {
            ItemCustomizationCache.Forget(Item);
        }
        finally
        {
            forgotten.RemoveEventHandler(null, listen);
        }

        Assert.That(heard, Is.EqualTo(new[] { Item }));
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static void AddShortLived(ItemCustomizationWatchers<object> watchers) =>
        watchers.Add(Item, new object(), ItemCustomizationStorage.Inventory, 1);
}
