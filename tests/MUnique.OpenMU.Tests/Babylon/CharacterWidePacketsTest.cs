// <copyright file="CharacterWidePacketsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.Character;
using MUnique.OpenMU.GameServer;
using MUnique.OpenMU.GameServer.RemoteView;
using MUnique.OpenMU.GameServer.RemoteView.Character;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.Packets.ServerToClient;
using MUnique.OpenMU.Persistence;
using MUnique.OpenMU.PlugIns;
using BasicAccount = MUnique.OpenMU.Persistence.BasicModel.Account;
using BasicCharacter = MUnique.OpenMU.Persistence.BasicModel.Character;
using BasicCharacterClass = MUnique.OpenMU.Persistence.BasicModel.CharacterClass;

/// <summary>
/// Tests the character packets of the Babylon protocol: the class codes after the character list and the created
/// character with its code.
/// </summary>
[TestFixture]
public class CharacterWidePacketsTest
{
    /// <summary>
    /// The Babylon client gets every character, also of the new classes, and their class codes.
    /// </summary>
    [Test]
    public async ValueTask BabylonListHasEveryCharacterAndItsCodeAsync()
    {
        var (player, output) = CreatePlayerWithGuildServer();
        player.Account = CreateAccount(("Lancer", 0, 128 + 56), ("Wizard", 1, 128 + 3), ("Knight", 2, 4));

        await new ShowCharacterListBabylonPlugIn(player).ShowCharacterListAsync().ConfigureAwait(false);

        var data = output.ToArray();
        var listLength = data[1];
        var list = new CharacterList(data.AsMemory(0, listLength));
        Assert.That(list.CharacterCount, Is.EqualTo(3));
        var codes = new CharacterClassCodes(data.AsMemory(listLength, data[listLength + 1]));
        Assert.That(codes.Header.Code, Is.EqualTo(0xEE));
        Assert.That(codes.CharacterCount, Is.EqualTo(3));
        var pairs = Enumerable.Range(0, codes.CharacterCount).Select(i => (codes[i].SlotIndex, codes[i].ClassCode)).ToList();
        Assert.That(pairs, Is.EqualTo(new[] { ((byte)0, (byte)56), ((byte)1, (byte)3), ((byte)2, (byte)8) }));
    }

    /// <summary>
    /// The Babylon client takes the Babylon character list and created character view plugins; Season 6 clients don't.
    /// </summary>
    [Test]
    public void BabylonClientGetsTheWideViewPlugIns()
    {
        var (player, _) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        var manager = player.GameContext.PlugInManager;
        manager.RegisterPlugIn<IViewPlugIn, ShowCharacterListPlugIn>();
        manager.RegisterPlugIn<IViewPlugIn, ShowCharacterListBabylonPlugIn>();
        manager.RegisterPlugIn<IViewPlugIn, ShowCreatedCharacterPlugIn>();
        manager.RegisterPlugIn<IViewPlugIn, ShowCreatedCharacterWidePlugIn>();

        player.ClientVersion = BabylonClient.Version;
        Assert.That(player.ViewPlugIns.GetPlugIn<IShowCharacterListPlugIn>(), Is.InstanceOf<ShowCharacterListBabylonPlugIn>());
        Assert.That(player.ViewPlugIns.GetPlugIn<IShowCreatedCharacterPlugIn>(), Is.InstanceOf<ShowCreatedCharacterWidePlugIn>());

        player.ClientVersion = new(6, 3, Network.PlugIns.ClientLanguage.English);
        Assert.That(player.ViewPlugIns.GetPlugIn<IShowCharacterListPlugIn>()?.GetType(), Is.EqualTo(typeof(ShowCharacterListPlugIn)));
        Assert.That(player.ViewPlugIns.GetPlugIn<IShowCreatedCharacterPlugIn>()?.GetType(), Is.EqualTo(typeof(ShowCreatedCharacterPlugIn)));
    }

    /// <summary>
    /// A created character is sent with its name, slot and class code.
    /// </summary>
    [Test]
    public async ValueTask CreatedCharacterHasItsCodeAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);
        var character = new BasicCharacter { Name = "Crusader", CharacterSlot = 4, CharacterClass = new BasicCharacterClass { Number = 128 + 120 } };

        await new ShowCreatedCharacterWidePlugIn(player).ShowCreatedCharacterAsync(character).ConfigureAwait(false);

        var packet = new CharacterCreatedWide(output.ToArray());
        Assert.That(packet.Name, Is.EqualTo("Crusader"));
        Assert.That(packet.SlotIndex, Is.EqualTo(4));
        Assert.That(packet.ClassCode, Is.EqualTo(120));
    }

    private static (RemotePlayer Player, System.IO.MemoryStream Output) CreatePlayerWithGuildServer()
    {
        var guildServer = new Mock<IGuildServer>();
        guildServer.Setup(g => g.GetGuildPositionAsync(It.IsAny<Guid>())).Returns(ValueTask.FromResult(GuildPosition.Undefined));
        var manager = new PlugInManager(null, new NullLoggerFactory(), null, null);
        manager.DiscoverAndRegisterPlugInsOf<IViewPlugIn>();
        var gameContext = new Mock<IGameServerContext>();
        gameContext.Setup(c => c.PersistenceContextProvider).Returns(new Mock<IPersistenceContextProvider>().Object);
        gameContext.Setup(c => c.Configuration).Returns(new GameConfiguration());
        gameContext.Setup(c => c.PlugInManager).Returns(manager);
        gameContext.Setup(c => c.FeaturePlugIns).Returns(new FeaturePlugInContainer(manager));
        gameContext.Setup(c => c.LoggerFactory).Returns(new NullLoggerFactory());
        gameContext.Setup(c => c.GuildServer).Returns(guildServer.Object);
        return CastleSiegeRemoteViewTestHelper.CreatePlayer(gameContext.Object, BabylonClient.Version);
    }

    private static BasicAccount CreateAccount(params (string Name, byte Slot, byte ClassNumber)[] characters)
    {
        var account = new BasicAccount { LoginName = "test" };
        foreach (var (name, slot, number) in characters)
        {
            account.Characters.Add(new BasicCharacter { Id = Guid.NewGuid(), Name = name, CharacterSlot = slot, CharacterClass = new BasicCharacterClass { Number = number } });
        }

        return account;
    }
}
