// <copyright file="HelperPlusPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.HelperPlus;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.PlayerActions;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Progression;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The MU Helper Plus of Season 21: the rules of the helper for the Babylon client (the plus mode for characters of its
/// level, its ranges and cost) and the moves of the plus mode into the hunting zones of the hunting grounds.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.HelperPlusPlugIn_Name), Description = nameof(PlugInResources.HelperPlusPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("4E8B1D63-9A27-4C05-8F3E-6D2A7B1C9E48")]
public class HelperPlusPlugIn : IFeaturePlugIn, ISupportCustomConfiguration<HelperPlusConfiguration>, ISupportDefaultCustomConfiguration, IDisabledByDefault, IObjectAddedToMapPlugIn
{
    private readonly ConditionalWeakTable<Player, object> _plusOn = new();

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    public HelperPlusConfiguration? Configuration { get; set; }

    /// <summary>
    /// Gets the active plugin of a game context.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The plugin, or <c>null</c> when the feature is off.</returns>
    public static HelperPlusPlugIn? Get(IGameContext gameContext) => gameContext.FeaturePlugIns.GetPlugIn<HelperPlusPlugIn>();

    /// <summary>
    /// Applies the cost of the plus mode to a cost of the helper.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="cost">The cost of the normal helper.</param>
    /// <returns>The cost.</returns>
    public static int ApplyCost(Player player, int cost)
        => Get(player.GameContext) is { Configuration: { } configuration } plugIn && plugIn.IsPlusOn(player)
            ? (int)Math.Min(int.MaxValue, (long)cost * configuration.PlusCostPercent / 100)
            : cost;

    /// <inheritdoc />
    public object CreateDefaultConfig() => new HelperPlusConfiguration();

    /// <summary>
    /// Determines whether the plus mode of the player is on.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns><c>true</c>, if it is on.</returns>
    public bool IsPlusOn(Player player) => this._plusOn.TryGetValue(player, out _);

    /// <summary>
    /// Gets the flags of the rules for the player on its map.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The flags.</returns>
    public HelperPlusFlags GetFlags(Player player)
    {
        var flags = HelperPlusFlags.None;
        if (player.CurrentMap?.Definition.Number is { } map && (this.Configuration?.Zones.Exists(z => z.MapNumber == map) ?? false))
        {
            flags |= HelperPlusFlags.MapHasZones;
        }

        if (this.MayUsePlus(player))
        {
            flags |= HelperPlusFlags.MayUsePlus;
        }

        if (this.IsPlusOn(player))
        {
            flags |= HelperPlusFlags.PlusOn;
        }

        return flags;
    }

    /// <summary>
    /// Switches the plus mode on or off and shows the rules; it only goes on for a character which may use it.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="enabled">If the plus mode should be on.</param>
    /// <returns>The task.</returns>
    public async ValueTask SetPlusModeAsync(Player player, bool enabled)
    {
        if (enabled && this.MayUsePlus(player))
        {
            this._plusOn.AddOrUpdate(player, this);
        }
        else
        {
            this._plusOn.Remove(player);
        }

        await this.ShowRulesAsync(player).ConfigureAwait(false);
    }

    /// <summary>
    /// Moves the character into a hunting zone of the plus mode, with the level requirement and costs of the ground's warp.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="zoneIndex">The zone index.</param>
    /// <returns>The result.</returns>
    public async ValueTask<HelperPlusMoveResult> MoveAsync(Player player, short zoneIndex)
    {
        if (this.Configuration is not { } configuration)
        {
            return HelperPlusMoveResult.Off;
        }

        if (!this.IsPlusOn(player) || !this.MayUsePlus(player))
        {
            return HelperPlusMoveResult.PlusModeOff;
        }

        if (configuration.Zones.Find(z => z.Index == zoneIndex) is not { } zone
            || configuration.Grounds.Find(g => g.Index == zone.Ground) is not { } ground
            || player.GameContext.Configuration.WarpList.FirstOrDefault(w => w.Index == ground.WarpIndex) is not { } warp
            || await player.GameContext.GetMapAsync((ushort)zone.MapNumber).ConfigureAwait(false) is not { } map)
        {
            return HelperPlusMoveResult.UnknownZone;
        }

        if (player.PlayerState.CurrentState != PlayerState.EnteredWorld || !player.IsAlive || map.Definition.TryGetRequirementError(player, out _))
        {
            return HelperPlusMoveResult.CanNotMove;
        }

        var requirement = player.SelectedCharacter?.GetEffectiveMoveLevelRequirement(warp.GetLevelRequirement(player.GameContext)) ?? int.MaxValue;
        if (requirement > (player.Attributes?[Stats.Level] ?? 0))
        {
            return HelperPlusMoveResult.LevelTooLow;
        }

        if (!player.TryRemoveMoney(warp.GetCosts(player.GameContext)))
        {
            return HelperPlusMoveResult.NotEnoughMoney;
        }

        await player.WarpToAsync(new ExitGate { Map = map.Definition, X1 = zone.X, X2 = zone.X, Y1 = zone.Y, Y2 = zone.Y }).ConfigureAwait(false);
        return HelperPlusMoveResult.Moved;
    }

    /// <summary>
    /// Shows the rules to the player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The task.</returns>
    public async ValueTask ShowRulesAsync(Player player)
    {
        var configuration = this.Configuration ?? new HelperPlusConfiguration();
        var flags = this.GetFlags(player);
        await player.InvokeViewPlugInAsync<IHelperPlusViewPlugIn>(p => p.ShowHelperPlusRulesAsync(flags, configuration)).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask ObjectAddedToMapAsync(GameMap map, ILocateable addedObject)
    {
        if (addedObject is Player player)
        {
            await this.ShowRulesAsync(player).ConfigureAwait(false);
        }
    }

    private bool MayUsePlus(Player player)
        => this.Configuration is { } configuration
           && (player.Attributes?[Stats.Level] ?? 0) + (player.Attributes?[Stats.MasterLevel] ?? 0) >= configuration.PlusMinimumTotalLevel
           && (!configuration.PartyLeaderOnly || player.Party?.PartyMaster is null || player.Party.PartyMaster == player);
}
