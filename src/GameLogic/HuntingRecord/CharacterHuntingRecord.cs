// <copyright file="CharacterHuntingRecord.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.HuntingRecord;

/// <summary>
/// The hunting record of a character, with what the rankings show of it.
/// </summary>
public class CharacterHuntingRecord
{
    /// <summary>
    /// Gets or sets the name of the character.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of the character class.
    /// </summary>
    public byte ClassNumber { get; set; }

    /// <summary>
    /// Gets or sets the total level (level and master level) when the record last changed.
    /// </summary>
    public int TotalLevel { get; set; }

    /// <summary>
    /// Gets or sets the days, one per map and day.
    /// </summary>
    public List<HuntingDay> Days { get; set; } = [];
}
