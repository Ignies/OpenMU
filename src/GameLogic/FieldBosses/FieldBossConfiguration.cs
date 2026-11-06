// <copyright file="FieldBossConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.FieldBosses;

/// <summary>
/// Configuration of the <see cref="FieldBossesFeaturePlugIn"/>.
/// </summary>
/// <remarks>
/// Since Season 19 Part 2-3 the field bosses share one rule: a random respawn 18 to 24 hours after the kill, and the
/// players leave the arena 5 minutes after the kill (S21Emu <c>BossFightSystem.xml</c>: RespawnHour 18, RespawnHourAddRand 6).
/// </remarks>
public class FieldBossConfiguration
{
    /// <summary>
    /// Gets or sets the shortest time from a kill to the next spawn.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.FieldBossConfiguration_RespawnMinimum_Name))]
    public TimeSpan RespawnMinimum { get; set; } = TimeSpan.FromHours(18);

    /// <summary>
    /// Gets or sets the longest random time which is added to <see cref="RespawnMinimum"/>.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.FieldBossConfiguration_RespawnRandom_Name))]
    public TimeSpan RespawnRandom { get; set; } = TimeSpan.FromHours(6);

    /// <summary>
    /// Gets or sets the time from a kill until the players are moved out of the arena.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.FieldBossConfiguration_ExitDelay_Name))]
    public TimeSpan ExitDelay { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the time from a lost battle (its time ran out) to the next spawn
    /// (S21Emu <c>FereaBoss.xml</c> CrystalBallRegenFail: 5 minutes).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.FieldBossConfiguration_FailedRespawn_Name))]
    public TimeSpan FailedRespawn { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets a value indicating whether the bosses spawn when the server starts; otherwise they wait one respawn time.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.FieldBossConfiguration_SpawnOnStart_Name))]
    public bool SpawnOnStart { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether a spawn and a kill are announced to every player of the server.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.FieldBossConfiguration_Announce_Name))]
    public bool Announce { get; set; } = true;

    /// <summary>
    /// Gets or sets the file which keeps the boss timers over a restart, relative to the server directory; empty for none.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.FieldBossConfiguration_StateFile_Name))]
    public string StateFile { get; set; } = "state/field_bosses.json";

    /// <summary>
    /// Gets or sets the bosses.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.FieldBossConfiguration_Bosses_Name))]
    public List<FieldBossDefinition> Bosses { get; set; } = [];
}
