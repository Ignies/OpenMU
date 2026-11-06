// <copyright file="Season21MiningInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.GameLogic.Mining;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the mining of Season 21: the mining spots (NPCs 605 to 607) in the safe zones of Lost Tower, Tarkan, Aida,
/// Swamp of Calmness, Raklion and Kubera Mine, the stages of <c>Mining.txt</c> and the <see cref="MiningFeaturePlugIn"/>.
/// </summary>
internal class Season21MiningInitializer
{
    /// <summary>
    /// The first number of the spawn areas of the spots; each spot adds its index.
    /// </summary>
    private const short FirstSpawnNumber = 7400;

    /// <summary>
    /// The NPC number of Bolo in Karutan, who sells the Pandora Pick.
    /// </summary>
    private const short BoloNumber = 578;

    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21MiningInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Season21MiningInitializer(IContext context, GameConfiguration gameConfiguration)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
    }

    /// <summary>
    /// Gets the spot kinds: the stages of <c>Mining.txt</c> (Index, Stage, Value, ItemRate, ItemCount, BonusItemRate, BonusItemCount).
    /// </summary>
    internal static IReadOnlyList<MiningSpotDefinition> Spots { get; } =
    [
        Spot(605, 2, [1, 3, 7, 16, 33], 250, 100),
        Spot(606, 4, [2, 6, 15, 34, 67], 190, 400),
        Spot(607, 8, [4, 12, 29, 65, 125], 180, 1000),
    ];

    /// <summary>
    /// Gets the spots of <c>MonsterSetBase</c>: map, NPC, x, y.
    /// </summary>
    internal static IReadOnlyList<(byte Map, short Npc, byte X, byte Y)> Spawns { get; } =
    [
        (4, 605, 202, 75), (4, 605, 206, 80), (4, 605, 211, 80), (4, 606, 212, 76), (4, 606, 208, 72), (4, 607, 207, 76),
        (8, 605, 188, 54), (8, 605, 198, 64), (8, 605, 206, 72), (8, 605, 206, 55), (8, 605, 186, 60), (8, 606, 164, 58),
        (8, 606, 197, 57), (8, 606, 192, 73), (8, 606, 203, 65), (8, 607, 205, 59), (8, 607, 201, 70), (8, 607, 199, 56),
        (33, 605, 83, 15), (33, 605, 76, 9), (33, 605, 88, 7), (33, 606, 91, 12), (33, 606, 83, 12), (33, 607, 79, 10),
        (56, 605, 139, 113), (56, 605, 145, 109), (56, 605, 143, 99), (56, 605, 135, 100), (56, 605, 130, 111), (56, 606, 134, 105),
        (56, 606, 142, 105), (56, 606, 138, 109), (56, 606, 137, 101), (56, 607, 141, 105), (56, 607, 145, 119), (56, 607, 142, 109),
        (57, 605, 211, 208), (57, 605, 220, 203), (57, 605, 222, 208), (57, 606, 223, 203), (57, 606, 217, 208), (57, 607, 224, 221),
        (123, 605, 235, 94), (123, 605, 237, 101), (123, 605, 229, 85), (123, 605, 223, 117), (123, 606, 222, 108), (123, 606, 234, 121),
        (123, 606, 228, 123), (123, 607, 240, 125), (123, 607, 216, 105),
    ];

    /// <summary>
    /// Applies the mining.
    /// </summary>
    public void Initialize()
    {
        var number = FirstSpawnNumber;
        foreach (var (map, npc, x, y) in Spawns)
        {
            Season21NpcSpawns.Place(this._context, this._gameConfiguration, number++, map, npc, NameOf(npc), x, y, Direction.South);
        }

        this.SellPickaxesWithFullDurability();
        this.Configure();
    }

    private static MiningSpotDefinition Spot(short npc, byte durabilityCost, int[] counts, int bonusRate, int bonusCount)
    {
        var spot = new MiningSpotDefinition { NpcNumber = npc };
        for (var i = 0; i < counts.Length; i++)
        {
            var last = i == counts.Length - 1;
            spot.Stages.Add(new MiningStage
            {
                DurabilityCost = durabilityCost,
                SuccessRate = 4750,
                ItemCount = counts[i],
                BonusRate = last ? bonusRate : 0,
                BonusCount = last ? bonusCount : 0,
            });
        }

        return spot;
    }

    private static LocalizedString NameOf(short npc) => npc switch
    {
        605 => LocalizedString.FromResource(() => MonsterNames.MiningAreaSmall),
        606 => LocalizedString.FromResource(() => MonsterNames.MiningAreaMedium),
        _ => LocalizedString.FromResource(() => MonsterNames.MiningAreaLarge),
    };

    private static MiningConfiguration CreateConfiguration()
    {
        var configuration = new MiningConfiguration();
        configuration.Spots.AddRange(Spots);
        configuration.Bundles.AddRange(
        [
            new MiningBundle { ItemGroup = 12, ItemNumber = 30, ItemLevel = 2, JewelCount = 30 },
            new MiningBundle { ItemGroup = 12, ItemNumber = 30, ItemLevel = 1, JewelCount = 20 },
            new MiningBundle { ItemGroup = 12, ItemNumber = 30, ItemLevel = 0, JewelCount = 10 },
        ]);
        return configuration;
    }

    /// <summary>
    /// The shop rows of the data give Bolo's pickaxe the durability 1, which is a count there; a pickaxe sells at its full durability.
    /// </summary>
    private void SellPickaxesWithFullDurability()
    {
        if (this._gameConfiguration.Monsters.FirstOrDefault(m => m.Number == BoloNumber)?.MerchantStore is not { } store)
        {
            return;
        }

        foreach (var item in store.Items.Where(i => i.Definition is { Group: 0, Number: 41 } && i.Durability < i.Definition.Durability))
        {
            item.Durability = item.Definition!.Durability;
        }
    }

    private void Configure()
    {
        var configuration = Season21PlugInConfigurations.GetOrCreate<MiningConfiguration>(this._context, this._gameConfiguration, typeof(MiningFeaturePlugIn).GUID, out var plugIn);
        if (configuration is null || configuration.Spots.Count > 0)
        {
            // A configuration which can't be read, or one which already has its spots, is the operator's.
            return;
        }

        // The first application turns the feature on; later ones leave the operator's switch alone.
        plugIn.IsActive = true;
        plugIn.SetConfiguration(CreateConfiguration(), null);
    }
}
