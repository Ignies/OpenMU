// <copyright file="LabyrinthPath.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using System.Text.Json.Serialization;

/// <summary>
/// A path of the Labyrinth of Dimensions: a board of 10 x 10 rooms (S21Emu <c>LabyrinthOfDimension_Path.xml</c>).
/// </summary>
/// <remarks>
/// <see cref="Rooms"/> keeps the 100 rooms in one line, row by row, separated by spaces, so that the 3000 rooms of the
/// data stay editable: each room is <c>area.type.index.TBLR</c>, the number of its <see cref="LabyrinthArea"/>, its
/// <see cref="LabyrinthAreaType"/>, the index of its <see cref="LabyrinthMission"/> and one digit per
/// <see cref="LabyrinthPortal"/> of the top, bottom, left and right side, e.g. <c>20.3.4.0200</c>.
/// </remarks>
public class LabyrinthPath
{
    /// <summary>
    /// The number of rows and of columns of the board.
    /// </summary>
    public const int Size = 10;

    /// <summary>
    /// The number of rooms of the board.
    /// </summary>
    public const int RoomCount = Size * Size;

    private string _rooms = string.Empty;

    private LabyrinthRoom[]? _parsed;

    /// <summary>
    /// Gets or sets the number of the path.
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Gets or sets the rooms, row by row (see the remarks).
    /// </summary>
    public string Rooms
    {
        get => this._rooms;
        set
        {
            this._rooms = value;
            this._parsed = null;
        }
    }

    /// <summary>
    /// Gets the index of the entrance room, or -1 when the path has none.
    /// </summary>
    [JsonIgnore]
    public int Entrance => Array.FindIndex(this.Parsed, room => room.Type == LabyrinthAreaType.Entrance);

    private LabyrinthRoom[] Parsed => this._parsed ??= Parse(this._rooms);

    /// <summary>
    /// Gets the index of the room next to a room, or -1 at the edge of the board.
    /// </summary>
    /// <param name="index">The index of the room.</param>
    /// <param name="direction">The direction.</param>
    /// <returns>The index of the neighbour, or -1.</returns>
    public static int GetNeighbour(int index, LabyrinthDirection direction)
    {
        var (row, column) = Math.DivRem(index, Size);
        (row, column) = direction switch
        {
            LabyrinthDirection.Top => (row - 1, column),
            LabyrinthDirection.Bottom => (row + 1, column),
            LabyrinthDirection.Left => (row, column - 1),
            _ => (row, column + 1),
        };
        return row is >= 0 and < Size && column is >= 0 and < Size ? (row * Size) + column : -1;
    }

    /// <summary>
    /// Gets a room.
    /// </summary>
    /// <param name="index">The index, row * 10 + column.</param>
    /// <returns>The room; <see cref="LabyrinthRoom.None"/> outside of the board.</returns>
    public LabyrinthRoom GetRoom(int index) => index is >= 0 and < RoomCount ? this.Parsed[index] : LabyrinthRoom.None;

    private static LabyrinthRoom[] Parse(string rooms)
    {
        var result = Enumerable.Repeat(LabyrinthRoom.None, RoomCount).ToArray();
        var tokens = rooms.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < tokens.Length && i < RoomCount; i++)
        {
            var parts = tokens[i].Split('.');
            if (parts.Length != 4 || parts[3].Length != 4
                || !int.TryParse(parts[0], out var area) || !int.TryParse(parts[1], out var type) || !int.TryParse(parts[2], out var mission))
            {
                continue;
            }

            var portals = parts[3].Select(c => c is >= '0' and <= '3' ? (LabyrinthPortal)(c - '0') : LabyrinthPortal.Closed).ToArray();
            result[i] = new LabyrinthRoom(area, Enum.IsDefined((LabyrinthAreaType)type) ? (LabyrinthAreaType)type : LabyrinthAreaType.None, mission, portals);
        }

        return result;
    }
}
