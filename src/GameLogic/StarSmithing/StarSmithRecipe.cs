// <copyright file="StarSmithRecipe.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The kinds of item a recipe works on.
/// </summary>
[Flags]
public enum StarSmithTargetKind
{
    /// <summary>No kind.</summary>
    None = 0,

    /// <summary>A weapon, staff or book.</summary>
    Weapon = 1,

    /// <summary>A helm, armour, pants, gloves, boots or shield.</summary>
    Armor = 2,

    /// <summary>Wings and capes.</summary>
    Wings = 4,

    /// <summary>A pendant.</summary>
    Pendant = 8,

    /// <summary>A ring.</summary>
    Ring = 16,
}

/// <summary>
/// The item an attempt works on, as a recipe's target rule reads it.
/// </summary>
/// <param name="Kind">Its kind.</param>
/// <param name="Level">Its level, +0 to +15.</param>
/// <param name="Excellent">Whether it has an excellent option.</param>
public readonly record struct StarSmithTarget(StarSmithTargetKind Kind, int Level, bool Excellent);

/// <summary>
/// One ingredient of a recipe: an item and how many of it the recipe takes.
/// </summary>
public class StarSmithIngredient
{
    /// <summary>Gets or sets the item group.</summary>
    public byte Group { get; set; }

    /// <summary>Gets or sets the item number.</summary>
    public short Number { get; set; }

    /// <summary>Gets or sets the fewest the recipe takes.</summary>
    public byte Minimum { get; set; } = 1;

    /// <summary>Gets or sets the most the recipe takes.</summary>
    public byte Maximum { get; set; } = 1;
}

/// <summary>
/// A Star Smithing recipe (Hidden-Babylon docs/star_smithing.md section 3): the items it works on, what it takes, its
/// chance. Every recipe is hidden until a player makes it once (section 9).
/// </summary>
public class StarSmithRecipe
{
    /// <summary>Gets or sets the recipe's number, its id in the Guidebook.</summary>
    public short Number { get; set; }

    /// <summary>Gets or sets the kinds of item it works on.</summary>
    public StarSmithTargetKind Targets { get; set; }

    /// <summary>Gets or sets the lowest item level it works on.</summary>
    public byte MinimumItemLevel { get; set; }

    /// <summary>Gets or sets whether the item must be excellent (true), must not be (false), or either (null).</summary>
    public bool? Excellent { get; set; }

    /// <summary>Gets or sets what it takes.</summary>
    public List<StarSmithIngredient> Ingredients { get; set; } = [];

    /// <summary>Gets or sets its base chance, in hundredths of a percent (0 to 10,000).</summary>
    public int BaseChance { get; set; }

    /// <summary>Gets or sets the highest chance it reaches, in hundredths of a percent.</summary>
    public int MaximumChance { get; set; } = 10_000;

    /// <summary>Gets or sets its constellation in the Forge (section 10), or null when it has none.</summary>
    public StarSmithShape? Shape { get; set; }

    /// <summary>Gets or sets what a failure does to the item.</summary>
    public StarSmithOnFail OnFail { get; set; }

    /// <summary>Gets or sets whether it is risky; only a risky recipe destroys an item.</summary>
    public bool Risky { get; set; }
}

/// <summary>
/// What a failed attempt does (section 3, step 5).
/// </summary>
public enum StarSmithOnFail
{
    /// <summary>The ingredients are lost, the item stays as it was.</summary>
    IngredientsLost,

    /// <summary>The item's customisation is cleared; the item stays.</summary>
    ItemDowngraded,

    /// <summary>The item is destroyed (only on a risky recipe without protection).</summary>
    ItemDestroyed,
}
