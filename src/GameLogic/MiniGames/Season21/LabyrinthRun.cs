// <copyright file="LabyrinthRun.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using System.Collections.Concurrent;
using System.Threading;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.Pathfinding;
using Nito.AsyncEx;

/// <summary>
/// The rooms of the floors of one run of the Labyrinth of Dimensions, for the character of the run.
/// </summary>
/// <remarks>
/// A floor plays a path chosen by chance and starts in its entrance room. Only the room the character is in exists on the
/// map: entering a room loads its area of the other map as the terrain of the run's map copy and moves the character to
/// the side it came from. A mission room keeps its monsters (they come back when killed) until its mission is done,
/// which opens its mission portals; the main mission clears the floor. A newly opened room may get the goblin while fewer
/// than <see cref="LabyrinthRoomsConfiguration.GoblinMaximumOpenRooms"/> rooms are open.
/// Which side of an area is "top" isn't in the data: the server takes the first y coordinate as the top and the first x
/// coordinate as the left; the client draws the room, the server only needs a walkable tile at the side.
/// </remarks>
public sealed class LabyrinthRun
{
    private static readonly LabyrinthMission NoMission = new();

    private readonly MiniGameContext _game;

    private readonly Player _player;

    private readonly LabyrinthRoomsPlugIn _plugIn;

    private readonly LabyrinthRoomsConfiguration _configuration;

    private readonly AsyncLock _lock = new();

    private readonly object _sync = new();

    private readonly ConcurrentDictionary<Monster, (short Number, int Points, int Visit)> _monsters = new();

    private readonly bool[] _discovered = new bool[LabyrinthPath.RoomCount];

    private readonly bool[] _cleared = new bool[LabyrinthPath.RoomCount];

    private LabyrinthPath? _path;

    private LabyrinthMissionProgress _progress = new(NoMission, 0, DateTime.UtcNow);

    private EventRound? _floor;

    private TaskCompletionSource? _floorCleared;

    private CancellationTokenSource? _floorEnded;

    private int _room = -1;

    private int _visit;

    private bool _missionChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="LabyrinthRun"/> class.
    /// </summary>
    /// <param name="game">The game of the run.</param>
    /// <param name="player">The character of the run.</param>
    /// <param name="plugIn">The plugin of the rooms.</param>
    /// <param name="configuration">The configuration of the rooms.</param>
    public LabyrinthRun(MiniGameContext game, Player player, LabyrinthRoomsPlugIn plugIn, LabyrinthRoomsConfiguration configuration)
    {
        this._game = game;
        this._player = player;
        this._plugIn = plugIn;
        this._configuration = configuration;
    }

    /// <summary>
    /// Gets the index of the room the character is in, row * 10 + column; -1 between floors.
    /// </summary>
    public int Room => this._room;

    /// <summary>
    /// Gets the character of the run.
    /// </summary>
    public Player Player => this._player;

    /// <summary>
    /// Starts a floor in the entrance room of a path chosen by chance.
    /// </summary>
    /// <param name="floor">The round of the floor, with its points and monster multipliers.</param>
    /// <param name="cleared">Completed when the main mission is done.</param>
    /// <param name="cancellationToken">The token of the game.</param>
    /// <returns>A task.</returns>
    public async ValueTask StartFloorAsync(EventRound floor, TaskCompletionSource cleared, CancellationToken cancellationToken)
    {
        using (await this._lock.LockAsync(cancellationToken).ConfigureAwait(false))
        {
            var paths = this._configuration.Paths.Where(p => p.Entrance >= 0).ToList();
            this._path = paths[Rand.NextInt(0, paths.Count)];
            this._floor = floor;
            this._floorCleared = cleared;
            Array.Clear(this._discovered);
            Array.Clear(this._cleared);
            this._floorEnded = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            await this.EnterRoomAsync(this._path.Entrance, null).ConfigureAwait(false);
        }

        var token = this._floorEnded.Token;
        _ = Task.Run(() => this.TickAsync(token), token);
    }

    /// <summary>
    /// Ends the floor: its monsters leave.
    /// </summary>
    /// <returns>A task.</returns>
    public async ValueTask EndFloorAsync()
    {
        using (await this._lock.LockAsync().ConfigureAwait(false))
        {
            if (this._floorEnded is { } floorEnded)
            {
                await floorEnded.CancelAsync().ConfigureAwait(false);
                floorEnded.Dispose();
                this._floorEnded = null;
            }

            await this.RemoveMonstersAsync().ConfigureAwait(false);
            this._room = -1;
            this._path = null;
        }
    }

    /// <summary>
    /// Moves the character through a portal of its room.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="direction">The direction.</param>
    /// <returns>The result.</returns>
    public async ValueTask<LabyrinthMoveResult> MoveAsync(Player player, LabyrinthDirection direction)
    {
        using (await this._lock.LockAsync().ConfigureAwait(false))
        {
            if (player != this._player || this._path is not { } path || this._room < 0)
            {
                return LabyrinthMoveResult.NotInRooms;
            }

            var portal = path.GetRoom(this._room).Portals[(int)direction];
            var next = LabyrinthPath.GetNeighbour(this._room, direction);
            if (portal is LabyrinthPortal.Closed or LabyrinthPortal.Key || next < 0 || path.GetRoom(next).Type == LabyrinthAreaType.None || this.FindArea(path.GetRoom(next)) is null)
            {
                return LabyrinthMoveResult.NoPortal;
            }

            if (portal == LabyrinthPortal.Mission && !this._cleared[this._room])
            {
                return LabyrinthMoveResult.MissionNotDone;
            }

            await this.EnterRoomAsync(next, Opposite(direction)).ConfigureAwait(false);
            return LabyrinthMoveResult.Moved;
        }
    }

    /// <summary>
    /// Counts the death of a monster of a room.
    /// </summary>
    /// <param name="monster">The monster.</param>
    /// <returns>The points of the kill, or <c>null</c> when it isn't a monster of the rooms.</returns>
    public int? MonsterDied(Monster monster)
    {
        if (!this._monsters.TryRemove(monster, out var info))
        {
            return null;
        }

        bool completed;
        lock (this._sync)
        {
            if (info.Visit != this._visit)
            {
                return info.Points;
            }

            var wasComplete = this._progress.IsComplete;
            this._progress.AddKill();
            completed = !wasComplete && this._progress.IsComplete;
            this._missionChanged = true;
        }

        _ = Task.Run(() => this.AfterKillAsync(info.Number, info.Visit, completed));
        return info.Points;
    }

    /// <summary>
    /// Counts the damage of the character on a monster of its room.
    /// </summary>
    /// <param name="monster">The monster.</param>
    /// <param name="damage">The damage.</param>
    public void RegisterHit(Monster monster, long damage)
    {
        if (!this._monsters.ContainsKey(monster))
        {
            return;
        }

        if (this.Update(progress => progress.AddDamage(damage)))
        {
            _ = Task.Run(() => this.MissionCompletedAsync(this._visit));
        }
    }

    /// <summary>
    /// Counts an item the character consumed: a forbidden potion starts the mission of the room again.
    /// </summary>
    /// <param name="item">The item.</param>
    public void ItemConsumed(Item item)
    {
        if (item.Definition is not { Group: LabyrinthRoomsPlugIn.PotionGroup } definition)
        {
            return;
        }

        var health = this._configuration.HealthPotions.Contains(definition.Number);
        if (!health && !this._configuration.ManaPotions.Contains(definition.Number))
        {
            return;
        }

        lock (this._sync)
        {
            this._missionChanged |= this._progress.UsedPotion(health, DateTime.UtcNow);
        }
    }

    private static LabyrinthDirection Opposite(LabyrinthDirection direction) => direction switch
    {
        LabyrinthDirection.Top => LabyrinthDirection.Bottom,
        LabyrinthDirection.Bottom => LabyrinthDirection.Top,
        LabyrinthDirection.Left => LabyrinthDirection.Right,
        _ => LabyrinthDirection.Left,
    };

    private static Point GetSidePoint(LabyrinthArea area, LabyrinthDirection? side)
    {
        var middleX = (byte)((area.StartX + area.EndX) / 2);
        var middleY = (byte)((area.StartY + area.EndY) / 2);
        return side switch
        {
            LabyrinthDirection.Top => new Point(middleX, area.StartY),
            LabyrinthDirection.Bottom => new Point(middleX, area.EndY),
            LabyrinthDirection.Left => new Point(area.StartX, middleY),
            LabyrinthDirection.Right => new Point(area.EndX, middleY),
            _ => new Point(middleX, middleY),
        };
    }

    private static Point FindWalkable(GameMapTerrain terrain, LabyrinthArea area, Point target)
    {
        var best = target;
        var bestDistance = int.MaxValue;
        for (int x = area.StartX; x <= area.EndX; x++)
        {
            for (int y = area.StartY; y <= area.EndY; y++)
            {
                var distance = Math.Abs(x - target.X) + Math.Abs(y - target.Y);
                if (distance < bestDistance && terrain.WalkMap[x, y])
                {
                    (best, bestDistance) = (new Point((byte)x, (byte)y), distance);
                }
            }
        }

        return best;
    }

    private LabyrinthArea? FindArea(LabyrinthRoom room) => this._configuration.Areas.FirstOrDefault(a => a.Number == room.Area);

    private bool Update(Action<LabyrinthMissionProgress> change)
    {
        lock (this._sync)
        {
            var wasComplete = this._progress.IsComplete;
            change(this._progress);
            this._missionChanged = true;
            return !wasComplete && this._progress.IsComplete;
        }
    }

    private async ValueTask EnterRoomAsync(int index, LabyrinthDirection? side)
    {
        await this.RemoveMonstersAsync().ConfigureAwait(false);
        var path = this._path!;
        var room = path.GetRoom(index);
        var area = this.FindArea(room) ?? new LabyrinthArea();
        var opened = !this._discovered[index];
        this._discovered[index] = true;

        var terrain = this._game.Map.Terrain;
        terrain.LoadTerrainData(this._plugIn.GetRoomTerrain(area, this._game.GameContext.Configuration));
        await this._player.MoveAsync(FindWalkable(terrain, area, GetSidePoint(area, side))).ConfigureAwait(false);

        var mission = this._cleared[index] ? NoMission : this._configuration.Missions.FirstOrDefault(m => m.Type == room.Type && m.Index == room.Index) ?? NoMission;
        lock (this._sync)
        {
            this._room = index;
            this._visit++;
            this._progress = new LabyrinthMissionProgress(mission, mission.Monsters.Sum(m => m.Count), DateTime.UtcNow);
            this._missionChanged = false;
        }

        foreach (var monster in mission.Monsters)
        {
            for (var i = 0; i < monster.Count; i++)
            {
                await this.SpawnAsync(monster.MonsterNumber, this._floor?.Points ?? 0, area).ConfigureAwait(false);
            }
        }

        if (opened && this._configuration.Goblin is { } goblin
            && this._discovered.Count(d => d) < this._configuration.GoblinMaximumOpenRooms
            && Rand.NextRandomBool(goblin.ChancePercent))
        {
            for (var i = 0; i < goblin.Count; i++)
            {
                await this.SpawnAsync(goblin.MonsterNumber, goblin.Points, area, respawns: false).ConfigureAwait(false);
            }
        }

        if (this._progress.IsComplete)
        {
            await this.CompleteRoomAsync(index).ConfigureAwait(false);
        }
        else
        {
            await this.ShowBoardAsync().ConfigureAwait(false);
            await this.ShowMissionAsync().ConfigureAwait(false);
        }
    }

    private async ValueTask SpawnAsync(short number, int points, LabyrinthArea area, bool respawns = true)
    {
        if (this._game.GameContext.Configuration.Monsters.FirstOrDefault(m => m.Number == number) is not { } definition)
        {
            return;
        }

        var range = (byte)Math.Max(Math.Min(area.EndX - area.StartX, area.EndY - area.StartY) / 2, 1);
        var x = (byte)((area.StartX + area.EndX) / 2);
        var y = (byte)((area.StartY + area.EndY) / 2);
        var floor = this._floor;
        if (await EventMonsterSpawner.SpawnAsync(this._game, definition, x, y, range, floor?.HealthMultiplier ?? 1, floor?.DamageMultiplier ?? 1, floor?.DefenseMultiplier ?? 1).ConfigureAwait(false) is { } monster)
        {
            this._monsters[monster] = (respawns ? number : (short)-1, points, this._visit);
        }
    }

    private async ValueTask RemoveMonstersAsync()
    {
        foreach (var monster in this._monsters.Keys.ToList())
        {
            this._monsters.TryRemove(monster, out _);
            await EventMonsterSpawner.RemoveAsync(this._game, monster).ConfigureAwait(false);
        }
    }

    private async Task AfterKillAsync(short number, int visit, bool completed)
    {
        try
        {
            using (await this._lock.LockAsync().ConfigureAwait(false))
            {
                if (visit != this._visit || this._room < 0)
                {
                    return;
                }

                if (completed)
                {
                    await this.CompleteRoomAsync(this._room).ConfigureAwait(false);
                }
                else
                {
                    if (number >= 0 && !this._progress.IsComplete && this.FindArea(this._path!.GetRoom(this._room)) is { } area)
                    {
                        await this.SpawnAsync(number, this._floor?.Points ?? 0, area).ConfigureAwait(false);
                    }

                    await this.ShowMissionAsync().ConfigureAwait(false);
                }
            }
        }
        catch (Exception ex)
        {
            this._player.Logger.LogError(ex, "Labyrinth room: error after a kill.");
        }
    }

    private async Task MissionCompletedAsync(int visit)
    {
        try
        {
            using (await this._lock.LockAsync().ConfigureAwait(false))
            {
                if (visit == this._visit && this._room >= 0)
                {
                    await this.CompleteRoomAsync(this._room).ConfigureAwait(false);
                }
            }
        }
        catch (Exception ex)
        {
            this._player.Logger.LogError(ex, "Labyrinth room: error at the end of a mission.");
        }
    }

    private async ValueTask CompleteRoomAsync(int index)
    {
        this._cleared[index] = true;
        await this.ShowMissionAsync().ConfigureAwait(false);
        await this.ShowBoardAsync().ConfigureAwait(false);
        if (this._path?.GetRoom(index).Type == LabyrinthAreaType.MainMission)
        {
            this._floorCleared?.TrySetResult();
        }
    }

    private async Task TickAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                var helper = this._player.Attributes?[Stats.IsMuHelperActive] > 0;
                bool completed;
                bool changed;
                int visit;
                lock (this._sync)
                {
                    var wasComplete = this._progress.IsComplete;
                    if (helper)
                    {
                        this._progress.AddHelperSecond();
                        this._missionChanged = true;
                    }

                    this._missionChanged |= this._progress.Tick(DateTime.UtcNow);
                    completed = !wasComplete && this._progress.IsComplete;
                    changed = this._missionChanged;
                    visit = this._visit;
                }

                if (completed)
                {
                    await this.MissionCompletedAsync(visit).ConfigureAwait(false);
                }
                else if (changed)
                {
                    await this.ShowMissionAsync().ConfigureAwait(false);
                }
                else
                {
                    // Nothing to show.
                }
            }
        }
        catch (OperationCanceledException)
        {
            // The floor ended.
        }
        catch (Exception ex)
        {
            this._player.Logger.LogError(ex, "Labyrinth room: error in the timer.");
        }
    }

    private async ValueTask ShowBoardAsync()
    {
        if (this._path is not { } path || this._room < 0)
        {
            return;
        }

        var room = path.GetRoom(this._room);
        var cleared = this._cleared[this._room];
        var portals = room.Portals.Select(p => p switch
        {
            LabyrinthPortal.Mission when cleared => LabyrinthPortal.Open,
            LabyrinthPortal.Key => LabyrinthPortal.Closed,
            _ => p,
        }).ToList();
        var board = new LabyrinthBoard(path.Number, this._room, this.FindArea(room) ?? new LabyrinthArea(), room.Type, portals, this._discovered.ToArray(), this._cleared.ToArray());
        await this._player.InvokeViewPlugInAsync<ILabyrinthRoomsViewPlugIn>(p => p.ShowBoardAsync(board)).ConfigureAwait(false);
    }

    private async ValueTask ShowMissionAsync()
    {
        LabyrinthMissionView view;
        lock (this._sync)
        {
            this._missionChanged = false;
            var room = this._path?.GetRoom(this._room) ?? LabyrinthRoom.None;
            view = new LabyrinthMissionView(room.Type, room.Index, this._progress.Goals, this._progress.IsComplete || this._cleared[Math.Max(this._room, 0)]);
        }

        await this._player.InvokeViewPlugInAsync<ILabyrinthRoomsViewPlugIn>(p => p.ShowMissionAsync(view)).ConfigureAwait(false);
    }
}
