// <copyright file="Season21SoulConverterInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.GameLogic.Rewards;
using MUnique.OpenMU.GameLogic.SoulConverter;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Configures the <see cref="SoulConverterPlugIn"/> with the tiers of <c>MonsterSoulConverter/</c> and their reward bags.
/// </summary>
internal class Season21SoulConverterInitializer
{
    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21SoulConverterInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Season21SoulConverterInitializer(IContext context, GameConfiguration gameConfiguration)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
    }

    /// <summary>
    /// Applies the monster soul converter.
    /// </summary>
    public void Initialize()
    {
        var configuration = Season21PlugInConfigurations.GetOrCreate<SoulConverterConfiguration>(this._context, this._gameConfiguration, typeof(SoulConverterPlugIn).GUID, out var plugIn);
        if (configuration is null || configuration.Tiers.Count > 0)
        {
            // A configuration which can't be read, or one which already has its tiers, is the operator's.
            return;
        }

        var data = Season21WorldSystemsData.Instance.SoulConverter;
        configuration.ActivationsPerWeek = data.Purchases;
        configuration.Tiers = data.Tiers.Select(CreateTier).ToList();

        // The first application turns the feature on; later ones leave the operator's switch alone.
        plugIn.IsActive = true;
        plugIn.SetConfiguration(configuration, null);
    }

    private static SoulConverterTier CreateTier(Season21WorldSystemsData.SoulTierRow row) => new()
    {
        Tier = row.Tier,
        ItemGroup = row.G,
        ItemNumber = row.I,
        MinimumTotalLevel = row.Level,
        MissionCount = row.Missions,
        Missions = row.Monsters.Select(m => new SoulMission { MapNumber = m.Map, MonsterNumber = m.Monster, RequiredSouls = m.Souls, AbsorbRate = m.Rate }).ToList(),
        Rewards = row.Rewards.Select(r => new SoulReward
        {
            Kind = Enum.Parse<SoulRewardKind>(r.Kind, true),
            Draws = r.Draws.Select(d => new SoulRewardDraw
            {
                Chance = d.C,
                MinimumRuud = d.RuudMin,
                MaximumRuud = d.RuudMax,
                Items = d.Items.Select(i => new RewardItem { Group = i.G, Number = i.I, Level = i.L, Count = i.D }).ToList(),
            }).ToList(),
        }).ToList(),
    };
}
