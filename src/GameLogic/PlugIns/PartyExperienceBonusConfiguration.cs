// <copyright file="PartyExperienceBonusConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns;

/// <summary>
/// The configuration of the <see cref="PartyExperienceBonusPlugIn"/>: the factor by which a party multiplies the
/// experience of a kill, by its number of members.
/// </summary>
public class PartyExperienceBonusConfiguration
{
    /// <summary>
    /// Gets or sets the bonus by the number of members (index 0 for one member); a larger party takes the last one.
    /// </summary>
    public List<double> Bonus { get; set; } = [1, 1.05, 1.1025, 1.157625, 1.21550625];

    /// <summary>
    /// Gets or sets the bonus of a party with at least <see cref="MixedClassMinimum"/> different classes.
    /// </summary>
    public List<double> MixedClassBonus { get; set; } = [1, 1.05, 1.1025, 1.157625, 1.21550625];

    /// <summary>
    /// Gets or sets the number of different classes (by their first class) from which the mixed class bonus applies.
    /// </summary>
    public int MixedClassMinimum { get; set; } = 3;

    /// <summary>
    /// Creates the configuration of the Season 21 renewal bonus, as the Season 21 server data defines it
    /// (PartyGeneralRenewalExperience and PartySpecialRenewalExperience).
    /// </summary>
    /// <returns>The configuration.</returns>
    public static PartyExperienceBonusConfiguration CreateSeason21() => new()
    {
        Bonus = [1.0, 1.3, 1.6, 1.9, 3.2],
        MixedClassBonus = [1.5, 2.5, 4.7, 5.5, 6.4],
    };

    /// <summary>
    /// Gets the bonus of a party.
    /// </summary>
    /// <param name="memberCount">The number of members which get experience.</param>
    /// <param name="classCount">The number of their different classes.</param>
    /// <returns>The factor.</returns>
    public double GetBonus(int memberCount, int classCount)
    {
        var table = classCount >= this.MixedClassMinimum && this.MixedClassBonus.Count > 0 ? this.MixedClassBonus : this.Bonus;
        if (table.Count == 0 || memberCount <= 0)
        {
            return 1;
        }

        return table[Math.Min(memberCount, table.Count) - 1];
    }
}
