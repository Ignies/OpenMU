// <copyright file="AddSeason21EventRulesUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using System.Text.Json;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Sets the event grades of Season 21 by total level, its party experience bonus and its Castle Siege registration.
/// </summary>
/// <remarks>
/// A grade whose maximum is above the maximum normal level is checked against the total level
/// (see <c>MiniGameDefinitionExtensions.GetBracketLevel</c>), so the grades keep the Season 6 game levels and the
/// tickets of the Season 6 client.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddSeason21EventRulesUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddSeason21EventRulesUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("8E2B6F14-3A7C-4D59-B1E0-6C94D2A7F358")]
public class AddSeason21EventRulesUpdatePlugIn : UpdatePlugInBase
{
    /// <summary>
    /// The seven grades of Blood Castle, Devil Square and Chaos Castle by total level (Season 21 server data, by the
    /// game level 1 to 7).
    /// </summary>
    private static readonly (int Minimum, int Maximum)[] Grades =
    [
        (50, 399), (400, 799), (800, 999), (1000, 1199), (1200, 1399), (1400, 1599), (1600, int.MaxValue),
    ];

    /// <summary>
    /// The entries a day of S21Emu <c>Event/EventEntryLimit.dat</c> (switched on there): 15 for Blood Castle, Chaos Castle,
    /// Devil Square, Doppelganger, Imperial Guardian, Illusion Temple and the Boss Battle, 10 for the Labyrinth.
    /// </summary>
    private static readonly (MiniGameType Type, int Limit)[] DailyEntryLimits =
    [
        (MiniGameType.BloodCastle, 15), (MiniGameType.ChaosCastle, 15), (MiniGameType.DevilSquare, 15), (MiniGameType.Doppelganger, 15),
        (MiniGameType.ImperialGuardian, 15), (MiniGameType.IllusionTemple, 15), (MiniGameType.BossBattle, 15), (MiniGameType.LabyrinthOfDimensions, 10),
    ];

    /// <summary>
    /// The five grades of Illusion Temple by total level (Season 21 server data).
    /// </summary>
    private static readonly (int Minimum, int Maximum)[] IllusionTempleGrades =
    [
        (180, 399), (400, 799), (800, 1399), (1400, 1599), (1600, int.MaxValue),
    ];

    /// <summary>
    /// The guild members a Castle Siege registration needs (S21Emu <c>GameServerInfo - Event.dat</c> CastleSiegeMinMemberToReg).
    /// </summary>
    private const int CastleSiegeMinMembers = 3;

    /// <summary>
    /// The total level of the guild master a Castle Siege registration needs (S21Emu CastleSiegeMasterMinLevelToReg).
    /// </summary>
    private const int CastleSiegeMinLevel = 400;

    /// <summary>
    /// The Season 6 registration rules (<c>CastleSiegeInitializer</c>), which the update replaces.
    /// </summary>
    private const int Season6CastleSiegeMinMembers = 20;

    /// <summary>
    /// The Season 6 guild master level, see <see cref="Season6CastleSiegeMinMembers"/>.
    /// </summary>
    private const int Season6CastleSiegeMinLevel = 200;

    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddSeason21EventRulesUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddSeason21EventRulesUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 05, 14, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    /// <remarks>
    /// Version 2: the daily entry limits of S21Emu Event/EventEntryLimit.dat.
    /// Version 3: the Castle Siege registration of S21Emu GameServerInfo - Event.dat.
    /// </remarks>
    public override int Version => 3;

    /// <inheritdoc />
    public override DateTime UpdatedAt => new(2026, 11, 09, 12, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21LevelCapsUpdatePlugIn).GUID, typeof(AddCastleSiegeDataUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        foreach (var definition in gameConfiguration.MiniGameDefinitions)
        {
            var grades = definition.Type switch
            {
                MiniGameType.BloodCastle or MiniGameType.DevilSquare or MiniGameType.ChaosCastle => Grades,
                MiniGameType.IllusionTemple => IllusionTempleGrades,
                _ => null,
            };

            if (grades is null || definition.MaximumCharacterLevel > gameConfiguration.MaximumLevel)
            {
                continue;
            }

            if (definition.GameLevel < 1 || definition.GameLevel > grades.Length)
            {
                definition.IsActive = false;
                continue;
            }

            var (minimum, maximum) = grades[definition.GameLevel - 1];
            definition.MinimumCharacterLevel = minimum;
            definition.MaximumCharacterLevel = maximum;
            definition.MinimumSpecialCharacterLevel = minimum;
            definition.MaximumSpecialCharacterLevel = maximum;
            definition.RequiresMasterClass = false;
        }

        SetPartyBonus(context, gameConfiguration);
        SetDailyEntryLimits(context, gameConfiguration);
        SetCastleSiegeRegistration(gameConfiguration);
        return ValueTask.CompletedTask;
    }

    private static void SetCastleSiegeRegistration(GameConfiguration gameConfiguration)
    {
        if (gameConfiguration.CastleSiegeConfiguration is not { } siege)
        {
            return;
        }

        // Values the operator changed stay.
        if (siege.RegisterMinMembers == Season6CastleSiegeMinMembers)
        {
            siege.RegisterMinMembers = CastleSiegeMinMembers;
        }

        if (siege.RegisterMinLevel == Season6CastleSiegeMinLevel)
        {
            siege.RegisterMinLevel = CastleSiegeMinLevel;
        }
    }

    private static void SetDailyEntryLimits(IContext context, GameConfiguration gameConfiguration)
    {
        // Limits the operator set stay.
        if (Season21PlugInConfigurations.GetOrCreate<Season21EventsConfiguration>(context, gameConfiguration, typeof(Season21EventsFeaturePlugIn).GUID, out var plugIn) is not { DailyEntryLimits.Count: 0 } configuration)
        {
            return;
        }

        configuration.DailyEntryLimits = DailyEntryLimits.Select(l => new EventEntryLimit { Type = l.Type, Limit = l.Limit }).ToList();
        plugIn.SetConfiguration(configuration, null);
    }

    private static void SetPartyBonus(IContext context, GameConfiguration gameConfiguration)
    {
        var typeId = typeof(PartyExperienceBonusPlugIn).GUID;
        var plugInConfiguration = gameConfiguration.PlugInConfigurations.FirstOrDefault(c => c.TypeId == typeId);
        if (plugInConfiguration is null)
        {
            plugInConfiguration = context.CreateNew<PlugInConfiguration>();
            plugInConfiguration.SetGuid(typeId);
            plugInConfiguration.TypeId = typeId;
            plugInConfiguration.IsActive = true;
            gameConfiguration.PlugInConfigurations.Add(plugInConfiguration);
        }

        if (!IsDefaultOrEmpty(plugInConfiguration.CustomConfiguration))
        {
            return;
        }

        plugInConfiguration.SetConfiguration(PartyExperienceBonusConfiguration.CreateSeason21(), null);
    }

    /// <summary>
    /// Determines whether a stored party bonus configuration is absent or the default one, which the update may
    /// replace. The bonus list is read with or without the reference metadata of the data initialization.
    /// </summary>
    private static bool IsDefaultOrEmpty(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return true;
        }

        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty(nameof(PartyExperienceBonusConfiguration.Bonus), out var bonus))
        {
            return true;
        }

        if (bonus.ValueKind == JsonValueKind.Object && bonus.TryGetProperty("$values", out var values))
        {
            bonus = values;
        }

        return bonus.ValueKind == JsonValueKind.Array
               && bonus.EnumerateArray().Select(e => e.GetDouble()).SequenceEqual(new PartyExperienceBonusConfiguration().Bonus);
    }
}
