// <copyright file="IllusionTempleLeagueEntryCheck.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The result of the entry check of the Illusion Temple League.
/// </summary>
public enum IllusionTempleLeagueEntryCheck
{
    /// <summary>
    /// The player may enter.
    /// </summary>
    Allowed,

    /// <summary>
    /// No league match today.
    /// </summary>
    NotOpen,

    /// <summary>
    /// The player is not in a party of enough members of his guild.
    /// </summary>
    GuildPartyRequired,

    /// <summary>
    /// The guild used all its entries of the eliminations.
    /// </summary>
    GuildEntriesUsed,

    /// <summary>
    /// The player used all his entries of the eliminations.
    /// </summary>
    PlayerEntriesUsed,

    /// <summary>
    /// The guild didn't qualify for this match.
    /// </summary>
    NotQualified,

    /// <summary>
    /// Two other guilds already play in this temple, or the guild's side is full.
    /// </summary>
    Full,
}
