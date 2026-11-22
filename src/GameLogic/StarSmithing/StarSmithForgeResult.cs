// <copyright file="StarSmithForgeResult.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The result of a Constellation Forge attempt (section 10, rule 5).
/// </summary>
/// <param name="State">Sealed or cracked.</param>
/// <param name="Recipe">The recipe the shape makes (or, cracked, the one it was on); null for the attunement.</param>
/// <param name="Path">The struck stars, in order.</param>
/// <param name="ElementMix">The struck stars' values summed by element, the effect's flavour.</param>
/// <param name="Dominant">The element with the most value (the lower element on a tie), or null when nothing is mixed.</param>
/// <param name="CleanStrikes">How many strikes were clean.</param>
/// <param name="Rarity">The rarity tier (Common when cracked).</param>
/// <param name="Extension">The links past an extendable shape that make the effect bigger.</param>
/// <param name="MagnitudeFactor">What a rolled magnitude is multiplied by: the extension and the smith level (0 when cracked).</param>
/// <param name="FirstCompletion">Whether this is the smith's first completion of the recipe, for the Guidebook.</param>
/// <param name="Fail">What happens to the item when cracked, by the recipe's onFail; null when sealed.</param>
public sealed record StarSmithForgeResult(
    StarSmithForgeState State,
    StarSmithRecipe? Recipe,
    IReadOnlyList<int> Path,
    IReadOnlyDictionary<StarSmithElement, int> ElementMix,
    StarSmithElement? Dominant,
    int CleanStrikes,
    StarSmithRarity Rarity,
    int Extension,
    double MagnitudeFactor,
    bool FirstCompletion,
    StarSmithOnFail? Fail)
{
    /// <summary>
    /// Scales a magnitude rolled from the recipe's outcome.
    /// </summary>
    /// <param name="rolled">The rolled magnitude.</param>
    /// <returns>The magnitude.</returns>
    public double Magnitude(double rolled) => rolled * this.MagnitudeFactor;
}
