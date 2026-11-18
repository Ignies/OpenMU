// <copyright file="StarSmithRules.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// Whether a smithing action may run (Readme.md): the switch, the hammer in a hand and its durability, the place, the
/// item, the lock, then the Zen of a recolour. Text is checked after these, by the text moderation.
/// </summary>
public static class StarSmithRules
{
    /// <summary>
    /// Decides an action.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="check">What the player has.</param>
    /// <returns>Done, or the first reason it may not run.</returns>
    public static StarSmithResultCode Check(StarSmithAction action, StarSmithCheck check)
    {
        if (!check.FeatureOn)
        {
            return StarSmithResultCode.Off;
        }

        if (!check.HammerHeld)
        {
            return StarSmithResultCode.NoHammer;
        }

        if (WearsHammer(action) && check.HammerDurability < check.DurabilityCost)
        {
            return StarSmithResultCode.HammerBroken;
        }

        if (!check.PlaceAllowed)
        {
            return StarSmithResultCode.NotHere;
        }

        if (!check.ItemFound)
        {
            return StarSmithResultCode.NoItem;
        }

        if (check.ItemLocked)
        {
            return StarSmithResultCode.Locked;
        }

        return action == StarSmithAction.Recolor && check.Zen < check.ZenCost ? StarSmithResultCode.NotEnoughZen : StarSmithResultCode.Done;
    }

    /// <summary>
    /// Whether an action takes durability from the hammer (a lock does not).
    /// </summary>
    /// <param name="action">The action.</param>
    /// <returns>True when it wears the hammer.</returns>
    public static bool WearsHammer(StarSmithAction action) => action != StarSmithAction.Lock;
}
