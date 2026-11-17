// <copyright file="ItemCustomizationCacheTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.GameLogic.ItemCustomization;

/// <summary>
/// The record cache: a write from outside (the Hidden plugin's site delivery, a moderation reset) drops the item at once,
/// so the next request reads the new record instead of waiting out the cache time.
/// </summary>
[TestFixture]
public class ItemCustomizationCacheTest
{
    /// <summary>
    /// A cached record is served until it is forgotten.
    /// </summary>
    [Test]
    public void ForgetDropsTheCachedRecord()
    {
        var id = Guid.NewGuid();
        var record = new DataModel.Entities.ItemCustomization { ItemId = id, Name = "Old", Version = 1 };
        ItemCustomizationCache.Store(id, record, long.MaxValue);
        Assert.That(ItemCustomizationCache.TryGet(id, 0, out var cached), Is.True);
        Assert.That(cached, Is.SameAs(record));

        ItemCustomizationCache.Forget(id);

        Assert.That(ItemCustomizationCache.TryGet(id, 0, out _), Is.False);
    }

    /// <summary>
    /// A remembered miss (no record yet) is forgotten too, so a record written right after shows on the next request.
    /// </summary>
    [Test]
    public void ForgetDropsARememberedMiss()
    {
        var id = Guid.NewGuid();
        ItemCustomizationCache.Store(id, null, long.MaxValue);
        Assert.That(ItemCustomizationCache.TryGet(id, 0, out var cached), Is.True);
        Assert.That(cached, Is.Null);

        ItemCustomizationCache.Forget(id);

        Assert.That(ItemCustomizationCache.TryGet(id, 0, out _), Is.False);
    }

    /// <summary>
    /// The glue reaches Forget by name, without referencing the fork: a public static method taking one Guid.
    /// </summary>
    [Test]
    public void ForgetIsReachableByName()
    {
        var type = Type.GetType("MUnique.OpenMU.GameLogic.ItemCustomization.ItemCustomizationCache, MUnique.OpenMU.GameLogic");
        var method = type?.GetMethod("Forget", [typeof(Guid)]);
        Assert.That(method is { IsPublic: true, IsStatic: true }, Is.True);
    }
}
