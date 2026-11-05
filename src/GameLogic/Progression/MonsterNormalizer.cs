// <copyright file="MonsterNormalizer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Progression;

using Stat = MUnique.OpenMU.GameLogic.Progression.Season6MonsterCurve.Stat;

/// <summary>
/// Re-derives the numbers of the monsters of a Season 21 map set on the Season 6 curve.
/// </summary>
/// <remarks>
/// The set's anchor level M comes from its power level (<see cref="ProgressionMapping.GetMonsterAnchorLevel"/>). Its
/// reference row is the median of its spawned normal monsters, per column. Each column of a monster is then the
/// Season 6 curve at M times the monster's own value over the reference, clamped by kind. So a monster keeps its relative
/// strength inside its map, the scale comes from Season 6, and placeholder rows of the source data (flat health rows,
/// elites identical to their normals, 8 to 30 million health bosses) land inside the Season 6 envelope.
/// Experience and money per kill follow from the level (the experience formula of the game logic, money = experience + 7),
/// so normalizing the level normalizes both.
/// </remarks>
public static class MonsterNormalizer
{
    /// <summary>
    /// The power level from which <see cref="ProgressionConfiguration.OverCapStrengthPercent"/> starts to scale.
    /// </summary>
    internal const double OverCapStart = 400;

    /// <summary>
    /// The power level at which <see cref="ProgressionConfiguration.OverCapStrengthPercent"/> is reached.
    /// </summary>
    internal const double OverCapEnd = 600;

    /// <summary>
    /// The health above which normalized health is rounded to hundreds instead of tens.
    /// </summary>
    private const double HealthRoundingThreshold = 5000;

    /// <summary>
    /// The map bosses of the Season 21 data: Lord Silvester, Core Magriffy and Nix, single spawns with 8 to 30 million health.
    /// </summary>
    private static readonly HashSet<short> BossNumbers = [673, 716, 746];

    private static readonly Limits NormalLimits = new(new(0.5, 2.0), new(0.6, 1.6), new(0.6, 1.6), new(0.6, 1.6), new(0.9, 1.1));

    /// <summary>
    /// Gets the kind of a monster.
    /// </summary>
    /// <param name="number">The monster number.</param>
    /// <param name="designation">The designation.</param>
    /// <returns>The kind.</returns>
    public static MonsterKind GetKind(short number, string designation)
    {
        if (BossNumbers.Contains(number))
        {
            return MonsterKind.Boss;
        }

        return designation.Contains("Elite", StringComparison.OrdinalIgnoreCase) ? MonsterKind.Elite : MonsterKind.Normal;
    }

    /// <summary>
    /// Normalizes the monsters of a set.
    /// </summary>
    /// <param name="season21Requirement">The Season 21 entry level of the set.</param>
    /// <param name="monsters">The monsters of the set.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The rows, in the order of <paramref name="monsters"/>; the source rows in the original mode.</returns>
    public static IReadOnlyList<MonsterStatRow> Normalize(int season21Requirement, IReadOnlyList<SetMonster> monsters, ProgressionConfiguration configuration)
    {
        if (configuration.Mode == ProgressionMode.Original)
        {
            return monsters.Select(m => m.Source).ToList();
        }

        var powerLevel = ProgressionMapping.GetPowerLevel(season21Requirement, configuration);
        var anchor = ProgressionMapping.GetMonsterAnchorLevel(powerLevel);
        var reference = GetReference(monsters);
        var strength = GetOverCapFactor(powerLevel, configuration);
        var healthFactor = strength * Percent(configuration.MonsterHealthPercent);
        var damageFactor = strength * Percent(configuration.MonsterDamagePercent);
        var defenseFactor = Percent(configuration.MonsterDefensePercent);
        var curveHealth = Season6MonsterCurve.GetValue(Stat.Health, anchor) * healthFactor;
        var curveDamage = Season6MonsterCurve.GetValue(Stat.Damage, anchor) * damageFactor;
        var curveDefense = Season6MonsterCurve.GetValue(Stat.Defense, anchor) * defenseFactor;
        var curveAttackRate = Season6MonsterCurve.GetValue(Stat.AttackRate, anchor) * defenseFactor;
        var curveDefenseRate = Season6MonsterCurve.GetValue(Stat.DefenseRate, anchor) * defenseFactor;

        var result = new List<MonsterStatRow>(monsters.Count);
        foreach (var monster in monsters)
        {
            var limits = GetLimits(GetKind(monster.Number, monster.Designation), configuration);
            var source = monster.Source;
            var averageDamage = (source.DamageMin + source.DamageMax) / 2.0;
            var damage = curveDamage * limits.Damage.Apply(averageDamage / reference.Damage);
            var spread = Math.Clamp(averageDamage > 0 ? Math.Abs(source.DamageMax - source.DamageMin) / averageDamage : 0, 0.04, 0.2);

            result.Add(new MonsterStatRow(
                Round(anchor * limits.Level.Apply(source.Level / reference.Level)),
                RoundHealth(curveHealth * limits.Health.Apply(source.Health / reference.Health)),
                Round(damage * (1 - (spread / 2))),
                Round(damage * (1 + (spread / 2))),
                Round(curveDefense * limits.Defense.Apply(source.Defense / reference.Defense)),
                Round(curveAttackRate * limits.Rates.Apply(source.AttackRate / reference.AttackRate)),
                Round(curveDefenseRate * limits.Rates.Apply(source.DefenseRate / reference.DefenseRate))));
        }

        return result;
    }

    private static Reference GetReference(IReadOnlyList<SetMonster> monsters)
    {
        var normals = monsters.Where(m => GetKind(m.Number, m.Designation) == MonsterKind.Normal).ToList();
        var pool = normals.Where(m => m.Spawned).ToList();
        if (pool.Count == 0)
        {
            pool = normals.Count > 0 ? normals : monsters.ToList();
        }

        return new Reference(
            Median(pool, r => r.Level),
            Median(pool, r => r.Health),
            Median(pool, r => (r.DamageMin + r.DamageMax) / 2.0),
            Median(pool, r => r.Defense),
            Median(pool, r => r.AttackRate),
            Median(pool, r => r.DefenseRate));
    }

    private static double Median(List<SetMonster> pool, Func<MonsterStatRow, double> selector)
    {
        var values = pool.Select(m => selector(m.Source)).Order().ToList();
        if (values.Count == 0)
        {
            return 1;
        }

        var middle = values.Count / 2;
        var median = values.Count % 2 == 1 ? values[middle] : (values[middle - 1] + values[middle]) / 2;
        return median > 0 ? median : 1;
    }

    private static Limits GetLimits(MonsterKind kind, ProgressionConfiguration configuration)
    {
        switch (kind)
        {
            case MonsterKind.Elite:
                var healthMin = Math.Clamp(configuration.EliteHealthMinimum, 1, 5);
                var damageMin = Math.Clamp(configuration.EliteDamageMinimum, 1, 5);
                return new(
                    new(healthMin, Math.Clamp(configuration.EliteHealthMaximum, healthMin, 5)),
                    new(damageMin, Math.Clamp(configuration.EliteDamageMaximum, damageMin, 5)),
                    new(1.1, 1.6),
                    new(1.1, 1.6),
                    new(1.08, 1.2));
            case MonsterKind.Boss:
                var health = Math.Clamp(configuration.BossHealthMultiplier, 5, 40);
                var damage = Math.Clamp(configuration.BossDamageMultiplier, 1, 3);
                return new(new(health, health), new(damage, damage), new(1.5, 1.5), new(1.3, 1.3), new(1.2, 1.2));
            default:
                return NormalLimits;
        }
    }

    private static double GetOverCapFactor(double powerLevel, ProgressionConfiguration configuration)
    {
        if (configuration.Mode != ProgressionMode.Normalized || powerLevel <= OverCapStart)
        {
            return 1;
        }

        var target = Math.Clamp(configuration.OverCapStrengthPercent, 50, 100) / 100.0;
        var progress = Math.Min(1, (powerLevel - OverCapStart) / (OverCapEnd - OverCapStart));
        return 1 - ((1 - target) * progress);
    }

    private static double Percent(int percent) => Math.Clamp(percent, 25, 400) / 100.0;

    private static int Round(double value) => (int)Math.Round(value);

    private static int RoundHealth(double health)
    {
        var step = health > HealthRoundingThreshold ? 100 : 10;
        return (int)Math.Round(health / step) * step;
    }

    /// <summary>
    /// The median row of a set's normal monsters, see <see cref="GetReference"/>.
    /// </summary>
    private readonly record struct Reference(double Level, double Health, double Damage, double Defense, double AttackRate, double DefenseRate);

    /// <summary>
    /// The factors a monster may stray from its map's reference row.
    /// </summary>
    private readonly record struct Limit(double Min, double Max)
    {
        public double Apply(double ratio) => Math.Clamp(ratio, this.Min, this.Max);
    }

    private readonly record struct Limits(Limit Health, Limit Damage, Limit Defense, Limit Rates, Limit Level);
}
