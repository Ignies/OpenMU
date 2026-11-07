// <copyright file="ArcaWarRequestKind.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ArcaWar;

/// <summary>
/// The kinds of requests of a player to the Arca War.
/// </summary>
public enum ArcaWarRequestKind
{
    /// <summary>
    /// A guild master registers the guild.
    /// </summary>
    RegisterGuild,

    /// <summary>
    /// A member of a registered guild registers.
    /// </summary>
    RegisterMember,

    /// <summary>
    /// A registered member enters the war.
    /// </summary>
    Enter,
}
