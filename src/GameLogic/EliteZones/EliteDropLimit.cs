// <copyright file="EliteDropLimit.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EliteZones;

/// <summary>
/// How many of an item a character may get from monster drops per week (S21Emu <c>Item/ItemDropLimit.xml</c>).
/// </summary>
public class EliteDropLimit
{
    /// <summary>
    /// Gets or sets the item group.
    /// </summary>
    public byte ItemGroup { get; set; }

    /// <summary>
    /// Gets or sets the item number.
    /// </summary>
    public short ItemNumber { get; set; }

    /// <summary>
    /// Gets or sets the most drops a character gets each week.
    /// </summary>
    [Range(0, 100000)]
    public int MaximumPerWeek { get; set; }
}
