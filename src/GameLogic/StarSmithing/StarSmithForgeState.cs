// <copyright file="StarSmithForgeState.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// Where an attempt stands.
/// </summary>
public enum StarSmithForgeState
{
    /// <summary>Stars are being struck.</summary>
    Drafting,

    /// <summary>The smith sealed the result.</summary>
    Sealed,

    /// <summary>A void cracked the attempt.</summary>
    Cracked,

    /// <summary>The smith walked away, or the attempt timed out: like a crack off every shape.</summary>
    Abandoned,
}
