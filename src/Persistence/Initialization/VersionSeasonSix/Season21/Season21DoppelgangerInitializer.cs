// <copyright file="Season21DoppelgangerInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using System.Text.Json;
using MUnique.OpenMU.GameLogic.MiniGames.Doppelganger;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Renews the Doppelganger for Season 21 (S21Emu <c>Event/DoubleGoerRenewal/DoppelGanger_EventInfo.xml</c>,
/// <c>EventItemBag</c> 78-87 and 452-455): seven rooms by total level, at least two players.
/// </summary>
/// <remarks>
/// The renewal's race of several parties on parallel lanes of one map (<c>DoppelGanger_PositionInfo.xml</c>) needs the
/// lanes in the client; the event keeps one party per map copy and its path, so a room is the scaling of that copy.
/// </remarks>
internal class Season21DoppelgangerInitializer
{
    /// <summary>
    /// The minimum level of the event (room 0 of the data).
    /// </summary>
    private const int MinimumLevel = 50;

    /// <summary>
    /// The minimum players of a game (<c>MinUserInParty</c> of the data).
    /// </summary>
    private const int MinimumPlayers = 2;

    /// <summary>
    /// The rooms: the highest total level, the multipliers of level, health, damage and defense, the experience and the
    /// bags of the silver (interim) and golden (final) chests.
    /// </summary>
    private static readonly (int MaximumLevel, float Level, float Health, float Damage, float Defense, long Experience, short Silver, short Golden)[] Rooms =
    [
        (399, 2.2f, 4.1f, 1.43f, 1.8f, 60_000, 78, 83),
        (799, 3.93f, 13.2f, 3.91f, 4.65f, 100_000, 79, 84),
        (899, 5.25f, 22.8f, 5.68f, 8f, 130_000, 80, 85),
        (1099, 5.65f, 31.8f, 7.38f, 9.8f, 160_000, 81, 86),
        (1299, 6.79f, 38f, 8.48f, 11.6f, 190_000, 82, 87),
        (1499, 6.79f, 49f, 11.38f, 16.7f, 210_000, 452, 454),
        (int.MaxValue, 6.79f, 59f, 13.82f, 21.2f, 240_000, 453, 455),
    ];

    /// <summary>
    /// The items of the bags, as <c>group * 512 + number</c>.
    /// </summary>
    private static readonly Dictionary<short, int[]> Bags = new()
    {
        [78] = [7181, 7182, 7184, 7190, 7210, 6159],
        [79] = [7182, 7181, 6159, 7184, 7190, 7210, 7504],
        [80] = [7182, 7181, 6159, 7184, 7190, 7210, 7457],
        [81] = [7182, 7181, 6159, 7184, 7190, 7210, 7456],
        [82] = [7182, 7181, 6159, 7184, 7190, 7210, 7455],
        [452] = [7182, 7181, 6159, 7184, 7190, 7210, 7455],
        [453] = [7182, 7181, 6159, 7184, 7190, 7210, 7455],
        [83] = [7182, 7181, 6159, 7184, 7190, 7210, 7504],
        [84] = [7182, 7181, 6159, 7184, 7190, 7210, 7504],
        [85] = [7182, 7181, 6159, 7184, 7190, 7210, 7504, 7457],
        [86] = [7182, 7181, 6159, 7184, 7190, 7210, 7456],
        [87] = [6708, 6709, 6693, 7581, 7221, 7520, 7615, 7181, 7182, 7184, 7190, 7210],
        [454] = [6708, 6709, 6693, 7581, 7221, 7520, 7615, 7181, 7182, 7184, 7190, 7210],
        [455] = [6708, 6709, 6693, 7581, 7221, 7520, 7615, 7181, 7182, 7184, 7190, 7210],
    };

    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21DoppelgangerInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Season21DoppelgangerInitializer(IContext context, GameConfiguration gameConfiguration)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
    }

    /// <summary>
    /// Applies the renewal: the entry rules of the definitions and the rooms of the event configuration.
    /// </summary>
    public void Initialize()
    {
        foreach (var definition in this._gameConfiguration.MiniGameDefinitions.Where(d => d.Type == MiniGameType.Doppelganger))
        {
            definition.MinimumCharacterLevel = MinimumLevel;
            definition.MaximumCharacterLevel = int.MaxValue;
            definition.MinimumSpecialCharacterLevel = MinimumLevel;
            definition.MaximumSpecialCharacterLevel = int.MaxValue;
            definition.MinimumPlayerCount = MinimumPlayers;
        }

        var typeId = typeof(DoppelgangerFeaturePlugIn).GUID;
        if (this._gameConfiguration.PlugInConfigurations.FirstOrDefault(c => c.TypeId == typeId) is not { } plugIn)
        {
            plugIn = this._context.CreateNew<PlugInConfiguration>();
            plugIn.SetGuid(typeId);
            plugIn.TypeId = typeId;
            plugIn.IsActive = true;
            this._gameConfiguration.PlugInConfigurations.Add(plugIn);
        }

        var referenceHandler = new GameConfigurationReferenceHandler(this._gameConfiguration);
        DoppelgangerEventDefinition? configuration;
        try
        {
            configuration = plugIn.GetConfiguration<DoppelgangerEventDefinition>(referenceHandler);
        }
        catch (JsonException)
        {
            // A configuration which can't be read is the operator's; it stays.
            return;
        }

        configuration ??= DoppelgangerEventDefinition.CreateDefault(this._gameConfiguration);
        configuration.MonsterScalings = Rooms.Select(this.CreateRoom).ToList();
        plugIn.SetConfiguration(configuration, referenceHandler);
    }

    private DoppelgangerMonsterScaling CreateRoom((int MaximumLevel, float Level, float Health, float Damage, float Defense, long Experience, short Silver, short Golden) room) => new()
    {
        MaximumPlayerLevel = room.MaximumLevel,
        LevelMultipliers = [room.Level],
        HealthMultipliers = [room.Health],
        DamageMultipliers = [room.Damage],
        DefenseMultipliers = [room.Defense],
        RewardExperience = room.Experience,
        InterimChestItems = Season21ItemBags.GetOrCreate(this._context, this._gameConfiguration, room.Silver, Bags[room.Silver]),
        FinalChestItems = Season21ItemBags.GetOrCreate(this._context, this._gameConfiguration, room.Golden, Bags[room.Golden]),
    };
}
