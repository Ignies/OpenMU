// <copyright file="ShowCharacterListPlugInTests.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameServer.RemoteView;
using MUnique.OpenMU.GameServer.RemoteView.Character;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.Network.Packets.ServerToClient;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.Persistence;
using MUnique.OpenMU.PlugIns;
using BasicAccount = MUnique.OpenMU.Persistence.BasicModel.Account;
using BasicCharacter = MUnique.OpenMU.Persistence.BasicModel.Character;
using BasicCharacterClass = MUnique.OpenMU.Persistence.BasicModel.CharacterClass;

/// <summary>
/// Tests the guild positions in the character lists of <see cref="ShowCharacterListPlugIn"/> and
/// <see cref="ShowCharacterListExtendedPlugIn"/>.
/// </summary>
[TestFixture]
public class ShowCharacterListPlugInTests
{
    /// <summary>
    /// The characters in the order the account holds them, which is not their slot order, as an account
    /// loaded from the database can have them.
    /// </summary>
    private static readonly (string Name, byte Slot, GuildPosition Position, GuildMemberRole ExpectedRole)[] StoredCharacters =
    [
        ("SlotThree", 3, GuildPosition.Undefined, GuildMemberRole.Undefined),
        ("SlotOne", 1, GuildPosition.BattleMaster, GuildMemberRole.BattleMaster),
        ("SlotFour", 4, GuildPosition.Undefined, GuildMemberRole.Undefined),
        ("SlotTwo", 2, GuildPosition.NormalMember, GuildMemberRole.NormalMember),
        ("SlotZero", 0, GuildPosition.GuildMaster, GuildMemberRole.GuildMaster),
    ];

    /// <summary>
    /// Each character in the character list has its own guild position.
    /// </summary>
    [Test]
    public async ValueTask CharacterListHasEachCharactersGuildPositionAsync()
    {
        var (player, output) = CreatePlayerWithCharacters(new ClientVersion(6, 3, ClientLanguage.Invariant));

        await new ShowCharacterListPlugIn(player).ShowCharacterListAsync().ConfigureAwait(false);

        var packet = new CharacterList(output.ToArray());
        Assert.That(packet.CharacterCount, Is.EqualTo(StoredCharacters.Length));
        for (var i = 0; i < StoredCharacters.Length; i++)
        {
            var character = packet[i];
            var expected = StoredCharacters.Single(c => c.Name == character.Name);
            Assert.That(character.SlotIndex, Is.EqualTo(i), "the list is in slot order");
            Assert.That(character.GuildPosition, Is.EqualTo(expected.ExpectedRole), character.Name);
        }
    }

    /// <summary>
    /// Each character in the extended character list has its own guild position.
    /// </summary>
    [Test]
    public async ValueTask ExtendedCharacterListHasEachCharactersGuildPositionAsync()
    {
        var (player, output) = CreatePlayerWithCharacters(new ClientVersion(106, 3, ClientLanguage.Invariant));

        await new ShowCharacterListExtendedPlugIn(player).ShowCharacterListAsync().ConfigureAwait(false);

        var packet = new CharacterListExtended(output.ToArray());
        Assert.That(packet.CharacterCount, Is.EqualTo(StoredCharacters.Length));
        for (var i = 0; i < StoredCharacters.Length; i++)
        {
            var character = packet[i];
            var expected = StoredCharacters.Single(c => c.Name == character.Name);
            Assert.That(character.SlotIndex, Is.EqualTo(i), "the list is in slot order");
            Assert.That(character.GuildPosition, Is.EqualTo(expected.ExpectedRole), character.Name);
        }
    }

    /// <summary>
    /// A Season 6 client doesn't get the characters of the classes after Season 6, and sees a fourth or fifth evolution
    /// of a Season 6 class as its third.
    /// </summary>
    [Test]
    public async ValueTask Season6CharacterListShowsOnlySeason6ClassesAsync()
    {
        var classes = new Dictionary<string, byte> { ["SlotThree"] = 184, ["SlotOne"] = 131 }; // Grow Lancer, Soul Wizard
        var (player, output) = CreatePlayerWithCharacters(new ClientVersion(6, 3, ClientLanguage.Invariant), classes);

        await new ShowCharacterListPlugIn(player).ShowCharacterListAsync().ConfigureAwait(false);

        var packet = new CharacterList(output.ToArray());
        Assert.That(packet.CharacterCount, Is.EqualTo(StoredCharacters.Length - 1));
        var names = Enumerable.Range(0, packet.CharacterCount).Select(i => packet[i].Name).ToList();
        Assert.That(names, Does.Not.Contain("SlotThree"));
        var soulWizard = Enumerable.Range(0, packet.CharacterCount).Select(i => packet[i]).Single(c => c.Name == "SlotOne");
        Assert.That(soulWizard.Appearance[0] >> 3, Is.EqualTo(3), "shown as grand master");
    }

    private static (RemotePlayer Player, System.IO.MemoryStream Output) CreatePlayerWithCharacters(ClientVersion clientVersion, IReadOnlyDictionary<string, byte>? classNumbers = null)
    {
        var account = new BasicAccount { LoginName = "test" };
        var positions = new Dictionary<Guid, GuildPosition>();
        foreach (var (name, slot, position, _) in StoredCharacters)
        {
            var character = new BasicCharacter
            {
                Id = Guid.NewGuid(),
                Name = name,
                CharacterSlot = slot,
                CharacterClass = new BasicCharacterClass { Number = classNumbers?.GetValueOrDefault(name) ?? 0 },
            };
            account.Characters.Add(character);
            positions[character.Id] = position;
        }

        var guildServer = new Mock<IGuildServer>();
        guildServer.Setup(g => g.GetGuildPositionAsync(It.IsAny<Guid>()))
            .Returns<Guid>(id => ValueTask.FromResult(positions[id]));

        var manager = new PlugInManager(null, new NullLoggerFactory(), null, null);
        manager.DiscoverAndRegisterPlugInsOf<IViewPlugIn>();
        var gameContext = new Mock<IGameServerContext>();
        gameContext.Setup(c => c.PersistenceContextProvider).Returns(new Mock<IPersistenceContextProvider>().Object);
        gameContext.Setup(c => c.Configuration).Returns(new GameConfiguration());
        gameContext.Setup(c => c.PlugInManager).Returns(manager);
        gameContext.Setup(c => c.FeaturePlugIns).Returns(new FeaturePlugInContainer(manager));
        gameContext.Setup(c => c.LoggerFactory).Returns(new NullLoggerFactory());
        gameContext.Setup(c => c.GuildServer).Returns(guildServer.Object);

        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(gameContext.Object, clientVersion);
        player.Account = account;
        return (player, output);
    }
}
