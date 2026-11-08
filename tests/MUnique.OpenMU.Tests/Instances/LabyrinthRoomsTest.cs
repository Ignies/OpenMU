// <copyright file="LabyrinthRoomsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Instances;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.MiniGames;
using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlayerActions;
using MUnique.OpenMU.GameLogic.PlayerActions.MiniGames;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using Moq;

/// <summary>
/// The 10 x 10 rooms of a floor of the Labyrinth of Dimensions (S21Emu <c>LabyrinthOfDimension_Path.xml</c>): each room is
/// an area of another map, opened through the portals of the rooms and the missions of the mission rooms.
/// </summary>
[TestFixture]
public class LabyrinthRoomsTest
{
    private const string Closed = "0.0.0.0000";

    private Season21EventWorld _world = null!;

    private Season21EventsFeaturePlugIn _events = null!;

    /// <summary>
    /// Initializes the world with the Instance Zone and the labyrinth.
    /// </summary>
    [OneTimeSetUp]
    public async ValueTask SetupAsync()
    {
        this._world = await Season21EventWorld.CreateAsync(new AddSeason21InstanceZoneUpdatePlugIn()).ConfigureAwait(false);
        this._events = new Season21EventsFeaturePlugIn { Configuration = this._world.GetStoredConfiguration<Season21EventsConfiguration>(typeof(Season21EventsFeaturePlugIn).GUID) };
        this._world.AddFeaturePlugIn(this._events);
        this._world.Register<IPlayerTalkToNpcPlugIn>(this._events);
    }

    /// <summary>
    /// A room of a path reads its area, type, mission index and the portals of its four sides.
    /// </summary>
    [Test]
    public void PathReadsItsRooms()
    {
        var path = new LabyrinthPath { Number = 1, Rooms = string.Join(' ', Enumerable.Repeat(Closed, 99).Prepend("20.3.4.0213")) };

        var room = path.GetRoom(0);

        Assert.That((room.Area, room.Type, room.Index), Is.EqualTo((20, LabyrinthAreaType.Mission, 4)));
        Assert.That(room.Portals, Is.EqualTo(new[] { LabyrinthPortal.Closed, LabyrinthPortal.Mission, LabyrinthPortal.Open, LabyrinthPortal.Key }));
        Assert.That(path.GetRoom(99).Type, Is.EqualTo(LabyrinthAreaType.None));
        Assert.That(path.Entrance, Is.EqualTo(-1));
    }

    /// <summary>
    /// The terrain of a room is the area of its map, without safe zones; everything else is blocked.
    /// </summary>
    [Test]
    public void RoomTerrainIsTheAreaOfItsMap()
    {
        var lorencia = this._world.Configuration.Maps.First(m => m.Number == 0);
        var area = new LabyrinthArea { Number = 0, MapNumber = 0, StartX = 78, StartY = 226, EndX = 93, EndY = 237 };

        var terrain = new GameMapTerrain(LabyrinthRoomsPlugIn.CreateRoomTerrain(area, lorencia.TerrainData));

        var source = new GameMapTerrain(lorencia);
        Assert.That(terrain.WalkMap[77, 230] || terrain.WalkMap[94, 230] || terrain.WalkMap[80, 225] || terrain.WalkMap[80, 238], Is.False);
        for (var x = 78; x <= 93; x++)
        {
            for (var y = 226; y <= 237; y++)
            {
                Assert.That(terrain.WalkMap[x, y], Is.EqualTo(source.WalkMap[x, y]), $"{x}/{y}");
                Assert.That(terrain.SafezoneMap[x, y], Is.False);
            }
        }
    }

    /// <summary>
    /// The goals of a mission count the kills, the damage and the helper seconds in the room; a potion of a kind the
    /// mission forbids starts it again, and so does the end of its time.
    /// </summary>
    [Test]
    public void MissionCountsItsGoals()
    {
        var start = new DateTime(2026, 11, 6, 12, 0, 0, DateTimeKind.Utc);
        var mission = new LabyrinthMission
        {
            Type = LabyrinthAreaType.MainMission,
            Goals = [new LabyrinthGoal { Kind = LabyrinthGoalKind.Kill, Target = 2 }, new LabyrinthGoal { Kind = LabyrinthGoalKind.Damage, Target = 1000 }, new LabyrinthGoal { Kind = LabyrinthGoalKind.HelperTime, Target = 2 }],
        };
        var progress = new LabyrinthMissionProgress(mission, 3, start);

        progress.AddKill();
        progress.AddKill();
        progress.AddDamage(999);
        progress.AddHelperSecond();
        Assert.That(progress.IsComplete, Is.False);
        progress.AddDamage(1);
        progress.AddHelperSecond();
        Assert.That(progress.IsComplete, Is.True);
        Assert.That(progress.Goals.Select(g => g.Progress), Is.EqualTo(new long[] { 2, 1000, 2 }));

        var noPotion = new LabyrinthMissionProgress(new LabyrinthMission { Goals = [new LabyrinthGoal { Kind = LabyrinthGoalKind.NoHealthPotion, Target = 15 }] }, 2, start);
        noPotion.AddKill();
        Assert.That(noPotion.UsedPotion(health: false, start), Is.False, "a mana potion is allowed");
        Assert.That(noPotion.UsedPotion(health: true, start), Is.True, "a health potion starts it again");
        Assert.That(noPotion.Kills, Is.Zero);
        noPotion.AddKill();
        noPotion.AddKill();
        Assert.That(noPotion.IsComplete, Is.True);

        var timed = new LabyrinthMissionProgress(new LabyrinthMission { Goals = [new LabyrinthGoal { Kind = LabyrinthGoalKind.KillInTime, Target = 15 }] }, 2, start);
        timed.AddKill();
        Assert.That(timed.Tick(start.AddMinutes(14)), Is.False);
        Assert.That(timed.Tick(start.AddMinutes(15)), Is.True, "the time ran out");
        Assert.That(timed.Kills, Is.Zero);
        timed.AddKill();
        timed.AddKill();
        Assert.That(timed.IsComplete, Is.True);
    }

    /// <summary>
    /// With the rooms on, a floor starts in the entrance room of a path; a portal leads to the next room, a mission portal
    /// only after the mission of the room, and the main mission clears the floor.
    /// </summary>
    [Test]
    public async ValueTask FloorIsPlayedThroughItsRoomsAsync()
    {
        var rooms = new LabyrinthRoomsPlugIn { Configuration = CreateRooms() };
        this._world.AddFeaturePlugIn(rooms);
        var original = this._events.Configuration!.Labyrinth!;
        this._events.Configuration.Labyrinth = new RoundsEventDefinition
        {
            SpawnPoints = original.SpawnPoints,
            RoundDuration = TimeSpan.FromSeconds(30),
            PauseAfterRound = TimeSpan.Zero,
            EndsWhenARoundFails = true,
            PartyIsATeam = false,
            Rounds = [new EventRound { MonsterNumber = 754, Count = 1, Points = 10, ClearPoints = 50 }],
        };
        try
        {
            var player = await this._world.CreatePlayerAsync(CharacterClassNumber.BladeMaster, 400, 100, 114, 232, 57).ConfigureAwait(false);
            await this._world.TalkAsync(player, 758).ConfigureAwait(false);
            var run = (Season21RoundsContext)player.CurrentMiniGame!;
            await Season21EventWorld.SkipUntilPlayingAsync(run).ConfigureAwait(false);
            var view = Mock.Get(player.ViewPlugIns.GetPlugIn<ILabyrinthRoomsViewPlugIn>()!);
            var move = new LabyrinthMoveAction();

            // The room is entered before its board is sent: wait for the board, not only for the room.
            await Season21EventWorld.WaitUntilAsync(() => run.Rooms?.Room == 0 && IsIn(player, 78, 226, 93, 237)
                && view.Invocations.Any(i => i.Method.Name == nameof(ILabyrinthRoomsViewPlugIn.ShowBoardAsync))).ConfigureAwait(false);
            Assert.That(Monsters(run, 85, 231), Is.Empty, "the entrance has no monsters");
            view.Verify(v => v.ShowBoardAsync(It.Is<LabyrinthBoard>(b => b.Room == 0 && b.Area.Number == 0 && b.Discovered[0] && !b.Discovered[1])), Times.AtLeastOnce);

            await move.MoveAsync(player, LabyrinthDirection.Left).ConfigureAwait(false);
            view.Verify(v => v.ShowMoveResultAsync(LabyrinthDirection.Left, LabyrinthMoveResult.NoPortal), Times.Once);

            await move.MoveAsync(player, LabyrinthDirection.Right).ConfigureAwait(false);
            Assert.That((run.Rooms!.Room, IsIn(player, 47, 146, 62, 157)), Is.EqualTo((1, true)));
            await Season21EventWorld.WaitUntilAsync(() => Monsters(run, 54, 151).Count == 1).ConfigureAwait(false);

            await move.MoveAsync(player, LabyrinthDirection.Right).ConfigureAwait(false);
            view.Verify(v => v.ShowMoveResultAsync(LabyrinthDirection.Right, LabyrinthMoveResult.MissionNotDone), Times.Once);

            await Season21EventWorld.KillAsync(Monsters(run, 54, 151)[0], player).ConfigureAwait(false);
            await Season21EventWorld.WaitUntilAsync(() => Monsters(run, 54, 151).Count == 1).ConfigureAwait(false);
            await Season21EventWorld.KillAsync(Monsters(run, 54, 151)[0], player).ConfigureAwait(false);
            Assert.That(run.GetScore(player), Is.EqualTo(20), "a kill in a room gives the points of the floor");
            await Season21EventWorld.WaitUntilAsync(() => view.Invocations.Any(i => i.Arguments.FirstOrDefault() is LabyrinthMissionView { Completed: true, Goals: [{ Kind: LabyrinthGoalKind.Kill, Progress: 2 }] })).ConfigureAwait(false);
            Assert.That(Monsters(run, 54, 151), Is.Empty, "a done mission spawns no more monsters");

            await move.MoveAsync(player, LabyrinthDirection.Right).ConfigureAwait(false);
            Assert.That((run.Rooms.Room, IsIn(player, 24, 21, 39, 32)), Is.EqualTo((2, true)));
            await Season21EventWorld.WaitUntilAsync(() => Monsters(run, 31, 26).Count == 1).ConfigureAwait(false);
            await Season21EventWorld.KillAsync(Monsters(run, 31, 26)[0], player).ConfigureAwait(false);

            await Season21EventWorld.WaitUntilAsync(() => run.ClearedRounds == 1).ConfigureAwait(false);
            Assert.That(run.GetScore(player), Is.EqualTo(30 + 50));
        }
        finally
        {
            this._events.Configuration.Labyrinth = original;
            this._world.PlugInManager.DeactivatePlugIn<LabyrinthRoomsPlugIn>();
        }
    }

    /// <summary>
    /// A floor starts when the client of the runner has loaded the map, not when the entry time is over: a slow loader
    /// was placed outside the entrance room, on a blocked tile, and sent back to the Instance Zone.
    /// </summary>
    [Test]
    public async ValueTask FloorWaitsForTheRunnerToLoadTheMapAsync()
    {
        var rooms = new LabyrinthRoomsPlugIn { Configuration = CreateRooms() };
        this._world.AddFeaturePlugIn(rooms);
        var definition = this._world.Configuration.MiniGameDefinitions.First(d => d.Type == MiniGameType.LabyrinthOfDimensions && d.MapCreationPolicy != MiniGameMapCreationPolicy.Shared);
        var enterDuration = definition.EnterDuration;
        definition.EnterDuration = TimeSpan.FromMilliseconds(100);
        try
        {
            var player = await this._world.CreatePlayerAsync(CharacterClassNumber.BladeMaster, 400, 100, 114, 232, 57).ConfigureAwait(false);
            var portal = player.CurrentMap!.GetNpcsInRange(player.Position, 255).First(n => n.Definition.Number == 758);
            await new TalkNpcAction().TalkToNpcAsync(player, portal).ConfigureAwait(false);
            var run = (Season21RoundsContext)player.CurrentMiniGame!;

            await Task.Delay(1000).ConfigureAwait(false);
            Assert.That(run.State, Is.EqualTo(MiniGameState.Open), "the client is still loading");

            await Season21EventWorld.CompleteMapChangeAsync(player).ConfigureAwait(false);
            await Season21EventWorld.WaitUntilAsync(() => run.Rooms?.Room == 0).ConfigureAwait(false);
            Assert.That((player.CurrentMap, IsIn(player, 78, 226, 93, 237)), Is.EqualTo((run.Map, true)));
        }
        finally
        {
            definition.EnterDuration = enterDuration;
            this._world.PlugInManager.DeactivatePlugIn<LabyrinthRoomsPlugIn>();
        }
    }

    private static LabyrinthRoomsConfiguration CreateRooms()
    {
        var rooms = Enumerable.Repeat(Closed, 100).ToArray();
        rooms[0] = "0.1.0.0001";
        rooms[1] = "27.3.0.0012";
        rooms[2] = "28.4.0.0010";
        return new LabyrinthRoomsConfiguration
        {
            Areas =
            [
                new LabyrinthArea { Number = 0, MapNumber = 0, StartX = 78, StartY = 226, EndX = 93, EndY = 237 },
                new LabyrinthArea { Number = 27, MapNumber = 0, StartX = 47, StartY = 146, EndX = 62, EndY = 157 },
                new LabyrinthArea { Number = 28, MapNumber = 0, StartX = 24, StartY = 21, EndX = 39, EndY = 32 },
            ],
            Missions =
            [
                new LabyrinthMission { Type = LabyrinthAreaType.Mission, Index = 0, Goals = [new LabyrinthGoal { Kind = LabyrinthGoalKind.Kill, Target = 2 }], Monsters = [new LabyrinthMonster { MonsterNumber = 754, Count = 1 }] },
                new LabyrinthMission { Type = LabyrinthAreaType.MainMission, Index = 0, Goals = [new LabyrinthGoal { Kind = LabyrinthGoalKind.Kill, Target = 1 }], Monsters = [new LabyrinthMonster { MonsterNumber = 754, Count = 1 }] },
            ],
            Paths = [new LabyrinthPath { Number = 1, Rooms = string.Join(' ', rooms) }],
            Goblin = new EventBonusMonster { MonsterNumber = 757, Count = 1, ChancePercent = 0, Points = 2000 },
        };
    }

    private static bool IsIn(Player player, int x1, int y1, int x2, int y2)
        => player.Position.X >= x1 && player.Position.X <= x2 && player.Position.Y >= y1 && player.Position.Y <= y2;

    private static List<Monster> Monsters(MiniGameContext game, byte x, byte y)
        => game.Map.GetAttackablesInRange(new MUnique.OpenMU.Pathfinding.Point(x, y), 10).OfType<Monster>().Where(m => m.IsAlive).ToList();
}
