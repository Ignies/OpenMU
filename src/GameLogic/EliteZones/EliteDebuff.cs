// <copyright file="EliteDebuff.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EliteZones;

/// <summary>
/// A debuff which the elites of a zone element put on the characters they hit (S21Emu <c>Monster/EliteMonster.txt</c>).
/// </summary>
public class EliteDebuff
{
    /// <summary>
    /// Gets or sets the element of the zones whose elites use it.
    /// </summary>
    public EliteZoneElement Element { get; set; }

    /// <summary>
    /// Gets or sets the kind.
    /// </summary>
    public EliteDebuffKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the value: bleeding the percent of the hit's damage a second, poisoning the lowest percent of the
    /// maximum health a second, the decreases their percent (attack speed: points).
    /// </summary>
    [Range(0, 1000)]
    public int Value { get; set; }

    /// <summary>
    /// Gets or sets the limit: bleeding the most damage a second (0 none), poisoning the highest percent.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int MaximumValue { get; set; }

    /// <summary>
    /// Gets or sets the duration in seconds.
    /// </summary>
    [Range(1, 600)]
    public int Seconds { get; set; } = 10;

    /// <summary>
    /// Gets or sets the chance of a hit to put it on, in percent.
    /// </summary>
    [Range(0, 100)]
    public int ChancePercent { get; set; } = 100;
}
