// <copyright file="LabyrinthLeagueConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using MUnique.OpenMU.GameLogic.Rewards;

/// <summary>
/// The configuration of the scores and leagues of the Labyrinth of Dimensions (S21Emu
/// <c>Event/LabyrinthOfDimension/LabyrinthOfDimension_League.xml</c>). The update of the leagues sets the values of the data.
/// </summary>
public class LabyrinthLeagueConfiguration
{
    /// <summary>
    /// Gets or sets the points and box chances of the floors, from floor 1.
    /// </summary>
    public List<LabyrinthFloorScore> Floors { get; set; } = [];

    /// <summary>
    /// Gets or sets the bonus points of the first floor cleared on a day.
    /// </summary>
    public int FirstPlayPoints { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the most points of a fast clear of a floor; they shrink by one percent per percent of
    /// <see cref="FastFinishMaxTime"/>.
    /// </summary>
    public int FastFinishPoints { get; set; } = 250;

    /// <summary>
    /// Gets or sets the time of a floor clear, from which no fast finish points are given.
    /// </summary>
    public TimeSpan FastFinishMaxTime { get; set; } = TimeSpan.FromSeconds(1200);

    /// <summary>
    /// Gets or sets the boxes of a floor clear (lower, medium and higher Maze of Dimensions box).
    /// </summary>
    public List<RewardItem> FloorBoxes { get; set; } = [];

    /// <summary>
    /// Gets or sets the days of the month of the league (UTC); a day which isn't listed counts no points.
    /// </summary>
    public List<LabyrinthLeagueDay> Schedule { get; set; } = [];

    /// <summary>
    /// Gets or sets the number of characters of a group.
    /// </summary>
    public int GroupSize { get; set; } = 5;

    /// <summary>
    /// Gets or sets the number of the first ranks of a group which go up a tier at the end of a week.
    /// </summary>
    public int PromotedRanks { get; set; } = 2;

    /// <summary>
    /// Gets or sets the rank of a group from which the characters go down a tier at the end of a week.
    /// </summary>
    public int DemotedRank { get; set; } = 5;

    /// <summary>
    /// Gets or sets the rewards of the tiers on the reward day.
    /// </summary>
    public List<LabyrinthTierReward> TierRewards { get; set; } = [];

    /// <summary>
    /// Gets or sets the path of the state file of the leagues, relative to the server directory; empty keeps it in memory.
    /// </summary>
    public string StateFile { get; set; } = "state/labyrinth_league.json";

    /// <summary>
    /// Gets or sets the minimum time between two writes of the state file.
    /// </summary>
    public TimeSpan SaveInterval { get; set; } = TimeSpan.FromMinutes(1);
}
