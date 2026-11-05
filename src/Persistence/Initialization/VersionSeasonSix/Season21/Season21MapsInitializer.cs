// <copyright file="Season21MapsInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// Adds the <see cref="Season21MapData"/> to a game configuration: the monsters, the maps, the gates and the warp list
/// entries which it doesn't contain yet. Existing data is kept, so applying it again changes nothing.
/// </summary>
internal class Season21MapsInitializer
{
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

    private void CreateGates(Season21MapSet set)
    {
        var normalLevel = (short)GetNormalLevel(set.EntryLevel, this._gameConfiguration.MaximumLevel);
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
            entrance.LevelRequirement = normalLevel;
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
            warp.LevelRequirement = normalLevel;
            warp.Gate = target;
            this._gameConfiguration.WarpList.Add(warp);
        }
    }

    private GameMapDefinition? FindMap(byte number)
        => this._gameConfiguration.Maps.FirstOrDefault(map => map.Number == number && map.Discriminator == 0);
}
