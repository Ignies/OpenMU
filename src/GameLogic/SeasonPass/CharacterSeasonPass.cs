// <copyright file="CharacterSeasonPass.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.SeasonPass;

/// <summary>
/// The season pass of a character in a season.
/// </summary>
public class CharacterSeasonPass
{
    /// <summary>
    /// Gets or sets the start of the season the state belongs to.
    /// </summary>
    public DateTime SeasonStart { get; set; }

    /// <summary>
    /// Gets or sets the pass experience.
    /// </summary>
    public int Experience { get; set; }

    /// <summary>
    /// Gets or sets the owned passes, one bit per <see cref="SeasonPassKind"/>.
    /// </summary>
    public byte Passes { get; set; } = 1;

    /// <summary>
    /// Gets or sets the claimed levels per pass, one bit per level (bit 0 = level 1).
    /// </summary>
    public uint[] Claimed { get; set; } = new uint[3];

    /// <summary>
    /// Gets or sets the day the daily missions count for.
    /// </summary>
    public DateTime Day { get; set; }

    /// <summary>
    /// Gets or sets the progress of the missions, by index; special missions have the key 128 + index.
    /// </summary>
    public Dictionary<int, SeasonPassMissionState> Missions { get; set; } = [];

    /// <summary>
    /// Gets or sets the claimed buffs: the effect index and the highest level of a claim.
    /// </summary>
    public Dictionary<int, int> Buffs { get; set; } = [];
}
