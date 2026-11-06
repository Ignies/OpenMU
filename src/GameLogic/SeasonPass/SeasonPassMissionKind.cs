// <copyright file="SeasonPassMissionKind.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.SeasonPass;

/// <summary>
/// What a mission of the season pass counts. The data only numbers its missions (the client's mission texts aren't
/// available), so an operator sets the kind of each; <see cref="None"/> doesn't progress.
/// </summary>
public enum SeasonPassMissionKind
{
    /// <summary>
    /// Nothing, the mission doesn't progress.
    /// </summary>
    None,

    /// <summary>
    /// Monsters killed.
    /// </summary>
    MonsterKills,

    /// <summary>
    /// Experience gained by kills, normal and master.
    /// </summary>
    Experience,

    /// <summary>
    /// Seconds spent in the game world.
    /// </summary>
    PlaySeconds,
}
