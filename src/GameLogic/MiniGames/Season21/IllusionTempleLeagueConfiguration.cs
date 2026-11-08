// <copyright file="IllusionTempleLeagueConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using MUnique.OpenMU.GameLogic.Rewards;

/// <summary>
/// The configuration of the Illusion Temple League: S21Emu <c>Event/IlusionTempleLeague/IllusionTempleLeague.xml</c> for
/// the days, the times and the semi-final pairing, the Webzen guides for the guild party of five, the five entries of a
/// guild and the Wings of Conqueror; the other values have no source and are ours. Days and times are the server's.
/// </summary>
public class IllusionTempleLeagueConfiguration
{
    /// <summary>
    /// Gets or sets the days of the month of the eliminations.
    /// </summary>
    public List<byte> EliminationDays { get; set; } = [3, 4, 5, 6, 7, 8, 9];

    /// <summary>
    /// Gets or sets the day of the month of the semi-finals.
    /// </summary>
    public byte SemiFinalDay { get; set; } = 11;

    /// <summary>
    /// Gets or sets the day of the month of the final.
    /// </summary>
    public byte FinalDay { get; set; } = 12;

    /// <summary>
    /// Gets or sets the day of the month from which the players of the champion get their reward.
    /// </summary>
    public byte RewardDay { get; set; } = 13;

    /// <summary>
    /// Gets or sets the times of the day at which the entry of the semi-finals opens; the eliminations open at every time
    /// of the timetable.
    /// </summary>
    public List<TimeOnly> SemiFinalTimes { get; set; } = [new(14, 55)];

    /// <summary>
    /// Gets or sets the times of the day at which the entry of the final opens.
    /// </summary>
    public List<TimeOnly> FinalTimes { get; set; } = [new(14, 55)];

    /// <summary>
    /// Gets or sets the minimum number of members of the player's guild in his party.
    /// </summary>
    public int MinimumGuildPartyMembers { get; set; } = 5;

    /// <summary>
    /// Gets or sets the number of elimination matches a guild may play in a month; 0 for no limit.
    /// </summary>
    public int MaximumGuildEntries { get; set; } = 5;

    /// <summary>
    /// Gets or sets the number of elimination matches a character may play in a month; 0 for no limit.
    /// </summary>
    public int MaximumPlayerEntries { get; set; }

    /// <summary>
    /// Gets or sets the slots of the semi-finals of the four best guilds of the eliminations, by rank: slots 1 and 2 play
    /// in the first temple, slots 3 and 4 in the second.
    /// </summary>
    public List<byte> SemiFinalSlots { get; set; } = [1, 4, 3, 2];

    /// <summary>
    /// Gets or sets the damage reduction of a hit of a player on another player in the league, in percent.
    /// </summary>
    public int DamageReduction { get; set; }

    /// <summary>
    /// Gets or sets the percent by which each occupied stone lowers the chance that the Cursed Statue appears again.
    /// </summary>
    public int StatueSpawnRateReductionPerPoint { get; set; }

    /// <summary>
    /// Gets or sets how long a sealed stone can't be unsealed by the other team.
    /// </summary>
    public TimeSpan StoneSealProtection { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets or sets the reward of each player of the champion who played the final (the Wings of Conqueror).
    /// </summary>
    public RewardItem? ChampionReward { get; set; } = new() { Group = 12, Number = 268, Count = 1 };

    /// <summary>
    /// Gets or sets the path of the state file of the guilds, relative to the server directory; empty keeps it in memory.
    /// </summary>
    public string GuildStateFile { get; set; } = "state/illusion_temple_league_guilds.json";

    /// <summary>
    /// Gets or sets the path of the state file of the characters, relative to the server directory; empty keeps it in
    /// memory.
    /// </summary>
    public string PlayerStateFile { get; set; } = "state/illusion_temple_league_players.json";
}
