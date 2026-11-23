// <copyright file="StarSmithBalance.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The balance tables (Readme.md "PR 6 contract"): the rarity tiers, the level curve, the scaling of a roll, the level's
/// chance bonus and the PvE and PvP caps. The code ceilings stop any cap, whatever the configuration says.
/// </summary>
public class StarSmithBalance
{
    /// <summary>Gets or sets the tiers, Common to Mythic.</summary>
    public List<StarSmithTier> Tiers { get; set; } = DefaultTiers();

    /// <summary>Gets or sets the experience curve: round(base * L^exponent) to the next level.</summary>
    public StarSmithCurve LevelCurve { get; set; } = new() { Base = 100, Exponent = 1.6 };

    /// <summary>Gets or sets the scaling of a roll by smith level: 1 + base * (L / 100)^exponent.</summary>
    public StarSmithCurve Scaling { get; set; } = new() { Base = 0.5, Exponent = 0.8 };

    /// <summary>Gets or sets the chance a smith level adds and the most it adds, in hundredths of a percent.</summary>
    public StarSmithChanceBonus Chance { get; set; } = new();

    /// <summary>Gets or sets the caps against monsters.</summary>
    public Dictionary<StarSmithBoostStat, StarSmithCap> Pve { get; set; } = DefaultCaps(pvp: false);

    /// <summary>Gets or sets the caps against players.</summary>
    public Dictionary<StarSmithBoostStat, StarSmithCap> Pvp { get; set; } = DefaultCaps(pvp: true);

    /// <summary>
    /// The code ceiling of a stat: no cap passes it.
    /// </summary>
    /// <param name="stat">The stat.</param>
    /// <returns>The ceiling, in the stat's unit.</returns>
    public static double Ceiling(StarSmithBoostStat stat) => stat switch
    {
        StarSmithBoostStat.Damage or StarSmithBoostStat.Defense or StarSmithBoostStat.AttackSpeed => 30,
        StarSmithBoostStat.MaxHealth or StarSmithBoostStat.MaxMana => 3000,
        StarSmithBoostStat.CriticalRate or StarSmithBoostStat.ExcellentRate => 20,
        StarSmithBoostStat.DamageAbsorbed => 25,
        _ => 0,
    };

    /// <summary>
    /// Whether a stat tells a player from a monster; one that cannot is capped by the lower of both tables.
    /// </summary>
    /// <param name="stat">The stat.</param>
    /// <returns>True for damage, defense and damage absorbed.</returns>
    public static bool HasPvpValue(StarSmithBoostStat stat)
        => stat is StarSmithBoostStat.Damage or StarSmithBoostStat.Defense or StarSmithBoostStat.DamageAbsorbed;

    /// <summary>
    /// The default caps.
    /// </summary>
    /// <param name="pvp">Whether for the PvP table.</param>
    /// <returns>The caps by stat.</returns>
    public static Dictionary<StarSmithBoostStat, StarSmithCap> DefaultCaps(bool pvp) => new()
    {
        [StarSmithBoostStat.Damage] = pvp ? new(3, 8) : new(6, 15),
        [StarSmithBoostStat.Defense] = pvp ? new(3, 8) : new(6, 15),
        [StarSmithBoostStat.MaxHealth] = new(300, 800),
        [StarSmithBoostStat.MaxMana] = new(300, 800),
        [StarSmithBoostStat.AttackSpeed] = new(5, 12),
        [StarSmithBoostStat.CriticalRate] = pvp ? new(2, 5) : new(3, 8),
        [StarSmithBoostStat.ExcellentRate] = pvp ? new(2, 5) : new(3, 8),
        [StarSmithBoostStat.DamageAbsorbed] = pvp ? new(2, 6) : new(4, 10),
    };

    /// <summary>
    /// The default tiers.
    /// </summary>
    /// <returns>The tiers, Common to Mythic.</returns>
    public static List<StarSmithTier> DefaultTiers() =>
    [
        new() { Name = "Common", Color = 0xffffff, EffectSlots = 0, BoostSlots = 1 },
        new() { Name = "Uncommon", Color = 0x3bd16f, EffectSlots = 0, BoostSlots = 2 },
        new() { Name = "Rare", Color = 0x3b8bff, EffectSlots = 1, BoostSlots = 2 },
        new() { Name = "Epic", Color = 0xa64dff, EffectSlots = 1, BoostSlots = 3 },
        new() { Name = "Legendary", Color = 0xff9a1f, EffectSlots = 2, BoostSlots = 3 },
        new() { Name = "Mythic", Color = 0xff3b3b, EffectSlots = 2, BoostSlots = 3 },
    ];

    /// <summary>
    /// The cap of a stat per item, clamped to the ceiling; a stat without a PvP value takes the lower of both tables.
    /// </summary>
    /// <param name="stat">The stat.</param>
    /// <param name="pvp">Whether against players.</param>
    /// <returns>The cap, in the stat's unit.</returns>
    public double PerItem(StarSmithBoostStat stat, bool pvp) => this.Cap(stat, pvp, c => c.PerItem);

    /// <summary>
    /// The cap of a stat across all worn items, clamped to the ceiling; a stat without a PvP value takes the lower of
    /// both tables.
    /// </summary>
    /// <param name="stat">The stat.</param>
    /// <param name="pvp">Whether against players.</param>
    /// <returns>The cap, in the stat's unit.</returns>
    public double Total(StarSmithBoostStat stat, bool pvp) => this.Cap(stat, pvp, c => c.Total);

    /// <summary>
    /// The boost slots of a rarity tier.
    /// </summary>
    /// <param name="rarity">The rarity.</param>
    /// <returns>The slots, 0 to 3.</returns>
    public int BoostSlots(StarSmithRarity rarity)
        => Math.Clamp(this.Tiers.ElementAtOrDefault((int)rarity)?.BoostSlots ?? 0, 0, StarSmithBoosts.Maximum);

    private double Cap(StarSmithBoostStat stat, bool pvp, Func<StarSmithCap, double> pick)
    {
        double Of(Dictionary<StarSmithBoostStat, StarSmithCap>? table) => table is not null && table.TryGetValue(stat, out var cap) ? pick(cap) : 0;
        var value = HasPvpValue(stat) ? Of(pvp ? this.Pvp : this.Pve) : Math.Min(Of(this.Pve), Of(this.Pvp));
        return Math.Clamp(value, 0, Ceiling(stat));
    }
}
