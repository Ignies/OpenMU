// <copyright file="ArcaWarStage.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ArcaWar;

/// <summary>
/// The stages of an Arca War (S21Emu <c>Event/ArcaBattle.xml</c>).
/// </summary>
public enum ArcaWarStage
{
    /// <summary>
    /// No war: waiting for the next start of the schedule.
    /// </summary>
    Closed,

    /// <summary>
    /// The guild masters register their guilds (<c>MasterRegTime</c>).
    /// </summary>
    MasterRegistration,

    /// <summary>
    /// The members of the registered guilds register (<c>MemberRegTime</c>).
    /// </summary>
    MemberRegistration,

    /// <summary>
    /// The wait before the war (<c>ProgressWait</c>).
    /// </summary>
    Waiting,

    /// <summary>
    /// The war (<c>Duration</c>).
    /// </summary>
    Playing,

    /// <summary>
    /// The time to leave the maps after the war (<c>ChannelClose</c>).
    /// </summary>
    Closing,
}
