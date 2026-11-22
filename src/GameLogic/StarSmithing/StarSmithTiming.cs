// <copyright file="StarSmithTiming.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// How a strike met the beat.
/// </summary>
public enum StarSmithTiming
{
    /// <summary>On the beat: a clean link.</summary>
    Clean,

    /// <summary>Near the beat: a shaky link.</summary>
    Shaky,

    /// <summary>Off the beat, or a second strike on one beat: still a link, of the lowest quality.</summary>
    Late,
}
