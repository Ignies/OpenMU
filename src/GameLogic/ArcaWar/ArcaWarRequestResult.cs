// <copyright file="ArcaWarRequestResult.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ArcaWar;

/// <summary>
/// The results of a request of a player to the Arca War.
/// </summary>
public enum ArcaWarRequestResult
{
    /// <summary>
    /// The request succeeded.
    /// </summary>
    Success,

    /// <summary>
    /// The stage of the war doesn't allow the request.
    /// </summary>
    NotOpen,

    /// <summary>
    /// The player isn't in a guild.
    /// </summary>
    NoGuild,

    /// <summary>
    /// Only the guild master may register the guild.
    /// </summary>
    NotGuildMaster,

    /// <summary>
    /// The guild or the player is registered already.
    /// </summary>
    AlreadyRegistered,

    /// <summary>
    /// The war has the most guilds it takes.
    /// </summary>
    TooManyGuilds,

    /// <summary>
    /// The guild of the player isn't registered.
    /// </summary>
    GuildNotRegistered,

    /// <summary>
    /// The guild has the most members it may register.
    /// </summary>
    TooManyMembers,

    /// <summary>
    /// The player isn't registered.
    /// </summary>
    NotRegistered,
}
