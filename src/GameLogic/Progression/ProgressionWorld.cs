// <copyright file="ProgressionWorld.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Progression;

using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// The effective numbers of the Season 21 maps of a game configuration in a normalized mode: the entry requirements
/// of their maps and the normalized rows of their monsters. Computed from the stored rows, which stay untouched.
/// </summary>
/// <remarks>
/// A Season 21 map is a map which requires a total level (<see cref="Stats.TotalLevel"/>). Its monsters are the ones
/// which spawn only on Season 21 maps; a Season 6 monster which a later map spawns too keeps its numbers.
/// The maps which share monsters, and the maps with the same safezone map and requirement (e.g. Acheron, Deventer,
/// Urk Mountain and Nars), form a set: its monsters are normalized together, at the median requirement of its maps.
/// </remarks>
public sealed class ProgressionWorld
{
    private readonly Dictionary<GameMapDefinition, int> _season21Requirements = [];

    private readonly Dictionary<MonsterDefinition, (MonsterStatRow Source, MonsterStatRow Normalized)> _monsters = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressionWorld"/> class.
    /// </summary>
    /// <param name="gameConfiguration">The game configuration.</param>
    /// <param name="configuration">The progression configuration.</param>
    public ProgressionWorld(GameConfiguration gameConfiguration, ProgressionConfiguration configuration)
    {
        this.Configuration = configuration;
        foreach (var map in gameConfiguration.Maps)
        {
            if (GetSeason21Requirement(map) is { } requirement)
            {
                this._season21Requirements[map] = requirement;
            }
        }

        this.NormalizeMonsters(gameConfiguration);
    }

    /// <summary>
    /// Gets the progression configuration.
    /// </summary>
    public ProgressionConfiguration Configuration { get; }

    /// <summary>
    /// Gets the Season 21 total-level requirement which is stored for a map, if it's a Season 21 map.
    /// </summary>
    /// <param name="map">The map.</param>
    /// <returns>The requirement, or <c>null</c>.</returns>
    public static int? GetSeason21Requirement(GameMapDefinition map)
    {
        return map.MapRequirements?.FirstOrDefault(r => r.Attribute == Stats.TotalLevel) is { } requirement
            ? requirement.MinimumValue
            : null;
    }

    /// <summary>
    /// Reads the numbers of a monster definition which the progression shapes.
    /// </summary>
    /// <param name="monster">The monster definition.</param>
    /// <returns>The row.</returns>
    public static MonsterStatRow GetSourceRow(MonsterDefinition monster)
    {
        return new MonsterStatRow(
            (int)monster[Stats.Level],
            (int)monster[Stats.MaximumHealth],
            (int)monster[Stats.MinimumPhysBaseDmg],
            (int)monster[Stats.MaximumPhysBaseDmg],
            (int)monster[Stats.DefenseBase],
            (int)monster[Stats.AttackRatePvm],
            (int)monster[Stats.DefenseRatePvm]);
    }

    /// <summary>
    /// Gets the entry requirement of a map, if it's a Season 21 map.
    /// </summary>
    /// <param name="map">The map.</param>
    /// <param name="requirement">The requirement in the configured mode.</param>
    /// <returns><c>true</c>, if the map is a Season 21 map.</returns>
    public bool TryGetRequirement(GameMapDefinition? map, out MapRequirement requirement)
    {
        if (map is not null && this._season21Requirements.TryGetValue(map, out var season21Requirement))
        {
            requirement = ProgressionMapping.GetRequirement(season21Requirement, this.Configuration);
            return true;
        }

        requirement = default;
        return false;
    }

    /// <summary>
    /// Gets the normal level which a warp list entry or a gate to a map requires.
    /// </summary>
    /// <param name="map">The target map.</param>
    /// <param name="storedRequirement">The stored requirement.</param>
    /// <returns>The level requirement.</returns>
    public int GetMoveLevelRequirement(GameMapDefinition? map, int storedRequirement)
    {
        return this.TryGetRequirement(map, out var requirement) ? requirement.Level : storedRequirement;
    }

    /// <summary>
    /// Gets the zen of a warp list entry.
    /// </summary>
    /// <param name="warp">The warp list entry.</param>
    /// <returns>The zen.</returns>
    public int GetWarpCosts(WarpInfo warp)
    {
        return warp.Gate?.Map is { } map && this._season21Requirements.TryGetValue(map, out var season21Requirement)
            ? ProgressionMapping.GetEntryZen(season21Requirement, warp.Costs, this.Configuration)
            : warp.Costs;
    }

    /// <summary>
    /// Gets the normalized row of a monster, if it's a monster of the Season 21 maps.
    /// </summary>
    /// <param name="monster">The monster definition.</param>
    /// <param name="source">The stored row.</param>
    /// <param name="normalized">The normalized row.</param>
    /// <returns><c>true</c>, if the monster is normalized.</returns>
    public bool TryGetMonster(MonsterDefinition monster, out MonsterStatRow source, out MonsterStatRow normalized)
    {
        if (this._monsters.TryGetValue(monster, out var rows))
        {
            (source, normalized) = rows;
            return true;
        }

        source = normalized = default;
        return false;
    }

    /// <summary>
    /// Gets the monster level with which item drops are rolled.
    /// </summary>
    /// <param name="monster">The monster definition.</param>
    /// <returns>The drop level.</returns>
    public int GetDropLevel(MonsterDefinition monster)
    {
        var level = this._monsters.TryGetValue(monster, out var rows) ? rows.Normalized.Level : (int)monster[Stats.Level];
        return ProgressionMapping.GetDropLevel(level, this.Configuration);
    }

    private static int Median(List<int> values)
    {
        values.Sort();
        return values[(values.Count - 1) / 2];
    }

    private void NormalizeMonsters(GameConfiguration gameConfiguration)
    {
        if (this._season21Requirements.Count == 0)
        {
            return;
        }

        var spawnMaps = new Dictionary<MonsterDefinition, HashSet<GameMapDefinition>>();
        foreach (var map in gameConfiguration.Maps)
        {
            foreach (var spawn in map.MonsterSpawns)
            {
                if (spawn.MonsterDefinition is { ObjectKind: NpcObjectKind.Monster } monster && spawn.Quantity > 0)
                {
                    spawnMaps.TryAdd(monster, []);
                    spawnMaps[monster].Add(map);
                }
            }
        }

        var monsters = spawnMaps
            .Where(p => p.Value.All(this._season21Requirements.ContainsKey))
            .ToDictionary(p => p.Key, p => p.Value);
        var sets = new SetUnion(this._season21Requirements.Keys);
        foreach (var maps in monsters.Values)
        {
            var first = maps.First();
            maps.ForEach(map => sets.Union(first, map));
        }

        foreach (var group in this._season21Requirements.GroupBy(p => (p.Key.SafezoneMap, p.Value)))
        {
            var first = group.First().Key;
            group.ForEach(p => sets.Union(first, p.Key));
        }

        foreach (var set in monsters.GroupBy(p => sets.Find(p.Value.First())))
        {
            var setMaps = this._season21Requirements.Keys.Where(map => sets.Find(map) == set.Key).ToList();
            var requirement = Median(setMaps.Select(map => this._season21Requirements[map]).ToList());
            var members = set.Select(p => p.Key).OrderBy(m => m.Number).ToList();
            var rows = members.Select(m => new SetMonster(m.Number, m.Designation, GetSourceRow(m), true)).ToList();
            var normalized = MonsterNormalizer.Normalize(requirement, rows, this.Configuration);
            for (var i = 0; i < members.Count; i++)
            {
                this._monsters[members[i]] = (rows[i].Source, normalized[i]);
            }
        }
    }

    /// <summary>
    /// A union-find of maps.
    /// </summary>
    private sealed class SetUnion
    {
        private readonly Dictionary<GameMapDefinition, GameMapDefinition> _parents;

        public SetUnion(IEnumerable<GameMapDefinition> maps)
        {
            this._parents = maps.ToDictionary(map => map, map => map);
        }

        public GameMapDefinition Find(GameMapDefinition map)
        {
            while (this._parents[map] != map)
            {
                map = this._parents[map] = this._parents[this._parents[map]];
            }

            return map;
        }

        public void Union(GameMapDefinition a, GameMapDefinition b)
        {
            this._parents[this.Find(a)] = this.Find(b);
        }
    }
}
