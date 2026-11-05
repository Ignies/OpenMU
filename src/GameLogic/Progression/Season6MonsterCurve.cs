// <copyright file="Season6MonsterCurve.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// The stat curve of the Season 6 normal monsters, by monster level.
/// </summary>
/// <remarks>
/// Data: the 108 normal monsters which spawn on Season 6 hunting maps, levels 14 to 148 (the Version075, Version095d and
/// SeasonSix map initializers; event maps, guards, traps and the Vulcanus heroes left out). Each point is the median
/// level and the median value of one 10-level bin, made monotone; between the points the curve is log-linear. Outside
/// the points it continues with the single power fit of levels 14 to 148 (health ~ L^3.10, damage ~ L^1.56,
/// defense ~ L^1.88, attack rate ~ L^1.51, defense rate ~ L^1.86).
/// Checks: level 140 gives 240,336 health, 2,041 damage, 776 defense; Raklion's Dark Mammoth (level 140) has 237,000,
/// 1,741-2,317 and 785.
/// </remarks>
public static class Season6MonsterCurve
{
    private static readonly double[] BinLevels = [16.5, 24.5, 35, 45, 54, 64, 74.5, 83.5, 95.5, 104, 114.5, 123, 133.5, 143];

    private static readonly double[] Health = [370, 725, 1150, 2500, 4000, 6000, 11250, 22000, 39000, 58000, 85250, 184370, 224000, 248000];

    private static readonly double[] Damage = [56.75, 90, 120, 142.5, 165, 205, 271.25, 355, 504.75, 622.5, 753.75, 987.5, 1870, 2122.5];

    private static readonly double[] Defense = [17, 30, 47.5, 70, 95, 130, 196, 262.5, 372.5, 495, 567.5, 620, 685, 820];

    private static readonly double[] AttackRate = [77, 122.5, 175, 225, 270, 320, 387.5, 503.5, 722.5, 915, 1080, 1265, 1746.5, 2240];

    private static readonly double[] DefenseRate = [17, 30, 42, 58, 80, 94, 126, 172.5, 232.5, 295, 435, 755, 889, 1430];

    /// <summary>
    /// The stats of the curve.
    /// </summary>
    public enum Stat
    {
        /// <summary>
        /// The maximum health.
        /// </summary>
        Health,

        /// <summary>
        /// The average of minimum and maximum damage.
        /// </summary>
        Damage,

        /// <summary>
        /// The defense.
        /// </summary>
        Defense,

        /// <summary>
        /// The attack rate.
        /// </summary>
        AttackRate,

        /// <summary>
        /// The defense rate.
        /// </summary>
        DefenseRate,
    }

    /// <summary>
    /// Gets the value of a stat at a monster level.
    /// </summary>
    /// <param name="stat">The stat.</param>
    /// <param name="level">The monster level.</param>
    /// <returns>The typical Season 6 value.</returns>
    public static double GetValue(Stat stat, double level)
    {
        var (values, exponent) = stat switch
        {
            Stat.Health => (Health, 3.1027),
            Stat.Damage => (Damage, 1.5649),
            Stat.Defense => (Defense, 1.8759),
            Stat.AttackRate => (AttackRate, 1.5110),
            _ => (DefenseRate, 1.8617),
        };

        var last = BinLevels.Length - 1;
        if (level <= BinLevels[0])
        {
            return values[0] * Math.Pow(level / BinLevels[0], exponent);
        }

        if (level >= BinLevels[last])
        {
            return values[last] * Math.Pow(level / BinLevels[last], exponent);
        }

        var upper = 1;
        while (BinLevels[upper] < level)
        {
            upper++;
        }

        var lower = upper - 1;
        var t = (Math.Log(level) - Math.Log(BinLevels[lower])) / (Math.Log(BinLevels[upper]) - Math.Log(BinLevels[lower]));
        return Math.Exp(Math.Log(values[lower]) + (t * (Math.Log(values[upper]) - Math.Log(values[lower]))));
    }
}
