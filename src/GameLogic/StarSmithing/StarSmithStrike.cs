// <copyright file="StarSmithStrike.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The answer to a strike.
/// </summary>
/// <param name="Result">Linked, cracked, or why it was refused.</param>
/// <param name="Timing">How it met the beat (Late when refused).</param>
/// <param name="Star">The struck star, revealed (default when refused).</param>
public readonly record struct StarSmithStrike(StarSmithStrikeResult Result, StarSmithTiming Timing, StarSmithStar Star);
