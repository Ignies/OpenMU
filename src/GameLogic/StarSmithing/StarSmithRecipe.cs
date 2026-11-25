// <copyright file="StarSmithRecipe.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// A Star Smithing recipe (Hidden-Babylon docs/star_smithing.md section 3): the items it works on, what it takes, its
/// chance. Every recipe is hidden until a player makes it once (section 9).
/// </summary>
public class StarSmithRecipe
{
    /// <summary>Gets or sets the recipe's number, its id in the Guidebook.</summary>
    public short Number { get; set; }

    /// <summary>Gets or sets its name, shown in the Guidebook once found.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the hint its sealed Guidebook row shows before it is found (a riddle or an ingredient clue; English, at most 64 bytes).</summary>
    public string Hint { get; set; } = string.Empty;

    /// <summary>Gets or sets the smith experience a success gives (a crack gives 30%, a first discovery doubles it).</summary>
    public int Experience { get; set; }

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

    /// <summary>Gets or sets its constellation's shape in the Forge (Readme.md "PR 7a contract"), or null when it has none.</summary>
    public StarSmithGeometry? Geometry { get; set; }

    /// <summary>Gets or sets the look its layer gives an item (none for the boost recipes).</summary>
    public StarSmithLook Look { get; set; }

    /// <summary>Gets or sets what a failure does to the item.</summary>
    public StarSmithOnFail OnFail { get; set; }

    /// <summary>Gets or sets a value indicating whether it is risky; only a risky recipe destroys an item.</summary>
    public bool Risky { get; set; }

    /// <summary>Gets or sets what its sealed Guidebook row shows: nothing, the riddle in <see cref="Hint"/>, or an ingredient.</summary>
    public StarSmithHintKind HintKind { get; set; } = StarSmithHintKind.Riddle;

    /// <summary>Gets or sets the item group of an ingredient hint.</summary>
    public byte HintGroup { get; set; }

    /// <summary>Gets or sets the item number of an ingredient hint.</summary>
    public short HintNumber { get; set; }

    /// <summary>Gets or sets the catalysts that add to its chance.</summary>
    public List<StarSmithCatalyst> Catalysts { get; set; } = [];

    /// <summary>Gets or sets its weighted outcome table; a seal rolls one.</summary>
    public List<StarSmithOutcome> Outcomes { get; set; } = [];
}
