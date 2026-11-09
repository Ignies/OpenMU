// <copyright file="GameMasterCommandsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using Moq;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.GremoryCase;
using MUnique.OpenMU.GameLogic.PlugIns.ChatCommands;
using MUnique.OpenMU.GameLogic.Resets;
using MUnique.OpenMU.GameLogic.Ruud;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.Character;
using MUnique.OpenMU.Interfaces;

/// <summary>
/// Game master commands which a game master needs to try out the Season 21 systems on a world.
/// </summary>
[TestFixture]
public class GameMasterCommandsTest
{
    /// <summary>
    /// A world without master levels (the stars mode of the progression sets the maximum to 0) says so and names the
    /// command which sets the stars, instead of "between 1 and 0".
    /// </summary>
    [Test]
    public async ValueTask SetMasterLevelNamesTheStarsWithoutMasterLevelsAsync()
    {
        var player = await CreateGameMasterAsync().ConfigureAwait(false);
        player.GameContext.Configuration.MaximumMasterLevel = 0;

        await new SetMasterLevelChatCommandPlugIn().HandleCommandAsync(player, "/setmasterlevel 10").ConfigureAwait(false);

        Mock.Get(player.ViewPlugIns.GetPlugIn<IShowMessagePlugIn>()!).Verify(v => v.ShowMessageAsync(It.Is<string>(m => m.Contains("/setresets")), MessageType.BlueNormal), Times.Once);
        Assert.That(player.Attributes![Stats.MasterLevel], Is.Zero);
    }

    /// <summary>
    /// A level or star change by a game master changes the points of the skill enhancement tree, so the tree goes out
    /// again.
    /// </summary>
    [Test]
    public async ValueTask LevelAndStarChangesShowTheTreeAsync()
    {
        var player = await CreateGameMasterAsync().ConfigureAwait(false);
        player.GameContext.Configuration.MaximumLevel = 400;
        player.GameContext.FeaturePlugIns.AddPlugIn(new ResetFeaturePlugIn { Configuration = new ResetConfiguration() }, true);
        var tree = Mock.Get(player.ViewPlugIns.GetPlugIn<IEnhanceTreeViewPlugIn>()!);

        await new SetLevelChatCommandPlugIn().HandleCommandAsync(player, "/setlevel 300").ConfigureAwait(false);
        tree.Verify(v => v.ShowEnhanceTreeAsync(), Times.Once);

        await new SetResetsChatCommandPlugIn().HandleCommandAsync(player, "/setresets 2").ConfigureAwait(false);
        tree.Verify(v => v.ShowEnhanceTreeAsync(), Times.Exactly(2));
    }

    /// <summary>
    /// A gift of a game master is made like the item of <c>/item</c>: a pentagram comes with its sockets.
    /// </summary>
    [Test]
    public async ValueTask GiftedPentagramHasItsSocketsAsync()
    {
        var player = await CreateGameMasterAsync().ConfigureAwait(false);
        Mock.Get(player.Account!).SetupProperty(a => a.GremoryCase);
        player.GameContext.FeaturePlugIns.AddPlugIn(new GremoryCaseFeaturePlugIn { Configuration = new GremoryCaseConfiguration() }, true);
        var pentagram = player.PersistenceContext.CreateNew<ItemDefinition>();
        (pentagram.Group, pentagram.Number, pentagram.Width, pentagram.Height, pentagram.Durability, pentagram.MaximumSockets) = (12, 200, 2, 2, 20, 4);
        player.GameContext.Configuration.Items.Add(pentagram);

        await new GremoryGiftChatCommandPlugIn().HandleCommandAsync(player, $"/gremgif {player.Name} 2 12 200").ConfigureAwait(false);

        Assert.That(player.SelectedCharacter!.GremoryCase!.Items.Single().SocketCount, Is.EqualTo(4));
    }

    /// <summary>
    /// A game master moves a character to a map by its number even when no warp of the move list leads there: the
    /// character lands on a gate of the map, its spawn gate first.
    /// </summary>
    [Test]
    public async ValueTask MoveReachesAMapWithoutWarpAsync()
    {
        var player = await CreateGameMasterAsync().ConfigureAwait(false);
        Mock.Get(player.GameContext.Configuration).Setup(c => c.WarpList).Returns(new List<WarpInfo>());
        var map = player.GameContext.Configuration.Maps.Single();
        var gate = player.PersistenceContext.CreateNew<ExitGate>();
        var spawn = player.PersistenceContext.CreateNew<ExitGate>();
        (gate.Map, gate.X1, gate.X2, gate.Y1, gate.Y2) = (map, 10, 10, 20, 20);
        (spawn.Map, spawn.IsSpawnGate) = (map, true);
        map.ExitGates.Add(gate);
        map.ExitGates.Add(spawn);

        Assert.That(await new GateProbe().GateOfAsync(player, "0").ConfigureAwait(false), Is.SameAs(spawn));
        map.ExitGates.Remove(spawn);
        Assert.That(await new GateProbe().GateOfAsync(player, "0").ConfigureAwait(false), Is.SameAs(gate));
    }

    /// <summary>
    /// A game master reads the Ruud of a character, his own without a name, like <c>/getmoney</c>.
    /// </summary>
    [Test]
    public async ValueTask GetRuudShowsTheRuudAsync()
    {
        var player = await CreateGameMasterAsync().ConfigureAwait(false);
        player.GameContext.FeaturePlugIns.AddPlugIn(new RuudFeaturePlugIn { Configuration = new RuudConfiguration() }, true);
        player.SelectedCharacter!.Ruud = 4321;
        var messages = Mock.Get(player.ViewPlugIns.GetPlugIn<IShowMessagePlugIn>()!);

        await new GetRuudChatCommandPlugIn().HandleCommandAsync(player, "/getruud").ConfigureAwait(false);
        await new GetRuudChatCommandPlugIn().HandleCommandAsync(player, $"/getruud {player.Name}").ConfigureAwait(false);

        messages.Verify(v => v.ShowMessageAsync(It.Is<string>(m => m.Contains("4321") && m.Contains(player.Name)), MessageType.BlueNormal), Times.Exactly(2));
    }

    private static async ValueTask<Player> CreateGameMasterAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        player.SelectedCharacter!.Name = "Gm";
        player.SelectedCharacter.CharacterStatus = CharacterStatus.GameMaster;
        ((GameContext)player.GameContext).PlayersByCharacterName[player.Name] = player;
        return player;
    }

    /// <summary>
    /// Shows the gate the move command picks.
    /// </summary>
    private sealed class GateProbe : MoveChatCommandPlugIn
    {
        /// <summary>
        /// Gets the gate of a map without coordinates.
        /// </summary>
        /// <param name="player">The game master.</param>
        /// <param name="map">The map.</param>
        /// <returns>The gate.</returns>
        public ValueTask<ExitGate?> GateOfAsync(Player player, string map) => this.GetExitGateAsync(player, map, default);
    }
}
