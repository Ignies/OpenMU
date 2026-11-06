// <copyright file="HelperPlusFlags.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.HelperPlus;

/// <summary>
/// The flags of the helper plus rules, as the Babylon client reads them.
/// </summary>
[Flags]
public enum HelperPlusFlags : byte
{
    /// <summary>
    /// Nothing.
    /// </summary>
    None = 0,

    /// <summary>
    /// The map has hunting zones of the plus mode.
    /// </summary>
    MapHasZones = 1,

    /// <summary>
    /// The character may use the plus mode.
    /// </summary>
    MayUsePlus = 2,

    /// <summary>
    /// The plus mode is on.
    /// </summary>
    PlusOn = 4,
}
