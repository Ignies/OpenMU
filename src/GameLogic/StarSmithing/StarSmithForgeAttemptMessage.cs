// <copyright file="StarSmithForgeAttemptMessage.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// What EE 9D tells the smith about a started attempt (Readme.md "PR 7a contract", "Wire"), built field by field so
/// nothing hidden (the faults but the revealed one) can slip into it.
/// </summary>
/// <param name="Result">0 started, else why not.</param>
/// <param name="TargetSlot">The item's slot.</param>
/// <param name="Strikes">The placements the attempt allows.</param>
/// <param name="Risk">The worst fate.</param>
/// <param name="FreeSlots">The slots still free on the item.</param>
/// <param name="Slots">The smith's slots by level.</param>
/// <param name="BeatIntervalMs">The beat interval.</param>
/// <param name="FirstBeatMs">The time to the first beat.</param>
/// <param name="RevealedFault">The fault a Jewel of Guardian reveals, or null.</param>
/// <param name="FaultRadius">The fault radius, in box units.</param>
/// <param name="CloseRadius">The close radius, in box units.</param>
/// <param name="NodeGap">The node gap, in box units.</param>
/// <param name="FaultCount">How many faults the attempt holds.</param>
/// <param name="Layers">The item's layers.</param>
public sealed record StarSmithForgeAttemptMessage(
    StarSmithResultCode Result,
    byte TargetSlot,
    int Strikes,
    StarSmithOnFail Risk,
    int FreeSlots,
    int Slots,
    int BeatIntervalMs,
    int FirstBeatMs,
    StarSmithFault? RevealedFault,
    ushort FaultRadius,
    ushort CloseRadius,
    ushort NodeGap,
    int FaultCount,
    IReadOnlyList<StarSmithLayer> Layers)
{
    /// <summary>
    /// A refused start: only the result and the slot.
    /// </summary>
    /// <param name="result">Why.</param>
    /// <param name="targetSlot">The item's slot.</param>
    /// <returns>The message.</returns>
    public static StarSmithForgeAttemptMessage Refused(StarSmithResultCode result, byte targetSlot)
        => new(result, targetSlot, 0, StarSmithOnFail.IngredientsLost, 0, 0, 0, 0, null, 0, 0, 0, 0, []);

    /// <summary>
    /// The message of a started attempt.
    /// </summary>
    /// <param name="forge">The forge.</param>
    /// <param name="targetSlot">The item's slot.</param>
    /// <param name="beat">The beat.</param>
    /// <param name="firstBeatMs">The time to the first beat.</param>
    /// <param name="rules">The rules.</param>
    /// <returns>The message.</returns>
    public static StarSmithForgeAttemptMessage From(StarSmithForge forge, byte targetSlot, StarSmithBeat beat, int firstBeatMs, StarSmithForgeRules rules)
        => new(
            StarSmithResultCode.Done,
            targetSlot,
            forge.Strikes,
            forge.Risk,
            forge.FreeSlots,
            forge.Slots,
            beat.IntervalMs,
            firstBeatMs,
            forge.Revealed >= 0 ? forge.Faults[forge.Revealed] : null,
            StarSmithPoint.ToBox(rules.FaultRadius),
            StarSmithPoint.ToBox(rules.CloseRadius),
            StarSmithPoint.ToBox(rules.NodeGap),
            forge.Faults.Count,
            forge.Layers);
}
