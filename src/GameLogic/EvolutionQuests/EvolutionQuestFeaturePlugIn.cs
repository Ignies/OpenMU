// <copyright file="EvolutionQuestFeaturePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EvolutionQuests;

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration.Quests;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.MiniGames;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlayerActions.Quests;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Views.NPC;
using MUnique.OpenMU.GameLogic.Views.Quest;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The evolution quests of the fourth and fifth class: the town NPCs which take a character to the trial map, the
/// monsters of each trial step, the kills of party members, and the experience hold of a third class character at the
/// level of the fourth evolution. The quests themselves are data (update "Add the evolution quests of Season 21").
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.EvolutionQuestFeaturePlugIn_Name), Description = nameof(PlugInResources.EvolutionQuestFeaturePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("3B0B2C59-6E0C-4C8B-9B1E-5C3F4E2D7A61")]
public class EvolutionQuestFeaturePlugIn : IFeaturePlugIn, ISupportCustomConfiguration<EvolutionQuestConfiguration>, ISupportDefaultCustomConfiguration,
    IExperienceCalculationPlugIn, IPlayerTalkToNpcPlugIn, IAttackableGotKilledPlugIn, IObjectAddedToMapPlugIn, IObjectRemovedFromMapPlugIn
{
    private static readonly EvolutionQuestConfiguration DefaultConfiguration = new();

    private readonly ConcurrentDictionary<Player, EvolutionTrial> _trials = new();

    private readonly ConditionalWeakTable<Player, object> _toldAboutHold = [];

    private readonly ConditionalWeakTable<CharacterClass, StrongBox<bool>> _classesWithQuest = [];

    private readonly QuestMonsterKillCountPlugIn _killCounter = new();

    /// <inheritdoc />
    public EvolutionQuestConfiguration? Configuration { get; set; }

    private EvolutionQuestConfiguration CurrentConfiguration => this.Configuration ?? DefaultConfiguration;

    /// <summary>
    /// Gets the active plugin of a game.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The plugin, or <c>null</c> when it's not active.</returns>
    public static EvolutionQuestFeaturePlugIn? Get(IGameContext gameContext) => gameContext.FeaturePlugIns.GetPlugIn<EvolutionQuestFeaturePlugIn>();

    /// <inheritdoc />
    public object CreateDefaultConfig() => new EvolutionQuestConfiguration();

    /// <summary>
    /// Gets the state of the selected character towards its next evolution.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The state.</returns>
    public EvolutionQuestState GetState(Player player)
    {
        if (player.SelectedCharacter?.CharacterClass is not { NextGenerationClass: { IsActive: true } next } characterClass
            || next.GetEvolution() is not { } evolution
            || !this.HasEvolutionQuest(characterClass, player.GameContext.Configuration))
        {
            return default;
        }

        var (level, key) = player.GetEvolutionRequirement(evolution);
        var ruleMet = player.MeetsEvolutionRule(evolution);
        var held = ruleMet && evolution == ClassEvolution.Fourth && this.CurrentConfiguration.HoldExperienceUntilFourthEvolution;
        return new EvolutionQuestState(evolution, level, key, ruleMet, held);
    }

    /// <summary>
    /// Shows the state of the selected character towards its next evolution.
    /// </summary>
    /// <param name="player">The player.</param>
    public ValueTask ShowStateAsync(Player player)
    {
        var state = this.GetState(player);
        return player.InvokeViewPlugInAsync<IEvolutionQuestStatePlugIn>(p => p.ShowEvolutionQuestStateAsync(state));
    }

    /// <summary>
    /// Called when the player started a quest, or asked for it while it's active: a trial step spawns its monsters if
    /// the player is on its map and they're not there yet.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="quest">The quest.</param>
    public ValueTask QuestRequestedAsync(Player player, QuestDefinition quest)
        => player.CurrentMap is { } map ? this.QuestRequestedAsync(player, quest, map) : ValueTask.CompletedTask;

    /// <summary>
    /// Ends the running trial of the player, e.g. after it completed the quest.
    /// </summary>
    /// <param name="player">The player.</param>
    public async ValueTask EndTrialAsync(Player player)
    {
        if (this._trials.TryRemove(player, out var trial))
        {
            await trial.EndAsync().ConfigureAwait(false);
            await ShowTrialTimeAsync(player, trial).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async ValueTask CalculateExperienceAsync(Player player, ExperienceCalculationArgs args)
    {
        if (args.Experience <= 0 || !this.GetState(player).ExperienceHeld)
        {
            return;
        }

        args.Experience = 0;
        if (this._toldAboutHold.TryAdd(player, this))
        {
            await player.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.EvolutionQuestExperienceHeld)).ConfigureAwait(false);
            await this.ShowStateAsync(player).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async ValueTask PlayerTalksToNpcAsync(Player player, NonPlayerCharacter npc, NpcTalkEventArgs eventArgs)
    {
        if (this.CurrentConfiguration.Entrances.FirstOrDefault(e => e.NpcNumber == npc.Definition.Number) is not { } entrance)
        {
            return;
        }

        eventArgs.HasBeenHandled = true;
        if (player.SelectedCharacter?.CharacterClass?.NextGenerationClass is not { IsActive: true } next
            || next.GetEvolution() != entrance.Evolution)
        {
            await ShowNpcMessageAsync(player, npc, player.GetLocalizedMessage(nameof(PlayerMessage.EvolutionTrialNotForYourClass))).ConfigureAwait(false);
            return;
        }

        if (!player.MeetsEvolutionRule(entrance.Evolution))
        {
            var (level, _) = player.GetEvolutionRequirement(entrance.Evolution);
            await ShowNpcMessageAsync(player, npc, player.GetLocalizedMessage(nameof(PlayerMessage.EvolutionTrialLevelTooLowFormat), level)).ConfigureAwait(false);
            return;
        }

        if (player.GameContext.Configuration.Maps.FirstOrDefault(m => m.Number == entrance.Map && m.Discriminator == 0) is not { } map)
        {
            player.Logger.LogWarning("The trial map {0} of NPC {1} doesn't exist.", entrance.Map, npc.Definition.Number);
            return;
        }

        player.OpenedNpc = null;
        if (PartyInstances.FindForMap(player.GameContext, map) is { } instance)
        {
            await PartyInstances.EnterAsync(player, instance).ConfigureAwait(false);
        }
        else
        {
            await player.WarpToAsync(new ExitGate { Map = map, X1 = entrance.X1, Y1 = entrance.Y1, X2 = entrance.X2, Y2 = entrance.Y2 }).ConfigureAwait(false);
        }

        // A warp changes the player state itself, so the talk action must leave it alone; a refused entry doesn't warp,
        // and the talk action closes the dialog, otherwise the next talk gets no answer.
        eventArgs.LeavesDialogOpen = player.PlayerState.CurrentState != PlayerState.NpcDialogOpened;
    }

    /// <inheritdoc />
    public async ValueTask AttackableGotKilledAsync(IAttackable killed, IAttacker? killer)
    {
        if (killed is not Monster monster
            || (killer as Player ?? (killer as IPlayerSurrogate)?.Owner) is not { } player
            || player.Party is not { } party
            || this._trials.IsEmpty
            || !this._trials.Values.Any(t => t.Contains(monster)))
        {
            return;
        }

        foreach (var member in party.PartyList.OfType<Player>().Where(m => m != killer && m.CurrentMap == monster.CurrentMap))
        {
            // The killer counts its own kill; the trial counts it for the party members on the map, too.
            await this._killCounter.AttackableGotKilledAsync(monster, member).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async ValueTask ObjectAddedToMapAsync(GameMap map, ILocateable addedObject)
    {
        if (addedObject is not Player player)
        {
            return;
        }

        await this.ShowStateAsync(player).ConfigureAwait(false);
        if (player.SelectedCharacter?.QuestStates is not { Count: > 0 } questStates)
        {
            return;
        }

        foreach (var quest in questStates.Select(s => s.ActiveQuest).OfType<QuestDefinition>().ToList())
        {
            await this.QuestRequestedAsync(player, quest, map).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async ValueTask ObjectRemovedFromMapAsync(GameMap map, ILocateable removedObject)
    {
        if (removedObject is Player player)
        {
            await this.EndTrialAsync(player).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Gets the running trial of a player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The trial, or <c>null</c>.</returns>
    internal EvolutionTrial? GetTrial(Player player) => this._trials.GetValueOrDefault(player);

    /// <summary>
    /// Shows the time left of the trial step, as the original's quest survival time (S21 client text 5189,
    /// Interface/GFx/TimeCounter): an event score of the party instance whose round is the quest number; 0 when it ended.
    /// </summary>
    private static ValueTask ShowTrialTimeAsync(Player player, EvolutionTrial trial)
        => player.InvokeViewPlugInAsync<ISeason21EventViewPlugIn>(p => p.ShowScoreAsync(MiniGameType.PartyInstance, (byte)trial.Step.QuestNumber, 0, 0, 1, (uint)Math.Ceiling(trial.TimeLeft.TotalSeconds)));

    private static ValueTask ShowNpcMessageAsync(Player player, NonPlayerCharacter npc, string message)
        => player.InvokeViewPlugInAsync<IShowMessageOfObjectPlugIn>(p => p.ShowMessageOfObjectAsync(message, npc));

    private async ValueTask QuestRequestedAsync(Player player, QuestDefinition quest, GameMap map)
    {
        if (this.CurrentConfiguration.GetStep(quest.Group, quest.Number) is { } step
            && step.Map == map.Definition.Number)
        {
            await this.StartTrialAsync(player, step, map).ConfigureAwait(false);
        }
    }

    private bool HasEvolutionQuest(CharacterClass characterClass, GameConfiguration gameConfiguration)
        => this._classesWithQuest.GetValue(characterClass, c => new StrongBox<bool>(c.HasEvolutionQuest(gameConfiguration))).Value;

    private async ValueTask StartTrialAsync(Player owner, EvolutionTrialStep step, GameMap map)
    {
        if (this._trials.TryGetValue(owner, out var running))
        {
            if (running.Step == step && running.IsEventRunning)
            {
                await ShowTrialTimeAsync(owner, running).ConfigureAwait(false);
                return;
            }

            await this.EndTrialAsync(owner).ConfigureAwait(false);
        }

        var trial = new EvolutionTrial(owner, step, map, this.EndTrialAsync);
        if (this._trials.TryAdd(owner, trial))
        {
            await trial.StartAsync().ConfigureAwait(false);
            await ShowTrialTimeAsync(owner, trial).ConfigureAwait(false);
        }
    }
}
