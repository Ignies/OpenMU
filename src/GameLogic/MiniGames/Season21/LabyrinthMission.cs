// <copyright file="LabyrinthMission.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The mission of the rooms of a kind and index of the Labyrinth of Dimensions (<c>MissionSettings</c> of S21Emu <c>LabyrinthOfDimension.xml</c>).
/// </summary>
public class LabyrinthMission
{
    /// <summary>
    /// Gets or sets the kind of room.
    /// </summary>
    public LabyrinthAreaType Type { get; set; }

    /// <summary>
    /// Gets or sets the index of the mission, which the rooms of the paths name.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets the goals; all of them have to be met.
    /// </summary>
    public ICollection<LabyrinthGoal> Goals { get; set; } = new List<LabyrinthGoal>();

    /// <summary>
    /// Gets or sets the monsters of the room, which come back while the mission isn't done.
    /// </summary>
    public ICollection<LabyrinthMonster> Monsters { get; set; } = new List<LabyrinthMonster>();
}
