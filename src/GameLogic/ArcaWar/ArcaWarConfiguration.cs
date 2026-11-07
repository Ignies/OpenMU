// <copyright file="ArcaWarConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ArcaWar;

using MUnique.OpenMU.GameLogic.Rewards;

/// <summary>
/// The configuration of the Arca War: S21Emu <c>Event/ArcaBattle.xml</c> for the schedule, the stages, the obelisks, the
/// monsters and the spawns, the webzen guide of the Arca War for the points, the experience, the buffs and the death
/// penalty. Values neither source has are settings with a default of their own (marked "no data"). Times are the server's.
/// </summary>
public class ArcaWarConfiguration
{
    /// <summary>
    /// Gets or sets the days of the week on which the war starts.
    /// </summary>
    public List<DayOfWeek> Days { get; set; } = [.. Enum.GetValues<DayOfWeek>()];

    /// <summary>
    /// Gets or sets the times of the day at which the registration of the guild masters opens.
    /// </summary>
    public List<TimeOnly> StartTimes { get; set; } = [new(10, 45)];

    /// <summary>
    /// Gets or sets the minutes in which the guild masters register their guilds.
    /// </summary>
    public int MasterRegistrationMinutes { get; set; } = 1;

    /// <summary>
    /// Gets or sets the minutes in which the members of the registered guilds register.
    /// </summary>
    public int MemberRegistrationMinutes { get; set; } = 1;

    /// <summary>
    /// Gets or sets the minutes between the registration and the war.
    /// </summary>
    public int WaitMinutes { get; set; } = 1;

    /// <summary>
    /// Gets or sets the minutes of the war.
    /// </summary>
    public int DurationMinutes { get; set; } = 30;

    /// <summary>
    /// Gets or sets the minutes after the war until the players are moved off the maps.
    /// </summary>
    public int CloseMinutes { get; set; } = 10;

    /// <summary>
    /// Gets or sets the registered members a guild needs to play (master included).
    /// </summary>
    public int MinimumGuildMembers { get; set; } = 1;

    /// <summary>
    /// Gets or sets the members a guild may register (master included; webzen: 20).
    /// </summary>
    public int MaximumGuildMembers { get; set; } = 20;

    /// <summary>
    /// Gets or sets the guilds the war takes, first come, first served (webzen: 6).
    /// </summary>
    public int MaximumGuilds { get; set; } = 6;

    /// <summary>
    /// Gets or sets the guilds a war needs to start; with fewer, it's cancelled (no data).
    /// </summary>
    public int MinimumGuilds { get; set; } = 1;

    /// <summary>
    /// Gets or sets the items a guild master pays to register the guild (webzen: 10 Signs of Lord), or <c>null</c> for none.
    /// </summary>
    public RewardItem? RegistrationFee { get; set; } = new() { Group = 14, Number = 21, Level = 3, Count = 10 };

    /// <summary>
    /// Gets or sets a value indicating whether the members of a guild can hurt each other (S21Emu <c>FriendlyFire</c>).
    /// </summary>
    public bool FriendlyFire { get; set; }

    /// <summary>
    /// Gets or sets the percentage by which the damage between members of different guilds is reduced during the war
    /// (S21Emu <c>DamageReduction OppositeTeam</c>, no value in the data).
    /// </summary>
    public float OppositeTeamDamageReduction { get; set; }

    /// <summary>
    /// Gets or sets the percentage by which the damage between members of one guild is reduced during the war, when
    /// <see cref="FriendlyFire"/> is on (S21Emu <c>DamageReduction SelfTeam</c>, no value in the data).
    /// </summary>
    public float SelfTeamDamageReduction { get; set; }

    /// <summary>
    /// Gets or sets the seconds a guild has to stand on all three auras of an obelisk before its shield drops for the
    /// guild (no data).
    /// </summary>
    public int CaptureSeconds { get; set; } = 5;

    /// <summary>
    /// Gets or sets the distance from an aura in which a member stands on it (no data).
    /// </summary>
    public byte AuraRange { get; set; } = 1;

    /// <summary>
    /// Gets or sets the points of a killed monster.
    /// </summary>
    public int MonsterPoints { get; set; } = 5;

    /// <summary>
    /// Gets or sets the points of a killed member of another guild.
    /// </summary>
    public int PlayerPoints { get; set; } = 10;

    /// <summary>
    /// Gets or sets the points of the member whose hit took an obelisk.
    /// </summary>
    public int ObeliskPoints { get; set; } = 50;

    /// <summary>
    /// Gets or sets the experience of the participants by their points; the highest row a participant reached applies.
    /// </summary>
    public List<ArcaExperienceReward> ExperienceRewards { get; set; } = [];

    /// <summary>
    /// Gets or sets the trophy of a killed monster and of the first kill of each member of another guild (Trophies of
    /// Battle), or <c>null</c> for none.
    /// </summary>
    public RewardItem? Trophy { get; set; }

    /// <summary>
    /// Gets or sets the days the members of a guild keep the buff of an obelisk it holds at the end of a war.
    /// </summary>
    public int BuffDays { get; set; } = 7;

    /// <summary>
    /// Gets or sets the buffs by element.
    /// </summary>
    public List<ArcaBuff> Buffs { get; set; } = [];

    /// <summary>
    /// Gets or sets the maps on which the members of the guilds holding obelisks have the bonus (webzen: Acheron, 91).
    /// </summary>
    public List<short> BonusMaps { get; set; } = [91];

    /// <summary>
    /// Gets or sets the number of the magic effect of the bonus (Arca Battle Reward 6), 0 for none.
    /// </summary>
    public short BonusEffectNumber { get; set; } = 187;

    /// <summary>
    /// Gets or sets the experience bonus in percent (S21Emu <c>Effect.txt</c> 187 value 1).
    /// </summary>
    public float BonusExperiencePercent { get; set; } = 20;

    /// <summary>
    /// Gets or sets the item drop bonus in percent (S21Emu <c>Effect.txt</c> 187 value 2).
    /// </summary>
    public float BonusDropPercent { get; set; } = 10;

    /// <summary>
    /// Gets or sets the number of the magic effect of the death penalty (Arca Battle Penalty), 0 for none.
    /// </summary>
    public short DeathPenaltyEffectNumber { get; set; }

    /// <summary>
    /// Gets or sets the seconds of the death penalty.
    /// </summary>
    public int DeathPenaltySeconds { get; set; } = 90;

    /// <summary>
    /// Gets or sets the factor of the movement speed during the death penalty (no data).
    /// </summary>
    public float DeathPenaltySpeedFactor { get; set; } = 0.5f;

    /// <summary>
    /// Gets or sets the obelisks.
    /// </summary>
    public List<ArcaObeliskDefinition> Obelisks { get; set; } = [];

    /// <summary>
    /// Gets or sets the monster groups.
    /// </summary>
    public List<ArcaMonsterGroup> MonsterGroups { get; set; } = [];

    /// <summary>
    /// Gets or sets the spawn groups; the registered guilds are spread over them in turn.
    /// </summary>
    public List<ArcaSpawnGroup> SpawnGroups { get; set; } = [];

    /// <summary>
    /// Gets or sets the path of the state file with the holders of the obelisks, relative to the server directory; empty
    /// keeps it in memory.
    /// </summary>
    public string StateFile { get; set; } = "state/arca_war.json";
}
