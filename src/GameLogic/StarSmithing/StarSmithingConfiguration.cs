// <copyright file="StarSmithingConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The settings of Star Smithing (Readme.md).
/// </summary>
public class StarSmithingConfiguration
{
    /// <summary>Gets or sets the item group of the Star Smith's Hammer.</summary>
    public byte HammerGroup { get; set; } = 2;

    /// <summary>Gets or sets the item number of the Star Smith's Hammer.</summary>
    public short HammerNumber { get; set; } = 1000;

    /// <summary>Gets or sets the durability a smithing action takes from the hammer (a lock takes none).</summary>
    public byte DurabilityPerAction { get; set; } = 5;

    /// <summary>Gets or sets the Zen a recolour costs.</summary>
    public int RecolorZen { get; set; } = 500_000;

    /// <summary>Gets or sets the dyes a recolour spends (Readme.md "Dyes").</summary>
    public List<StarSmithDye> Dyes { get; set; } = DefaultDyes();

    /// <summary>Gets or sets the recipes the Constellation Forge can make (Readme.md "Recipes").</summary>
    public List<StarSmithRecipe> Recipes { get; set; } = DefaultRecipes();

    /// <summary>Gets or sets the balance tables: tiers, level curve, scaling, chance bonus and caps (Readme.md "PR 6 contract").</summary>
    public StarSmithBalance Balance { get; set; } = new();

    /// <summary>Gets or sets the Forge's dealing rules.</summary>
    public StarSmithForgeRules Forge { get; set; } = new();

    /// <summary>Gets or sets the time between the Forge's beats, in milliseconds.</summary>
    public int BeatIntervalMs { get; set; } = 800;

    /// <summary>Gets or sets how far from a beat a strike is still clean, in milliseconds.</summary>
    public int BeatToleranceMs { get; set; } = 90;

    /// <summary>Gets or sets the most round trip that widens the clean window, in milliseconds.</summary>
    public int RoundTripCapMs { get; set; } = 150;

    /// <summary>Gets or sets how long an attempt may go without a strike before it is abandoned, in seconds.</summary>
    public int ForgeIdleTimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// The default dyes, group 14 from number 1000: eight basic ones (four shades of their colour, strength up to 85),
    /// five rare ones (three special colours, full strength, the keep mask), the Prismatic Dye (the hue shift) and the
    /// Radiant Dye (the glow).
    /// </summary>
    /// <returns>The dyes.</returns>
    public static List<StarSmithDye> DefaultDyes()
    {
        StarSmithDye Basic(short number, params int[] swatches) => new() { Number = number, Swatches = [.. swatches], MaximumStrength = 85 };
        StarSmithDye Rare(short number, params int[] swatches) => new() { Number = number, Swatches = [.. swatches], MaximumStrength = 100, Keep = true };
        return
        [
            Basic(1000, 0xff8a8a, 0xe03a3a, 0xa81c1c, 0x5c0c0c), // Crimson
            Basic(1001, 0xffc28a, 0xf08a2a, 0xb85a10, 0x6a3006), // Amber
            Basic(1002, 0xfff0a0, 0xf2c94c, 0xc0952a, 0x6e5310), // Gold
            Basic(1003, 0xa8f0a0, 0x4cc24c, 0x237a2a, 0x0f3d14), // Verdant
            Basic(1004, 0x7fc4ff, 0x1e90ff, 0x1060c0, 0x0a3470), // Azure
            Basic(1005, 0xd4a8ff, 0x9a4cf2, 0x6420b0, 0x34105e), // Violet
            Basic(1006, 0xffffff, 0xece6d6, 0xc8bfa8, 0x9a907a), // Ivory
            Basic(1007, 0x8a8a8a, 0x4a4a4a, 0x262626, 0x0c0c0c), // Ebony
            Rare(1008, 0xff2a2a, 0x8a0018, 0x2a0006), // Bloodmoon
            Rare(1009, 0x9ff6ff, 0xff7a2a, 0x3a1cff), // Frostfire
            Rare(1010, 0xfff6d0, 0xb8d8ff, 0xffd86a), // Starlight
            Rare(1011, 0x2a1a4a, 0x0a3a3a, 0x050510), // Abyssal
            Rare(1012, 0x6a1ab8, 0xd4a017, 0x0f2a6a), // Royal
            new() { Number = 1013, Swatches = [0xff3b3b, 0xffd23b, 0x3bff6e, 0x3bb8ff, 0xb23bff, 0xff3bd2], MaximumStrength = 100, Hue = true, Keep = true }, // Prismatic
            new() { Number = 1014, Radiant = true, MaximumStrength = 0 }, // Radiant
        ];
    }

    /// <summary>
    /// The default recipes (Readme.md "Recipes"): the design's five weapon shapes (Hidden-Babylon docs/star_smithing.md
    /// section 10), two for armour, two for wings, one for pendants and one for rings, each with a hint and an outcome
    /// table. All take the same family of ingredients, any mix of the Season 6 skill orbs and scrolls, so one sky holds
    /// every shape of its item kind and exploring finds the hidden ones. A 60% base chance (95% at most); Storm Spine
    /// is the one risky recipe.
    /// </summary>
    /// <returns>The recipes.</returns>
    public static List<StarSmithRecipe> DefaultRecipes()
    {
        StarSmithRecipe Make(StarSmithTargetKind targets, short number, string name, string hint, int experience, int[] stars, StarSmithOutcome[] outcomes, StarSmithShapeMatch match = StarSmithShapeMatch.Exact, StarSmithOnFail onFail = StarSmithOnFail.IngredientsLost, bool risky = false) => new()
        {
            Number = number,
            Name = name,
            Hint = hint,
            HintKind = hint.Length == 0 ? StarSmithHintKind.None : StarSmithHintKind.Riddle,
            Experience = experience,
            Targets = targets,
            Ingredients = SkillOrbsAndScrolls(),
            BaseChance = 6000,
            MaximumChance = 9500,
            Shape = new StarSmithShape { Stars = [.. stars], Match = match },
            OnFail = onFail,
            Risky = risky,
            Outcomes = [.. outcomes],
        };

        const StarSmithTargetKind weapon = StarSmithTargetKind.Weapon;
        const StarSmithBoostStat dmg = StarSmithBoostStat.Damage, def = StarSmithBoostStat.Defense, hp = StarSmithBoostStat.MaxHealth, mana = StarSmithBoostStat.MaxMana;
        const StarSmithBoostStat speed = StarSmithBoostStat.AttackSpeed, crit = StarSmithBoostStat.CriticalRate, exc = StarSmithBoostStat.ExcellentRate, absorb = StarSmithBoostStat.DamageAbsorbed;
        var tempest = Make(StarSmithTargetKind.Wings, 9, "Tempest Pinions", string.Empty, 70, [11, 3, 9, 5, 8], [Out(50, StarSmithRarity.Common, (speed, 1, 2)), Out(30, StarSmithRarity.Rare, (speed, 2, 3), (dmg, 1, 2)), Out(10, StarSmithRarity.Legendary, (speed, 2, 4), (dmg, 1.5, 3), (mana, 50, 150))]);
        tempest.HintKind = StarSmithHintKind.Ingredient;
        tempest.HintGroup = 12;
        tempest.HintNumber = 7;
        return
        [
            Make(weapon, 1, "The Serpent", "Seven scales, from the point down to the hand.", 120, [0, 1, 2, 3, 4, 5, 6], [Out(50, StarSmithRarity.Common, (dmg, 1, 2)), Out(30, StarSmithRarity.Uncommon, (dmg, 1.5, 3), (crit, 0.5, 1)), Out(15, StarSmithRarity.Rare, (dmg, 2, 4), (exc, 1, 2)), Out(5, StarSmithRarity.Epic, (dmg, 3, 5), (crit, 1, 2), (speed, 2, 4))], onFail: StarSmithOnFail.ItemDowngraded),
            Make(weapon, 2, "Twin Fangs", "Two fangs bite across the guard.", 40, [9, 6, 10], [Out(60, StarSmithRarity.Common, (crit, 0.5, 1)), Out(30, StarSmithRarity.Uncommon, (crit, 1, 2), (dmg, 0.5, 1.5)), Out(10, StarSmithRarity.Epic, (crit, 1.5, 3), (dmg, 1, 2), (exc, 0.5, 1))], StarSmithShapeMatch.Extendable),
            Make(weapon, 3, "Ember Ring", "A ring of embers wraps the grip.", 40, [6, 7, 8], [Out(60, StarSmithRarity.Common, (speed, 1, 2)), Out(30, StarSmithRarity.Uncommon, (speed, 1, 3), (dmg, 0.5, 1.5)), Out(10, StarSmithRarity.Epic, (speed, 2, 4), (dmg, 1, 2), (mana, 30, 80))], StarSmithShapeMatch.Extendable),
            Make(weapon, 4, "Storm Spine", "Lightning climbs the blade in leaps.", 60, [6, 4, 2, 0], [Out(60, StarSmithRarity.Common, (dmg, 2, 3)), Out(30, StarSmithRarity.Rare, (dmg, 3, 5), (speed, 2, 3)), Out(10, StarSmithRarity.Legendary, (dmg, 4, 6), (speed, 3, 5), (crit, 1.5, 3))], onFail: StarSmithOnFail.ItemDestroyed, risky: true),
            Make(weapon, 5, "Wyrm Coil", "Four stars knot themselves around the blade.", 60, [1, 11, 3, 5], [Out(60, StarSmithRarity.Common, (exc, 0.5, 1)), Out(30, StarSmithRarity.Uncommon, (exc, 1, 2), (dmg, 0.5, 1)), Out(10, StarSmithRarity.Epic, (exc, 1.5, 3), (dmg, 1, 2), (crit, 0.5, 1.5))]),
            Make(StarSmithTargetKind.Armor, 6, "Aegis Weave", "Four plates lock over the heart.", 50, [3, 4, 8, 7], [Out(60, StarSmithRarity.Common, (def, 1, 2)), Out(30, StarSmithRarity.Uncommon, (def, 1.5, 3), (hp, 30, 80)), Out(10, StarSmithRarity.Epic, (def, 2, 4), (hp, 60, 150), (absorb, 0.5, 1.5))]),
            Make(StarSmithTargetKind.Armor, 7, "Bulwark Knot", "A knot of three holds where the blows land.", 40, [2, 9, 6], [Out(60, StarSmithRarity.Common, (absorb, 0.5, 1)), Out(30, StarSmithRarity.Uncommon, (absorb, 1, 2), (def, 0.5, 1.5)), Out(10, StarSmithRarity.Epic, (absorb, 1.5, 3), (def, 1, 2), (hp, 50, 120))], StarSmithShapeMatch.Extendable),
            Make(StarSmithTargetKind.Wings, 8, "Featherfall", "Four feathers drift and never touch the ground.", 60, [1, 10, 4, 7], [Out(60, StarSmithRarity.Common, (hp, 40, 100)), Out(30, StarSmithRarity.Uncommon, (hp, 60, 150), (absorb, 0.5, 1)), Out(10, StarSmithRarity.Epic, (hp, 100, 220), (absorb, 1, 2), (dmg, 1, 2))]),
            tempest,
            Make(StarSmithTargetKind.Pendant, 10, "Heart of Embers", string.Empty, 40, [6, 2, 10], [Out(60, StarSmithRarity.Common, (mana, 40, 100)), Out(30, StarSmithRarity.Uncommon, (mana, 60, 150), (dmg, 0.5, 1.5)), Out(10, StarSmithRarity.Epic, (mana, 100, 220), (dmg, 1, 2), (exc, 0.5, 1.5))], StarSmithShapeMatch.Extendable),
            Make(StarSmithTargetKind.Ring, 11, "Circle of Tides", "Three tides meet and never end.", 40, [0, 4, 8], [Out(60, StarSmithRarity.Common, (hp, 30, 80)), Out(30, StarSmithRarity.Uncommon, (hp, 50, 120), (mana, 30, 80)), Out(10, StarSmithRarity.Epic, (hp, 80, 180), (mana, 50, 120), (crit, 0.5, 1.5))], StarSmithShapeMatch.Extendable),
        ];
    }

    private static StarSmithOutcome Out(int weight, StarSmithRarity tier, params (StarSmithBoostStat Stat, double Minimum, double Maximum)[] boosts) => new()
    {
        Weight = weight,
        Tier = tier,
        Boosts = [.. boosts.Select(b => new StarSmithBoostRange { Stat = b.Stat, Minimum = b.Minimum, Maximum = b.Maximum })],
    };

    /// <summary>
    /// The Season 6 skill orbs (12/7-48) and scrolls (15/0-14), any mix up to ten of each.
    /// </summary>
    private static List<StarSmithIngredient> SkillOrbsAndScrolls()
    {
        short[] orbs = [7, 8, 9, 10, 11, 12, 13, 14, 16, 17, 18, 19, 21, 22, 23, 24, 35, 44, 45, 46, 47, 48];
        return
        [
            .. orbs.Select(n => new StarSmithIngredient { Group = 12, Number = n, Minimum = 0, Maximum = 10 }),
            .. Enumerable.Range(0, 15).Select(n => new StarSmithIngredient { Group = 15, Number = (short)n, Minimum = 0, Maximum = 10 }),
        ];
    }
}
