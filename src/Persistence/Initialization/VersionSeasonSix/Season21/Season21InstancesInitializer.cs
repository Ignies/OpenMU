// <copyright file="Season21InstancesInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.Interfaces;

/// <summary>
/// Adds the party instances of Season 21 (<see cref="MiniGameType.PartyInstance"/>, S21Emu <c>InstanceSystem.txt</c>).
/// Keeps what exists.
/// </summary>
/// <remarks>
/// The game level of an instance is its content type of the data: 2 is the trial of the fourth and fifth class quest
/// (Trial Area, up to 5 players). The Labyrinth of Dimensions (1) and the Imperial Guardian (3-6) are added by their
/// own updates.
/// </remarks>
internal class Season21InstancesInitializer
{
    /// <summary>
    /// The game level (content type) of the trial.
    /// </summary>
    internal const byte TrialContentType = 2;

    /// <summary>
    /// The number of the exit gate into the Trial Area, gate 540 of the data.
    /// </summary>
    internal const short TrialGateNumber = 540;

    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21InstancesInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Season21InstancesInitializer(IContext context, GameConfiguration gameConfiguration)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
    }

    /// <summary>
    /// Adds what the configuration doesn't contain yet.
    /// </summary>
    public void Initialize()
    {
        if (this._gameConfiguration.Maps.FirstOrDefault(m => m.Number == 121 && m.Discriminator == 0) is not { } trialArea)
        {
            return;
        }

        var gate = this.GetOrCreateGate(trialArea, TrialGateNumber, 147, 27, 148, 28);
        this.GetOrCreateInstance(
            TrialContentType,
            LocalizedString.FromResource(() => MiniGameNames.Season21Trial),
            LocalizedString.FromResource(() => MiniGameDescriptions.Season21Trial),
            gate,
            5,
            TimeSpan.FromMinutes(30));
    }

    /// <summary>
    /// Gets or creates the exit gate of a map, by its number.
    /// </summary>
    /// <param name="map">The map.</param>
    /// <param name="number">The number.</param>
    /// <param name="x1">The x1.</param>
    /// <param name="y1">The y1.</param>
    /// <param name="x2">The x2.</param>
    /// <param name="y2">The y2.</param>
    /// <returns>The gate.</returns>
    internal ExitGate GetOrCreateGate(GameMapDefinition map, short number, byte x1, byte y1, byte x2, byte y2)
    {
        var id = GuidHelper.CreateGuid<ExitGate>(map.Number, number);
        if (map.ExitGates.FirstOrDefault(g => g.GetId() == id) is { } existing)
        {
            return existing;
        }

        var gate = this._context.CreateNew<ExitGate>();
        gate.SetGuid(map.Number, number);
        gate.Map = map;
        (gate.X1, gate.Y1, gate.X2, gate.Y2) = (x1, y1, x2, y2);
        map.ExitGates.Add(gate);
        return gate;
    }

    /// <summary>
    /// Gets or creates the party instance of a content type.
    /// </summary>
    /// <param name="contentType">The content type, which is the game level.</param>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="entrance">The entrance.</param>
    /// <param name="maximumPlayers">The maximum players.</param>
    /// <param name="duration">The duration.</param>
    /// <returns>The definition.</returns>
    internal MiniGameDefinition GetOrCreateInstance(byte contentType, LocalizedString name, LocalizedString description, ExitGate entrance, int maximumPlayers, TimeSpan duration)
    {
        var id = GuidHelper.CreateGuid<MiniGameDefinition>((short)MiniGameType.PartyInstance, contentType);
        if (this._gameConfiguration.MiniGameDefinitions.FirstOrDefault(d => d.GetId() == id) is { } existing)
        {
            return existing;
        }

        var definition = this._context.CreateNew<MiniGameDefinition>();
        definition.SetGuid((short)MiniGameType.PartyInstance, contentType);
        definition.Type = MiniGameType.PartyInstance;
        definition.GameLevel = contentType;
        definition.Name = name;
        definition.Description = description;
        definition.MapCreationPolicy = MiniGameMapCreationPolicy.OnePerParty;
        definition.AllowParty = true;
        definition.ArePlayerKillersAllowedToEnter = true;
        definition.MaximumPlayerCount = maximumPlayers;
        definition.MinimumPlayerCount = 1;
        definition.MinimumCharacterLevel = 1;
        definition.MaximumCharacterLevel = int.MaxValue;
        definition.MinimumSpecialCharacterLevel = 1;
        definition.MaximumSpecialCharacterLevel = int.MaxValue;
        definition.EnterDuration = TimeSpan.FromSeconds(10);
        definition.GameDuration = duration;
        definition.ExitDuration = TimeSpan.FromMinutes(1);
        definition.Entrance = entrance;
        this._gameConfiguration.MiniGameDefinitions.Add(definition);
        return definition;
    }
}
