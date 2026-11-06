// <copyright file="Season21FieldBossesInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.FieldBosses;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the field bosses of Season 21: their monsters, spawn areas, item bags and the <see cref="FieldBossesFeaturePlugIn"/>.
/// </summary>
internal class Season21FieldBossesInitializer
{
    /// <summary>
    /// The first number of the spawn areas of the bosses; the maps update numbers its spawns from 10 up.
    /// </summary>
    private const short FirstSpawnNumber = 1900;

    /// <summary>
    /// The parent number of the drop groups' ids; each boss adds its index, each section its number.
    /// </summary>
    private const short DropGroupNumberOffset = 2200;

    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21FieldBossesInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Season21FieldBossesInitializer(IContext context, GameConfiguration gameConfiguration)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
    }

    /// <summary>
    /// Applies the bosses.
    /// </summary>
    public void Initialize()
    {
        this.CreateMonsters();
        this.CreateSpawns();
        this.CreateDrops();
        this.Configure();
    }

    private static short KeyOf(ItemDefinition item) => (short)((item.Group * 512) + item.Number);

    private GameMapDefinition? FindMap(byte number)
        => this._gameConfiguration.Maps.FirstOrDefault(map => map.Number == number && map.Discriminator == 0);

    private MonsterDefinition? FindMonster(short number) => this._gameConfiguration.Monsters.FirstOrDefault(m => m.Number == number);

    private void CreateMonsters()
    {
        foreach (var (mapNumber, introducedIn, row) in Season21FieldBossData.Monsters.Where(m => this.FindMonster(m.Row.Number) is null))
        {
            if (this.FindMap(mapNumber) is not null)
            {
                this._gameConfiguration.Monsters.Add(Season21MapsInitializer.CreateMonster(this._context, this._gameConfiguration, row, introducedIn));
            }
        }
    }

    private void CreateSpawns()
    {
        var number = FirstSpawnNumber;
        foreach (var (mapNumber, monsterNumber, x, y) in Season21FieldBossData.Spawns)
        {
            number++;
            if (this.FindMap(mapNumber) is not { } map || this.FindMonster(monsterNumber) is not { } monster)
            {
                continue;
            }

            var spawns = map.MonsterSpawns.Where(s => s.MonsterDefinition == monster).ToList();
            spawns.ForEach(s => s.SpawnTrigger = SpawnTrigger.ManuallyForEvent);
            if (spawns.Count > 0)
            {
                continue;
            }

            var area = this._context.CreateNew<MonsterSpawnArea>();
            area.SetGuid(map.Number, number);
            area.GameMap = map;
            area.MonsterDefinition = monster;
            area.Quantity = 1;
            area.SpawnTrigger = SpawnTrigger.ManuallyForEvent;
            (area.X1, area.Y1, area.X2, area.Y2) = (x, y, x, y);
            map.MonsterSpawns.Add(area);
        }
    }

    private void CreateDrops()
    {
        var items = this._gameConfiguration.Items.GroupBy(KeyOf).ToDictionary(g => g.Key, g => g.First());
        short bossIndex = 0;
        foreach (var (monsterNumber, sections) in Season21FieldBossData.Drops)
        {
            bossIndex++;
            if (this.FindMonster(monsterNumber) is not { } monster)
            {
                continue;
            }

            var parent = (short)(DropGroupNumberOffset + bossIndex);
            short sectionNumber = 0;
            var groups = 0;
            foreach (var section in sections)
            {
                sectionNumber++;
                var sectionItems = section.Select(i => items.GetValueOrDefault(i)).OfType<ItemDefinition>().ToList();
                if (sectionItems.Count == 0)
                {
                    continue;
                }

                var id = GuidHelper.CreateGuid<DropItemGroup>(parent, sectionNumber);
                var group = this._gameConfiguration.DropItemGroups.FirstOrDefault(g => g is IIdentifiable identifiable && identifiable.Id == id);
                if (group is null)
                {
                    group = this._context.CreateNew<DropItemGroup>();
                    group.SetGuid(parent, sectionNumber);
                    group.Description = LocalizedString.FromResource(() => DropGroupDescriptions.Season21BossDrops);
                    group.ItemType = SpecialItemType.RandomItem;
                    group.Chance = 1.0;
                    this._gameConfiguration.DropItemGroups.Add(group);
                }

                foreach (var item in sectionItems.Where(i => !group.PossibleItems.Contains(i)))
                {
                    group.PossibleItems.Add(item);
                }

                if (!monster.DropItemGroups.Contains(group))
                {
                    monster.DropItemGroups.Add(group);
                }

                groups++;
            }

            monster.NumberOfMaximumItemDrops = Math.Max(monster.NumberOfMaximumItemDrops, groups);
        }
    }

    private void Configure()
    {
        var configuration = Season21PlugInConfigurations.GetOrCreate<FieldBossConfiguration>(this._context, this._gameConfiguration, typeof(FieldBossesFeaturePlugIn).GUID, out var plugIn);
        if (configuration is null)
        {
            return;
        }

        var missing = Season21FieldBossData.CreateBosses()
            .Where(b => this.FindMap((byte)b.MapNumber) is not null && configuration.Bosses.All(c => !string.Equals(c.Name, b.Name, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        if (missing.Count > 0)
        {
            // The first application turns the feature on; later ones leave the operator's switch alone.
            plugIn.IsActive |= configuration.Bosses.Count == 0;
            configuration.Bosses.AddRange(missing);
            plugIn.SetConfiguration(configuration, null);
        }
    }
}
