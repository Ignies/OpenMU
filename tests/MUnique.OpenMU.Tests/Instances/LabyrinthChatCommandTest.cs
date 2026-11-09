// <copyright file="LabyrinthChatCommandTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Instances;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.GameLogic.PlugIns.ChatCommands;
using MUnique.OpenMU.GameLogic.Ruud;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.Updates;

/// <summary>
/// Tests <c>/labyrinth</c>, the way of a game master through the Labyrinth of Dimensions, in a world of its own.
/// </summary>
[TestFixture]
public class LabyrinthChatCommandTest
{
    /// <summary>
    /// A game master enters the labyrinth at a floor of his choice without a ticket (<c>/labyrinth 2</c>), and inside
    /// clears the floor he is on, which goes on with the next floor without the pause (<c>/labyrinth</c>).
    /// </summary>
    [Test]
    public async ValueTask GameMasterEntersAndAdvancesTheLabyrinthAsync()
    {
        var world = await Season21EventWorld.CreateAsync(new AddSeason21InstancesUpdatePlugIn(), new AddSeason21InstanceZoneUpdatePlugIn()).ConfigureAwait(false);
        var events = new Season21EventsFeaturePlugIn { Configuration = world.GetStoredConfiguration<Season21EventsConfiguration>(typeof(Season21EventsFeaturePlugIn).GUID) };
        world.AddFeaturePlugIn(events);
        events.Configuration!.Labyrinth = new RoundsEventDefinition
        {
            SpawnPoints = events.Configuration.Labyrinth!.SpawnPoints,
            RoundDuration = TimeSpan.FromSeconds(30),
            PauseAfterRound = TimeSpan.FromMinutes(1),
            EndsWhenARoundFails = true,
            PartyIsATeam = false,
            Rounds =
            [
                new EventRound { MonsterNumber = 754, Count = 1, ClearPoints = 50, ClearRuud = 30 },
                new EventRound { MonsterNumber = 754, Count = 1, ClearPoints = 100, ClearRuud = 70 },
                new EventRound { MonsterNumber = 754, Count = 1, ClearPoints = 150, ClearRuud = 130 },
            ],
        };
        var gameMaster = await world.CreatePlayerAsync(CharacterClassNumber.BladeMaster, 400, 100).ConfigureAwait(false);
        gameMaster.SelectedCharacter!.CharacterStatus = CharacterStatus.GameMaster;
        var command = new LabyrinthChatCommandPlugIn();

        await command.HandleCommandAsync(gameMaster, "/labyrinth 2").ConfigureAwait(false);
        await Season21EventWorld.CompleteMapChangeAsync(gameMaster).ConfigureAwait(false);
        var run = gameMaster.CurrentMiniGame as Season21RoundsContext;
        Assert.That(run?.Definition.Type, Is.EqualTo(MiniGameType.LabyrinthOfDimensions));
        await Season21EventWorld.SkipUntilPlayingAsync(run!).ConfigureAwait(false);
        await Season21EventWorld.WaitUntilAsync(() => run!.Round == 2).ConfigureAwait(false);

        await command.HandleCommandAsync(gameMaster, "/labyrinth").ConfigureAwait(false);
        await Season21EventWorld.WaitUntilAsync(() => run!.Round == 3).ConfigureAwait(false);
        Assert.That(run!.ClearedRounds, Is.EqualTo(1));
        Assert.That(gameMaster.GetRuud(), Is.EqualTo(70));
    }
}
