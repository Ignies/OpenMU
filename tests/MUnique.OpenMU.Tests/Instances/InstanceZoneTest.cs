// <copyright file="InstanceZoneTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Instances;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.EvolutionQuests;
using MUnique.OpenMU.GameLogic.MiniGames;
using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Ruud;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using Moq;

/// <summary>
/// Runs the Instance Zone (map 114) and the Labyrinth of Dimensions (map 115) of Season 21: the trials are entered from
/// the zone, the labyrinth is a solo instance of floors.
/// </summary>
[TestFixture]
public class InstanceZoneTest
{
    private Season21EventWorld _world = null!;

    private Season21EventsFeaturePlugIn _events = null!;

    /// <summary>
    /// Initializes the world with the quests, the instances and the Instance Zone.
    /// </summary>
    [OneTimeSetUp]
    public async ValueTask SetupAsync()
    {
        this._world = await Season21EventWorld.CreateAsync(
            new AddSeason21EvolutionQuestsUpdatePlugIn(),
            new AddSeason21InstancesUpdatePlugIn(),
            new AddSeason21InstanceZoneUpdatePlugIn()).ConfigureAwait(false);
        var quests = new EvolutionQuestFeaturePlugIn { Configuration = this._world.GetStoredConfiguration<EvolutionQuestConfiguration>(typeof(EvolutionQuestFeaturePlugIn).GUID) };
        this._events = new Season21EventsFeaturePlugIn { Configuration = this._world.GetStoredConfiguration<Season21EventsConfiguration>(typeof(Season21EventsFeaturePlugIn).GUID) };
        this._world.AddFeaturePlugIn(quests);
        this._world.AddFeaturePlugIn(this._events);
        this._world.Register<IPlayerTalkToNpcPlugIn>(quests);
        this._world.Register<IPlayerTalkToNpcPlugIn>(this._events);
        this._world.Register<IObjectAddedToMapPlugIn>(quests);
        this._world.Register<IObjectRemovedFromMapPlugIn>(quests);
    }

    /// <summary>
    /// Sent in Lorencia takes a character to the Instance Zone, Cent there into the trial instance of its party.
    /// </summary>
    [Test]
    public async ValueTask TrialIsEnteredFromTheInstanceZoneAsync()
    {
        var player = await this._world.CreatePlayerAsync(CharacterClassNumber.BladeMaster, 400, 400).ConfigureAwait(false);
        await this._world.TalkAsync(player, 764).ConfigureAwait(false);
        Assert.That(player.CurrentMap?.Definition.Number, Is.EqualTo(114));
        Assert.That(player.CurrentMiniGame, Is.Null);

        await this._world.TalkAsync(player, 765).ConfigureAwait(false);
        Assert.That(player.CurrentMiniGame?.Definition.Type, Is.EqualTo(MiniGameType.PartyInstance));
        Assert.That(player.CurrentMap?.Definition.Number, Is.EqualTo(EvolutionQuestConfiguration.Season21TrialMap));
        await player.WarpToSafezoneAsync().ConfigureAwait(false);
        await Season21EventWorld.CompleteMapChangeAsync(player).ConfigureAwait(false);
    }

    /// <summary>
    /// The labyrinth has its 41 floors with the multipliers, Ruud and boxes of the data.
    /// </summary>
    [Test]
    public void LabyrinthHasTheFloorsOfTheData()
    {
        var floors = this._events.Configuration!.Labyrinth!.Rounds;
        Assert.That(floors, Has.Count.EqualTo(41));
        Assert.That((floors[0].ClearRuud, floors[6].ClearRuud, floors[7].ClearRuud), Is.EqualTo((30, 400, 0)));
        Assert.That((floors[40].HealthMultiplier, floors[40].DamageMultiplier, floors[40].DefenseMultiplier), Is.EqualTo((5.0f, 9.5f, 10.0f)));
        Assert.That(floors[9].ClearRandomItems, Is.Not.Empty);
        Assert.That(this._events.Configuration.Entrances.Single().NpcNumber, Is.EqualTo((short)758));
    }

    /// <summary>
    /// The Portal of Dimension takes a player into a labyrinth of its own; a cleared floor gives its Ruud, a floor which
    /// runs out ends the run.
    /// </summary>
    [Test]
    public async ValueTask LabyrinthIsASoloRunOfFloorsAsync()
    {
        var original = this._events.Configuration!.Labyrinth!;
        this._events.Configuration.Labyrinth = new RoundsEventDefinition
        {
            SpawnPoints = original.SpawnPoints,
            RoundDuration = TimeSpan.FromSeconds(5),
            PauseAfterRound = TimeSpan.Zero,
            EndsWhenARoundFails = true,
            PartyIsATeam = false,
            Rounds =
            [
                new EventRound { MonsterNumber = 754, Count = 1, ClearPoints = 50, ClearRuud = 30 },
                new EventRound { MonsterNumber = 754, Count = 1, ClearPoints = 100, ClearRuud = 70 },
                new EventRound { MonsterNumber = 754, Count = 1, ClearPoints = 150, ClearRuud = 130 },
            ],
        };
        try
        {
            var player = await this._world.CreatePlayerAsync(CharacterClassNumber.BladeMaster, 400, 100, 114, 232, 57).ConfigureAwait(false);
            var other = await this._world.CreatePlayerAsync(CharacterClassNumber.GrandMaster, 400, 100, 114, 232, 58).ConfigureAwait(false);
            await this._world.TalkAsync(player, 758).ConfigureAwait(false);
            await this._world.TalkAsync(other, 758).ConfigureAwait(false);
            var run = (Season21RoundsContext)player.CurrentMiniGame!;
            Assert.That(run.Definition.Type, Is.EqualTo(MiniGameType.LabyrinthOfDimensions));
            Assert.That(other.CurrentMiniGame, Is.Not.SameAs(run));
            await Season21EventWorld.SkipUntilPlayingAsync(run).ConfigureAwait(false);

            await Season21EventWorld.WaitUntilAsync(() => run.Round == 1 && Monsters(run).Count == 1).ConfigureAwait(false);
            await Season21EventWorld.KillAsync(Monsters(run)[0], player).ConfigureAwait(false);
            await Season21EventWorld.WaitUntilAsync(() => run.ClearedRounds == 1).ConfigureAwait(false);
            Assert.That(player.GetRuud(), Is.EqualTo(30));

            await Season21EventWorld.WaitUntilAsync(() => run.State is MiniGameState.Ended or MiniGameState.Disposed).ConfigureAwait(false);
            Assert.That(run.Round, Is.EqualTo(2), "the run ended on the floor which ran out");
            Mock.Get(player.ViewPlugIns.GetPlugIn<ISeason21EventViewPlugIn>()!).Verify(v => v.ShowResultAsync(MiniGameType.LabyrinthOfDimensions, false, 50, 1, 0), Times.Once);
        }
        finally
        {
            this._events.Configuration.Labyrinth = original;
        }
    }

    private static List<Monster> Monsters(MiniGameContext game)
        => game.Map.GetAttackablesInRange(new MUnique.OpenMU.Pathfinding.Point(21, 12), 30).OfType<Monster>().Where(m => m.IsAlive).ToList();
}
