// <copyright file="TormentedSquareTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Instances;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.MiniGames;
using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlayerActions.MiniGames;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using Moq;

/// <summary>
/// Runs the Tormented Square of Season 21 (S21Emu <c>Event/TormentedSquare</c>): rounds of monsters on map 102, points
/// per kill and per cleared round, ranks at the end.
/// </summary>
[TestFixture]
public class TormentedSquareTest
{
    private Season21EventWorld _world = null!;

    private Season21EventsFeaturePlugIn _plugIn = null!;

    /// <summary>
    /// Initializes the world with the event.
    /// </summary>
    [OneTimeSetUp]
    public async ValueTask SetupAsync()
    {
        this._world = await Season21EventWorld.CreateAsync(new AddSeason21TormentedSquareUpdatePlugIn()).ConfigureAwait(false);
        this._plugIn = new Season21EventsFeaturePlugIn
        {
            Configuration = this._world.GetStoredConfiguration<Season21EventsConfiguration>(typeof(Season21EventsFeaturePlugIn).GUID),
        };
        this._world.AddFeaturePlugIn(this._plugIn);
    }

    /// <summary>
    /// The update adds the arena, the event and its fifteen rounds of the data.
    /// </summary>
    [Test]
    public void EventHasTheRoundsOfTheData()
    {
        var definition = this._world.Configuration.MiniGameDefinitions.Single(d => d.Type == MiniGameType.TormentedSquare);
        Assert.That(definition.Entrance?.Map?.Number, Is.EqualTo(102));
        Assert.That(definition.MapCreationPolicy, Is.EqualTo(MiniGameMapCreationPolicy.Shared));
        var rounds = this._plugIn.Configuration!.TormentedSquare!.Rounds;
        Assert.That(rounds, Has.Count.EqualTo(15));
        Assert.That((rounds[0].MonsterNumber, rounds[0].Count, rounds[0].Points, rounds[0].ClearPoints), Is.EqualTo(((short)572, 130, 300, 3000)));
        Assert.That(rounds[4].IsBossRound, Is.True);
        Assert.That(rounds[14].Points, Is.EqualTo(200_000));
        Assert.That(rounds[0].BonusMonsters.Single().MonsterNumber, Is.EqualTo((short)654));
        Assert.That(this._plugIn.Configuration.TormentedSquare.SpawnPoints, Has.Count.EqualTo(130));
        var missing = rounds.Select(r => r.MonsterNumber).Concat([(short)654, (short)655, (short)656]).Where(n => this._world.Configuration.Monsters.All(m => m.Number != n));
        Assert.That(missing, Is.Empty);
    }

    /// <summary>
    /// Two players play two short rounds: the killer gets the points of its kills, everybody the points of the cleared
    /// round, the second round runs out, and the ranks follow the points.
    /// </summary>
    [Test]
    public async ValueTask KillsAndClearedRoundsGivePointsAndRanksAsync()
    {
        var original = this._plugIn.Configuration!.TormentedSquare!;
        this._plugIn.Configuration.TormentedSquare = new RoundsEventDefinition
        {
            SpawnPoints = original.SpawnPoints,
            RoundDuration = TimeSpan.FromSeconds(2),
            PauseAfterRound = TimeSpan.Zero,
            Rounds =
            [
                new EventRound { MonsterNumber = original.Rounds[0].MonsterNumber, Count = 3, Points = 300, ClearPoints = 3000 },
                new EventRound { MonsterNumber = original.Rounds[1].MonsterNumber, Count = 2, Points = 600, ClearPoints = 6000 },
            ],
        };
        try
        {
            var hunter = await this._world.CreatePlayerAsync(CharacterClassNumber.BladeMaster, 400, 100).ConfigureAwait(false);
            var watcher = await this._world.CreatePlayerAsync(CharacterClassNumber.GrandMaster, 400, 100).ConfigureAwait(false);
            foreach (var player in new[] { hunter, watcher })
            {
                await new EnterMiniGameAction().TryEnterMiniGameAsync(player, MiniGameType.TormentedSquare, 1, 0xFF).ConfigureAwait(false);
                await Season21EventWorld.CompleteMapChangeAsync(player).ConfigureAwait(false);
            }

            var game = (Season21RoundsContext)hunter.CurrentMiniGame!;
            Assert.That(watcher.CurrentMiniGame, Is.SameAs(game));
            await Season21EventWorld.SkipUntilPlayingAsync(game).ConfigureAwait(false);
            await Season21EventWorld.WaitUntilAsync(() => game.Round == 1 && Monsters(game).Count == 3).ConfigureAwait(false);

            foreach (var monster in Monsters(game))
            {
                await Season21EventWorld.KillAsync(monster, hunter).ConfigureAwait(false);
            }

            await Season21EventWorld.WaitUntilAsync(() => game.ClearedRounds == 1).ConfigureAwait(false);
            Assert.That(game.GetScore(hunter), Is.EqualTo((3 * 300) + 3000));
            Assert.That(game.GetScore(watcher), Is.EqualTo(3000));

            await Season21EventWorld.WaitUntilAsync(() => game.State is MiniGameState.Ended or MiniGameState.Disposed).ConfigureAwait(false);
            Assert.That(game.ClearedRounds, Is.EqualTo(1), "the second round ran out");
            Mock.Get(hunter.ViewPlugIns.GetPlugIn<ISeason21EventViewPlugIn>()!).Verify(v => v.ShowResultAsync(MiniGameType.TormentedSquare, false, 3900, 1, 0), Times.Once);
            Mock.Get(watcher.ViewPlugIns.GetPlugIn<ISeason21EventViewPlugIn>()!).Verify(v => v.ShowResultAsync(MiniGameType.TormentedSquare, false, 3000, 2, 0), Times.Once);
        }
        finally
        {
            this._plugIn.Configuration.TormentedSquare = original;
        }
    }

    private static List<Monster> Monsters(MiniGameContext game)
        => game.Map.GetAttackablesInRange(new MUnique.OpenMU.Pathfinding.Point(128, 128), 255).OfType<Monster>().Where(m => m.IsAlive).ToList();
}
