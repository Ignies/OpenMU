// <copyright file="StarSmithRoll.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// What a seal rolled from its recipe's outcome table.
/// </summary>
/// <param name="OutcomeIndex">The outcome's index in the table.</param>
/// <param name="Boosts">The boosts, scaled and capped per item.</param>
public sealed record StarSmithRoll(int OutcomeIndex, IReadOnlyList<StarSmithBoost> Boosts);
