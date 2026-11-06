// <copyright file="MuunGrade.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Muuns;

/// <summary>
/// The grade of a Muun (MuunInfo.xml <c>SubType</c>).
/// </summary>
public enum MuunGrade : byte
{
    /// <summary>
    /// A normal Muun.
    /// </summary>
    Normal = 0,

    /// <summary>
    /// A premium Muun.
    /// </summary>
    Premium = 1,

    /// <summary>
    /// A tempest Muun, which evolves with the tempest evolution stone.
    /// </summary>
    Tempest = 2,
}
