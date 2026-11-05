// <copyright file="Season21MapsInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.Interfaces;

/// <summary>
/// Adds the <see cref="Season21MapData"/> to a game configuration: the monsters, the maps, the gates and the warp list
/// entries which it doesn't contain yet. Existing data is kept, so applying it again changes nothing.
/// </summary>
internal class Season21MapsInitializer
{
    /// <summary>
    /// The chance of a random item drop of a monster, which the drop list of a monster shares, as the common random
    /// item drop group of the Season 6 data.
    /// </summary>
    private const double RandomItemDropChance = 0.3;

    /// <summary>
    /// The offset of the parent numbers of the guids of the drop groups, beyond the numbers of the Season 6 groups.
    /// </summary>
    private const short DropGroupNumberOffset = 2100;

    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    private readonly IReadOnlyList<GameServerConfiguration> _gameServers;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21MapsInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    /// <param name="gameServers">The game servers; a new map is hosted by the ones which host its safezone map.</param>
    public Season21MapsInitializer(IContext context, GameConfiguration gameConfiguration, IReadOnlyList<GameServerConfiguration> gameServers)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
        this._gameServers = gameServers;
    }

    /// <summary>
    /// Gets the normal level which the gates and warp list entries require for a total level.
    /// </summary>
    /// <remarks>
    /// The gates and the warp list check the normal level (<see cref="Stats.Level"/>), the map requirements the total
    /// level. Master levels are gained only at the maximum level, so a character with the total level always has this
    /// normal level.
    /// </remarks>
    /// <param name="totalLevel">The total level.</param>
    /// <param name="maximumLevel">The maximum level of the game configuration.</param>
    /// <returns>The normal level.</returns>
    public static int GetNormalLevel(int totalLevel, short maximumLevel) => maximumLevel > 0 ? Math.Min(totalLevel, maximumLevel) : totalLevel;

    /// <summary>
    /// Adds the sets which the game configuration doesn't contain yet.
    /// </summary>
    public void Initialize()
    {
        foreach (var set in Season21MapData.All)
        {
            this.CreateMonsters(set);
            this.CreateMaps(set);
            this.CreateGates(set);
        }

        this.CreateDropGroups();
    }

    /// <summary>
    /// Gets the version of a monster: the one of the first map which spawns it.
    /// </summary>
    private static GameVersion IntroducedIn(Season21MapSet set, short monster)
    {
        var maps = set.Spawns.Where(spawn => spawn.Monster == monster)
            .SelectMany(spawn => spawn.Map == 0 ? set.Maps : set.Maps.Where(map => map.Number == spawn.Map))
            .Select(map => map.IntroducedIn ?? set.IntroducedIn)
            .ToList();
        return maps.Count > 0 ? maps.Min() : set.IntroducedIn;
    }

    private static int EntryLevelOf(Season21MapSet set, short mapNumber)
        => set.Maps.FirstOrDefault(map => map.Number == mapNumber)?.EntryLevel ?? set.EntryLevel;

    private void CreateMonsters(Season21MapSet set)
    {
        foreach (var row in set.Monsters.Where(row => this._gameConfiguration.Monsters.All(m => m.Number != row.Number)))
        {
            var monster = this._context.CreateNew<MonsterDefinition>();
            monster.SetGuid(row.Number);
            monster.Number = row.Number;
            monster.Designation = row.Name;
            monster.IntroducedIn = IntroducedIn(set, row.Number);
            monster.MoveRange = row.MoveRange;
            monster.AttackRange = row.AttackRange;
            monster.ViewRange = row.ViewRange;
            monster.MoveDelay = TimeSpan.FromMilliseconds(row.MoveDelay);
            monster.AttackDelay = TimeSpan.FromMilliseconds(row.AttackDelay);
            monster.RespawnDelay = TimeSpan.FromSeconds(row.RespawnDelay);
            monster.Attribute = row.Attribute;
            monster.NumberOfMaximumItemDrops = 1;
            var attributes = new Dictionary<AttributeDefinition, float>
            {
                { Stats.Level, row.Level },
                { Stats.MaximumHealth, row.Health },
                { Stats.MinimumPhysBaseDmg, row.MinimumDamage },
                { Stats.MaximumPhysBaseDmg, row.MaximumDamage },
                { Stats.DefenseBase, row.Defense },
                { Stats.AttackRatePvm, row.AttackRate },
                { Stats.DefenseRatePvm, row.DefenseRate },
            };
            AttributeDefinition[] resistances = [Stats.IceResistance, Stats.PoisonResistance, Stats.LightningResistance, Stats.FireResistance, Stats.EarthResistance, Stats.WindResistance, Stats.WaterResistance];
            for (var i = 0; i < resistances.Length; i++)
            {
                if (row.Resistances[i] > 0)
                {
                    attributes.Add(resistances[i], row.Resistances[i] / 255f);
                }
            }

            monster.AddAttributes(attributes, this._context, this._gameConfiguration);
            this._gameConfiguration.Monsters.Add(monster);
        }

        foreach (var row in set.Npcs.Where(row => this._gameConfiguration.Monsters.All(m => m.Number != row.Number)))
        {
            var npc = this._context.CreateNew<MonsterDefinition>();
            npc.SetGuid(row.Number);
            npc.Number = row.Number;
            npc.Designation = row.Name;
            npc.IntroducedIn = set.IntroducedIn;
            npc.ObjectKind = NpcObjectKind.PassiveNpc;
            this._gameConfiguration.Monsters.Add(npc);
        }
    }

    private void CreateMaps(Season21MapSet set)
    {
        foreach (var map in set.Maps.Where(map => this._gameConfiguration.Maps.All(m => m.Number != map.Number)))
        {
            if (this._gameConfiguration.Maps.FirstOrDefault(m => m.Number == map.SafezoneMap && m.Discriminator == 0) is not { } safezoneMap)
            {
                continue;
            }

            var initializer = new Season21MapInitializer(this._context, this._gameConfiguration, set, map);
            initializer.Initialize();
            initializer.SetSafezoneMap();
            var definition = this._gameConfiguration.Maps.Single(m => m.Number == map.Number);
            foreach (var server in this._gameServers.Where(server => server.Maps.Contains(safezoneMap)))
            {
                server.Maps.Add(definition);
            }
        }
    }

    /// <summary>
    /// Gives each monster of the data its drop list, as drop groups of the monster: one per drop weight, whose chances
    /// share the chance of a random item drop by their weights. Being groups of the monster, they aren't filtered by
    /// the monster level, which is above any drop level on the later maps. Only the monsters of the data get them,
    /// so a Season 6 monster which a later map spawns keeps its drops. Items which the configuration doesn't have are
    /// left out.
    /// </summary>
    private void CreateDropGroups()
    {
        var items = this._gameConfiguration.Items.ToDictionary(item => (short)((item.Group * 512) + item.Number));
        var ownMonsters = Season21MapData.All.SelectMany(set => set.Monsters).Select(row => row.Number).ToHashSet();
        short listNumber = 0;
        foreach (var list in Season21DropData.All)
        {
            listNumber++;
            var monsters = this._gameConfiguration.Monsters.Where(m => ownMonsters.Contains(m.Number) && list.Monsters.Contains(m.Number)).ToList();
            var buckets = list.Buckets
                .Select(bucket => (bucket.Weight, Items: bucket.Items.Select(i => items.GetValueOrDefault(i)).OfType<ItemDefinition>().ToList()))
                .Where(bucket => bucket.Items.Count > 0)
                .ToList();
            var totalWeight = buckets.Sum(bucket => (double)bucket.Weight * bucket.Items.Count);
            short bucketNumber = 0;
            foreach (var (weight, bucketItems) in buckets)
            {
                bucketNumber++;
                var guid = GuidHelper.CreateGuid<DropItemGroup>((short)(DropGroupNumberOffset + listNumber), bucketNumber);
                var group = this._gameConfiguration.DropItemGroups.FirstOrDefault(g => g is IIdentifiable identifiable && identifiable.Id == guid);
                if (group is null)
                {
                    group = this._context.CreateNew<DropItemGroup>();
                    group.SetGuid(guid);
                    group.Description = LocalizedString.FromResource(() => DropGroupDescriptions.Season21MonsterDrops);
                    group.Chance = RandomItemDropChance * weight * bucketItems.Count / totalWeight;
                    group.ItemType = SpecialItemType.RandomItem;
                    bucketItems.ForEach(group.PossibleItems.Add);
                    this._gameConfiguration.DropItemGroups.Add(group);
                }

                foreach (var monster in monsters.Where(m => !m.DropItemGroups.Contains(group)))
                {
                    monster.DropItemGroups.Add(group);
                }
            }
        }
    }

    private void CreateGates(Season21MapSet set)
    {
        var targets = new Dictionary<short, ExitGate>();
        foreach (var gate in set.Gates.Where(gate => gate.Kind != Season21GateKind.Entrance))
        {
            if (this.FindMap(gate.Map) is not { } map)
            {
                continue;
            }

            var target = map.ExitGates.FirstOrDefault(g => g.X1 == gate.X1 && g.Y1 == gate.Y1 && g.X2 == gate.X2 && g.Y2 == gate.Y2);
            if (target is null)
            {
                target = this._context.CreateNew<ExitGate>();
                target.Map = map;
                (target.X1, target.Y1, target.X2, target.Y2) = (gate.X1, gate.Y1, gate.X2, gate.Y2);
                target.Direction = gate.Direction;
                target.IsSpawnGate = gate.Kind == Season21GateKind.Spawn;
                map.ExitGates.Add(target);
            }

            targets[gate.Number] = target;
        }

        foreach (var gate in set.Gates.Where(gate => gate.Kind == Season21GateKind.Entrance))
        {
            if (this.FindMap(gate.Map) is not { } map
                || map.EnterGates.Any(g => g.Number == gate.Number)
                || !targets.TryGetValue(gate.Target, out var target))
            {
                continue;
            }

            var entrance = this._context.CreateNew<EnterGate>();
            entrance.Number = gate.Number;
            entrance.TargetGate = target;
            entrance.LevelRequirement = this.NormalLevelOf(set, target);
            (entrance.X1, entrance.Y1, entrance.X2, entrance.Y2) = (gate.X1, gate.Y1, gate.X2, gate.Y2);
            map.EnterGates.Add(entrance);
        }

        foreach (var row in set.Warps.Where(row => this._gameConfiguration.WarpList.All(w => w.Index != row.Index)))
        {
            if (!targets.TryGetValue(row.Gate, out var target))
            {
                continue;
            }

            var warp = this._context.CreateNew<WarpInfo>();
            warp.Index = row.Index;
            warp.Name = row.Name;
            warp.Costs = set.EntryMoney;
            warp.LevelRequirement = this.NormalLevelOf(set, target);
            warp.Gate = target;
            this._gameConfiguration.WarpList.Add(warp);
        }
    }

    private short NormalLevelOf(Season21MapSet set, ExitGate target)
        => (short)GetNormalLevel(EntryLevelOf(set, target.Map?.Number ?? 0), this._gameConfiguration.MaximumLevel);

    private GameMapDefinition? FindMap(byte number)
        => this._gameConfiguration.Maps.FirstOrDefault(map => map.Number == number && map.Discriminator == 0);
}
