// <copyright file="StarSmithForge.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// Where an attempt stands.
/// </summary>
public enum StarSmithForgeState
{
    /// <summary>Stars are being struck.</summary>
    Drafting,

    /// <summary>The smith sealed the result.</summary>
    Sealed,

    /// <summary>A void cracked the attempt.</summary>
    Cracked,
}

/// <summary>
/// The answer to a strike.
/// </summary>
public enum StarSmithStrikeResult
{
    /// <summary>The star is struck and linked.</summary>
    Linked,

    /// <summary>The star was a void: the attempt cracked.</summary>
    Cracked,

    /// <summary>Refused: the attempt is sealed or cracked.</summary>
    NotDrafting,

    /// <summary>Refused: no such star.</summary>
    NoSuchStar,

    /// <summary>Refused: the ingredients allow no more strikes.</summary>
    NoStrikesLeft,

    /// <summary>Refused: the star is already struck.</summary>
    StarUsed,

    /// <summary>Refused: the star may not be linked to the last one.</summary>
    NotAdjacent,
}

/// <summary>
/// The rarity tiers (section 3, "Rarity").
/// </summary>
public enum StarSmithRarity
{
    /// <summary>Common, white: no effect, one boost.</summary>
    Common,

    /// <summary>Uncommon, green: no effect, two boosts.</summary>
    Uncommon,

    /// <summary>Rare, blue: one effect, two boosts.</summary>
    Rare,

    /// <summary>Epic, purple: one effect, three boosts.</summary>
    Epic,

    /// <summary>Legendary, orange: two effects, three boosts.</summary>
    Legendary,

    /// <summary>Mythic, red: two effects, three boosts and an aura.</summary>
    Mythic,
}

/// <summary>
/// The answer to a strike.
/// </summary>
/// <param name="Result">Linked, cracked, or why it was refused.</param>
/// <param name="Timing">How it met the beat (Late when refused).</param>
/// <param name="Star">The struck star, revealed (default when refused).</param>
public readonly record struct StarSmithStrike(StarSmithStrikeResult Result, StarSmithTiming Timing, StarSmithStar Star);

/// <summary>
/// One Constellation Forge attempt (section 10): the smith strikes stars on the beat, then seals, or a void cracks it.
/// Pure and deterministic: the same sky, beat and strikes give the same result.
/// </summary>
public sealed class StarSmithForge
{
    private StarSmithForge()
    {
    }

    /// <summary>Gets where the attempt stands.</summary>
    public StarSmithForgeState State { get; private set; }

    /// <summary>Gets the struck stars, in order.</summary>
    public IReadOnlyList<int> Path => [];

    /// <summary>Gets how each strike met the beat, in order.</summary>
    public IReadOnlyList<StarSmithTiming> Timings => [];

    /// <summary>Gets how many stars may still be struck.</summary>
    public int StrikesLeft => 0;

    /// <summary>Gets the result once sealed or cracked.</summary>
    public StarSmithForgeResult? Result { get; private set; }

    /// <summary>
    /// Starts an attempt.
    /// </summary>
    /// <param name="sky">The dealt sky.</param>
    /// <param name="beat">The beat.</param>
    /// <param name="startMs">The server time it starts.</param>
    /// <param name="roundTripMs">The client's measured round trip.</param>
    /// <param name="smithLevel">The smith's level.</param>
    /// <param name="known">The recipes in the smith's Guidebook.</param>
    /// <returns>The attempt, drafting.</returns>
    public static StarSmithForge Start(StarSmithSky sky, StarSmithBeat beat, long startMs, int roundTripMs, int smithLevel, IReadOnlySet<short> known) => new();

    /// <summary>
    /// The rarity tier of a result: the share of clean strikes (up to 1,000 permille) plus a quarter of it for the
    /// smith level (250 at level 100). Common below 300, Uncommon below 500, Rare below 700, Epic below 900, Legendary
    /// below 1,100, Mythic from there.
    /// </summary>
    /// <param name="clean">The clean strikes.</param>
    /// <param name="strikes">All strikes.</param>
    /// <param name="smithLevel">The smith's level.</param>
    /// <returns>The tier.</returns>
    public static StarSmithRarity Rarity(int clean, int strikes, int smithLevel) => StarSmithRarity.Mythic;

    /// <summary>
    /// Strikes a star.
    /// </summary>
    /// <param name="star">The star's index in the sky.</param>
    /// <param name="serverTimeMs">The server time the strike arrived.</param>
    /// <returns>The answer.</returns>
    public StarSmithStrike Strike(int star, long serverTimeMs) => default;

    /// <summary>
    /// Seals the drawn constellation and keeps its result.
    /// </summary>
    /// <returns>The result, or null when nothing is drawn or the attempt is no longer drafting.</returns>
    public StarSmithForgeResult? Seal() => null;

    /// <summary>
    /// The guidance for the path so far: the smith's known recipes it is still a prefix of.
    /// </summary>
    /// <returns>The recipes.</returns>
    public IReadOnlyList<StarSmithRecipe> Guidance() => [];
}
