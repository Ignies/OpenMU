// <copyright file="MonsterKind.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// The kind of a monster, which decides how far its numbers may stray from its map's normal monsters.
/// </summary>
public enum MonsterKind
{
    /// <summary>
    /// A normal monster.
    /// </summary>
    Normal,

    /// <summary>
    /// An elite monster (its designation contains "Elite").
    /// </summary>
    Elite,

    /// <summary>
    /// A map boss.
    /// </summary>
    Boss,
}
