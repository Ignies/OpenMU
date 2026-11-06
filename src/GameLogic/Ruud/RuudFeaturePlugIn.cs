// <copyright file="RuudFeaturePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Ruud;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Views.Economy;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Ruud, the second currency of Season 21: the Ruud shop at its NPC, the Ruud of killed monsters,
/// and the Ruud shown to the player when it enters a map.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.RuudFeaturePlugIn_Name), Description = nameof(PlugInResources.RuudFeaturePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("D41A6C27-8B3E-4F09-9A5D-2E7C1B6F3A80")]
public class RuudFeaturePlugIn : IFeaturePlugIn, IDisabledByDefault, ISupportCustomConfiguration<RuudConfiguration>, ISupportDefaultCustomConfiguration,
    IPlayerTalkToNpcPlugIn, IAttackableGotKilledPlugIn, IObjectAddedToMapPlugIn
{
    /// <summary>
    /// The feature id of the fork marker.
    /// </summary>
    public const string FeatureId = "ruud";

    /// <inheritdoc />
    public RuudConfiguration? Configuration { get; set; }

    /// <summary>
    /// Gets the active plugin of the game, or <c>null</c> when Ruud is off.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The active plugin.</returns>
    public static RuudFeaturePlugIn? Get(IGameContext gameContext) => gameContext.FeaturePlugIns.GetPlugIn<RuudFeaturePlugIn>();

    /// <summary>
    /// Determines whether the NPC opens the Ruud shop, i.e. Ruud is on and the NPC is its shop NPC.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <param name="npcNumber">The number of the NPC.</param>
    /// <returns><c>true</c>, if the NPC opens the Ruud shop.</returns>
    public static bool IsShopNpc(IGameContext gameContext, short npcNumber)
        => Get(gameContext) is { } plugIn && (plugIn.Configuration ?? new RuudConfiguration()).ShopNpcNumber == npcNumber;

    /// <summary>
    /// Gets the shop items which the character of the player may buy, with their index in the configuration.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The shop items.</returns>
    public IReadOnlyList<(int Index, RuudShopEntry Entry)> GetShopEntries(Player player)
    {
        var baseClass = player.SelectedCharacter?.CharacterClass is { } characterClass ? ClassCodes.GetBaseClass(characterClass.Number) : null;
        var items = player.GameContext.Configuration.Items;
        return (this.Configuration?.ShopEntries ?? [])
            .Select((entry, index) => (index, entry))
            .Where(e => (e.entry.Class is null || e.entry.Class == baseClass)
                        && items.Any(d => d.IsActive && d.Group == e.entry.Group && d.Number == e.entry.Number))
            .ToList();
    }

    /// <inheritdoc />
    public object CreateDefaultConfig() => new RuudConfiguration();

    /// <inheritdoc />
    public async ValueTask PlayerTalksToNpcAsync(Player player, NonPlayerCharacter npc, NpcTalkEventArgs eventArgs)
    {
        if (npc.Definition.Number != (this.Configuration ?? new RuudConfiguration()).ShopNpcNumber)
        {
            return;
        }

        eventArgs.HasBeenHandled = true;
        if (player.ViewPlugIns.GetPlugIn<IRuudViewPlugIn>() is not { } view)
        {
            await player.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.RuudShopNeedsNewerClient), player.GetRuud()).ConfigureAwait(false);
            return;
        }

        eventArgs.LeavesDialogOpen = true;
        await view.ShowRuudAsync(player.GetRuud(), 0).ConfigureAwait(false);
        await view.ShowRuudShopAsync(this.GetShopEntries(player)).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask AttackableGotKilledAsync(IAttackable killed, IAttacker? killer)
    {
        if (killed is not Monster monster
            || killer is not Player player
            || this.Configuration?.MonsterRewards.FirstOrDefault(r => r.MonsterNumber == monster.Definition.Number) is not { } reward
            || reward.Maximum <= 0)
        {
            return;
        }

        var amount = Rand.NextInt(Math.Max(reward.Minimum, 0), Math.Max(reward.Minimum, reward.Maximum) + 1);
        await player.AddRuudAsync(amount).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask ObjectAddedToMapAsync(GameMap map, ILocateable addedObject)
    {
        if (addedObject is Player player && player.ViewPlugIns.GetPlugIn<IRuudViewPlugIn>() is { } view)
        {
            await view.ShowRuudAsync(player.GetRuud(), 0).ConfigureAwait(false);
        }
    }
}
