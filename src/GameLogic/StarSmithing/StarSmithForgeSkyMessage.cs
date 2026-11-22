// <copyright file="StarSmithForgeSkyMessage.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// What the client may know of a dealt sky (EE 9D), built field by field: positions, links, hints, the strikes, the
/// beat and the risk. Never a star's element, value or void, except the void a Jewel of Guardian reveals.
/// </summary>
/// <param name="Result">Done, or why the attempt did not start.</param>
/// <param name="TargetSlot">The inventory slot of the item worked on.</param>
/// <param name="Strikes">How many stars may be struck.</param>
/// <param name="Risk">The worst fate among the sky's recipes, after protection.</param>
/// <param name="StarCount">How many stars the sky holds.</param>
/// <param name="BeatIntervalMs">The time between beats.</param>
/// <param name="FirstBeatMs">The time from sending to the first beat.</param>
/// <param name="Entries">The stars in index order, then the links, then the hints; at most 255.</param>
public sealed record StarSmithForgeSkyMessage(
    StarSmithResultCode Result,
    byte TargetSlot,
    int Strikes,
    StarSmithOnFail Risk,
    int StarCount,
    int BeatIntervalMs,
    int FirstBeatMs,
    IReadOnlyList<StarSmithForgeSkyEntry> Entries)
{
    /// <summary>The kind of a star entry.</summary>
    public const byte StarKind = 0;

    /// <summary>The kind of a link entry.</summary>
    public const byte LinkKind = 1;

    /// <summary>The kind of a hint entry.</summary>
    public const byte HintKind = 2;

    /// <summary>The flag of a star a Jewel of Guardian reveals as a void.</summary>
    public const byte RevealedVoidFlag = 1;

    /// <summary>The most entries one packet holds.</summary>
    public const int MaximumEntries = byte.MaxValue;

    /// <summary>
    /// A refused start: only the result and the slot.
    /// </summary>
    /// <param name="result">Why it did not start.</param>
    /// <param name="targetSlot">The slot named.</param>
    /// <returns>The message.</returns>
    public static StarSmithForgeSkyMessage Refused(StarSmithResultCode result, byte targetSlot)
        => new(result, targetSlot, 0, StarSmithOnFail.IngredientsLost, 0, 0, 0, []);

    /// <summary>
    /// Builds the message of a dealt sky, field by field.
    /// </summary>
    /// <param name="sky">The sky.</param>
    /// <param name="targetSlot">The inventory slot of the item worked on.</param>
    /// <param name="beat">The beat.</param>
    /// <param name="firstBeatMs">The time from sending to the first beat.</param>
    /// <returns>The message.</returns>
    public static StarSmithForgeSkyMessage From(StarSmithSky sky, byte targetSlot, StarSmithBeat beat, int firstBeatMs)
    {
        var entries = new List<StarSmithForgeSkyEntry>();
        for (var i = 0; i < sky.Stars.Count; i++)
        {
            var star = sky.Stars[i];
            var revealed = star.Revealed && star.IsVoid;
            entries.Add(new StarSmithForgeSkyEntry(StarKind, (byte)i, Unit(star.X), Unit(star.Y), revealed ? RevealedVoidFlag : (byte)0));
        }

        foreach (var (a, b) in sky.Links)
        {
            entries.Add(new StarSmithForgeSkyEntry(LinkKind, (byte)a, (byte)b, 0, 0));
        }

        foreach (var hint in sky.Hints)
        {
            entries.Add(new StarSmithForgeSkyEntry(HintKind, (byte)hint.From, (byte)hint.To, Unit(hint.Clarity), 0));
        }

        return new StarSmithForgeSkyMessage(
            StarSmithResultCode.Done,
            targetSlot,
            sky.Strikes,
            sky.Risk,
            sky.Stars.Count,
            beat.IntervalMs,
            Math.Max(0, firstBeatMs),
            entries.Take(MaximumEntries).ToList());
    }

    private static byte Unit(double value) => (byte)Math.Round(Math.Clamp(value, 0, 1) * byte.MaxValue);
}
