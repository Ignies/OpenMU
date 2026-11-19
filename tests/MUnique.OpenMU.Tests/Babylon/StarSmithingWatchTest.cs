// <copyright file="StarSmithingWatchTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using System.Reflection;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The smith's state follows the hammer on every character the player enters, not only the first one.
/// </summary>
[TestFixture]
public class StarSmithingWatchTest
{
    /// <summary>
    /// A character entering the world gets a new inventory; equipping the hammer there must still send EE 91.
    /// </summary>
    [Test]
    public async ValueTask WatchesTheInventoryOfEveryCharacterAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        var plugIn = new StarSmithingFeaturePlugIn();
        var before = Handlers(player);
        await plugIn.ObjectAddedToMapAsync(null!, player).ConfigureAwait(false);
        Assert.That(Handlers(player), Is.EqualTo(before + 1), "the smith watches the first character's inventory");

        // Back to the character selection and into the world again: a new inventory for the same player.
        var storages = typeof(Player).GetField("_storages", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(player)!;
        storages.GetType().GetMethod("CreateForCharacter")!.Invoke(storages, [player.SelectedCharacter]);
        before = Handlers(player);
        await plugIn.ObjectAddedToMapAsync(null!, player).ConfigureAwait(false);
        Assert.That(Handlers(player), Is.EqualTo(before + 1), "the new inventory is watched too");
    }

    private static int Handlers(Player player)
    {
        var field = typeof(InventoryStorage).GetField(nameof(InventoryStorage.EquippedItemsChanged), BindingFlags.Instance | BindingFlags.NonPublic);
        return (field?.GetValue(player.Inventory) as Delegate)?.GetInvocationList().Length ?? 0;
    }
}
