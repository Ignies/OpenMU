// <copyright file="ChaosCastleSurvivalConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using MUnique.OpenMU.GameLogic.Rewards;

/// <summary>
/// The configuration of the tournament of the Chaos Castle Survival of the Fittest: S21Emu <c>Event/ChaosCastleSurvival.dat</c>
/// for the days and the final time, webzen guide 161 for the points and the numbers who go on. Days and times are the
/// server's.
/// </summary>
public class ChaosCastleSurvivalConfiguration
{
    /// <summary>
    /// Gets or sets the days of the month of the try-outs.
    /// </summary>
    public List<byte> TryOutDays { get; set; } = [8, 9, 10];

    /// <summary>
    /// Gets or sets the days of the month of the semi-finals.
    /// </summary>
    public List<byte> SemiFinalDays { get; set; } = [11, 12];

    /// <summary>
    /// Gets or sets the days of the month of the final.
    /// </summary>
    public List<byte> FinalDays { get; set; } = [13];

    /// <summary>
    /// Gets or sets the times of the day at which the final starts; the other stages start at every time of the timetable.
    /// </summary>
    public List<TimeOnly> FinalTimes { get; set; } = [new(1, 25)];

    /// <summary>
    /// Gets or sets the points of a killed monster.
    /// </summary>
    public int MonsterKillPoints { get; set; } = 1;

    /// <summary>
    /// Gets or sets the points of a killed player.
    /// </summary>
    public int PlayerKillPoints { get; set; } = 2;

    /// <summary>
    /// Gets or sets the points of the winner of a game.
    /// </summary>
    public int WinnerPoints { get; set; } = 3;

    /// <summary>
    /// Gets or sets the number of the best of the try-outs who may play the semi-finals.
    /// </summary>
    public int SemiFinalists { get; set; } = 20;

    /// <summary>
    /// Gets or sets the number of the best of the semi-finals who may play the final.
    /// </summary>
    public int Finalists { get; set; } = 10;

    /// <summary>
    /// Gets or sets the reward of the champion, the winner of the final (the Conqueror's Badge).
    /// </summary>
    public RewardItem? ChampionReward { get; set; } = new() { Group = 13, Number = 276, Count = 1 };

    /// <summary>
    /// Gets or sets the path of the state file of the tournament, relative to the server directory; empty keeps it in memory.
    /// </summary>
    public string StateFile { get; set; } = "state/chaos_castle_survival.json";
}
