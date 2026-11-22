// <copyright file="StarSmithBeat.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// How a strike met the beat.
/// </summary>
public enum StarSmithTiming
{
    /// <summary>On the beat: a clean link.</summary>
    Clean,

    /// <summary>Near the beat: a shaky link.</summary>
    Shaky,

    /// <summary>Off the beat, or a second strike on one beat: still a link, of the lowest quality.</summary>
    Late,
}

/// <summary>
/// The hammer's rhythm (section 10, rule 3). Beats fall every <paramref name="IntervalMs"/> after the start; a strike
/// is judged on the server's clock alone. The window is the tolerance plus the measured round trip, which counts only
/// up to <paramref name="RoundTripCapMs"/>; within the window a strike is clean, within twice it shaky, beyond it late.
/// </summary>
/// <param name="IntervalMs">The time between beats, in milliseconds.</param>
/// <param name="ToleranceMs">How far from a beat a strike is still clean without any round trip, in milliseconds.</param>
/// <param name="RoundTripCapMs">The most round trip that widens the window, in milliseconds.</param>
public sealed record StarSmithBeat(int IntervalMs = 800, int ToleranceMs = 90, int RoundTripCapMs = 150)
{
    /// <summary>
    /// The clean window around a beat, each side.
    /// </summary>
    /// <param name="roundTripMs">The client's measured round trip.</param>
    /// <returns>The window, in milliseconds.</returns>
    public int Window(int roundTripMs) => 0;

    /// <summary>
    /// The beat nearest to a time; the first beat falls one interval after the start.
    /// </summary>
    /// <param name="startMs">The server time the forge started.</param>
    /// <param name="timeMs">The server time of the strike.</param>
    /// <returns>The beat's number, from 1.</returns>
    public long BeatOf(long startMs, long timeMs) => 0;

    /// <summary>
    /// Judges a strike by its distance to the nearest beat.
    /// </summary>
    /// <param name="startMs">The server time the forge started.</param>
    /// <param name="timeMs">The server time of the strike.</param>
    /// <param name="roundTripMs">The client's measured round trip.</param>
    /// <returns>The timing.</returns>
    public StarSmithTiming Judge(long startMs, long timeMs, int roundTripMs) => StarSmithTiming.Late;
}
