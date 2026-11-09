// <copyright file="GuideQuestPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.GuideQuests;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Rewards;
using MUnique.OpenMU.GameLogic.Views.Inventory;
using MUnique.OpenMU.GameLogic.Views.Quest;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The guide quests of Season 21 (Readme.md): a chain of steps per class, accepted and completed from the client's guide window.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.GuideQuestPlugIn_Name), Description = nameof(PlugInResources.GuideQuestPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("7A2C5E91-3D84-4B6F-A0E7-9C1B4D6F2E58")]
public class GuideQuestPlugIn : IFeaturePlugIn, ISupportCustomConfiguration<GuideQuestConfiguration>, ISupportDefaultCustomConfiguration, IDisabledByDefault,
    IAttackableGotKilledPlugIn, IAttackableMovedPlugIn, ICharacterLevelUpPlugIn, IObjectAddedToMapPlugIn
{
    /// <summary>
    /// The quest group of the Season 6 class quests (OpenMU's <c>QuestConstants.LegacyQuestGroup</c>).
    /// </summary>
    private const short LegacyQuestGroup = 0;

    private readonly ConditionalWeakTable<Player, GuideQuestStep> _moveTargets = new();

    private GuideQuestConfiguration? _configuration;

    private Dictionary<short, GuideQuestStep> _steps = [];

    private CharacterStateStore<CharacterGuideQuest> _store = CharacterStateStore<CharacterGuideQuest>.Get(null);

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    public GuideQuestConfiguration? Configuration
    {
        get => this._configuration;
        set
        {
            this._configuration = value;
            this._steps = value?.Steps.GroupBy(s => s.Number).ToDictionary(g => g.Key, g => g.First()) ?? [];
            this._store = CharacterStateStore<CharacterGuideQuest>.Get(value?.StateFile);
        }
    }

    /// <summary>
    /// Gets the active plugin of the game, if any.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The plugin.</returns>
    public static GuideQuestPlugIn? Get(IGameContext gameContext) => gameContext.FeaturePlugIns.GetPlugIn<GuideQuestPlugIn>();

    /// <inheritdoc />
    public object CreateDefaultConfig() => new GuideQuestConfiguration();

    /// <summary>
    /// Gets what the player is shown: the accepted step, else the next one of its class.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The view.</returns>
    public GuideQuestView GetView(Player player)
    {
        if (player.SelectedCharacter is not { } character || this.Configuration is not { } configuration)
        {
            return default;
        }

        var baseClass = character.CharacterClass is { } characterClass ? ClassCodes.GetBaseClass(characterClass.Number) : null;
        var chain = configuration.Steps.Where(s => s.IsFor(baseClass)).ToList();
        if (chain.Count == 0)
        {
            return default;
        }

        var (lastFinished, active, stored) = this.Read(character.Id);
        if (active != 0 && chain.FindIndex(s => s.Number == active) is var activeIndex and >= 0)
        {
            var step = chain[activeIndex];
            var progress = this.GetProgress(player, step, stored);
            var status = progress >= step.GetRequiredProgress() ? GuideQuestStatus.Ready : GuideQuestStatus.Active;
            return new GuideQuestView(status, step, activeIndex + 1, chain.Count, progress);
        }

        var next = GetNextIndex(chain, lastFinished);
        if (next >= chain.Count)
        {
            return new GuideQuestView(GuideQuestStatus.Finished, null, 0, chain.Count, 0);
        }

        var nextStep = chain[next];
        var nextStatus = player.Level < nextStep.MinimumLevel ? GuideQuestStatus.LevelTooLow : GuideQuestStatus.Offered;
        return new GuideQuestView(nextStatus, nextStep, next + 1, chain.Count, 0);
    }

    /// <summary>
    /// Sends what the player is shown.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="result">The answer to the request which caused it.</param>
    public async ValueTask ShowAsync(Player player, GuideQuestResult result = GuideQuestResult.None)
    {
        var view = this.GetView(player);
        await player.InvokeViewPlugInAsync<IGuideQuestViewPlugIn>(p => p.ShowGuideQuestAsync(view, result)).ConfigureAwait(false);
    }

    /// <summary>
    /// Accepts the offered step.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="number">The number of the step the client showed.</param>
    public async ValueTask AcceptAsync(Player player, short number)
    {
        var view = this.GetView(player);
        if (view.Step is not { } step || step.Number != number || view.Status is not (GuideQuestStatus.Offered or GuideQuestStatus.LevelTooLow))
        {
            await this.ShowAsync(player).ConfigureAwait(false);
            return;
        }

        if (view.Status == GuideQuestStatus.LevelTooLow)
        {
            await this.ShowAsync(player, GuideQuestResult.LevelTooLow).ConfigureAwait(false);
            return;
        }

        this.Update(player, state => (state.Active, state.Progress) = (number, 0));
        if (step.Objective == GuideQuestObjective.MoveTo)
        {
            this._moveTargets.AddOrUpdate(player, step);
            this.CheckArrival(player, step);
        }

        this._store.Save(TimeSpan.Zero, player.Logger);
        await this.ShowAsync(player, GuideQuestResult.Accepted).ConfigureAwait(false);
    }

    /// <summary>
    /// Completes the accepted step when its objective is met, and gives its rewards.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="number">The number of the step the client showed.</param>
    public async ValueTask CompleteAsync(Player player, short number)
    {
        var view = this.GetView(player);
        if (view.Step is not { } step || step.Number != number || view.Status is not (GuideQuestStatus.Active or GuideQuestStatus.Ready))
        {
            await this.ShowAsync(player).ConfigureAwait(false);
            return;
        }

        if (view.Status != GuideQuestStatus.Ready)
        {
            await this.ShowAsync(player, GuideQuestResult.NotReady).ConfigureAwait(false);
            return;
        }

        if (step.Objective == GuideQuestObjective.CollectItem)
        {
            await TakeItemsAsync(player, step).ConfigureAwait(false);
        }

        this.Update(player, state => (state.LastFinished, state.Active, state.Progress) = (number, 0, 0));
        this._moveTargets.Remove(player);
        this._store.Save(TimeSpan.Zero, player.Logger);
        await GiveRewardsAsync(player, step).ConfigureAwait(false);
        await this.ShowAsync(player, GuideQuestResult.Completed).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask AttackableGotKilledAsync(IAttackable killed, IAttacker? killer)
    {
        if (killer is not Player player
            || killed is not Monster monster
            || player.SelectedCharacter is not { } character
            || this.Read(character.Id) is not { Active: not 0 } state
            || !this._steps.TryGetValue(state.Active, out var step)
            || step.Objective != GuideQuestObjective.HuntMonster
            || step.Target != monster.Definition.Number
            || state.Progress >= step.Count)
        {
            return;
        }

        this.Update(player, s => s.Progress++);
        this._store.Save(this.Configuration?.SaveInterval ?? TimeSpan.Zero, player.Logger);
        await this.ShowAsync(player).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void AttackableMoved(IAttackable attackable)
    {
        if (attackable is Player player && this._moveTargets.TryGetValue(player, out var step) && this.CheckArrival(player, step))
        {
            _ = this.ShowAsync(player).AsTask();
        }
    }

    /// <inheritdoc />
    public void CharacterLeveledUp(Player player)
    {
        _ = this.ShowAsync(player).AsTask();
    }

    /// <inheritdoc />
    public async ValueTask ObjectAddedToMapAsync(GameMap map, ILocateable addedObject)
    {
        if (addedObject is not Player player)
        {
            return;
        }

        if (this.GetView(player) is { Status: GuideQuestStatus.Active, Step: { Objective: GuideQuestObjective.MoveTo } step })
        {
            this._moveTargets.AddOrUpdate(player, step);
            this.CheckArrival(player, step);
        }

        await this.ShowAsync(player).ConfigureAwait(false);
    }

    private static int GetNextIndex(List<GuideQuestStep> chain, short lastFinished)
    {
        if (lastFinished == 0)
        {
            return 0;
        }

        var index = chain.FindIndex(s => s.Number == lastFinished);
        if (index >= 0)
        {
            return index + 1;
        }

        // The class changed to one whose chain doesn't have the step: it goes on after it.
        index = chain.FindIndex(s => s.Number > lastFinished);
        return index >= 0 ? index : chain.Count;
    }

    private static int CountItems(Player player, GuideQuestStep step)
        => player.Inventory?.Items
            .Where(i => IsCollected(i, step))
            .Sum(i => i.IsStackable() ? (int)i.Durability : 1) ?? 0;

    private static bool IsCollected(Item item, GuideQuestStep step)
        => item.Definition is { } definition && definition.Group == step.Target && definition.Number == step.TargetNumber;

    private static bool HasFinishedClassQuest(Player player, int number)
    {
        var finished = player.SelectedCharacter?.QuestStates?.FirstOrDefault(s => s.Group == LegacyQuestGroup)?.LastFinishedQuest?.Number ?? -1;
        if (finished >= number)
        {
            return true;
        }

        // A character which got its class otherwise (created so, or by a game master) passed the quest of it.
        var evolution = player.SelectedCharacter?.CharacterClass?.GetEvolution();
        return number <= 1 ? evolution >= ClassEvolution.Second : evolution >= ClassEvolution.Third;
    }

    private static async ValueTask TakeItemsAsync(Player player, GuideQuestStep step)
    {
        var remaining = step.GetRequiredProgress();
        foreach (var item in player.Inventory!.Items.Where(i => IsCollected(i, step)).ToList())
        {
            if (remaining <= 0)
            {
                break;
            }

            var count = item.IsStackable() ? (int)item.Durability : 1;
            if (count > remaining)
            {
                item.Durability -= remaining;
                await player.InvokeViewPlugInAsync<IItemDurabilityChangedPlugIn>(p => p.ItemDurabilityChangedAsync(item, false)).ConfigureAwait(false);
                break;
            }

            remaining -= count;
            await player.DestroyInventoryItemAsync(item).ConfigureAwait(false);
        }
    }

    private static async ValueTask GiveRewardsAsync(Player player, GuideQuestStep step)
    {
        if (step.Experience > 0)
        {
            await player.AddExperienceAsync(step.Experience, null).ConfigureAwait(false);
        }

        if (step.Money > 0 && player.TryAddMoney(step.Money))
        {
            await player.InvokeViewPlugInAsync<IUpdateMoneyPlugIn>(p => p.UpdateMoneyAsync()).ConfigureAwait(false);
        }

        foreach (var item in step.Items)
        {
            await RewardDelivery.DeliverAsync(player, item, true).ConfigureAwait(false);
        }
    }

    private int GetProgress(Player player, GuideQuestStep step, int stored) => step.Objective switch
    {
        GuideQuestObjective.HuntMonster => Math.Min(stored, step.GetRequiredProgress()),
        GuideQuestObjective.CollectItem => Math.Min(CountItems(player, step), step.GetRequiredProgress()),
        GuideQuestObjective.ReachLevel => player.Level,
        GuideQuestObjective.CompleteQuest => HasFinishedClassQuest(player, step.Target) ? 1 : 0,
        _ => stored,
    };

    private bool CheckArrival(Player player, GuideQuestStep step)
    {
        var distance = this.Configuration?.ArrivalDistance ?? 5;
        if (player.CurrentMap?.Definition.Number != step.Map
            || Math.Abs(player.Position.X - step.X) > distance
            || Math.Abs(player.Position.Y - step.Y) > distance)
        {
            return false;
        }

        this._moveTargets.Remove(player);
        this.Update(player, state => state.Progress = 1);
        this._store.Save(this.Configuration?.SaveInterval ?? TimeSpan.Zero, player.Logger);
        return true;
    }

    private (short LastFinished, short Active, int Progress) Read(Guid characterId)
    {
        lock (this._store.SyncRoot)
        {
            return this._store.Find(characterId) is { } state ? (state.LastFinished, state.Active, state.Progress) : default;
        }
    }

    private void Update(Player player, Action<CharacterGuideQuest> change)
    {
        if (player.SelectedCharacter is not { } character)
        {
            return;
        }

        lock (this._store.SyncRoot)
        {
            change(this._store.GetOrCreate(character.Id));
        }
    }
}
