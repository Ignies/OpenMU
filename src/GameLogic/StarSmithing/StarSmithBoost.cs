// <copyright file="StarSmithBoost.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// A boost on a smithed item: its stat and its value times 100 (already scaled and capped per item).
/// </summary>
/// <param name="Stat">The stat.</param>
/// <param name="Value">The value times 100 (6.25% is 625).</param>
public readonly record struct StarSmithBoost(StarSmithBoostStat Stat, int Value);
