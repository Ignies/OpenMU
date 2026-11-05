// <copyright file="Season21MapInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.Interfaces;

/// <summary>
/// The initialization of a map of a <see cref="Season21MapSet"/>.
/// </summary>
/// <param name="context">The context.</param>
/// <param name="gameConfiguration">The game configuration.</param>
/// <param name="set">The set of the map.</param>
/// <param name="map">The map.</param>
internal class Season21MapInitializer(IContext context, GameConfiguration gameConfiguration, Season21MapSet set, Season21Map map)
    : BaseMapInitializer(context, gameConfiguration)
{
    /// <summary>
    /// The prefix of the embedded terrain files, <c>Resources/Season21/Terrain{number}.att</c>.
    /// </summary>
    internal const string TerrainPrefix = "Season21.";

    /// <summary>
    /// The number of the first monster spawn; the ones before are the ones of the NPCs.
    /// </summary>
    private const short FirstMonsterSpawnNumber = 10;

    // Field initializers run before the base constructor, which already needs the map number.
    private readonly Season21MapSet _set = set;

    private readonly Season21Map _map = map;

    /// <inheritdoc/>
    protected override byte MapNumber => this._map.Number;

    /// <inheritdoc/>
    protected override LocalizedString MapName => this._map.Name;

    /// <inheritdoc/>
    protected override byte SafezoneMapNumber => this._map.SafezoneMap;

    /// <inheritdoc/>
    protected override string TerrainVersionPrefix => TerrainPrefix;

    /// <inheritdoc/>
    protected override IEnumerable<MonsterSpawnArea> CreateNpcSpawns()
    {
        short number = 1;
        foreach (var npc in this._set.Npcs.Where(npc => npc.Map == this._map.Number))
        {
            yield return this.CreateMonsterSpawn(number++, this.NpcDictionary[npc.Number], npc.X, npc.Y, npc.Direction);
        }
    }

    /// <inheritdoc/>
    protected override IEnumerable<MonsterSpawnArea> CreateMonsterSpawns()
    {
        var number = FirstMonsterSpawnNumber;
        foreach (var spawn in this._set.Spawns.Where(spawn => spawn.Map == 0 || spawn.Map == this._map.Number))
        {
            yield return this.CreateMonsterSpawn(number++, this.NpcDictionary[spawn.Monster], spawn.X1, spawn.X2, spawn.Y1, spawn.Y2, spawn.Quantity);
        }
    }

    /// <inheritdoc/>
    protected override void CreateMapAttributeRequirements()
    {
        this.CreateRequirement(Stats.TotalLevel, this._map.EntryLevel ?? this._set.EntryLevel);
    }

    /// <summary>
    /// Adds the drop item groups of the safezone map, because the default drop item groups are only known during the
    /// initialization of a new database.
    /// </summary>
    protected override void InitializeDropItemGroups()
    {
        var safezoneMap = this.GameConfiguration.Maps.FirstOrDefault(map => map.Number == this._map.SafezoneMap && map.Discriminator == 0);
        foreach (var group in safezoneMap?.DropItemGroups ?? [])
        {
            this.MapDefinition!.DropItemGroups.Add(group);
        }
    }

    /// <inheritdoc/>
    protected override void AdditionalInitialization(GameMapDefinition mapDefinition)
    {
        mapDefinition.IntroducedIn = this._map.IntroducedIn ?? this._set.IntroducedIn;
    }
}
