// <copyright file="LabyrinthGoalKind.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The goals of a mission of a room of the Labyrinth of Dimensions (<c>TypeX</c> of S21Emu <c>MissionSettings</c>).
/// </summary>
public enum LabyrinthGoalKind
{
    /// <summary>
    /// No goal.
    /// </summary>
    None,

    /// <summary>
    /// Kill the given number of monsters in the room.
    /// </summary>
    Kill,

    /// <summary>
    /// Kill the monsters of the room within the given minutes.
    /// </summary>
    KillInTime,

    /// <summary>
    /// Deal the given damage to the monsters of the room.
    /// </summary>
    Damage,

    /// <summary>
    /// Let the MU Helper run for the given seconds in the room.
    /// </summary>
    HelperTime,

    /// <summary>
    /// Kill the monsters of the room within the given minutes without a health potion.
    /// </summary>
    NoHealthPotion,

    /// <summary>
    /// Kill the monsters of the room within the given minutes without a mana potion.
    /// </summary>
    NoManaPotion,

    /// <summary>
    /// Gain the given experience in the room; not used by the data, so it counts as met.
    /// </summary>
    Experience,
}
