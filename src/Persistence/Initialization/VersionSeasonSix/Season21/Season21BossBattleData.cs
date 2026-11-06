// <copyright file="Season21BossBattleData.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.GameLogic.Rewards;
using MUnique.OpenMU.Interfaces;

/// <summary>
/// The Boss Battle Together of Season 21, from S21Emu <c>Event/BossBattleEvent.xml</c>, <c>EventMapEntryInfoManager.xml</c>
/// (total level 800) and <c>MonsterSetBase/139 - Boss Battle Together.xml</c>, on map 139 (<c>Terrain140.att</c>, gate 681).
/// </summary>
internal static class Season21BossBattleData
{
    /// <summary>
    /// The number of the map.
    /// </summary>
    internal const byte MapNumber = 139;

    /// <summary>
    /// Gets the map, its bosses (the copies 919-924 of the data) and its NPCs.
    /// </summary>
    public static Season21MapSet Zone { get; } = new(
        GameVersion.Season15Episode1,
        0,
        0,
        [new(MapNumber, LocalizedString.FromResource(() => MapNames.BossBattleZone), 0, 140)],
        [
            new(919, LocalizedString.FromResource(() => MonsterNames.IllusionOfKundunBattle), Level: 147, Health: 100000000, MinimumDamage: 2000, MaximumDamage: 2500, Defense: 1500, AttackRate: 2000, DefenseRate: 1000, MoveRange: 9, AttackRange: 15, ViewRange: 30, MoveDelay: 800, AttackDelay: 1500, RespawnDelay: 43200, Attribute: 2, Resistances: [60, 60, 60, 60, 0, 0, 0]),
            new(921, LocalizedString.FromResource(() => MonsterNames.MedusaBattle), Level: 175, Health: 100000000, MinimumDamage: 7040, MaximumDamage: 7800, Defense: 1450, AttackRate: 5900, DefenseRate: 3800, MoveRange: 9, AttackRange: 15, ViewRange: 30, MoveDelay: 800, AttackDelay: 1100, RespawnDelay: 10, Attribute: 2, Resistances: [254, 254, 150, 150, 0, 0, 0]),
            new(922, LocalizedString.FromResource(() => MonsterNames.SelupanBattle), Level: 145, Health: 100000000, MinimumDamage: 2500, MaximumDamage: 3000, Defense: 1250, AttackRate: 2200, DefenseRate: 1800, MoveRange: 9, AttackRange: 15, ViewRange: 30, MoveDelay: 800, AttackDelay: 1100, RespawnDelay: 10, Attribute: 2, Resistances: [254, 254, 150, 150, 0, 0, 0]),
            new(923, LocalizedString.FromResource(() => MonsterNames.NixBattle), Level: 190, Health: 100000000, MinimumDamage: 6000, MaximumDamage: 7500, Defense: 3100, AttackRate: 8800, DefenseRate: 5400, MoveRange: 9, AttackRange: 15, ViewRange: 30, MoveDelay: 300, AttackDelay: 1100, RespawnDelay: 86400, Attribute: 2, Resistances: [0, 0, 0, 0, 0, 0, 0]),
            new(924, LocalizedString.FromResource(() => MonsterNames.GodOfDarknessBattle), Level: 300, Health: 100000000, MinimumDamage: 10000, MaximumDamage: 12000, Defense: 3000, AttackRate: 10000, DefenseRate: 6950, MoveRange: 9, AttackRange: 15, ViewRange: 30, MoveDelay: 500, AttackDelay: 1100, RespawnDelay: 86400, Attribute: 2, Resistances: [0, 0, 0, 0, 0, 0, 0]),
        ],
        [],
        [new(681, Season21GateKind.Spawn, MapNumber, 122, 111, 134, 111)],
        []);

    /// <summary>
    /// Creates the battle: one of the five bosses at 124/149 with 80,000,000 health and 2,000,000 more for every three
    /// players; 1,000,000 damage give a Jewel of Harmony Box (20/78) and a Group Boss Battle Bonus Box (20/110), the most
    /// damage and the last hit two more bonus boxes each (the data's items 10318 and 10350, which are wide items).
    /// </summary>
    /// <returns>The definition.</returns>
    public static BossBattleDefinition CreateDefinition() => new()
    {
        Bosses = [919, 921, 922, 923, 924],
        X = 124,
        Y = 149,
        Health = 80_000_000,
        BonusHealth = 2_000_000,
        BonusHealthPlayers = 3,
        DamageForRewards = 1_000_000,
        DamageRewards = [Item(78), Item(110)],
        TopDamageRewards = [Item(110), Item(110)],
        LastHitRewards = [Item(110), Item(110)],
    };

    private static RewardItem Item(short number) => new() { Group = 20, Number = number };
}
