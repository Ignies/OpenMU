// <copyright file="HuntingDay.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.HuntingRecord;

/// <summary>
/// The hunting of a character on one map on one day (UTC).
/// </summary>
public class HuntingDay
{
    /// <summary>
    /// Gets or sets the map number.
    /// </summary>
    public short Map { get; set; }

    /// <summary>
    /// Gets or sets the day, at 00:00 UTC.
    /// </summary>
    public DateTime Day { get; set; }

    /// <summary>
    /// Gets or sets the seconds spent on the map.
    /// </summary>
    public long Seconds { get; set; }

    /// <summary>
    /// Gets or sets the monsters killed.
    /// </summary>
    public long Kills { get; set; }

    /// <summary>
    /// Gets or sets the experience gained by kills, normal and master.
    /// </summary>
    public long Experience { get; set; }

    /// <summary>
    /// Gets or sets the damage dealt to monsters.
    /// </summary>
    public long Damage { get; set; }
}
