// <copyright file="MuunConditionType.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Muuns;

/// <summary>
/// The condition under which the option of a Muun applies (MuunOption.xml <c>Option1_ConditionType</c>).
/// </summary>
public enum MuunConditionType : byte
{
    /// <summary>
    /// Always.
    /// </summary>
    None = 0,

    /// <summary>
    /// On the map with the number of the first value.
    /// </summary>
    Map = 1,

    /// <summary>
    /// When the total level (level and master level) of the character is between the two values.
    /// </summary>
    CharacterLevel = 2,

    /// <summary>
    /// Within the hours of the day (UTC) between the two values, 1 to 24: 1 is the hour from 0:00 to 1:00.
    /// </summary>
    Hours = 3,

    /// <summary>
    /// On the days of the first value, a bit mask (64 Sunday, 32 Monday, 16 Tuesday, 8 Wednesday, 4 Thursday, 2 Friday, 1 Saturday); -1 every day.
    /// </summary>
    Weekdays = 4,

    /// <summary>
    /// By the hours played; applies always, the play time isn't counted (designed).
    /// </summary>
    PlayTime = 7,
}
