// <copyright file="MuunKind.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Muuns;

/// <summary>
/// The kind of a Muun (MuunInfo.xml <c>Type</c>).
/// </summary>
public enum MuunKind : byte
{
    /// <summary>
    /// A Muun which follows its owner.
    /// </summary>
    Normal = 1,

    /// <summary>
    /// A Muun which its owner can ride (Tibetton, Repen, Pon, Griffs).
    /// </summary>
    Mount = 4,

    /// <summary>
    /// A hero Muun: it is at its highest level from the start and doesn't evolve.
    /// </summary>
    Hero = 5,
}
