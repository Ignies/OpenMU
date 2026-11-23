// <copyright file="StarSmithBoostSeen.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// A stat a found recipe gave in the outcomes the smith has seen, with the range of those outcomes, unscaled, times 100.
/// </summary>
/// <param name="Stat">The stat.</param>
/// <param name="Minimum">The least value times 100.</param>
/// <param name="Maximum">The most value times 100.</param>
public readonly record struct StarSmithBoostSeen(StarSmithBoostStat Stat, int Minimum, int Maximum);
