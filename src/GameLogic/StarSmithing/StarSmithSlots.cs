// <copyright file="StarSmithSlots.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// How many constellation layers a smith may put on an item, by level (design section 13).
/// </summary>
public static class StarSmithSlots
{
    /// <summary>The most layers an item holds.</summary>
    public const int Maximum = 4;

    /// <summary>
    /// The slots of a smith level: 1 up to 20, 2 from 21, 3 from 50, 4 from 80.
    /// </summary>
    /// <param name="level">The smith's level.</param>
    /// <returns>The slots.</returns>
    public static int For(int level) => level switch
    {
        <= 20 => 1,
        < 50 => 2,
        < 80 => 3,
        _ => Maximum,
    };
}
