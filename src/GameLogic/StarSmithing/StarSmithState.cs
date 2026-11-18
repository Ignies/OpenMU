// <copyright file="StarSmithState.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The smith's state the client shows (EE 91).
/// </summary>
/// <param name="HammerHeld">Whether a Star Smith's Hammer is in a hand.</param>
/// <param name="HammerBroken">Whether that hammer has not enough durability for an action.</param>
/// <param name="Level">The smith's level.</param>
/// <param name="Experience">The experience into the level.</param>
/// <param name="NextLevelExperience">The experience the level needs to reach the next one.</param>
/// <param name="HammerDurability">The hammer's durability.</param>
public readonly record struct StarSmithState(bool HammerHeld, bool HammerBroken, int Level, long Experience, long NextLevelExperience, int HammerDurability);
