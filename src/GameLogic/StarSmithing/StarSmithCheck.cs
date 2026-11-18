// <copyright file="StarSmithCheck.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// What <see cref="StarSmithRules.Check"/> looks at, read from the player before an action.
/// </summary>
/// <param name="FeatureOn">Whether Star Smithing is on.</param>
/// <param name="HammerHeld">Whether a Star Smith's Hammer is in a hand.</param>
/// <param name="HammerDurability">The hammer's durability.</param>
/// <param name="DurabilityCost">The durability an action takes from the hammer.</param>
/// <param name="PlaceAllowed">Whether smithing is allowed where the player is (no trade, shop, duel or event map).</param>
/// <param name="ItemFound">Whether the target item is there (and is not the hammer).</param>
/// <param name="ItemLocked">Whether the target item's customisation is locked.</param>
/// <param name="Zen">The player's Zen.</param>
/// <param name="ZenCost">The Zen a recolour costs.</param>
public readonly record struct StarSmithCheck(bool FeatureOn, bool HammerHeld, int HammerDurability, int DurabilityCost, bool PlaceAllowed, bool ItemFound, bool ItemLocked, long Zen, long ZenCost);
