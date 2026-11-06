// <copyright file="Season21MuunsInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Muuns;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.Persistence.Initialization.Properties;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The Muuns of Season 21 (<c>Item/Muun - Settings</c>): the configuration of the <see cref="MuunFeaturePlugIn"/>, which the
/// first application switches on, the evolution target option of the evolution stones and Monica, the exchange NPC, in Elbeland.
/// </summary>
internal class Season21MuunsInitializer
{
    /// <summary>
    /// The number of Monica, the Muun exchange NPC.
    /// </summary>
    public const short Monica = 682;

    /// <summary>
    /// The item group of the Muuns.
    /// </summary>
    internal const byte MuunGroup = 16;

    /// <summary>
    /// The number of the item option definition which holds the evolution target of the evolution stones.
    /// </summary>
    internal const short EvolutionTargetOptionNumber = 0x1680;

    private const short FirstSpawnNumber = 6820;

    private static readonly TimeSpan FirstHealthDecayInterval = TimeSpan.FromMinutes(10);

    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21MuunsInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Season21MuunsInitializer(IContext context, GameConfiguration gameConfiguration)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
    }

    /// <summary>
    /// Creates the configuration of the plugin from the data.
    /// </summary>
    /// <returns>The configuration.</returns>
    public static MuunConfiguration CreateConfiguration()
    {
        var data = Season21MuunData.Data;
        var evolved = data.Muuns.Where(m => m.Evolution >= 0).Select(m => m.Evolution).ToHashSet();
        return new MuunConfiguration
        {
            ItemGroup = MuunGroup,
            EvolutionStoneEnergy = data.StoneEnergy,
            RankEnergy = data.RankEnergy.Select(r => new MuunEnergyPoints { Key = r.Rank, Points = r.Points }).ToList(),
            LevelEnergy = data.LevelEnergy.Select(r => new MuunEnergyPoints { Key = r.Level, Points = r.Points }).ToList(),
            ExchangeNpcNumber = Monica,
            ExchangeOffers = data.Exchange.OrderBy(e => e.Index).Select(e => new MuunExchangeOffer
            {
                CostGroup = e.CostGroup,
                CostNumber = e.CostNumber,
                CostCount = e.CostCount,
                ResultGroup = e.Group,
                ResultNumber = e.Number,
                TargetNumber = e.TargetNumber,
            }).ToList(),
            Muuns = data.Muuns.Select(m => new MuunDefinition
            {
                Number = m.Number,
                Kind = Enum.IsDefined((MuunKind)m.Type) ? (MuunKind)m.Type : MuunKind.Normal,
                Grade = (MuunGrade)m.SubType,
                Rank = m.Rank,
                OptionGroup = m.OptionGroup,
                AddOptionType = m.AddOption,
                AddOptionDays = m.Days,
                EvolvesTo = m.Evolution >= 0 ? m.Evolution : null,
                IsEvolved = evolved.Contains(m.Number),
            }).ToList(),
            Options = data.Options.Select(o => new MuunOptionDefinition
            {
                Group = o.Group,
                Type = (MuunOptionType)o.Type,
                MinimumLevel = o.Minimum,
                MaximumLevel = o.Maximum,
                Values = [.. o.Values],
                Condition = CreateCondition(o.Condition),
                EvolvedCondition = CreateCondition(o.EvolvedCondition),
                Name = o.Name,
            }).ToList(),
            AddOptions = data.AddOptions.Select(a => new MuunAddOptionDefinition
            {
                Type = a.Type,
                MinimumLevel = a.Minimum,
                MaximumLevel = a.Maximum,
                Kind = (MuunAddOptionKind)a.Kind,
                Values = [.. a.Values],
            }).ToList(),
        };
    }

    /// <summary>
    /// Applies the data. A configuration with Muuns stays, with the operator's switch.
    /// </summary>
    public void Initialize()
    {
        this.AddEvolutionTargetOption();
        var number = FirstSpawnNumber;
        foreach (var npc in Season21MuunData.Data.Monica)
        {
            Season21NpcSpawns.Place(this._context, this._gameConfiguration, ++number, npc.Map, npc.Npc, LocalizedString.FromResource(() => MonsterNames.Monica), npc.X, npc.Y, (Direction)npc.Direction);
        }

        var configuration = Season21PlugInConfigurations.GetOrCreate<MuunConfiguration>(this._context, this._gameConfiguration, typeof(MuunFeaturePlugIn).GUID, out var plugIn);
        if (configuration is { Muuns.Count: 0 })
        {
            plugIn.SetConfiguration(CreateConfiguration(), null);
            plugIn.IsActive = true;
        }
        else if (configuration is not null && configuration.HealthDecayInterval == FirstHealthDecayInterval)
        {
            // Version 1 wrote the designed 10 minutes; a value the operator changed stays.
            configuration.HealthDecayInterval = new MuunConfiguration().HealthDecayInterval;
            plugIn.SetConfiguration(configuration, null);
        }
    }

    private static MuunCondition CreateCondition(int[] values)
        => new() { Type = (MuunConditionType)values[0], Value1 = values[1], Value2 = values.Length > 2 ? values[2] : -1 };

    private void AddEvolutionTargetOption()
    {
        var configuration = CreateConfiguration();
        var stones = this._gameConfiguration.Items
            .Where(i => i.Group == MuunGroup && (i.Number == configuration.EvolutionStoneNumber || i.Number == configuration.TempestEvolutionStoneNumber))
            .ToList();
        if (stones.Count == 0)
        {
            return;
        }

        var id = GuidHelper.CreateGuid<ItemOptionDefinition>(EvolutionTargetOptionNumber);
        if (this._gameConfiguration.ItemOptions.FirstOrDefault(o => o.GetId() == id) is not { } definition)
        {
            definition = this._context.CreateNew<ItemOptionDefinition>();
            definition.SetGuid(EvolutionTargetOptionNumber);
            definition.Name = "Muun Evolution Target";
            definition.MaximumOptionsPerItem = 1;
            var option = this._context.CreateNew<IncreasableItemOption>();
            option.SetGuid(EvolutionTargetOptionNumber, 0);
            option.OptionType = this.GetOptionType(Season21ItemOptionTypes.MuunEvolutionTarget);
            definition.PossibleOptions.Add(option);
            this._gameConfiguration.ItemOptions.Add(definition);
        }

        foreach (var stone in stones.Where(s => !s.PossibleItemOptions.Contains(definition)))
        {
            stone.PossibleItemOptions.Add(definition);
        }
    }

    private ItemOptionType GetOptionType(ItemOptionType type)
    {
        if (this._gameConfiguration.ItemOptionTypes.FirstOrDefault(t => t.Id == type.Id) is { } existing)
        {
            return existing;
        }

        var persistent = this._context.CreateNew<ItemOptionType>();
        persistent.Id = type.Id;
        persistent.Name = type.Name;
        persistent.Description = type.Description;
        persistent.IsVisible = type.IsVisible;
        this._gameConfiguration.ItemOptionTypes.Add(persistent);
        return persistent;
    }
}
