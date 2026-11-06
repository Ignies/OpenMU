// <copyright file="IHuntingRecordViewPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.World;

using MUnique.OpenMU.GameLogic.HuntingRecord;

/// <summary>
/// Interface of a view whose implementation shows the hunting record and the ranking.
/// </summary>
public interface IHuntingRecordViewPlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows the hunting record of a map, one row a day, newest first.
    /// </summary>
    /// <param name="mapNumber">The map number.</param>
    /// <param name="days">The days.</param>
    ValueTask ShowHuntingRecordAsync(short mapNumber, IReadOnlyList<HuntingDay> days);

    /// <summary>
    /// Shows a page of a ranking.
    /// </summary>
    /// <param name="kind">The kind.</param>
    /// <param name="page">The page, from 0.</param>
    /// <param name="pageCount">The number of pages.</param>
    /// <param name="ownRank">The rank of the own character, 0 when it is not ranked.</param>
    /// <param name="entries">The characters of the page.</param>
    ValueTask ShowRankingPageAsync(RankingKind kind, int page, int pageCount, int ownRank, IReadOnlyList<RankingEntry> entries);
}
