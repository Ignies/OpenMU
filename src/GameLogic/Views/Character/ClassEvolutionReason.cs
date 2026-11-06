// <copyright file="ClassEvolutionReason.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.Character;

/// <summary>
/// The reason of a class evolution.
/// </summary>
public enum ClassEvolutionReason : byte
{
    /// <summary>
    /// A quest evolved the character.
    /// </summary>
    Quest = 0,

    /// <summary>
    /// A game master evolved the character.
    /// </summary>
    GameMaster = 1,

    /// <summary>
    /// An item evolved the character.
    /// </summary>
    Item = 2,
}
