// <copyright file="StarSmithForgeResult.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// What a seal did to a layer (EE 9F).
/// </summary>
public enum StarSmithLayerChange : byte
{
    /// <summary>A new layer.</summary>
    Added,

    /// <summary>A reseal raised the layer's tier and took the new hull.</summary>
    Raised,

    /// <summary>A reseal that was not higher kept the layer as it was.</summary>
    Kept,
}

/// <summary>
/// One sealed figure.
/// </summary>
/// <param name="Recipe">The constellation.</param>
/// <param name="Tier">The figure's own tier.</param>
/// <param name="Score">The figure's score.</param>
/// <param name="Change">What it did to the item's layers.</param>
/// <param name="LayerIndex">The layer's index on the item.</param>
public sealed record StarSmithSealedFigure(StarSmithRecipe Recipe, StarSmithSignTier Tier, int Score, StarSmithLayerChange Change, int LayerIndex);

/// <summary>
/// The end of a Forge attempt (Readme.md "PR 7a contract", "Stash, seal and slots").
/// </summary>
/// <param name="State">Sealed, cracked or abandoned.</param>
/// <param name="Figures">The sealed figures, in the order banked.</param>
/// <param name="Lost">The banked figures a crack or an abandon lost.</param>
/// <param name="FateRecipe">On a crack, the constellation whose onFail decided; else null.</param>
/// <param name="Fail">What happens to the item when cracked or abandoned; null when sealed.</param>
/// <param name="Placements">All accepted placements.</param>
/// <param name="CleanPlacements">The clean ones.</param>
/// <param name="Attunement">Whether the seal is the attunement (nothing banked).</param>
/// <param name="Rarity">The item's rarity after a seal: the best of the seal and every layer.</param>
/// <param name="Layers">The item's layers after a seal.</param>
/// <param name="BoostRecipe">The sealed constellation with an outcome table that rolls the boosts, or null.</param>
/// <param name="BoostTier">Its tier.</param>
public sealed record StarSmithForgeResult(
    StarSmithForgeState State,
    IReadOnlyList<StarSmithSealedFigure> Figures,
    int Lost,
    StarSmithRecipe? FateRecipe,
    StarSmithOnFail? Fail,
    int Placements,
    int CleanPlacements,
    bool Attunement,
    StarSmithRarity Rarity,
    IReadOnlyList<StarSmithLayer> Layers,
    StarSmithRecipe? BoostRecipe,
    StarSmithSignTier BoostTier);
