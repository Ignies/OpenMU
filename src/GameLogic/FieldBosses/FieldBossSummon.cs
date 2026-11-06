// <copyright file="FieldBossSummon.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.FieldBosses;

/// <summary>
/// Monsters which spawn with a stage or a health phase of a field boss.
/// </summary>
public class FieldBossSummon
{
    /// <summary>
    /// Gets or sets the monster number.
    /// </summary>
    public short MonsterNumber { get; set; }

    /// <summary>
    /// Gets or sets how many spawn.
    /// </summary>
    [Range(1, 100)]
    public int Count { get; set; } = 1;

    /// <summary>
    /// Gets or sets the lowest x coordinate of the area they spawn in.
    /// </summary>
    public byte X1 { get; set; }

    /// <summary>
    /// Gets or sets the lowest y coordinate of the area they spawn in.
    /// </summary>
    public byte Y1 { get; set; }

    /// <summary>
    /// Gets or sets the highest x coordinate of the area they spawn in.
    /// </summary>
    public byte X2 { get; set; }

    /// <summary>
    /// Gets or sets the highest y coordinate of the area they spawn in.
    /// </summary>
    public byte Y2 { get; set; }
}
