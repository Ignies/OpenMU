// <copyright file="ItemCustomizationReportGate.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ItemCustomization;

/// <summary>
/// Which reports go through: one per reporter, item and version, at most one every ten seconds per reporter. The
/// database's unique index on the same three keeps it across restarts.
/// </summary>
public sealed class ItemCustomizationReportGate
{
    /// <summary>The least time between two reports of a player, in milliseconds.</summary>
    public const long Interval = 10_000;

    private const int MaximumKept = 50_000;

    private readonly HashSet<(string Reporter, Guid ItemId, int Version)> _reported = [];

    private readonly Dictionary<string, long> _last = new(StringComparer.Ordinal);

    /// <summary>
    /// Lets a report through, or says why not.
    /// </summary>
    /// <param name="reporter">The reporting character.</param>
    /// <param name="itemId">The item.</param>
    /// <param name="version">The version of its customisation.</param>
    /// <param name="now">The current tick, in milliseconds.</param>
    /// <returns>The result.</returns>
    public ItemCustomizationReportResult Try(string reporter, Guid itemId, int version, long now)
    {
        lock (this._reported)
        {
            if (this._reported.Contains((reporter, itemId, version)))
            {
                return ItemCustomizationReportResult.Already;
            }

            if (this._last.TryGetValue(reporter, out var last) && now - last < Interval)
            {
                return ItemCustomizationReportResult.TooFast;
            }

            if (this._reported.Count > MaximumKept)
            {
                this._reported.Clear();
                this._last.Clear();
            }

            this._reported.Add((reporter, itemId, version));
            this._last[reporter] = now;
            return ItemCustomizationReportResult.Sent;
        }
    }
}
