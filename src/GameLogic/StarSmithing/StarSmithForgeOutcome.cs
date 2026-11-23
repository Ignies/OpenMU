// <copyright file="StarSmithForgeOutcome.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// How a Forge attempt ended (the Outcome byte of EE 9F).
/// </summary>
public enum StarSmithForgeOutcome : byte
{
    /// <summary>The smith sealed it.</summary>
    Sealed,

    /// <summary>A void cracked it.</summary>
    Cracked,

    /// <summary>The smith abandoned it: the ingredients are lost, the item stays.</summary>
    Abandoned,

    /// <summary>It was idle too long; handled as an abandon.</summary>
    TimedOut,

    /// <summary>A seal or abandon was refused: no attempt is running, or nothing is struck yet.</summary>
    Refused,
}
