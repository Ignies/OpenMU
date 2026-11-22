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
    /// The default recipes, the design's five weapon shapes (Hidden-Babylon docs/star_smithing.md section 10, the
    /// demo's Guidebook): The Serpent, Twin Fangs, Ember Ring, Storm Spine and Wyrm Coil. All take the same family of
    /// ingredients, any mix of the Season 6 skill orbs and scrolls, so one sky holds every shape and exploring finds
    /// the hidden ones. A 60% base chance (95% at most); Storm Spine is the one risky recipe.
    /// </summary>
    /// <returns>The recipes.</returns>
    public static List<StarSmithRecipe> DefaultRecipes()
    {
        StarSmithRecipe Weapon(short number, string name, string hint, int experience, int[] stars, StarSmithShapeMatch match = StarSmithShapeMatch.Exact, StarSmithOnFail onFail = StarSmithOnFail.IngredientsLost, bool risky = false) => new()
        {
            Number = number,
            Name = name,
            Hint = hint,
            Experience = experience,
            Targets = StarSmithTargetKind.Weapon,
            Ingredients = SkillOrbsAndScrolls(),
            BaseChance = 6000,
            MaximumChance = 9500,
            Shape = new StarSmithShape { Stars = [.. stars], Match = match },
            OnFail = onFail,
            Risky = risky,
        };

        return
        [
            Weapon(1, "The Serpent", "Seven scales, from the point down to the hand.", 120, [0, 1, 2, 3, 4, 5, 6], onFail: StarSmithOnFail.ItemDowngraded),
            Weapon(2, "Twin Fangs", "Two fangs bite across the guard.", 40, [9, 6, 10], StarSmithShapeMatch.Extendable),
            Weapon(3, "Ember Ring", "A ring of embers wraps the grip.", 40, [6, 7, 8], StarSmithShapeMatch.Extendable),
            Weapon(4, "Storm Spine", "Lightning climbs the blade in leaps.", 60, [6, 4, 2, 0], onFail: StarSmithOnFail.ItemDestroyed, risky: true),
            Weapon(5, "Wyrm Coil", "Four stars knot themselves around the blade.", 60, [1, 11, 3, 5]),
        ];
    }

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
