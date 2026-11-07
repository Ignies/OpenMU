// <copyright file="Season21GoldenInvasionInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.GameLogic.PlugIns.InvasionEvents;
using MUnique.OpenMU.GameLogic.PlugIns.PeriodicTasks;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the golden invasion of Season 21 (S21Emu <c>Event/InvasionManager.xml</c>, invasion 2): the ten later golden monsters
/// (493 to 502) with their Box of Kundun, the Golden Derkon (79) also in Elbeland, and the roster and timetable of the data.
/// </summary>
internal class Season21GoldenInvasionInitializer
{
    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21GoldenInvasionInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Season21GoldenInvasionInitializer(IContext context, GameConfiguration gameConfiguration)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
    }

    /// <summary>
    /// Gets the later golden monsters of <c>Monster/Monster.xml</c> with the level of the Box of Kundun of their item bag (EventItemBag 117 to 126).
    /// </summary>
    internal static IReadOnlyList<(Season21Monster Row, byte KundunBox)> Monsters { get; } =
    [
        (new(493, LocalizedString.FromResource(() => MonsterNames.GoldenDarkKnight), Level: 60, Health: 5000, MinimumDamage: 180, MaximumDamage: 195, Defense: 115, AttackRate: 300, DefenseRate: 88, MoveRange: 3, AttackRange: 4, ViewRange: 7, MoveDelay: 400, AttackDelay: 2000, RespawnDelay: 10, Attribute: 2, Resistances: [5, 5, 5, 7, 0, 0, 0]), 1),
        (new(494, LocalizedString.FromResource(() => MonsterNames.GoldenDevil), Level: 72, Health: 10000, MinimumDamage: 250, MaximumDamage: 280, Defense: 190, AttackRate: 365, DefenseRate: 120, MoveRange: 3, AttackRange: 1, ViewRange: 7, MoveDelay: 400, AttackDelay: 1400, RespawnDelay: 10, Attribute: 2, Resistances: [8, 8, 8, 8, 0, 0, 0]), 2),
        (new(495, LocalizedString.FromResource(() => MonsterNames.GoldenStoneGolem), Level: 84, Health: 25000, MinimumDamage: 375, MaximumDamage: 425, Defense: 275, AttackRate: 530, DefenseRate: 190, MoveRange: 3, AttackRange: 4, ViewRange: 7, MoveDelay: 400, AttackDelay: 1400, RespawnDelay: 20, Attribute: 2, Resistances: [10, 10, 10, 10, 0, 0, 0]), 3),
        (new(496, LocalizedString.FromResource(() => MonsterNames.GoldenCrust), Level: 92, Health: 34500, MinimumDamage: 489, MaximumDamage: 540, Defense: 360, AttackRate: 620, DefenseRate: 240, MoveRange: 3, AttackRange: 1, ViewRange: 5, MoveDelay: 400, AttackDelay: 1400, RespawnDelay: 10, Attribute: 2, Resistances: [13, 13, 14, 13, 0, 0, 0]), 3),
        (new(497, LocalizedString.FromResource(() => MonsterNames.GoldenSatyros), Level: 108, Health: 95000, MinimumDamage: 950, MaximumDamage: 1000, Defense: 600, AttackRate: 900, DefenseRate: 350, MoveRange: 1, AttackRange: 6, ViewRange: 7, MoveDelay: 400, AttackDelay: 1500, RespawnDelay: 150, Attribute: 2, Resistances: [30, 30, 35, 30, 0, 0, 0]), 4),
        (new(498, LocalizedString.FromResource(() => MonsterNames.GoldenTwinTail), Level: 130, Health: 145000, MinimumDamage: 1040, MaximumDamage: 1085, Defense: 865, AttackRate: 1080, DefenseRate: 440, MoveRange: 3, AttackRange: 6, ViewRange: 7, MoveDelay: 500, AttackDelay: 1500, RespawnDelay: 10, Attribute: 2, Resistances: [28, 28, 28, 28, 0, 0, 0]), 4),
        (new(499, LocalizedString.FromResource(() => MonsterNames.GoldenIronKnight), Level: 145, Health: 200000, MinimumDamage: 1743, MaximumDamage: 2092, Defense: 650, AttackRate: 1940, DefenseRate: 840, MoveRange: 3, AttackRange: 6, ViewRange: 7, MoveDelay: 500, AttackDelay: 1500, RespawnDelay: 10, Attribute: 2, Resistances: [150, 30, 30, 30, 0, 0, 0]), 5),
        (new(500, LocalizedString.FromResource(() => MonsterNames.GoldenNapin), Level: 112, Health: 77000, MinimumDamage: 1441, MaximumDamage: 2017, Defense: 585, AttackRate: 1350, DefenseRate: 840, MoveRange: 3, AttackRange: 3, ViewRange: 6, MoveDelay: 500, AttackDelay: 1500, RespawnDelay: 10, Attribute: 2, Resistances: [150, 30, 30, 30, 0, 0, 0]), 4),
        (new(501, LocalizedString.FromResource(() => MonsterNames.GreatGoldenDragon), Level: 142, Health: 95000, MinimumDamage: 1917, MaximumDamage: 2301, Defense: 660, AttackRate: 2000, DefenseRate: 800, MoveRange: 3, AttackRange: 6, ViewRange: 7, MoveDelay: 500, AttackDelay: 1500, RespawnDelay: 10, Attribute: 2, Resistances: [150, 30, 30, 30, 0, 0, 0]), 5),
        (new(502, LocalizedString.FromResource(() => MonsterNames.GoldenRabbit), Level: 40, Health: 3000, MinimumDamage: 130, MaximumDamage: 135, Defense: 60, AttackRate: 200, DefenseRate: 47, MoveRange: 3, AttackRange: 4, ViewRange: 7, MoveDelay: 400, AttackDelay: 1600, RespawnDelay: 10, Attribute: 2, Resistances: [3, 1, 1, 1, 0, 0, 0]), 1),
    ];

    /// <summary>
    /// Gets the roster of the golden invasion of the data: monster, count, maps; <c>true</c> when it spawns on every map of the list.
    /// </summary>
    internal static IReadOnlyList<(ushort Monster, ushort Count, ushort[] Maps, bool AllMaps)> Roster { get; } =
    [
        (43, 5, [0], false),
        (53, 5, [2], false),
        (54, 5, [2], false),
        (78, 5, [3], false),
        (79, 1, [0, 2, 3, 51], true),
        (80, 3, [7], false),
        (81, 5, [7], false),
        (82, 3, [8], false),
        (83, 5, [8], false),
        (493, 3, [1], false),
        (494, 3, [4], false),
        (495, 3, [33], false),
        (496, 3, [10], false),
        (497, 3, [37], false),
        (498, 3, [38], false),
        (499, 3, [57], false),
        (500, 3, [56], false),
        (501, 1, [37, 57], true),
        (502, 5, [51], false),
    ];

    /// <summary>
    /// Gets the start times of the data: every four hours from 00:25.
    /// </summary>
    internal static IReadOnlyList<TimeOnly> Timetable { get; } = PeriodicTaskConfiguration.GenerateTimeSequence(TimeSpan.FromHours(4), new TimeOnly(0, 25)).ToList();

    /// <summary>
    /// Gets the duration of the data (<c>InvasionTime="600"</c>).
    /// </summary>
    internal static TimeSpan Duration { get; } = TimeSpan.FromSeconds(600);

    /// <summary>
    /// Gets the monsters of the Underworld and Summon invasions (S21Emu <c>Monster.xml</c>), which Season 6 doesn't define.
    /// </summary>
    internal static IReadOnlyList<Season21Monster> InvasionMonsterRows { get; } =
    [
        new(55, LocalizedString.FromResource(() => MonsterNames.DeathKing), Level: 88, Health: 30000, MinimumDamage: 430, MaximumDamage: 500, Defense: 120, AttackRate: 150, DefenseRate: 37, MoveRange: 2, AttackRange: 2, ViewRange: 6, MoveDelay: 400, AttackDelay: 1800, RespawnDelay: 86400, Attribute: 2, Resistances: [2, 2, 2, 2, 0, 0, 0]),
        new(56, LocalizedString.FromResource(() => MonsterNames.DeathBone), Level: 24, Health: 700, MinimumDamage: 85, MaximumDamage: 90, Defense: 29, AttackRate: 120, DefenseRate: 29, MoveRange: 3, AttackRange: 1, ViewRange: 5, MoveDelay: 400, AttackDelay: 1600, RespawnDelay: 20, Attribute: 2, Resistances: [1, 1, 1, 1, 0, 0, 0]),
        new(652, LocalizedString.FromResource(() => MonsterNames.GoldenGoblin), Level: 100, Health: 120000, MinimumDamage: 100, MaximumDamage: 200, Defense: 200, AttackRate: 500, DefenseRate: 300, MoveRange: 3, AttackRange: 1, ViewRange: 7, MoveDelay: 400, AttackDelay: 1400, RespawnDelay: 3600, Attribute: 2, Resistances: [0, 0, 0, 0, 0, 0, 0]),
    ];

    /// <summary>
    /// Applies the invasion.
    /// </summary>
    public void Initialize()
    {
        foreach (var (row, box) in Monsters.Where(m => this._gameConfiguration.Monsters.All(d => d.Number != m.Row.Number)))
        {
            var monster = Season21MapsInitializer.CreateMonster(this._context, this._gameConfiguration, row, GameVersion.Season5Episode1);
            this._gameConfiguration.Monsters.Add(monster);
            this.AddBoxOfKundun(monster, box);
        }

        this.Configure();
    }

    /// <summary>
    /// Adds the monsters of the Underworld and Summon invasions and switches both on, with the roster of the data
    /// unless the operator configured one.
    /// </summary>
    public void AddUnderworldAndSummon()
    {
        foreach (var row in InvasionMonsterRows.Where(r => this._gameConfiguration.Monsters.All(d => d.Number != r.Number)))
        {
            this._gameConfiguration.Monsters.Add(Season21MapsInitializer.CreateMonster(this._context, this._gameConfiguration, row, GameVersion.Season5Episode1));
        }

        this.SwitchOn(new UnderworldInvasionPlugIn());
        this.SwitchOn(new SummonInvasionPlugIn());
    }

    private static bool IsSeason6Default(PeriodicInvasionConfiguration configuration)
    {
        var standard = (PeriodicInvasionConfiguration)new GoldenInvasionPlugIn().CreateDefaultConfig();
        return configuration.Timetable.SequenceEqual(standard.Timetable)
               && configuration.TaskDuration == standard.TaskDuration
               && configuration.Mobs.Count == standard.Mobs.Count
               && configuration.Mobs.All(mob => standard.Mobs.FirstOrDefault(s => s.MonsterId == mob.MonsterId) is { } s
                                               && s.Count == mob.Count
                                               && s.MapIds.SequenceEqual(mob.MapIds)
                                               && s.MapStrategy == mob.MapStrategy);
    }

    private void SwitchOn(SimpleInvasionPlugIn invasion)
    {
        var configuration = Season21PlugInConfigurations.GetOrCreate<PeriodicInvasionConfiguration>(this._context, this._gameConfiguration, invasion.GetType().GUID, out var plugIn);
        if (configuration is not { Mobs.Count: > 0 })
        {
            plugIn.SetConfiguration((PeriodicInvasionConfiguration)invasion.CreateDefaultConfig(), null);
        }

        plugIn.IsActive = true;
    }

    private void AddBoxOfKundun(MonsterDefinition monster, byte level)
    {
        if (this._gameConfiguration.Items.FirstOrDefault(i => i.Group == 14 && i.Number == 11) is not { } box)
        {
            return;
        }

        var drop = this._context.CreateNew<DropItemGroup>();
        drop.Chance = 1;
        drop.ItemLevel = (byte)(7 + level);
        drop.Description = DropGroupDescriptions.ResourceManager.GetLocalizedString($"BoxOfKundunPlus{level}");
        drop.Monster = monster;
        drop.PossibleItems.Add(box);
        monster.DropItemGroups.Add(drop);
        this._gameConfiguration.DropItemGroups.Add(drop);
    }

    private void Configure()
    {
        if (this._gameConfiguration.PlugInConfigurations.FirstOrDefault(c => c.TypeId == typeof(GoldenInvasionPlugIn).GUID) is not { } plugIn)
        {
            return;
        }

        PeriodicInvasionConfiguration? configuration;
        try
        {
            configuration = plugIn.GetConfiguration<PeriodicInvasionConfiguration>(new Updates.IdIgnoringReferenceHandler())
                            ?? (PeriodicInvasionConfiguration)new GoldenInvasionPlugIn().CreateDefaultConfig();
        }
        catch (System.Text.Json.JsonException)
        {
            // A configuration which can't be read is the operator's; it stays.
            return;
        }

        var replace = IsSeason6Default(configuration);
        if (replace)
        {
            // The operator kept the Season 6 roster: the invasion becomes the one of the data.
            configuration.Mobs.Clear();
            configuration.Timetable = Timetable.ToList();
            configuration.TaskDuration = Duration;
        }

        var changed = replace;
        foreach (var (monster, count, maps, allMaps) in Roster)
        {
            if (configuration.Mobs.FirstOrDefault(m => m.MonsterId == monster) is { } existing)
            {
                foreach (var map in maps.Where(map => !existing.MapIds.Contains(map)))
                {
                    existing.MapIds.Add(map);
                    changed = true;
                }

                continue;
            }

            configuration.Mobs.Add(new InvasionSpawnConfiguration(monster, count, maps.ToList(), allMaps ? SpawnMapStrategy.AllMaps : SpawnMapStrategy.RandomMap));
            changed = true;
        }

        if (changed)
        {
            plugIn.SetConfiguration(configuration, null);
        }
    }
}
