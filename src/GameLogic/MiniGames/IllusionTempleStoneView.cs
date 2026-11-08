// <copyright file="IllusionTempleStoneView.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames;

/// <summary>
/// A Captured Stone Statue of the renewed Illusion Temple as the players see it.
/// </summary>
/// <param name="Index">The index of the stone, 0 to 9 (NPC 659 to 668).</param>
/// <param name="Holder">The team which sealed the hollow into it, <c>null</c> while it's free.</param>
/// <param name="X">The x coordinate.</param>
/// <param name="Y">The y coordinate.</param>
public readonly record struct IllusionTempleStoneView(byte Index, IllusionTempleTeam? Holder, byte X, byte Y);
