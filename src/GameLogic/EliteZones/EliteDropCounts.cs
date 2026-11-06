// <copyright file="EliteDropCounts.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EliteZones;

/// <summary>
/// The drops of the limited items in the current week, per character and item (kept in the state file).
/// </summary>
public sealed class EliteDropCounts
{
    /// <summary>
    /// Gets or sets the start day of the week the counts belong to.
    /// </summary>
    public DateTime WeekStart { get; set; }

    /// <summary>
    /// Gets or sets the counts by "character id:group:number".
    /// </summary>
    public Dictionary<string, int> Counts { get; set; } = [];
}
