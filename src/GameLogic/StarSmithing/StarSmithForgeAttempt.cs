// <copyright file="StarSmithForgeAttempt.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// A running Forge attempt, kept on the player (<see cref="Player.StarSmithForge"/>): the server's seed and every input
/// of the deal, the forge, and each strike with the server time it arrived, so the attempt can be dealt and played
/// again (<see cref="Replay"/>) to verify it. Thread safe: the idle timeout runs beside the smith's packets.
/// </summary>
public sealed class StarSmithForgeAttempt
{
    private readonly object _lock = new();

    private readonly List<(int Star, long TimeMs)> _strikes = [];

    private readonly IReadOnlyList<StarSmithRecipe> _recipes;

    private readonly StarSmithTarget _target;

    private readonly int _smithLevel;

    private readonly int _ingredientCount;

    private readonly IReadOnlyCollection<(byte Group, short Number)> _catalysts;

    private readonly StarSmithForgeRules _rules;

    private readonly StarSmithBeat _beat;

    private readonly IReadOnlySet<short> _known;

    private StarSmithForgeState? _ending;

    private StarSmithForgeAttempt(long seed, IReadOnlyList<StarSmithRecipe> recipes, StarSmithTarget target, int smithLevel, int ingredientCount, IReadOnlyCollection<(byte Group, short Number)> catalysts, StarSmithForgeRules rules, StarSmithBeat beat, long startMs, int roundTripMs, IReadOnlySet<short> known, byte targetSlot, Guid itemId)
    {
        this.Seed = seed;
        this._recipes = recipes;
        this._target = target;
        this._smithLevel = smithLevel;
        this._ingredientCount = ingredientCount;
        this._catalysts = catalysts;
        this._rules = rules;
        this._beat = beat;
        this._known = known;
        this.StartMs = startMs;
        this.LastActivityMs = startMs;
        this.RoundTripMs = roundTripMs;
        this.TargetSlot = targetSlot;
        this.ItemId = itemId;
        this.Sky = StarSmithSky.Deal(seed, recipes, target, smithLevel, ingredientCount, catalysts, 0, rules);
        this.Forge = StarSmithForge.Start(this.Sky, beat, startMs, roundTripMs, smithLevel, known);
    }

    /// <summary>Gets the seed the server chose.</summary>
    public long Seed { get; }

    /// <summary>Gets the server time the attempt started; the beat counts from it.</summary>
    public long StartMs { get; }

    /// <summary>Gets the server time of the last accepted strike, or of the start.</summary>
    public long LastActivityMs { get; private set; }

    /// <summary>Gets the round trip the beat allows for, the server's own measurement.</summary>
    public int RoundTripMs { get; }

    /// <summary>Gets the inventory slot of the item worked on.</summary>
    public byte TargetSlot { get; }

    /// <summary>Gets the persistent id of the item worked on.</summary>
    public Guid ItemId { get; }

    /// <summary>Gets the dealt sky. It holds hidden data: never send it as it is.</summary>
    public StarSmithSky Sky { get; }

    /// <summary>Gets the forge.</summary>
    public StarSmithForge Forge { get; }

    /// <summary>Gets or sets the stat an attunement seal (no recipe) boosts, from the ingredients' family.</summary>
    public StarSmithBoostStat AttunementStat { get; set; }

    /// <summary>Gets the beat.</summary>
    public StarSmithBeat Beat => this._beat;

    /// <summary>Gets the accepted strikes with their server times, in order.</summary>
    public IReadOnlyList<(int Star, long TimeMs)> Strikes
    {
        get
        {
            lock (this._lock)
            {
                return this._strikes.ToList();
            }
        }
    }

    /// <summary>
    /// Deals the sky and starts the forge.
    /// </summary>
    /// <param name="seed">The seed, chosen by the server.</param>
    /// <param name="recipes">The recipes the item and ingredients can make.</param>
    /// <param name="target">The item worked on.</param>
    /// <param name="smithLevel">The smith's level.</param>
    /// <param name="ingredientCount">How many ingredients are in the grid.</param>
    /// <param name="catalysts">The catalysts by item.</param>
    /// <param name="rules">The dealing rules.</param>
    /// <param name="beat">The beat.</param>
    /// <param name="startMs">The server time it starts.</param>
    /// <param name="roundTripMs">The round trip, the server's own measurement (0 when there is none).</param>
    /// <param name="known">The recipes in the smith's Guidebook.</param>
    /// <param name="targetSlot">The inventory slot of the item.</param>
    /// <param name="itemId">The item's persistent id.</param>
    /// <returns>The attempt, drafting.</returns>
    public static StarSmithForgeAttempt Start(long seed, IReadOnlyList<StarSmithRecipe> recipes, StarSmithTarget target, int smithLevel, int ingredientCount, IReadOnlyCollection<(byte Group, short Number)> catalysts, StarSmithForgeRules rules, StarSmithBeat beat, long startMs, int roundTripMs, IReadOnlySet<short> known, byte targetSlot, Guid itemId)
        => new(seed, recipes, target, smithLevel, ingredientCount, catalysts, rules, beat, startMs, roundTripMs, known, targetSlot, itemId);

    /// <summary>
    /// Strikes a star at a server time; an accepted strike is kept with its time.
    /// </summary>
    /// <param name="star">The star's index.</param>
    /// <param name="serverTimeMs">The server time it arrived.</param>
    /// <returns>The answer.</returns>
    public StarSmithStrike Strike(int star, long serverTimeMs)
    {
        lock (this._lock)
        {
            var strike = this.Forge.Strike(star, serverTimeMs);
            if (strike.Result is StarSmithStrikeResult.Linked or StarSmithStrikeResult.Cracked)
            {
                this._strikes.Add((star, serverTimeMs));
                this.LastActivityMs = serverTimeMs;
                if (strike.Result == StarSmithStrikeResult.Cracked)
                {
                    this._ending = StarSmithForgeState.Cracked;
                }
            }

            return strike;
        }
    }

    /// <summary>
    /// Seals the attempt.
    /// </summary>
    /// <returns>The result, or null when nothing is struck or it is no longer drafting.</returns>
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
    /// Whether the attempt is still drafting and has had no strike for the timeout.
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
    /// The guidance for the path so far, as recipe numbers.
    /// </summary>
    /// <returns>The numbers.</returns>
    public IReadOnlyList<short> Guidance()
    {
        lock (this._lock)
        {
            return this.Forge.Guidance().Select(r => r.Number).ToList();
        }
    }

    /// <summary>
    /// Deals the sky again from the kept seed and inputs and plays the kept strikes and the ending again, to verify
    /// the attempt.
    /// </summary>
    /// <returns>The replayed forge.</returns>
    public StarSmithForge Replay()
    {
        List<(int Star, long TimeMs)> strikes;
        StarSmithForgeState? ending;
        lock (this._lock)
        {
            strikes = this._strikes.ToList();
            ending = this._ending;
        }

        var sky = StarSmithSky.Deal(this.Seed, this._recipes, this._target, this._smithLevel, this._ingredientCount, this._catalysts, 0, this._rules);
        var forge = StarSmithForge.Start(sky, this._beat, this.StartMs, this.RoundTripMs, this._smithLevel, this._known);
        foreach (var (star, time) in strikes)
        {
            forge.Strike(star, time);
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
