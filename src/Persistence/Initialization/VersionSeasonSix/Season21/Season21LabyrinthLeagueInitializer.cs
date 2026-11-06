// <copyright file="Season21LabyrinthLeagueInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.GameLogic.Rewards;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Configures the <see cref="LabyrinthLeaguePlugIn"/> with S21Emu <c>Event/LabyrinthOfDimension/LabyrinthOfDimension_League.xml</c>
/// and lets the three Maze of Dimensions boxes (14/403-405) be opened into the items of their bags (240-242).
/// </summary>
internal class Season21LabyrinthLeagueInitializer
{
    private const byte BoxGroup = 14;

    private static readonly (short Number, short Bag)[] Boxes = [(403, 240), (404, 241), (405, 242)];

    /// <summary>
    /// The chances of the lower, medium and higher box (n/10000) by the last floor of a range (<c>RewardInfo/FloorClear</c>).
    /// </summary>
    private static readonly (int LastFloor, int Lower, int Medium, int Higher)[] BoxChances =
    [
        (5, 10000, 0, 0),
        (9, 9000, 1000, 0),
        (15, 8000, 2000, 0),
        (20, 7000, 3000, 0),
        (25, 5000, 5000, 0),
        (30, 3000, 6000, 1000),
        (35, 1000, 6000, 3000),
        (40, 0, 5000, 5000),
        (41, 0, 3000, 7000),
    ];

    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21LabyrinthLeagueInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Season21LabyrinthLeagueInitializer(IContext context, GameConfiguration gameConfiguration)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
    }

    /// <summary>
    /// Creates the configuration of the data: 41 floors (50 points per floor for the first clear of the day, 25 for a
    /// repeat), 1000 points for the first play of the day, up to 250 for a clear within 20 minutes, the box chances, the
    /// schedule of four weeks from the 1st, 8th, 15th and 22nd with the reward day on the 29th, and 2, 5 and 10 higher boxes
    /// for Silver, Gold and Legend.
    /// </summary>
    /// <returns>The configuration.</returns>
    public static LabyrinthLeagueConfiguration CreateConfiguration() => new()
    {
        Floors =
        [
            .. Enumerable.Range(1, 41).Select(floor =>
            {
                var chances = BoxChances.First(c => floor <= c.LastFloor);
                return new LabyrinthFloorScore
                {
                    FirstPoints = 50 * floor,
                    RepeatPoints = 25 * floor,
                    BoxChances = [chances.Lower / 10000.0, chances.Medium / 10000.0, chances.Higher / 10000.0],
                };
            }),
        ],
        FirstPlayPoints = 1000,
        FastFinishPoints = 250,
        FastFinishMaxTime = TimeSpan.FromSeconds(1200),
        FloorBoxes = [.. Boxes.Select(b => new RewardItem { Group = BoxGroup, Number = b.Number })],
        Schedule =
        [
            .. Enumerable.Range(1, 28).Select(day => new LabyrinthLeagueDay { Day = (byte)day, Kind = day % 7 == 1 ? LabyrinthDayKind.Start : LabyrinthDayKind.Play }),
            new LabyrinthLeagueDay { Day = 29, Kind = LabyrinthDayKind.Reward },
        ],
        TierRewards =
        [
            new LabyrinthTierReward { Tier = LabyrinthLeagueTier.Silver, Item = new RewardItem { Group = BoxGroup, Number = 405, Count = 2 } },
            new LabyrinthTierReward { Tier = LabyrinthLeagueTier.Gold, Item = new RewardItem { Group = BoxGroup, Number = 405, Count = 5 } },
            new LabyrinthTierReward { Tier = LabyrinthLeagueTier.Legend, Item = new RewardItem { Group = BoxGroup, Number = 405, Count = 10 } },
        ],
    };

    /// <summary>
    /// Applies the leagues; a configuration the operator already has stays, and so does the switch after the first run.
    /// </summary>
    public void Initialize()
    {
        this.AddBoxContents();
        var configuration = Season21PlugInConfigurations.GetOrCreate<LabyrinthLeagueConfiguration>(this._context, this._gameConfiguration, typeof(LabyrinthLeaguePlugIn).GUID, out var plugIn);
        if (configuration is null || configuration.Floors.Count > 0)
        {
            return;
        }

        // The first application turns the feature on; later ones leave the operator's switch alone.
        plugIn.IsActive = true;
        plugIn.SetConfiguration(CreateConfiguration(), null);
    }

    private void AddBoxContents()
    {
        foreach (var (number, bag) in Boxes)
        {
            if (this._gameConfiguration.Items.FirstOrDefault(i => i.Group == BoxGroup && i.Number == number) is not { } box
                || box.DropItems.Count > 0)
            {
                continue;
            }

            var group = this._context.CreateNew<ItemDropItemGroup>();
            group.ItemType = SpecialItemType.RandomItem;
            group.SourceItemLevel = 0;
            group.Chance = 1.0;
            group.Description = box.Name;
            foreach (var code in Season21InstanceZoneData.BoxBags[bag])
            {
                if (this._gameConfiguration.Items.FirstOrDefault(i => i.Group == code / 512 && i.Number == code % 512) is { } item)
                {
                    group.PossibleItems.Add(item);
                }
            }

            box.DropItems.Add(group);
        }
    }
}
