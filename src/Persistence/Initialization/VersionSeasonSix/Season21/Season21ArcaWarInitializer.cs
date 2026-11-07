// <copyright file="Season21ArcaWarInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.ArcaWar;
using MUnique.OpenMU.GameLogic.PlayerActions.Craftings;
using MUnique.OpenMU.GameLogic.Rewards;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Configures the <see cref="ArcaWarPlugIn"/> with S21Emu <c>Event/ArcaBattle.xml</c> (schedule, stages, obelisks, auras,
/// monster groups and player spawns), <c>Monster/Monster.xml</c> (the Fire Tower, 598) and <c>Effect.txt</c> (the buffs
/// 179-183, the penalty 184 and the reward 187 of Acheron, named by <c>Define.lua</c>), with the points, experience,
/// trophies, penalty time and Sign of Lord fee of the webzen guide of the Arca War; places Sir Lesnar (584, S21Emu
/// <c>MonsterSetBase</c>) with the exchange of the Trophies of Battle (<c>EventItemBag</c> 153-162, the rates of
/// <c>GameServerInfo - ChaosMix.dat</c>).
/// </summary>
internal class Season21ArcaWarInitializer
{
    /// <summary>
    /// The number of the obelisk monster.
    /// </summary>
    internal const short FireTowerNumber = 598;

    /// <summary>
    /// The magic effect of the death penalty (Arca Battle Penalty).
    /// </summary>
    internal const short PenaltyEffectNumber = 184;

    /// <summary>
    /// The magic effect of the reward of Acheron (Arca Battle Reward 6).
    /// </summary>
    internal const short BonusEffectNumber = 187;

    /// <summary>
    /// The number of Sir Lesnar.
    /// </summary>
    internal const short SirLesnarNumber = 584;

    /// <summary>
    /// The first number of the spawn areas of Sir Lesnar.
    /// </summary>
    private const short SirLesnarSpawnNumber = 5840;

    /// <summary>
    /// The rate of the first trophy of a tier and the addition of each further one (<c>TrophiesOfBattleMixRate</c>,
    /// <c>TrophiesOfBattleMixRateAdd</c>).
    /// </summary>
    private const byte TrophyRate = 50;

    private const byte TrophyRateAdd = 5;

    /// <summary>
    /// The buffs: element (<c>Define.lua</c> <c>EFFECT_ARCA_*TOWER</c>), effect, group and the value of <c>Effect.txt</c>,
    /// read with the meanings of the webzen guide (skill damage, defense, critical chance, damage, damage reduction).
    /// </summary>
    private static readonly (ArcaElement Element, short Effect, byte Group, ArcaBuffKind Kind, float Value)[] Buffs =
    [
        (ArcaElement.Fire, 179, 103, ArcaBuffKind.SkillDamage, 25),
        (ArcaElement.Water, 180, 104, ArcaBuffKind.Defense, 50),
        (ArcaElement.Earth, 181, 105, ArcaBuffKind.CriticalDamageChance, 5),
        (ArcaElement.Wind, 182, 106, ArcaBuffKind.Damage, 30),
        (ArcaElement.Darkness, 183, 107, ArcaBuffKind.DamageReduction, 10),
    ];

    private static readonly (byte Map, byte X, byte Y, byte Direction)[] SirLesnarSpawns =
    [
        (0, 114, 132, 3), // Lorencia
        (2, 240, 53, 7), // Devias
        (3, 183, 126, 2), // Noria
    ];

    /// <summary>
    /// The items of the trophy bags (153-162, one for each tier): the count of the first bag and the addition of each
    /// further one, at most 255.
    /// </summary>
    private static readonly (byte Group, short Number, byte First, byte Step)[] TrophyBagItems =
    [
        (12, 288, 3, 3), // Elemental Rune
        (14, 467, 2, 2), // Guardian Enhanced Stone
        (14, 468, 1, 1), // Elite Guardian Enhanced Stone
        (13, 468, 3, 3), // Golden Sentence
        (14, 224, 2, 2), // Bless of Light (Greater)
        (0, 41, 25, 25), // Pandora Pick (Two-Handed)
        (14, 342, 1, 1), // Steel of Heaven
        (14, 469, 5, 5), // Guardian Elite Option Change Piece
        (14, 448, 1, 0), // Ruud Box (3000)
        (14, 341, 1, 0), // Sealed Stone Shard
    ];

    private static readonly (byte Index, short Map, byte X, byte Y, (byte X, byte Y)[] Auras)[] Obelisks =
    [
        (1, 92, 89, 41, [(89, 58), (84, 30), (104, 36)]), // Alkmaar
        (2, 92, 188, 101, [(175, 102), (192, 110), (200, 96)]), // Ubaid
        (3, 96, 162, 54, [(154, 60), (162, 45), (165, 67)]), // Deventer
    ];

    private static readonly (short Map, short[] Monsters, (byte, byte, byte, byte)[] Spots)[] MonsterGroups =
    [
        (92, [592, 593, 594, 595, 596], [(77, 31, 87, 38), (84, 43, 98, 45), (90, 39, 97, 45), (87, 48, 98, 55)]),
        (92, [592, 593, 594, 595, 596], [(178, 101, 190, 100), (188, 94, 201, 98), (173, 101, 188, 105), (186, 89, 195, 99)]),
        (96, [611, 612, 613, 614, 615], [(152, 58, 156, 62), (161, 43, 164, 46), (164, 65, 167, 68), (160, 52, 164, 56)]),
    ];

    private static readonly (short Map, (byte, byte, byte, byte)[] Areas)[] SpawnGroups =
    [
        (92, [(77, 118, 87, 130), (110, 153, 123, 163), (133, 139, 145, 150), (169, 37, 179, 47), (12, 18, 25, 34)]),
        (92, [(237, 42, 246, 53), (207, 205, 218, 214), (167, 199, 175, 211), (151, 198, 156, 200), (185, 148, 194, 157)]),
        (96, [(57, 80, 77, 94), (109, 47, 126, 64), (60, 26, 84, 37), (143, 89, 163, 104), (169, 15, 187, 28)]),
    ];

    private static readonly (int Points, long Experience)[] ExperienceRewards =
    [
        (0, 300_000), (11, 500_000), (51, 700_000), (101, 1_000_000), (301, 2_000_000), (501, 3_000_000),
    ];

    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21ArcaWarInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Season21ArcaWarInitializer(IContext context, GameConfiguration gameConfiguration)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
    }

    /// <summary>
    /// Creates the configuration of the data.
    /// </summary>
    /// <returns>The configuration.</returns>
    public static ArcaWarConfiguration CreateConfiguration() => new()
    {
        StartTimes = [new(10, 45)],
        MasterRegistrationMinutes = 1,
        MemberRegistrationMinutes = 1,
        WaitMinutes = 1,
        DurationMinutes = 30,
        CloseMinutes = 10,
        MinimumGuildMembers = 1,
        Obelisks =
        [
            .. Obelisks.Select(o => new ArcaObeliskDefinition
            {
                Index = o.Index,
                MapNumber = o.Map,
                MonsterNumber = FireTowerNumber,
                X = o.X,
                Y = o.Y,
                Health = 500_000,
                OccupiedHealth = 300_000,
                RegenSeconds = 20,
                Auras = [.. o.Auras.Select(a => new ArcaPosition { X = a.X, Y = a.Y })],
            }),
        ],
        MonsterGroups =
        [
            .. MonsterGroups.Select(g => new ArcaMonsterGroup
            {
                MapNumber = g.Map,
                Count = 10,
                RegenMinutes = 10,
                Monsters = [.. g.Monsters],
                Spots = [.. g.Spots.Select(Area)],
            }),
        ],
        SpawnGroups = [.. SpawnGroups.Select(g => new ArcaSpawnGroup { MapNumber = g.Map, Areas = [.. g.Areas.Select(Area)] })],
        ExperienceRewards = [.. ExperienceRewards.Select(r => new ArcaExperienceReward { MinimumPoints = r.Points, Experience = r.Experience })],
        Trophy = new RewardItem { Group = 13, Number = 147, Count = 1 },
        Buffs = [.. Buffs.Select(b => new ArcaBuff { Element = b.Element, EffectNumber = b.Effect, Kind = b.Kind, Value = b.Value })],
        DeathPenaltyEffectNumber = PenaltyEffectNumber,
    };

    /// <summary>
    /// Adds the Fire Tower and the effects when they're missing and applies the configuration; a configuration the operator
    /// already has stays, and so does the switch after the first run.
    /// </summary>
    public void Initialize()
    {
        this.AddFireTower();
        foreach (var buff in Buffs)
        {
            this.AddEffect(buff.Effect, buff.Group, false);
        }

        this.AddEffect(PenaltyEffectNumber, 108, true);
        this.AddEffect(BonusEffectNumber, 255, false);
        this.PlaceSirLesnar();

        var configuration = Season21PlugInConfigurations.GetOrCreate<ArcaWarConfiguration>(this._context, this._gameConfiguration, typeof(ArcaWarPlugIn).GUID, out var plugIn);
        if (configuration is null || configuration.Obelisks.Count > 0)
        {
            return;
        }

        // The first application turns the feature on; later ones leave the operator's switch alone.
        plugIn.IsActive = true;
        plugIn.SetConfiguration(CreateConfiguration(), null);
    }

    private static ArcaArea Area((byte StartX, byte StartY, byte EndX, byte EndY) a)
        => new() { StartX = a.StartX, StartY = a.StartY, EndX = a.EndX, EndY = a.EndY };

    private void AddFireTower()
    {
        if (this._gameConfiguration.Monsters.Any(m => m.Number == FireTowerNumber))
        {
            return;
        }

        var row = new Season21Monster(FireTowerNumber, LocalizedString.FromResource(() => MonsterNames.FireTower), 2, 3000, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, [0, 0, 0, 0, 0, 0, 0]);
        var tower = Season21MapsInitializer.CreateMonster(this._context, this._gameConfiguration, row, GameVersion.Season9Episode1);
        tower.ObjectKind = NpcObjectKind.Destructible;
        tower.NumberOfMaximumItemDrops = 0;
        this._gameConfiguration.Monsters.Add(tower);
    }

    private void PlaceSirLesnar()
    {
        var number = SirLesnarSpawnNumber;
        foreach (var (map, x, y, direction) in SirLesnarSpawns)
        {
            Season21NpcSpawns.Place(this._context, this._gameConfiguration, number++, map, SirLesnarNumber, LocalizedString.FromResource(() => MonsterNames.SirLesnar), x, y, (Direction)direction);
        }

        if (this._gameConfiguration.Monsters.FirstOrDefault(m => m.Number == SirLesnarNumber) is not { } lesnar)
        {
            return;
        }

        if (lesnar.NpcWindow == NpcWindow.Undefined)
        {
            lesnar.NpcWindow = NpcWindow.NpcDialog;
        }

        if (lesnar.ItemCraftings.All(c => c.Number != TrophiesOfBattleCrafting.Number)
            && this._gameConfiguration.Items.FirstOrDefault(i => i.Group == 13 && i.Number == 147) is { } trophy)
        {
            lesnar.ItemCraftings.Add(this.CreateTrophyExchange(trophy));
        }
    }

    private ItemCrafting CreateTrophyExchange(ItemDefinition trophy)
    {
        var crafting = this._context.CreateNew<ItemCrafting>();
        crafting.Name = LocalizedString.FromResource(() => CraftingNames.TrophiesOfBattleExchange);
        crafting.Number = TrophiesOfBattleCrafting.Number;
        crafting.ItemCraftingHandlerClassName = typeof(TrophiesOfBattleCrafting).FullName!;
        var settings = this._context.CreateNew<SimpleCraftingSettings>();
        crafting.SimpleCraftingSettings = settings;
        settings.SuccessPercent = TrophyRate;
        settings.MaximumSuccessPercent = 100;

        var trophies = this._context.CreateNew<ItemCraftingRequiredItem>();
        trophies.PossibleItems.Add(trophy);
        trophies.MinimumAmount = 1;
        trophies.MaximumAmount = TrophiesOfBattleCrafting.TierSize * 10;
        trophies.AddPercentage = TrophyRateAdd;
        settings.RequiredItems.Add(trophies);

        for (byte tier = 1; tier <= 10; tier++)
        {
            foreach (var (group, number, first, step) in TrophyBagItems)
            {
                if (this._gameConfiguration.Items.FirstOrDefault(i => i.Group == group && i.Number == number) is not { } definition)
                {
                    continue;
                }

                var result = this._context.CreateNew<ItemCraftingResultItem>();
                result.ItemDefinition = definition;
                result.Reference = tier;
                result.Durability = (byte)Math.Min(byte.MaxValue, first + (step * (tier - 1)));
                settings.ResultItems.Add(result);
            }
        }

        return crafting;
    }

    private void AddEffect(short number, byte group, bool stopByDeath)
    {
        if (this._gameConfiguration.MagicEffects.Any(e => e.Number == number))
        {
            return;
        }

        var effect = this._context.CreateNew<MagicEffectDefinition>();
        effect.SetGuid(number);
        effect.Number = number;
        effect.Name = Season21SkillNames.ResourceManager.GetLocalizedString($"Effect{number}");
        effect.SubType = group;
        effect.InformObservers = true;
        effect.SendDuration = true;
        effect.StopByDeath = stopByDeath;
        this._gameConfiguration.MagicEffects.Add(effect);
    }
}
