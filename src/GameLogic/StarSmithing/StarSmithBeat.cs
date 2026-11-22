// <copyright file="StarSmithBeat.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The hammer's rhythm (section 10, rule 3). Beats fall every <paramref name="IntervalMs"/> after the start; a strike
/// is judged on the server's clock alone. The window is the tolerance plus the measured round trip, which counts only
/// up to <paramref name="RoundTripCapMs"/>; within the window a strike is clean, within twice it (at most half a beat)
/// shaky, beyond it late.
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
    public int Window(int roundTripMs) => this.ToleranceMs + Math.Clamp(roundTripMs, 0, this.RoundTripCapMs);

    /// <summary>
    /// The beat nearest to a time; the first beat falls one interval after the start.
    /// </summary>
    /// <param name="startMs">The server time the forge started.</param>
    /// <param name="timeMs">The server time of the strike.</param>
    /// <returns>The beat's number, from 1.</returns>
    public long BeatOf(long startMs, long timeMs)
    {
        var elapsed = timeMs - startMs;
        return elapsed <= 0 || this.IntervalMs <= 0 ? 1 : Math.Max(1, (elapsed + (this.IntervalMs / 2)) / this.IntervalMs);
    }

    /// <summary>
    /// Judges a strike by its distance to the nearest beat.
    /// </summary>
    /// <param name="startMs">The server time the forge started.</param>
    /// <param name="timeMs">The server time of the strike.</param>
    /// <param name="roundTripMs">The client's measured round trip.</param>
    /// <returns>The timing.</returns>
    public StarSmithTiming Judge(long startMs, long timeMs, int roundTripMs)
    {
        var offset = Math.Abs(timeMs - startMs - (this.BeatOf(startMs, timeMs) * this.IntervalMs));
        var window = this.Window(roundTripMs);
        if (offset <= window)
        {
            return StarSmithTiming.Clean;
        }

        return offset <= Math.Min(2L * window, this.IntervalMs / 2) ? StarSmithTiming.Shaky : StarSmithTiming.Late;
    }
}
