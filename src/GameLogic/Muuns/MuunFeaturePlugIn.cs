// <copyright file="MuunFeaturePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Muuns;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Views.Muuns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The Muun system of Season 21: the Muun inventory with its two equipped Muuns, their options on the owner, level up,
/// evolution, the energy converter, the exchange NPC and the mounts. Shows the Muuns to the player on every map entry,
/// checks the conditions of the options periodically and lets equipped Muuns lose health.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.MuunFeaturePlugIn_Name), Description = nameof(PlugInResources.MuunFeaturePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("5C2E8A17-9B4D-4F36-A0E1-7D3B6C9F2E48")]
public class MuunFeaturePlugIn : IFeaturePlugIn, IDisabledByDefault, ISupportCustomConfiguration<MuunConfiguration>, ISupportDefaultCustomConfiguration, IObjectAddedToMapPlugIn, IPeriodicTaskPlugIn, IPlayerTalkToNpcPlugIn
{
    /// <summary>
    /// The feature id of the fork marker.
    /// </summary>
    public const string FeatureId = "muun";

    private MuunConfiguration? _indexedConfiguration;
    private Dictionary<short, MuunDefinition> _muuns = [];
    private ILookup<short, MuunOptionDefinition> _options = Array.Empty<MuunOptionDefinition>().ToLookup(o => o.Group);
    private ILookup<short, MuunAddOptionDefinition> _addOptions = Array.Empty<MuunAddOptionDefinition>().ToLookup(o => o.Type);
    private DateTime _nextCheckUtc = DateTime.UtcNow;
    private DateTime _nextDecayUtc = DateTime.UtcNow;

    /// <inheritdoc />
    public MuunConfiguration? Configuration { get; set; }

    /// <summary>
    /// Gets the active plugin of the game, or <c>null</c> when the Muun system is off.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The active plugin.</returns>
    public static MuunFeaturePlugIn? Get(IGameContext gameContext) => gameContext.FeaturePlugIns.GetPlugIn<MuunFeaturePlugIn>();

    /// <summary>
    /// Gets the current configuration, or the default one.
    /// </summary>
    /// <returns>The configuration.</returns>
    public MuunConfiguration GetConfiguration() => this.Configuration ??= new MuunConfiguration();

    /// <summary>
    /// Gets the Muun of an item, if the item is a Muun.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The Muun.</returns>
    public MuunDefinition? GetMuun(Item item)
        => item.Definition is { } definition ? this.GetMuun(definition.Group, definition.Number) : null;

    /// <summary>
    /// Gets the Muun of an item group and number.
    /// </summary>
    /// <param name="group">The item group.</param>
    /// <param name="number">The item number.</param>
    /// <returns>The Muun.</returns>
    public MuunDefinition? GetMuun(byte group, short number)
    {
        this.Index();
        return group == this.GetConfiguration().ItemGroup && this._muuns.TryGetValue(number, out var muun) ? muun : null;
    }

    /// <summary>
    /// Gets the option row of a Muun for a total level of its owner.
    /// </summary>
    /// <param name="muun">The Muun.</param>
    /// <param name="totalLevel">The total level of the owner.</param>
    /// <returns>The option row.</returns>
    public MuunOptionDefinition? GetOption(MuunDefinition muun, int totalLevel)
    {
        this.Index();
        return this._options[muun.OptionGroup].FirstOrDefault(o => o.AppliesTo(totalLevel));
    }

    /// <summary>
    /// Gets the additional option row of a Muun for a total level of its owner.
    /// </summary>
    /// <param name="muun">The Muun.</param>
    /// <param name="totalLevel">The total level of the owner.</param>
    /// <returns>The additional option row.</returns>
    public MuunAddOptionDefinition? GetAddOption(MuunDefinition muun, int totalLevel)
    {
        this.Index();
        return this._addOptions[muun.AddOptionType].FirstOrDefault(o => o.AppliesTo(totalLevel));
    }

    /// <inheritdoc />
    public object CreateDefaultConfig() => new MuunConfiguration();

    /// <inheritdoc />
    public async ValueTask ObjectAddedToMapAsync(GameMap map, ILocateable addedObject)
    {
        if (addedObject is Player player && player.ViewPlugIns.GetPlugIn<IMuunViewPlugIn>() is not null)
        {
            await MuunService.ShowAsync(player).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async ValueTask ExecuteTaskAsync(GameContext gameContext)
    {
        var now = DateTime.UtcNow;
        await this.UseAttackSkillsAsync(gameContext, now).ConfigureAwait(false);
        if (now < this._nextCheckUtc)
        {
            return;
        }

        var configuration = this.GetConfiguration();
        this._nextCheckUtc = now + configuration.CheckInterval;
        var decay = configuration.HealthDecayAmount > 0 && now >= this._nextDecayUtc;
        if (decay)
        {
            this._nextDecayUtc = now + configuration.HealthDecayInterval;
        }

        foreach (var player in await gameContext.GetPlayersAsync().ConfigureAwait(false))
        {
            if (player.PlayerState.CurrentState != PlayerState.EnteredWorld || player.SelectedCharacter?.MuunInventory is null)
            {
                continue;
            }

            try
            {
                MuunService.AddPlayTime(player, now);
                if (decay)
                {
                    await MuunService.DecayHealthAsync(player, configuration.HealthDecayAmount).ConfigureAwait(false);
                }

                await MuunService.RefreshAsync(player, false).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                player.Logger.LogError(ex, "Error when updating the Muuns of player '{player}'.", player);
            }
        }
    }

    /// <inheritdoc />
    public void ForceStart() => this._nextCheckUtc = DateTime.UtcNow;

    /// <inheritdoc />
    public async ValueTask PlayerTalksToNpcAsync(Player player, NonPlayerCharacter npc, NpcTalkEventArgs eventArgs)
    {
        var configuration = this.GetConfiguration();
        if (npc.Definition.Number != configuration.ExchangeNpcNumber)
        {
            return;
        }

        eventArgs.HasBeenHandled = true;
        if (player.ViewPlugIns.GetPlugIn<IMuunViewPlugIn>() is not { } view)
        {
            await player.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.MuunNeedsNewerClient)).ConfigureAwait(false);
            return;
        }

        eventArgs.LeavesDialogOpen = true;
        await view.ShowMuunExchangeAsync(configuration.ExchangeOffers).ConfigureAwait(false);
    }

    private async ValueTask UseAttackSkillsAsync(GameContext gameContext, DateTime now)
    {
        foreach (var player in await gameContext.GetPlayersAsync().ConfigureAwait(false))
        {
            if (player.PlayerState.CurrentState != PlayerState.EnteredWorld || player.SelectedCharacter?.MuunInventory is null)
            {
                continue;
            }

            try
            {
                await MuunService.UseAttackSkillsAsync(player, now).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                player.Logger.LogError(ex, "Error when the Muun of player '{player}' attacked.", player);
            }
        }
    }

    private void Index()
    {
        var configuration = this.GetConfiguration();
        if (ReferenceEquals(configuration, this._indexedConfiguration))
        {
            return;
        }

        this._muuns = configuration.Muuns.GroupBy(m => m.Number).ToDictionary(g => g.Key, g => g.First());
        this._options = configuration.Options.ToLookup(o => o.Group);
        this._addOptions = configuration.AddOptions.ToLookup(o => o.Type);
        this._indexedConfiguration = configuration;
    }
}
