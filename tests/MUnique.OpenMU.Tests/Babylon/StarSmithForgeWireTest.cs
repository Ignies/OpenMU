// <copyright file="StarSmithForgeWireTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.StarSmithing;
using MUnique.OpenMU.GameLogic.Views.StarSmithing;
using Moq;

/// <summary>
/// The Constellation Forge on the wire (Readme.md "Wire"): the attempt kept on the player (seed, forge, strikes with
/// server times), the packets EE 9D-9F, and the feature plug-in's start, strike, seal, abandon and idle timeout.
/// </summary>
[TestFixture]
public class StarSmithForgeWireTest
{
    private const byte HammerSlot = 0;

    private const byte TargetSlot = 12;

    private const byte IngredientSlot = 13;

    private const string PlugIn = "MUnique.OpenMU.GameServer.RemoteView.StarSmithing.StarSmithingBabylonPlugIn, MUnique.OpenMU.GameServer";

    private static readonly StarSmithBeat Beat = new(IntervalMs: 800, ToleranceMs: 90, RoundTripCapMs: 150);

    private static readonly StarSmithTarget Sword = new(StarSmithTargetKind.Weapon, Level: 0, Excellent: false);

    private long _now;

    /// <summary>
    /// An attempt keeps its seed and every strike with its server time; replaying it deals the same sky and gives the
    /// same result.
    /// </summary>
    [Test]
    public void AnAttemptIsKeptAndReplays()
    {
        var attempt = StarSmithForgeAttempt.Start(4711, [Sure(7)], Sword, 1, 1, [], new StarSmithForgeRules(), Beat, startMs: 1000, roundTripMs: 0, new HashSet<short>(), TargetSlot, Guid.NewGuid());
        Assert.That(attempt.Strike(0, 1800).Result, Is.EqualTo(StarSmithStrikeResult.Linked));
        Assert.That(attempt.Strike(1, 2600).Result, Is.EqualTo(StarSmithStrikeResult.Linked));
        Assert.That(attempt.Strike(2, 3400).Result, Is.EqualTo(StarSmithStrikeResult.Linked));
        var sealedResult = attempt.Seal();

        Assert.That(attempt.Seed, Is.EqualTo(4711));
        Assert.That(attempt.Strikes, Is.EqualTo(new[] { (0, 1800L), (1, 2600L), (2, 3400L) }));
        Assert.That(sealedResult!.Recipe!.Number, Is.EqualTo(7));

        var replay = attempt.Replay();
        Assert.That(replay.State, Is.EqualTo(StarSmithForgeState.Sealed));
        Assert.That(replay.Result!.Recipe, Is.SameAs(sealedResult.Recipe));
        Assert.That(replay.Result.Rarity, Is.EqualTo(sealedResult.Rarity));
        Assert.That(replay.Path, Is.EqualTo(sealedResult.Path));
    }

    /// <summary>
    /// A refused strike is not kept and changes nothing.
    /// </summary>
    [Test]
    public void ARefusedStrikeIsNotKept()
    {
        var attempt = StarSmithForgeAttempt.Start(4711, [Sure(7)], Sword, 1, 1, [], new StarSmithForgeRules(), Beat, 0, 0, new HashSet<short>(), TargetSlot, Guid.NewGuid());
        attempt.Strike(0, 800);
        Assert.That(attempt.Strike(99, 1600).Result, Is.EqualTo(StarSmithStrikeResult.NoSuchStar));
        Assert.That(attempt.Strike(0, 1600).Result, Is.EqualTo(StarSmithStrikeResult.StarUsed));
        Assert.That(attempt.Strikes, Has.Count.EqualTo(1));
        Assert.That(attempt.LastActivityMs, Is.EqualTo(800));
        Assert.That(attempt.Forge.StrikesLeft, Is.EqualTo(2));
    }

    /// <summary>
    /// Abandoning is like a crack off every shape: the ingredients are lost, the item stays; then the attempt is over.
    /// An idle attempt is due for the timeout one minute after its last strike (or its start).
    /// </summary>
    [Test]
    public void AbandonAndIdle()
    {
        var attempt = StarSmithForgeAttempt.Start(4711, [Sure(7)], Sword, 1, 1, [], new StarSmithForgeRules(), Beat, 0, 0, new HashSet<short>(), TargetSlot, Guid.NewGuid());
        Assert.That(attempt.IsIdle(59_999, 60_000), Is.False);
        Assert.That(attempt.IsIdle(60_000, 60_000), Is.True);
        attempt.Strike(0, 800);
        Assert.That(attempt.IsIdle(60_000, 60_000), Is.False, "a strike counts as activity");

        var abandoned = attempt.Abandon();
        Assert.That(abandoned!.State, Is.EqualTo(StarSmithForgeState.Abandoned));
        Assert.That(abandoned.Fail, Is.EqualTo(StarSmithOnFail.IngredientsLost));
        Assert.That(abandoned.Recipe, Is.Null);
        Assert.That(abandoned.Rarity, Is.EqualTo(StarSmithRarity.Common));
        Assert.That(attempt.Abandon(), Is.Null);
        Assert.That(attempt.Seal(), Is.Null);
        Assert.That(attempt.Strike(1, 1600).Result, Is.EqualTo(StarSmithStrikeResult.NotDrafting));
        Assert.That(attempt.IsIdle(1_000_000, 60_000), Is.False, "an ended attempt is never idle");
    }

    /// <summary>
    /// EE 9D: the header, then the stars (index, X and Y as 0 to 255, the revealed void flag), the links and the hints.
    /// </summary>
    [Test]
    public async ValueTask TheSkyBytesAsync()
    {
        StarSmithStar[] stars =
        [
            new(0.0, 1.0, StarSmithElement.Fire, 3, false),
            new(0.5, 0.5, StarSmithElement.Light, 5, true, Revealed: true),
        ];
        var recipe = new StarSmithRecipe { Number = 1, Targets = StarSmithTargetKind.Weapon, Shape = new() { Stars = [0, 1] }, OnFail = StarSmithOnFail.ItemDowngraded };
        var sky = new StarSmithSky(stars, [(0, 1)], [new StarSmithHint(0, 1, 1.0)], 2, [recipe], guarded: true);

        var bytes = await SkyBytesAsync(sky).ConfigureAwait(false);

        Assert.That(bytes, Is.EqualTo(new byte[]
        {
            0xC2, 0, 35, 0xEE, 0x9D, 0, TargetSlot, 2, 1, 2, 0x20, 0x03, 0x20, 0x03, 4,
            0, 0, 0, 255, 0,
            0, 1, 128, 128, 1,
            1, 0, 1, 0, 0,
            2, 0, 1, 255, 0,
        }));
    }

    /// <summary>
    /// The sky packet never carries a star's element, value or void before it is struck: two skies that differ only
    /// there give the same bytes. Only a void a Jewel of Guardian reveals is flagged.
    /// </summary>
    [Test]
    public async ValueTask TheSkyHidesUnstruckStarsAsync()
    {
        for (var seed = 1; seed <= 10; seed++)
        {
            var dealt = StarSmithSky.Deal(seed, [Sure(7), Doomed(8)], Sword, 50, 2, [(14, 31)]);
            var scrambled = new StarSmithSky(
                dealt.Stars.Select((s, i) => s.Revealed ? s : s with { Element = (StarSmithElement)((i * 3) % 5), Value = 1 + (i % 5), IsVoid = i % 2 == 0 }).ToList(),
                dealt.Links,
                dealt.Hints,
                dealt.Strikes,
                dealt.Recipes,
                dealt.Guarded);

            var bytes = await SkyBytesAsync(dealt).ConfigureAwait(false);
            Assert.That(await SkyBytesAsync(scrambled).ConfigureAwait(false), Is.EqualTo(bytes), $"seed {seed}");

            var flagged = Enumerable.Range(0, dealt.Stars.Count).Where(i => bytes[15 + (i * 5) + 4] != 0).ToList();
            Assert.That(flagged, Is.EqualTo(Enumerable.Range(0, dealt.Stars.Count).Where(i => dealt.Stars[i].Revealed).ToList()), $"seed {seed}");
            Assert.That(flagged, Has.Count.EqualTo(1), "the Guardian reveals one void");
        }
    }

    /// <summary>
    /// EE 9E: the answer, the struck star (element, value, void), the timing, the strikes left and the guidance; a refused
    /// strike shows nothing of the star.
    /// </summary>
    [Test]
    public async ValueTask TheStrikeBytesAsync()
    {
        var linked = new StarSmithStrike(StarSmithStrikeResult.Linked, StarSmithTiming.Shaky, new StarSmithStar(0.1, 0.2, StarSmithElement.Storm, 4, false));
        Assert.That(await StrikeBytesAsync(3, linked, 2, [1, 300]).ConfigureAwait(false), Is.EqualTo(new byte[] { 0xC1, 16, 0xEE, 0x9E, 0, 3, 2, 4, 0, 1, 2, 2, 1, 0, 0x2C, 1 }));

        var cracked = new StarSmithStrike(StarSmithStrikeResult.Cracked, StarSmithTiming.Clean, new StarSmithStar(0.1, 0.2, StarSmithElement.Shadow, 2, true));
        Assert.That(await StrikeBytesAsync(7, cracked, 0, []).ConfigureAwait(false), Is.EqualTo(new byte[] { 0xC1, 12, 0xEE, 0x9E, 1, 7, 3, 2, 1, 0, 0, 0 }));

        var refused = new StarSmithStrike(StarSmithStrikeResult.NotAdjacent, StarSmithTiming.Late, new StarSmithStar(0.1, 0.2, StarSmithElement.Light, 5, true));
        Assert.That(await StrikeBytesAsync(5, refused, 2, [1]).ConfigureAwait(false), Is.EqualTo(new byte[] { 0xC1, 12, 0xEE, 0x9E, 6, 5, 0, 0, 0, 2, 2, 0 }));
    }

    /// <summary>
    /// EE 9F: the outcome, the flags, the recipe, rarity, extension, dominant element, the fate, the element mix, the
    /// clean strikes and the strikes; a refused seal fills nothing else.
    /// </summary>
    [Test]
    public async ValueTask TheResultBytesAsync()
    {
        var mix = new Dictionary<StarSmithElement, int> { [StarSmithElement.Fire] = 5, [StarSmithElement.Frost] = 4, [StarSmithElement.Storm] = 0, [StarSmithElement.Shadow] = 0, [StarSmithElement.Light] = 0 };
        var sealedResult = new StarSmithForgeResult(StarSmithForgeState.Sealed, Sure(300), [0, 1, 2, 3], mix, StarSmithElement.Fire, 3, StarSmithRarity.Epic, 1, 1.1, true, null);
        Assert.That(await ResultBytesAsync(StarSmithForgeOutcome.Sealed, sealedResult).ConfigureAwait(false), Is.EqualTo(new byte[] { 0xC1, 19, 0xEE, 0x9F, 0, 2, 0x2C, 1, 3, 1, 0, 0xFF, 5, 4, 0, 0, 0, 3, 4 }));

        var cracked = new StarSmithForgeResult(StarSmithForgeState.Cracked, Doomed(8), [0, 1], mix, StarSmithElement.Fire, 1, StarSmithRarity.Common, 0, 0, false, StarSmithOnFail.ItemDestroyed);
        Assert.That(await ResultBytesAsync(StarSmithForgeOutcome.Cracked, cracked).ConfigureAwait(false), Is.EqualTo(new byte[] { 0xC1, 19, 0xEE, 0x9F, 1, 0, 8, 0, 0, 0, 0, 2, 5, 4, 0, 0, 0, 1, 2 }));

        var attuned = sealedResult with { Recipe = null, FirstCompletion = false, Dominant = null, Rarity = StarSmithRarity.Uncommon, Extension = 0 };
        Assert.That(await ResultBytesAsync(StarSmithForgeOutcome.Sealed, attuned).ConfigureAwait(false), Is.EqualTo(new byte[] { 0xC1, 19, 0xEE, 0x9F, 0, 1, 0, 0, 1, 0, 0xFF, 0xFF, 5, 4, 0, 0, 0, 3, 4 }));

        Assert.That(await ResultBytesAsync(StarSmithForgeOutcome.Refused, null).ConfigureAwait(false), Is.EqualTo(new byte[] { 0xC1, 19, 0xEE, 0x9F, 4, 0, 0, 0, 0, 0, 0xFF, 0xFF, 0, 0, 0, 0, 0, 0, 0 }));
    }

    /// <summary>
    /// Start, three strikes on the beat and a seal: the sky goes out, the ingredient and the hammer's durability are spent
    /// at the start, the seal writes the rarity into the item's customisation and ends the attempt.
    /// </summary>
    [Test]
    public async ValueTask StartStrikesAndSealAppliesAsync()
    {
        var (player, plugIn, hammer) = await SmithAsync(Sure(7)).ConfigureAwait(false);
        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<IStarSmithingViewPlugIn>()!);

        await plugIn.StartForgeAsync(player, TargetSlot, [IngredientSlot], []).ConfigureAwait(false);
        var attempt = player.StarSmithForge;
        Assert.That(attempt, Is.Not.Null);
        view.Verify(v => v.SendForgeSkyAsync(It.Is<StarSmithForgeSkyMessage>(m => m.Result == StarSmithResultCode.Done && m.Strikes == 3)), Times.Once);
        Assert.That(player.Inventory!.GetItem(IngredientSlot), Is.Null, "the ingredient is spent at the start");
        Assert.That(hammer.Durability, Is.EqualTo(250));

        foreach (var star in new byte[] { 0, 1, 2 })
        {
            this._now += 800;
            await plugIn.StrikeForgeAsync(player, star).ConfigureAwait(false);
        }

        view.Verify(v => v.SendForgeStrikeAsync(It.IsAny<int>(), It.Is<StarSmithStrike>(s => s.Result == StarSmithStrikeResult.Linked && s.Timing == StarSmithTiming.Clean), It.IsAny<int>(), It.IsAny<IReadOnlyList<short>>()), Times.Exactly(3));
        Assert.That(attempt!.Strikes.Select(s => s.TimeMs), Is.EqualTo(new long[] { 800, 1600, 2400 }), "stamped with the server's clock");

        await plugIn.SealForgeAsync(player).ConfigureAwait(false);
        view.Verify(v => v.SendForgeResultAsync(StarSmithForgeOutcome.Sealed, It.Is<StarSmithForgeResult>(r => r.Recipe!.Number == 7 && r.Rarity == StarSmithRarity.Epic)), Times.Once);
        Assert.That(player.StarSmithForge, Is.Null, "the attempt is over");
        var record = await CustomizationAsync(player).ConfigureAwait(false);
        Assert.That(record?.Rarity, Is.EqualTo((byte)StarSmithRarity.Epic));
        Assert.That(player.Inventory.GetItem(TargetSlot), Is.Not.Null);
    }

    /// <summary>
    /// A seal rolls the recipe's outcome and stores its boosts and the smith's level on the item's record; a
    /// downgrade clears them again.
    /// </summary>
    [Test]
    public async ValueTask ASealStoresItsBoostsAsync()
    {
        var recipe = Sure(7);
        recipe.Outcomes = [new StarSmithOutcome { Weight = 1, Tier = StarSmithRarity.Common, Boosts = [new() { Stat = StarSmithBoostStat.Defense, Minimum = 2, Maximum = 2 }] }];
        var (player, plugIn, _) = await SmithAsync(recipe).ConfigureAwait(false);
        player.SelectedCharacter!.StarSmithLevel = 40;
        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<IStarSmithingViewPlugIn>()!);
        await plugIn.StartForgeAsync(player, TargetSlot, [IngredientSlot], []).ConfigureAwait(false);
        foreach (var star in new byte[] { 0, 1, 2 })
        {
            this._now += 800;
            await plugIn.StrikeForgeAsync(player, star).ConfigureAwait(false);
        }

        StarSmithForgeResult? sealedResult = null;
        view.Setup(v => v.SendForgeResultAsync(StarSmithForgeOutcome.Sealed, It.IsAny<StarSmithForgeResult>())).Callback<StarSmithForgeOutcome, StarSmithForgeResult?>((_, r) => sealedResult = r);
        await plugIn.SealForgeAsync(player).ConfigureAwait(false);

        var record = await CustomizationAsync(player).ConfigureAwait(false);
        Assert.That(sealedResult, Is.Not.Null);
        var expected = (int)Math.Round(Math.Min(2 * sealedResult!.MagnitudeFactor, 6) * 100);
        Assert.That(StarSmithBoosts.Parse(record?.Boosts), Is.EqualTo(new[] { new StarSmithBoost(StarSmithBoostStat.Defense, expected) }));
        Assert.That(expected, Is.GreaterThan(200), "scaled by the smith level");
        Assert.That(record!.SmithLevel, Is.EqualTo(40));
    }

    /// <summary>
    /// A void on a risky recipe's shape cracks the attempt and destroys the item, as its onFail says.
    /// </summary>
    [Test]
    public async ValueTask AVoidCracksWithTheRecipesFateAsync()
    {
        var (player, plugIn, _) = await SmithAsync(Doomed(8)).ConfigureAwait(false);
        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<IStarSmithingViewPlugIn>()!);
        await plugIn.StartForgeAsync(player, TargetSlot, [IngredientSlot], []).ConfigureAwait(false);
        var attempt = player.StarSmithForge!;

        foreach (var star in new byte[] { 0, 1, 2 })
        {
            this._now += 800;
            await plugIn.StrikeForgeAsync(player, star).ConfigureAwait(false);
            if (attempt.Sky.Stars[star].IsVoid)
            {
                break;
            }
        }

        view.Verify(v => v.SendForgeResultAsync(StarSmithForgeOutcome.Cracked, It.Is<StarSmithForgeResult>(r => r.Fail == StarSmithOnFail.ItemDestroyed && r.Recipe!.Number == 8)), Times.Once);
        Assert.That(player.Inventory!.GetItem(TargetSlot), Is.Null, "the item is destroyed");
        Assert.That(player.StarSmithForge, Is.Null);
    }

    /// <summary>
    /// A refused strike is answered and changes nothing.
    /// </summary>
    [Test]
    public async ValueTask ARefusedStrikeChangesNothingAsync()
    {
        var (player, plugIn, _) = await SmithAsync(Sure(7)).ConfigureAwait(false);
        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<IStarSmithingViewPlugIn>()!);
        await plugIn.StartForgeAsync(player, TargetSlot, [IngredientSlot], []).ConfigureAwait(false);
        var attempt = player.StarSmithForge!;

        this._now += 800;
        await plugIn.StrikeForgeAsync(player, 99).ConfigureAwait(false);

        view.Verify(v => v.SendForgeStrikeAsync(99, It.Is<StarSmithStrike>(s => s.Result == StarSmithStrikeResult.NoSuchStar), 3, It.IsAny<IReadOnlyList<short>>()), Times.Once);
        Assert.That(attempt.Strikes, Is.Empty);
        Assert.That(attempt.Forge.State, Is.EqualTo(StarSmithForgeState.Drafting));
        Assert.That(player.StarSmithForge, Is.SameAs(attempt));
    }

    /// <summary>
    /// Abandon: the ingredients are lost, the item stays; then a seal is refused.
    /// </summary>
    [Test]
    public async ValueTask AbandonKeepsTheItemAsync()
    {
        var (player, plugIn, _) = await SmithAsync(Sure(7)).ConfigureAwait(false);
        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<IStarSmithingViewPlugIn>()!);
        await plugIn.StartForgeAsync(player, TargetSlot, [IngredientSlot], []).ConfigureAwait(false);
        this._now += 800;
        await plugIn.StrikeForgeAsync(player, 0).ConfigureAwait(false);

        await plugIn.AbandonForgeAsync(player).ConfigureAwait(false);

        view.Verify(v => v.SendForgeResultAsync(StarSmithForgeOutcome.Abandoned, It.Is<StarSmithForgeResult>(r => r.Fail == StarSmithOnFail.IngredientsLost)), Times.Once);
        Assert.That(player.StarSmithForge, Is.Null);
        Assert.That(player.Inventory!.GetItem(TargetSlot), Is.Not.Null);
        Assert.That(player.Inventory.GetItem(IngredientSlot), Is.Null);
        Assert.That(await CustomizationAsync(player).ConfigureAwait(false), Is.Null, "nothing written");

        await plugIn.SealForgeAsync(player).ConfigureAwait(false);
        view.Verify(v => v.SendForgeResultAsync(StarSmithForgeOutcome.Refused, null), Times.Once);
    }

    /// <summary>
    /// An attempt idle for a minute times out like an abandon.
    /// </summary>
    [Test]
    public async ValueTask AnIdleAttemptTimesOutAsync()
    {
        var (player, plugIn, _) = await SmithAsync(Sure(7)).ConfigureAwait(false);
        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<IStarSmithingViewPlugIn>()!);
        await plugIn.StartForgeAsync(player, TargetSlot, [IngredientSlot], []).ConfigureAwait(false);

        this._now = 59_999;
        await plugIn.TimeOutIdleForgeAsync(player).ConfigureAwait(false);
        Assert.That(player.StarSmithForge, Is.Not.Null);

        this._now = 60_000;
        await plugIn.TimeOutIdleForgeAsync(player).ConfigureAwait(false);
        Assert.That(player.StarSmithForge, Is.Null);
        view.Verify(v => v.SendForgeResultAsync(StarSmithForgeOutcome.TimedOut, It.Is<StarSmithForgeResult>(r => r.Fail == StarSmithOnFail.IngredientsLost)), Times.Once);
        Assert.That(player.Inventory!.GetItem(TargetSlot), Is.Not.Null);
    }

    /// <summary>
    /// A seal, strike or abandon outside an attempt is refused; a second start while one runs too.
    /// </summary>
    [Test]
    public async ValueTask OutsideAnAttemptIsRefusedAsync()
    {
        var (player, plugIn, _) = await SmithAsync(Sure(7)).ConfigureAwait(false);
        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<IStarSmithingViewPlugIn>()!);

        await plugIn.SealForgeAsync(player).ConfigureAwait(false);
        await plugIn.AbandonForgeAsync(player).ConfigureAwait(false);
        view.Verify(v => v.SendForgeResultAsync(StarSmithForgeOutcome.Refused, null), Times.Exactly(2));
        await plugIn.StrikeForgeAsync(player, 0).ConfigureAwait(false);
        view.Verify(v => v.SendForgeStrikeAsync(0, It.Is<StarSmithStrike>(s => s.Result == StarSmithStrikeResult.NotDrafting), 0, It.IsAny<IReadOnlyList<short>>()), Times.Once);

        await plugIn.StartForgeAsync(player, TargetSlot, [IngredientSlot], []).ConfigureAwait(false);
        await plugIn.SealForgeAsync(player).ConfigureAwait(false);
        view.Verify(v => v.SendForgeResultAsync(StarSmithForgeOutcome.Refused, null), Times.Exactly(3), "nothing struck yet");
        Assert.That(player.StarSmithForge, Is.Not.Null, "a refused seal keeps the attempt");

        await plugIn.StartForgeAsync(player, TargetSlot, [], []).ConfigureAwait(false);
        view.Verify(v => v.SendForgeSkyAsync(It.Is<StarSmithForgeSkyMessage>(m => m.Result == StarSmithResultCode.AttemptRunning)), Times.Once);
    }

    /// <summary>
    /// The start checks its slots: the item, the ingredients that make a recipe, no slot twice.
    /// </summary>
    [Test]
    public async ValueTask TheStartChecksItsSlotsAsync()
    {
        var (player, plugIn, _) = await SmithAsync(Sure(7)).ConfigureAwait(false);
        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<IStarSmithingViewPlugIn>()!);

        await plugIn.StartForgeAsync(player, TargetSlot, [], []).ConfigureAwait(false);
        await plugIn.StartForgeAsync(player, TargetSlot, [IngredientSlot, IngredientSlot], []).ConfigureAwait(false);
        await plugIn.StartForgeAsync(player, TargetSlot, [TargetSlot], []).ConfigureAwait(false);
        await plugIn.StartForgeAsync(player, 40, [IngredientSlot], []).ConfigureAwait(false);

        view.Verify(v => v.SendForgeSkyAsync(It.Is<StarSmithForgeSkyMessage>(m => m.Result == StarSmithResultCode.NoRecipe)), Times.Once);
        view.Verify(v => v.SendForgeSkyAsync(It.Is<StarSmithForgeSkyMessage>(m => m.Result == StarSmithResultCode.WrongSlot)), Times.Exactly(2));
        view.Verify(v => v.SendForgeSkyAsync(It.Is<StarSmithForgeSkyMessage>(m => m.Result == StarSmithResultCode.NoItem)), Times.Once);
        Assert.That(player.StarSmithForge, Is.Null);
        Assert.That(player.Inventory!.GetItem(IngredientSlot), Is.Not.Null, "nothing is spent by a refused start");
    }

    private static StarSmithRecipe Sure(short number) => new()
    {
        Number = number,
        Targets = StarSmithTargetKind.Weapon,
        Ingredients = [new StarSmithIngredient { Group = 14, Number = 13 }],
        BaseChance = 10_000,
        Shape = new() { Stars = [0, 1, 2] },
        OnFail = StarSmithOnFail.ItemDowngraded,
    };

    private static StarSmithRecipe Doomed(short number) => new()
    {
        Number = number,
        Targets = StarSmithTargetKind.Weapon,
        Ingredients = [new StarSmithIngredient { Group = 14, Number = 13 }],
        MaximumChance = 0,
        Shape = new() { Stars = [0, 1, 2] },
        OnFail = StarSmithOnFail.ItemDestroyed,
        Risky = true,
    };

    private static async ValueTask<DataModel.Entities.ItemCustomization?> CustomizationAsync(Player player)
    {
        var id = ((MUnique.OpenMU.Persistence.IIdentifiable)player.Inventory!.GetItem(TargetSlot)!).Id;
        return (await player.PersistenceContext.GetItemCustomizationsAsync([id]).ConfigureAwait(false)).FirstOrDefault();
    }

    private static async ValueTask<byte[]> SkyBytesAsync(StarSmithSky sky)
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(GameServer.BabylonClient.Version);
        await CallAsync(player, "SendForgeSkyAsync", StarSmithForgeSkyMessage.From(sky, TargetSlot, Beat, 800)).ConfigureAwait(false);
        return output.ToArray();
    }

    private static async ValueTask<byte[]> StrikeBytesAsync(int star, StarSmithStrike strike, int left, short[] guidance)
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(GameServer.BabylonClient.Version);
        await CallAsync(player, "SendForgeStrikeAsync", star, strike, left, (IReadOnlyList<short>)guidance).ConfigureAwait(false);
        return output.ToArray();
    }

    private static async ValueTask<byte[]> ResultBytesAsync(StarSmithForgeOutcome outcome, StarSmithForgeResult? result)
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(GameServer.BabylonClient.Version);
        await CallAsync(player, "SendForgeResultAsync", outcome, result!).ConfigureAwait(false);
        return output.ToArray();
    }

    private static async ValueTask CallAsync(object player, string method, params object[] arguments)
    {
        var type = Type.GetType(PlugIn);
        Assert.That(type, Is.Not.Null);
        var send = type!.GetMethod(method);
        Assert.That(send, Is.Not.Null);
        await ((ValueTask)send!.Invoke(Activator.CreateInstance(type, player), arguments)!).ConfigureAwait(false);
    }

    private static async ValueTask<Item> AddAsync(Player player, byte slot, byte group, short number, byte durability = 1)
    {
        var item = player.PersistenceContext.CreateNew<Item>();
        item.Definition = new MUnique.OpenMU.Persistence.BasicModel.ItemDefinition { Group = group, Number = number, Width = 1, Height = 1, Durability = durability };
        item.Durability = durability;
        await player.Inventory!.AddItemAsync(slot, item).ConfigureAwait(false);
        return item;
    }

    private async ValueTask<(Player Player, StarSmithingFeaturePlugIn PlugIn, Item Hammer)> SmithAsync(StarSmithRecipe recipe)
    {
        this._now = 0;
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        var hammer = await AddAsync(player, HammerSlot, 2, 1000, 255).ConfigureAwait(false);
        await AddAsync(player, TargetSlot, 0, 1).ConfigureAwait(false);
        await AddAsync(player, IngredientSlot, 14, 13).ConfigureAwait(false);
        var plugIn = new StarSmithingFeaturePlugIn
        {
            Configuration = new StarSmithingConfiguration { Recipes = [recipe] },
            Clock = () => this._now,
            NewSeed = () => 4711,
        };
        return (player, plugIn, hammer);
    }
}
