// <copyright file="RemoveOrphanPlugInConfigurationsPlugInBase.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Removes the configurations of plugins this server does not have: upstream releases (0.9.10 to master) created them
/// for plugins it since removed or never had, and the plugin manager warned about each on every start
/// ("Unknown plugin type for id"). Only the listed types are removed: a plugin loaded later from an external assembly
/// is unknown while the update runs, and its configuration must stay.
/// </summary>
public abstract class RemoveOrphanPlugInConfigurationsPlugInBase : UpdatePlugInBase
{
    /// <summary>
    /// The plugin types of upstream OpenMU (v0.9.10, v0.10.0, v0.10.1, v0.11.0 and master 230207a3c) this server does not have.
    /// </summary>
    internal static readonly IReadOnlySet<Guid> RemovedPlugInTypes = new HashSet<Guid>
    {
        new("b3234cc4-1365-463a-b0de-603f9d7d86a2"), // BotChatPlugIn
        new("6e0b6c2f-8f7e-4c71-9c55-2b7c1e4a9d13"), // BotMerchantPlugIn
        new("825b3883-edd9-4638-ba85-7a2852a86997"), // BotSayChatCommandPlugIn
        new("3d6f9337-6a79-4f89-b995-4b7e722e63d1"), // LuckyCoinFeaturePlugIn
        new("fbb2f1ab-8097-41b0-a10e-8323c10d053a"), // GladiatorsHonorConsumeHandlerPlugIn
        new("5ef7c564-b32b-4630-9380-0233becfa663"), // PlasmaStormSkillPlugIn
        new("75b3625c-6e8e-4fff-a0a4-3fa9c3895c70"), // WaitingItemsChatCommandPlugIn
        new("cc3c6192-b0e9-441f-8e7d-d432b562ab86"), // ShowWarpListAfterEnterWorldPlugIn
        new("5c057e6e-fecf-4216-ab78-1918ab00224e"), // TalismanOfResurrectionSpawnGatePlugIn
        new("03e6a271-821e-49b8-b6e9-b24e95b6d16e"), // VulcanusExperiencePlugIn
        new("112a4b4f-814b-415b-9a35-fb3c08c0aa87"), // WaitingItemsDeliveryPlugIn
        new("db463aa9-e091-42e4-a926-c68ba24cfdf9"), // InventoryRequestHandlerPlugIn
        new("e8340144-4dc4-4f7a-8f2b-530ee9853742"), // LuckyCoinCountRequestHandlerPlugIn
        new("8998e334-1714-4f4f-9467-d4dd76915bce"), // LuckyCoinExchangeRequestHandlerPlugIn
        new("14052a86-62e5-4037-9e17-6775a05e2613"), // LuckyCoinRegistrationRequestHandlerPlugIn
        new("7b1c2d3e-4f50-4a61-9b72-8c93d0a1b2c4"), // GuildSoccerScoreUpdatePlugIn
        new("7b1c2d3e-4f50-4a61-9b72-8c93d0a1b2c5"), // GuildSoccerTimeUpdatePlugIn
        new("e7d90509-ebd1-4685-8c5a-015316c2af4b"), // LuckyCoinViewPlugIn
        new("d679bffc-2096-40b8-8bd9-1da7771b9152"), // ShowWarpListExtendedPlugIn
        new("86ae7cba-fa9b-4d1b-b112-c7fa87be4b37"), // AddGladiatorsHonorUpdatePlugIn
        new("9374e428-cf5c-44b1-aab9-0369c77af7c6"), // AddIsQuestItemFlagPlugIn
        new("dc8775d2-78bd-4db3-ab51-fe84b9ddf7a2"), // AddLuckyCoinUpdatePlugIn
        new("e3a8f7c9-2d4b-4a1e-9f3c-8b5d7a6c1e4f"), // AddProjectileCountToTripleShotUpdatePlugIn
        new("cde78d33-bdaf-4e0b-bea4-29dc58f5d72a"), // AddProtectionTalismansUpdatePlugIn
        new("a2377a4a-23e9-40de-bea0-6bb457e14e52"), // AddSealItemDropRateUpdatePlugIn
        new("7f6a597b-9639-4319-97fd-1966e08836eb"), // AddShieldRatioAttributesPlugIn
        new("363c62f6-e039-42c3-b5ec-1990c4c71a16"), // AddTalismanOfResurrectionUpdatePlugIn
        new("fec39e1d-85ac-4768-b7e0-d5b0c8a83c26"), // AdjustIceArrowFreezeDurationUpdatePlugIn
        new("fa5f8867-a3e3-48aa-9453-9d756049aece"), // FieldMonsterSpawnsUpdatePlugIn075
        new("f849a946-b5d7-4590-b398-07edfcbe891f"), // FieldMonsterSpawnsUpdatePlugIn095d
        new("5b20a974-d2aa-4b49-adf6-2b5f97d15828"), // FieldMonsterSpawnsUpdatePlugInSeason6
        new("b9938e4d-8f63-48df-ae45-6739d1e2a8c7"), // FixAreaSkillsUpdatePlugIn
        new("d8d93f9c-9e73-43d3-92b0-ab6b9096322e"), // FixCrywolfGateTerrainUpdatePlugIn
        new("435dba26-4732-4763-a349-fc1033d8be32"), // FixDestroyPhantomArmorNamesPlugIn
        new("a7c9d4e1-8f2b-4a3c-9e6d-7b8f9a0e1c2d"), // FixItemRequirementsPlugIn2
        new("810a707d-6439-451f-98ef-c25e60e98c85"), // MonsterStatsUpdatePlugIn075
        new("cc51ed38-797a-4b5c-821d-49d164ec8011"), // MonsterStatsUpdatePlugIn095d
        new("3cd69b94-3ce1-4f26-8bec-73357d67bfd3"), // MonsterStatsUpdatePlugInSeason6
        new("b1f16cd8-c941-403e-a4b8-18c5f303ef14"), // RestrictWarpsOutsideOfOriginalWarpWindowPlugIn
    };

    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveOrphanPlugInConfigurationsPlugInBase"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory; the removed ids are logged with it.</param>
    protected RemoveOrphanPlugInConfigurationsPlugInBase(ILoggerFactory? loggerFactory)
    {
        this._logger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger(this.GetType());
    }

    /// <inheritdoc />
    public override string Name => "Remove configurations of removed plugins";

    /// <inheritdoc />
    public override string Description => "Removes the configurations of plugins this server no longer has, which the plugin manager warned about on every start.";

    /// <inheritdoc />
    public override bool IsMandatory => true;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 17, 21, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    protected override async ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        // Every row, linked or not: the plugin manager reads them all, so taking one out of the collection is not enough.
        var orphans = (await context.GetAsync<PlugInConfiguration>().ConfigureAwait(false))
            .Where(configuration => RemovedPlugInTypes.Contains(configuration.TypeId))
            .ToList();
        foreach (var orphan in orphans)
        {
            gameConfiguration.PlugInConfigurations.Remove(orphan);
            await context.DeleteAsync(orphan).ConfigureAwait(false);
            this._logger.LogInformation("Removed the configuration of plugin {TypeId}, which this server no longer has.", orphan.TypeId);
        }
    }
}
