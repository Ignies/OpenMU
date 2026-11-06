// <copyright file="Season21EventsInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using System.Text.Json;
using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the events of Season 21 which run as mini games: their maps, definitions and configurations. Keeps what exists,
/// including an event configuration the operator changed.
/// </summary>
internal class Season21EventsInitializer
{
    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    private readonly IReadOnlyList<GameServerConfiguration> _gameServers;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21EventsInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    /// <param name="gameServers">The game servers.</param>
    public Season21EventsInitializer(IContext context, GameConfiguration gameConfiguration, IReadOnlyList<GameServerConfiguration> gameServers)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
        this._gameServers = gameServers;
    }

    /// <summary>
    /// Adds the Tormented Square: map 102, the event for up to 20 players and its rounds, and switches its start on.
    /// </summary>
    public void AddTormentedSquare()
    {
        new Season21MapsInitializer(this._context, this._gameConfiguration, this._gameServers).Initialize(Season21TormentedSquareData.Arena);
        if (this.FindSpawnGate(Season21TormentedSquareData.MapNumber) is not { } entrance)
        {
            return;
        }

        var definition = this.GetOrCreateDefinition(MiniGameType.TormentedSquare, 1, LocalizedString.FromResource(() => MiniGameNames.TormentedSquare), LocalizedString.FromResource(() => MiniGameDescriptions.TormentedSquare), entrance);
        if (definition.MaximumPlayerCount == 0)
        {
            definition.MapCreationPolicy = MiniGameMapCreationPolicy.Shared;
            definition.MaximumPlayerCount = 20;
            definition.MinimumPlayerCount = 1;
            definition.EnterDuration = TimeSpan.FromMinutes(2);
            definition.GameDuration = TimeSpan.FromMinutes(25);
            definition.ExitDuration = TimeSpan.FromMinutes(1);
            definition.AllowParty = true;
        }

        // The first application adds the rounds and switches the timetable on; later ones keep the operator's.
        if (this.UpdateEvents(events => events.TormentedSquare is null && (events.TormentedSquare = Season21TormentedSquareData.CreateDefinition()) is not null))
        {
            this.SwitchOn(typeof(TormentedSquareStartPlugIn).GUID);
        }
    }

    private ExitGate? FindSpawnGate(byte mapNumber)
        => this._gameConfiguration.Maps.FirstOrDefault(m => m.Number == mapNumber && m.Discriminator == 0)?.ExitGates.FirstOrDefault(g => g.IsSpawnGate);

    private MiniGameDefinition GetOrCreateDefinition(MiniGameType type, byte level, LocalizedString name, LocalizedString description, ExitGate entrance)
    {
        var id = GuidHelper.CreateGuid<MiniGameDefinition>((short)type, level);
        if (this._gameConfiguration.MiniGameDefinitions.FirstOrDefault(d => d.GetId() == id) is { } existing)
        {
            return existing;
        }

        var definition = this._context.CreateNew<MiniGameDefinition>();
        definition.SetGuid((short)type, level);
        definition.Type = type;
        definition.GameLevel = level;
        definition.Name = name;
        definition.Description = description;
        definition.Entrance = entrance;
        definition.ArePlayerKillersAllowedToEnter = true;
        definition.MinimumCharacterLevel = 1;
        definition.MaximumCharacterLevel = int.MaxValue;
        definition.MinimumSpecialCharacterLevel = 1;
        definition.MaximumSpecialCharacterLevel = int.MaxValue;
        this._gameConfiguration.MiniGameDefinitions.Add(definition);
        return definition;
    }

    /// <summary>
    /// Changes the configuration of the events, when the change says it changed something.
    /// </summary>
    /// <returns><c>true</c>, if the configuration changed.</returns>
    private bool UpdateEvents(Func<Season21EventsConfiguration, bool> update)
    {
        var plugIn = this.GetOrCreatePlugIn(typeof(Season21EventsFeaturePlugIn).GUID);
        Season21EventsConfiguration configuration;
        try
        {
            configuration = plugIn.GetConfiguration<Season21EventsConfiguration>(new Updates.IdIgnoringReferenceHandler()) ?? new Season21EventsConfiguration();
        }
        catch (JsonException)
        {
            // A configuration which can't be read is the operator's; it stays.
            return false;
        }

        if (!update(configuration))
        {
            return false;
        }

        plugIn.SetConfiguration(configuration, null);
        plugIn.IsActive = true;
        return true;
    }

    private void SwitchOn(Guid typeId) => this.GetOrCreatePlugIn(typeId).IsActive = true;

    private PlugInConfiguration GetOrCreatePlugIn(Guid typeId)
    {
        if (this._gameConfiguration.PlugInConfigurations.FirstOrDefault(c => c.TypeId == typeId) is { } existing)
        {
            return existing;
        }

        var plugIn = this._context.CreateNew<PlugInConfiguration>();
        plugIn.SetGuid(typeId);
        plugIn.TypeId = typeId;
        this._gameConfiguration.PlugInConfigurations.Add(plugIn);
        return plugIn;
    }
}
