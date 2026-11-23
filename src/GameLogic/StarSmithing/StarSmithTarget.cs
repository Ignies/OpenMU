// <copyright file="StarSmithTarget.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The item an attempt works on, as a recipe's target rule reads it.
/// </summary>
/// <param name="Kind">Its kind.</param>
/// <param name="Level">Its level, +0 to +15.</param>
/// <param name="Excellent">Whether it has an excellent option.</param>
public readonly record struct StarSmithTarget(StarSmithTargetKind Kind, int Level, bool Excellent);
