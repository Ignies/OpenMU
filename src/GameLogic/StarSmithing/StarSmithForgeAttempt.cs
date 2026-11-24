// <copyright file="StarSmithForgeAttempt.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// A running Forge attempt, kept on the player (<see cref="Player.StarSmithForge"/>): the server's seed and every input
/// of the start, the forge, and each accepted node with the server time it arrived and each stash, so the attempt can be
/// played again (<see cref="Replay"/>) to verify it. Thread safe: the idle timeout runs beside the smith's packets.
/// </summary>
public sealed class StarSmithForgeAttempt
{
    private readonly object _lock = new();

    private readonly List<(StarSmithPoint? Point, long TimeMs)> _actions = [];

    private readonly IReadOnlyList<StarSmithRecipe> _recipes;

    private readonly int _smithLevel;

    private readonly int _ingredientCount;

    private readonly bool _guarded;

    private readonly StarSmithForgeRules _rules;

    private readonly IReadOnlySet<short> _known;

    private readonly IReadOnlyList<StarSmithLayer> _layers;

    private StarSmithForgeState? _ending;

    private StarSmithForgeAttempt(long seed, IReadOnlyList<StarSmithRecipe> recipes, int smithLevel, int ingredientCount, bool guarded, StarSmithForgeRules rules, StarSmithBeat beat, long startMs, int roundTripMs, IReadOnlySet<short> known, IReadOnlyList<StarSmithLayer> layers, byte targetSlot, Guid itemId)
    {
        this.Seed = seed;
        this._recipes = recipes;
        this._smithLevel = smithLevel;
        this._ingredientCount = ingredientCount;
        this._guarded = guarded;
        this._rules = rules;
        this._known = known;
        this._layers = layers;
        this.Beat = beat;
        this.StartMs = startMs;
        this.LastActivityMs = startMs;
        this.RoundTripMs = roundTripMs;
        this.TargetSlot = targetSlot;
        this.ItemId = itemId;
        this.Forge = StarSmithForge.Start(seed, recipes, smithLevel, ingredientCount, guarded, rules, beat, startMs, roundTripMs, known, layers);
    }

    /// <summary>Gets the seed the server chose.</summary>
    public long Seed { get; }

    /// <summary>Gets the server time the attempt started; the beat counts from it.</summary>
    public long StartMs { get; }

    /// <summary>Gets the server time of the last accepted node or stash, or of the start.</summary>
    public long LastActivityMs { get; private set; }

    /// <summary>Gets the round trip the beat allows for, the server's own measurement.</summary>
    public int RoundTripMs { get; }

    /// <summary>Gets the inventory slot of the item worked on.</summary>
    public byte TargetSlot { get; }

    /// <summary>Gets the persistent id of the item worked on.</summary>
    public Guid ItemId { get; }

    /// <summary>Gets the forge. It holds hidden data (the faults): never send it as it is.</summary>
    public StarSmithForge Forge { get; }

    /// <summary>Gets or sets the stat an attunement seal (no constellation) boosts, from the ingredients' family.</summary>
    public StarSmithBoostStat AttunementStat { get; set; }

    /// <summary>Gets the beat.</summary>
    public StarSmithBeat Beat { get; }

    /// <summary>
    /// Starts an attempt on the constellations whose target rule fits the item.
    /// </summary>
    /// <param name="seed">The seed, chosen by the server.</param>
    /// <param name="recipes">The recipes the ingredients allow.</param>
    /// <param name="target">The item worked on.</param>
    /// <param name="smithLevel">The smith's level.</param>
    /// <param name="ingredientCount">How many ingredients are in the grid.</param>
    /// <param name="catalysts">The catalysts by item.</param>
    /// <param name="rules">The rules.</param>
    /// <param name="beat">The beat.</param>
    /// <param name="startMs">The server time it starts.</param>
    /// <param name="roundTripMs">The round trip, the server's own measurement (0 when there is none).</param>
    /// <param name="known">The constellations in the smith's Guidebook.</param>
    /// <param name="layers">The item's layers.</param>
    /// <param name="targetSlot">The inventory slot of the item.</param>
    /// <param name="itemId">The item's persistent id.</param>
    /// <returns>The attempt, drafting.</returns>
    public static StarSmithForgeAttempt Start(long seed, IReadOnlyList<StarSmithRecipe> recipes, StarSmithTarget target, int smithLevel, int ingredientCount, IReadOnlyCollection<(byte Group, short Number)> catalysts, StarSmithForgeRules rules, StarSmithBeat beat, long startMs, int roundTripMs, IReadOnlySet<short> known, IReadOnlyList<StarSmithLayer> layers, byte targetSlot, Guid itemId)
    {
        var fitting = recipes.Where(r => StarSmithRecipes.Fits(r, target)).ToList();
        var guarded = catalysts.Any(c => c.Group == rules.GuardianGroup && c.Number == rules.GuardianNumber);
        return new(seed, fitting, smithLevel, ingredientCount, guarded, rules, beat, startMs, roundTripMs, known, layers, targetSlot, itemId);
    }

    /// <summary>
    /// Places a node at a server time; an accepted node is kept with its time.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="serverTimeMs">The server time it arrived.</param>
    /// <returns>The answer.</returns>
    public StarSmithFigureAnswer Place(StarSmithPoint point, long serverTimeMs)
    {
        lock (this._lock)
        {
            var answer = this.Forge.Place(point, serverTimeMs);
            if (answer.Result is StarSmithFigureResult.Done or StarSmithFigureResult.Cracked)
            {
                this._actions.Add((point, serverTimeMs));
                this.LastActivityMs = serverTimeMs;
                if (answer.Result == StarSmithFigureResult.Cracked)
                {
                    this._ending = StarSmithForgeState.Cracked;
                }
            }

            return answer;
        }
    }

    /// <summary>
    /// Banks the figure being drawn.
    /// </summary>
    /// <returns>The answer.</returns>
    public StarSmithFigureAnswer Stash()
    {
        lock (this._lock)
        {
            var answer = this.Forge.Stash();
            if (answer.Result == StarSmithFigureResult.Done)
            {
                this._actions.Add((null, 0));
            }

            return answer;
        }
    }

    /// <summary>
    /// Seals the attempt.
    /// </summary>
    /// <returns>The result, or null when nothing is placed or it is no longer drafting.</returns>
    public StarSmithForgeResult? Seal()
    {
        lock (this._lock)
        {
            var result = this.Forge.Seal();
            if (result is not null)
            {
                this._ending = StarSmithForgeState.Sealed;
            }

            return result;
        }
    }

    /// <summary>
    /// Abandons the attempt (also the idle timeout).
    /// </summary>
    /// <returns>The result, or null when it is no longer drafting.</returns>
    public StarSmithForgeResult? Abandon()
    {
        lock (this._lock)
        {
            var result = this.Forge.Abandon();
            if (result is not null)
            {
                this._ending = StarSmithForgeState.Abandoned;
            }

            return result;
        }
    }

    /// <summary>
    /// Whether the attempt is still drafting and has had no node for the timeout.
    /// </summary>
    /// <param name="nowMs">The server time.</param>
    /// <param name="timeoutMs">The idle timeout.</param>
    /// <returns>True when it is due.</returns>
    public bool IsIdle(long nowMs, int timeoutMs)
    {
        lock (this._lock)
        {
            return this.Forge.State == StarSmithForgeState.Drafting && nowMs - this.LastActivityMs >= timeoutMs;
        }
    }

    /// <summary>
    /// Starts the forge again from the kept seed and inputs and plays the kept nodes, stashes and the ending again, to
    /// verify the attempt.
    /// </summary>
    /// <returns>The replayed forge.</returns>
    public StarSmithForge Replay()
    {
        List<(StarSmithPoint? Point, long TimeMs)> actions;
        StarSmithForgeState? ending;
        lock (this._lock)
        {
            actions = this._actions.ToList();
            ending = this._ending;
        }

        var forge = StarSmithForge.Start(this.Seed, this._recipes, this._smithLevel, this._ingredientCount, this._guarded, this._rules, this.Beat, this.StartMs, this.RoundTripMs, this._known, this._layers);
        foreach (var (point, time) in actions)
        {
            if (point is { } at)
            {
                forge.Place(at, time);
            }
            else
            {
                forge.Stash();
            }
        }

        if (ending == StarSmithForgeState.Sealed)
        {
            forge.Seal();
        }
        else if (ending == StarSmithForgeState.Abandoned)
        {
            forge.Abandon();
        }

        return forge;
    }
}
