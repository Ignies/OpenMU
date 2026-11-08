// <copyright file="IllusionTempleLeagueStanding.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The league of a guild and a character in the current month.
/// </summary>
/// <param name="Stage">The stage of the day.</param>
/// <param name="Rank">The rank of the guild in the eliminations, 0 when it played none.</param>
/// <param name="GuildEntries">The elimination matches of the guild.</param>
/// <param name="PlayerEntries">The elimination matches of the character.</param>
/// <param name="Stones">The occupied stones of the guild in the eliminations.</param>
/// <param name="Kills">The kills of the guild in the eliminations.</param>
/// <param name="LevelSum">The level sums of the guild in the eliminations.</param>
/// <param name="Qualification">How far the guild came.</param>
/// <param name="MaximumGuildEntries">The elimination matches a guild may play, 0 for no limit.</param>
/// <param name="MaximumPlayerEntries">The elimination matches a character may play, 0 for no limit.</param>
public readonly record struct IllusionTempleLeagueStanding(
    IllusionTempleLeagueStage Stage,
    int Rank,
    int GuildEntries,
    int PlayerEntries,
    int Stones,
    int Kills,
    int LevelSum,
    IllusionTempleLeagueQualification Qualification,
    int MaximumGuildEntries,
    int MaximumPlayerEntries);
