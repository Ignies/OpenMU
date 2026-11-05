// <copyright file="ItemChatCommandPlugInTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using Moq;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic.PlugIns.ChatCommands;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.Interfaces;

/// <summary>
/// Tests the item chat command of game masters.
/// </summary>
[TestFixture]
public class ItemChatCommandPlugInTest
{
    /// <summary>
    /// An inactive item isn't available in the game, so a game master can't make it either: e.g. an item which the
    /// item format of the client can't carry yet.
    /// </summary>
    [Test]
    public async Task InactiveItemIsNotCreatedAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        player.SelectedCharacter!.CharacterStatus = CharacterStatus.GameMaster;
        player.CurrentMap = await player.GameContext.GetMapAsync(0).ConfigureAwait(false);
        player.GameContext.Configuration.Items.Add(new ItemDefinition { Group = 16, Number = 1, Width = 1, Height = 1, Durability = 1, IsActive = false, Name = "Muun" });

        await new ItemChatCommandPlugIn().HandleCommandAsync(player, "/item 16 1").ConfigureAwait(false);

        Mock.Get(player.ViewPlugIns.GetPlugIn<IShowMessagePlugIn>()!).Verify(v => v!.ShowMessageAsync("16 1 does not exist.", MessageType.BlueNormal), Times.Once);
    }
}
