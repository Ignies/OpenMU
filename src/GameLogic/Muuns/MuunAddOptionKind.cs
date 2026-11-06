// <copyright file="MuunAddOptionKind.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Muuns;

/// <summary>
/// How the additional option of a Muun changes its option (MuunAddOption.xml <c>AddOptionType</c>).
/// </summary>
public enum MuunAddOptionKind : byte
{
    /// <summary>
    /// It doesn't change it.
    /// </summary>
    None = 0,

    /// <summary>
    /// It multiplies the value of the option.
    /// </summary>
    Multiplier = 1,

    /// <summary>
    /// It adds to the value of the option.
    /// </summary>
    Bonus = 2,

    /// <summary>
    /// It multiplies the value of the option, by 2 or 3 in the data.
    /// </summary>
    LowMultiplier = 3,
}
