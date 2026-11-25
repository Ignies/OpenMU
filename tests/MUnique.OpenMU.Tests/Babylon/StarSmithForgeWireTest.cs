// <copyright file="StarSmithForgeWireTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using System.Buffers.Binary;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.StarSmithing;
using MUnique.OpenMU.GameLogic.Views.StarSmithing;
using Moq;

/// <summary>
/// The Constellation Forge on the wire (Readme.md "PR 7a contract", "Wire"): the attempt kept on the player, the
/// packets EE 9D-9F, and the feature plug-in's start, node, stash, seal, abandon and idle timeout, with the layers and
/// finds they write.
/// </summary>
[TestFixture]
public class StarSmithForgeWireTest
{
    private const byte HammerSlot = 0;

    private const byte TargetSlot = 12;

    private const byte IngredientSlot = 13;

    private const int Interval = 800;

    private const string PlugIn = "MUnique.OpenMU.GameServer.RemoteView.StarSmithing.StarSmithingBabylonPlugIn, MUnique.OpenMU.GameServer";

    private static readonly StarSmithBeat Beat = new(IntervalMs: Interval, ToleranceMs: 90, RoundTripCapMs: 150);

    private static readonly StarSmithTarget Sword = new(StarSmithTargetKind.Weapon, Level: 0, Excellent: false);

    private long _now;

    /// <summary>
    /// An attempt keeps every accepted node and stash; replaying it gives the same result. A refused node is not kept.
    /// </summary>
    [Test]
    public void AnAttemptIsKeptAndReplays()
    {
        var attempt = StarSmithForgeAttempt.Start(4711, [Fire()], Sword, 1, 3, [], NoFaults(), Beat, 0, 0, new HashSet<short>(), [], TargetSlot, Guid.NewGuid());
        var points = FireAt(0.3, 0.5);
        for (var i = 0; i < points.Count; i++)
        {
            Assert.That(attempt.Place(points[i], (i + 1) * Interval).Result, Is.EqualTo(StarSmithFigureResult.Done));
        }

        Assert.That(attempt.Place(points[1], 9 * Interval).Result, Is.EqualTo(StarSmithFigureResult.FigureFull).Or.EqualTo(StarSmithFigureResult.TooNear));
        Assert.That(attempt.LastActivityMs, Is.EqualTo(4 * Interval), "a refused node is no activity");
        var sealedResult = attempt.Seal();
        var replay = attempt.Replay();
        Assert.Multiple(() =>
        {
            Assert.That(replay.State, Is.EqualTo(StarSmithForgeState.Sealed));
            Assert.That(StarSmithLayers.Format(replay.Result!.Layers), Is.EqualTo(StarSmithLayers.Format(sealedResult!.Layers)));
            Assert.That(replay.Result.Rarity, Is.EqualTo(sealedResult.Rarity));
        });
    }

    /// <summary>
    /// Abandoning loses the ingredients and keeps the item; an idle attempt is due one minute after its last node.
    /// </summary>
    [Test]
    public void AbandonAndIdle()
    {
        var attempt = StarSmithForgeAttempt.Start(4711, [Fire()], Sword, 1, 1, [], NoFaults(), Beat, 0, 0, new HashSet<short>(), [], TargetSlot, Guid.NewGuid());
        Assert.That(attempt.IsIdle(59_999, 60_000), Is.False);
        Assert.That(attempt.IsIdle(60_000, 60_000), Is.True);
        attempt.Place(new(10000, 10000), Interval);
        Assert.That(attempt.IsIdle(60_000, 60_000), Is.False, "a node counts as activity");

        var abandoned = attempt.Abandon();
        Assert.Multiple(() =>
        {
            Assert.That(abandoned!.State, Is.EqualTo(StarSmithForgeState.Abandoned));
            Assert.That(abandoned.Fail, Is.EqualTo(StarSmithOnFail.IngredientsLost));
            Assert.That(attempt.Abandon(), Is.Null);
            Assert.That(attempt.Seal(), Is.Null);
            Assert.That(attempt.Place(new(20000, 20000), 2 * Interval).Result, Is.EqualTo(StarSmithFigureResult.NoAttempt));
            Assert.That(attempt.IsIdle(1_000_000, 60_000), Is.False, "an ended attempt is never idle");
        });
    }

    /// <summary>
    /// EE 9D: the header, the counts, the beat, the revealed fault, the radii in box units and the item's layers.
    /// </summary>
    [Test]
    public async ValueTask TheAttemptBytesAsync()
    {
        var layer = new StarSmithLayer(101, StarSmithSignTier.Venom, 1, [new(1, 2), new(3, 4), new(5, 6)]);
        var message = new StarSmithForgeAttemptMessage(StarSmithResultCode.Done, TargetSlot, 10, StarSmithOnFail.ItemDowngraded, 1, 2, 800, 700, new StarSmithFault(new(100, 200), new(300, 400)), 1638, 2621, 1966, 3, [layer]);
        var bytes = await BytesAsync("SendForgeAttemptAsync", message).ConfigureAwait(false);
        Assert.Multiple(() =>
        {
            Assert.That(bytes, Has.Length.EqualTo(179));
            Assert.That(bytes[..31], Is.EqualTo(new byte[]
            {
                0xC1, 179, 0xEE, 0x9D, 0, TargetSlot, 10, 1, 1, 2, 0x20, 0x03, 0xBC, 0x02, 1,
                100, 0, 200, 0, 0x2C, 1, 0x90, 1, 0x66, 0x06, 0x3D, 0x0A, 0xAE, 0x07, 3, 1,
            }));
            Assert.That(bytes[31..48], Is.EqualTo(new byte[] { 101, 0, 3, 1, 3, 1, 0, 2, 0, 3, 0, 4, 0, 5, 0, 6, 0 }));
            Assert.That(bytes[48..], Is.All.EqualTo(0));
        });

        var refused = await BytesAsync("SendForgeAttemptAsync", StarSmithForgeAttemptMessage.Refused(StarSmithResultCode.AttemptRunning, TargetSlot)).ConfigureAwait(false);
        Assert.That(refused[..6], Is.EqualTo(new byte[] { 0xC1, 179, 0xEE, 0x9D, 12, TargetSlot }));
        Assert.That(refused[6..], Is.All.EqualTo(0));
    }

    /// <summary>
    /// The faults are never sent: attempts on different seeds give the same EE 9D, unless a Jewel of Guardian reveals
    /// one, and then only its 8 bytes differ.
    /// </summary>
    [Test]
    public async ValueTask TheAttemptHidesTheFaultsAsync()
    {
        var rules = new StarSmithForgeRules();
        async ValueTask<byte[]> Of(long seed, bool guarded)
        {
            var forge = StarSmithForge.Start(seed, [Fire()], 1, 3, guarded, rules, Beat, 0, 0, new HashSet<short>(), []);
            return await BytesAsync("SendForgeAttemptAsync", StarSmithForgeAttemptMessage.From(forge, TargetSlot, Beat, 800, rules)).ConfigureAwait(false);
        }

        var first = await Of(1, false).ConfigureAwait(false);
        for (var seed = 2; seed <= 10; seed++)
        {
            Assert.That(await Of(seed, false).ConfigureAwait(false), Is.EqualTo(first), $"seed {seed}");
            var guarded = await Of(seed, true).ConfigureAwait(false);
            Assert.That(guarded[14], Is.EqualTo(1));
            Assert.That(guarded[..14].Concat(guarded[23..]), Is.EqualTo(first[..14].Concat(first[23..])), $"seed {seed}");
        }
    }

    /// <summary>
    /// EE 9E: kind, answer, timing, strikes, flags, counts, the match, the measured turn and ratio, the guide, the
    /// placements and the hull; a refusal fills only the counts.
    /// </summary>
    [Test]
    public async ValueTask TheNodeBytesAsync()
    {
        var fire = Fire();
        var measure = new StarSmithMeasure(true, false, false, [1000, 1000, 874, 1000], 937, StarSmithSignTier.Virulent);
        var answer = new StarSmithFigureAnswer(
            StarSmithFigureKind.Node,
            StarSmithFigureResult.Done,
            StarSmithTiming.Shaky,
            6,
            StarSmithFigureFlags.Closed | StarSmithFigureFlags.Found,
            4,
            1,
            fire,
            measure,
            -118.24,
            1.0349,
            [102, 113],
            [(StarSmithTiming.Clean, 1000), (StarSmithTiming.Clean, 1000), (StarSmithTiming.Clean, 874), (StarSmithTiming.Shaky, (int?)null)],
            [new(10, 20), new(30, 40), new(50, 60)]);
        var bytes = await BytesAsync("SendForgeNodeAsync", answer).ConfigureAwait(false);
        Assert.Multiple(() =>
        {
            Assert.That(bytes, Has.Length.EqualTo(107));
            Assert.That(bytes[..22], Is.EqualTo(new byte[] { 0xC1, 107, 0xEE, 0x9E, 0, 0, 1, 6, 9, 4, 1, 101, 0, 1, 4, 0xA9, 0x03, 0x62, 0xFB, 0x0B, 0x04, 2 }));
            Assert.That(bytes[22..26], Is.EqualTo(new byte[] { 102, 0, 113, 0 }));
            Assert.That(bytes[38..50], Is.EqualTo(new byte[] { 0, 0xE8, 0x03, 0, 0xE8, 0x03, 0, 0x6A, 0x03, 1, 0xFF, 0xFF }));
            Assert.That(bytes[74..87], Is.EqualTo(new byte[] { 3, 10, 0, 20, 0, 30, 0, 40, 0, 50, 0, 60, 0 }));
        });

        var refused = await BytesAsync("SendForgeNodeAsync", StarSmithFigureAnswer.Refused(StarSmithFigureKind.Stash, StarSmithFigureResult.NoSlot, 3, 2, 1)).ConfigureAwait(false);
        Assert.That(refused[..11], Is.EqualTo(new byte[] { 0xC1, 107, 0xEE, 0x9E, 1, 8, 0xFF, 3, 0, 2, 1 }));
        Assert.That(refused[11..15], Is.EqualTo(new byte[] { 0, 0, 0, 0xFF }));
        Assert.That(refused[15..], Is.All.EqualTo(0));
    }

    /// <summary>
    /// EE 9F: the outcome, flags, rarity, fate, placements, experience, the fate's constellation and the figures.
    /// </summary>
    [Test]
    public async ValueTask TheResultBytesAsync()
    {
        var fire = Fire();
        var layers = new[] { new StarSmithLayer(101, StarSmithSignTier.Chroma, 1, [new(1, 2), new(3, 4), new(5, 6)]) };
        var sealedResult = new StarSmithForgeResult(StarSmithForgeState.Sealed, [new StarSmithSealedFigure(fire, StarSmithSignTier.Chroma, 1000, StarSmithLayerChange.Raised, 0)], 0, null, null, 5, 4, false, StarSmithRarity.Mythic, layers, null, StarSmithSignTier.Faint);
        Assert.That(await BytesAsync("SendForgeResultAsync", StarSmithForgeOutcome.Sealed, sealedResult, 80, true).ConfigureAwait(false), Is.EqualTo(new byte[]
        {
            0xC1, 41, 0xEE, 0x9F, 0, 2, 5, 0xFF, 4, 5, 80, 0, 0, 0, 0, 0, 1,
            101, 0, 5, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        }));

        var cracked = new StarSmithForgeResult(StarSmithForgeState.Cracked, [], 2, fire, StarSmithOnFail.ItemDestroyed, 3, 1, false, StarSmithRarity.Common, [], null, StarSmithSignTier.Faint);
        var crackBytes = await BytesAsync("SendForgeResultAsync", StarSmithForgeOutcome.Cracked, cracked, 12, false).ConfigureAwait(false);
        Assert.That(crackBytes[..17], Is.EqualTo(new byte[] { 0xC1, 41, 0xEE, 0x9F, 1, 0, 0, 2, 1, 3, 12, 0, 0, 0, 101, 0, 2 }));

        var refused = await BytesAsync("SendForgeResultAsync", StarSmithForgeOutcome.Refused, null!, 0, false).ConfigureAwait(false);
        Assert.That(refused[..8], Is.EqualTo(new byte[] { 0xC1, 41, 0xEE, 0x9F, 4, 0, 0, 0xFF }));
        Assert.That(refused[8..], Is.All.EqualTo(0));
    }

    /// <summary>
    /// Start, Fire on the beat and a seal: EE 9D goes out, the ingredient and durability are spent at the start, the
    /// fourth node finds Fire (the Guidebook has it at once, made 0), and the seal writes the layer and the rarity,
    /// counts it made and doubles the first seal's experience.
    /// </summary>
    [Test]
    public async ValueTask FireOnTheBeatIsFoundAndSealedAsync()
    {
        var (player, plugIn, hammer) = await SmithAsync(Fire(), ingredients: 2).ConfigureAwait(false);
        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<IStarSmithingViewPlugIn>()!);

        await plugIn.StartForgeAsync(player, TargetSlot, [IngredientSlot, 14], []).ConfigureAwait(false);
        view.Verify(v => v.SendForgeAttemptAsync(It.Is<StarSmithForgeAttemptMessage>(m => m.Result == StarSmithResultCode.Done && m.Strikes == 4 && m.FreeSlots == 1 && m.Slots == 1 && m.RevealedFault == null)), Times.Once);
        Assert.That(player.Inventory!.GetItem(IngredientSlot), Is.Null, "the ingredient is spent at the start");
        Assert.That(hammer.Durability, Is.EqualTo(250));

        foreach (var point in FireAt(0.3, 0.5))
        {
            this._now += Interval;
            await plugIn.PlaceForgeNodeAsync(player, point).ConfigureAwait(false);
        }

        view.Verify(v => v.SendForgeNodeAsync(It.Is<StarSmithFigureAnswer>(a => a.Result == StarSmithFigureResult.Done && a.Timing == StarSmithTiming.Clean)), Times.Exactly(4));
        view.Verify(v => v.SendForgeNodeAsync(It.Is<StarSmithFigureAnswer>(a => a.Flags.HasFlag(StarSmithFigureFlags.Found) && a.Match!.Number == 101)), Times.Once);
        Assert.That(player.SelectedCharacter!.StarSmithGuidebook, Is.EqualTo("101:0"), "found at once");

        await plugIn.SealForgeAsync(player).ConfigureAwait(false);
        view.Verify(v => v.SendForgeResultAsync(StarSmithForgeOutcome.Sealed, It.Is<StarSmithForgeResult>(r => r.Figures.Single().Tier == StarSmithSignTier.Virulent), 80, true), Times.Once);
        var record = await CustomizationAsync(player).ConfigureAwait(false);
        Assert.Multiple(() =>
        {
            Assert.That(player.StarSmithForge, Is.Null, "the attempt is over");
            Assert.That(record?.Rarity, Is.EqualTo((byte)StarSmithRarity.Legendary));
            Assert.That(StarSmithLayers.Parse(record?.Layers).Select(l => (l.Number, l.Tier, l.Look, l.Hull.Count)), Is.EqualTo(new[] { ((short)101, StarSmithSignTier.Virulent, (byte)1, 3) }));
            Assert.That(record!.Boosts, Is.Null, "a looks constellation rolls no boost");
            Assert.That(player.SelectedCharacter.StarSmithGuidebook, Is.EqualTo("101:1"));
            Assert.That(player.SelectedCharacter.StarSmithExperience, Is.EqualTo(80));
        });
    }

    /// <summary>
    /// A boost constellation sealed rolls its outcome at its tier and stores the boosts; a downgrade clears boosts and
    /// layers again.
    /// </summary>
    [Test]
    public async ValueTask ABoostConstellationStoresItsBoostsAsync()
    {
        var recipe = Fire();
        recipe.Number = 7;
        recipe.Look = StarSmithLook.None;
        recipe.Outcomes = [new StarSmithOutcome { Weight = 1, Tier = StarSmithRarity.Common, Boosts = [new() { Stat = StarSmithBoostStat.Defense, Minimum = 2, Maximum = 2 }] }];
        var (player, plugIn, _) = await SmithAsync(recipe, ingredients: 2).ConfigureAwait(false);
        player.SelectedCharacter!.StarSmithLevel = 40;
        await plugIn.StartForgeAsync(player, TargetSlot, [IngredientSlot, 14], []).ConfigureAwait(false);
        foreach (var point in FireAt(0.3, 0.5))
        {
            this._now += Interval;
            await plugIn.PlaceForgeNodeAsync(player, point).ConfigureAwait(false);
        }

        await plugIn.SealForgeAsync(player).ConfigureAwait(false);
        var record = await CustomizationAsync(player).ConfigureAwait(false);
        var expected = (int)Math.Round(Math.Min(2 * new StarSmithBalance().LevelFactor(40), 6) * 100);
        Assert.Multiple(() =>
        {
            Assert.That(StarSmithBoosts.Parse(record?.Boosts), Is.EqualTo(new[] { new StarSmithBoost(StarSmithBoostStat.Defense, expected) }));
            Assert.That(record!.SmithLevel, Is.EqualTo(40));
            Assert.That(StarSmithLayers.Parse(record.Layers).Single().Look, Is.EqualTo(0));
            Assert.That(StarSmithBoostBook.Get(record.ItemId), Is.EqualTo(StarSmithBoosts.Parse(record.Boosts)), "in play at once");
        });
        StarSmithBoostBook.Forget(record!.ItemId);
    }

    /// <summary>
    /// A level 21 smith stashes Fire and seals Gold beside it: two layers in the order banked.
    /// </summary>
    [Test]
    public async ValueTask StashThenSealWritesTwoLayersAsync()
    {
        var gold = Fire();
        gold.Number = 113;
        gold.Look = StarSmithLook.Gold;
        gold.Geometry = StarSmithGeometry.Of(4, true, [90, 90, 90], [1, 1, 1, 1]);
        var (player, plugIn, _) = await SmithAsync([Fire(), gold], ingredients: 7).ConfigureAwait(false);
        player.SelectedCharacter!.StarSmithLevel = 21;
        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<IStarSmithingViewPlugIn>()!);
        await plugIn.StartForgeAsync(player, TargetSlot, [IngredientSlot, 14, 15, 16, 17, 18, 19], []).ConfigureAwait(false);
        foreach (var point in FireAt(0.1, 0.3))
        {
            this._now += Interval;
            await plugIn.PlaceForgeNodeAsync(player, point).ConfigureAwait(false);
        }

        await plugIn.StashForgeAsync(player).ConfigureAwait(false);
        view.Verify(v => v.SendForgeNodeAsync(It.Is<StarSmithFigureAnswer>(a => a.Kind == StarSmithFigureKind.Stash && a.Result == StarSmithFigureResult.Done && a.Banked == 1)), Times.Once);
        foreach (var point in StarSmithGeometryTest.Draw(gold.Geometry, 0, 0.1, false, false, 0.6, 0.7))
        {
            this._now += Interval;
            await plugIn.PlaceForgeNodeAsync(player, point).ConfigureAwait(false);
        }

        await plugIn.SealForgeAsync(player).ConfigureAwait(false);
        var record = await CustomizationAsync(player).ConfigureAwait(false);
        Assert.That(StarSmithLayers.Parse(record?.Layers).Select(l => l.Number), Is.EqualTo(new short[] { 101, 113 }));
        Assert.That(player.SelectedCharacter.StarSmithGuidebook, Is.EqualTo("101:1;113:1"));
    }

    /// <summary>
    /// A node on a fault cracks the attempt; on a risky constellation's way the item is destroyed, and the stash is
    /// lost with it.
    /// </summary>
    [Test]
    public async ValueTask AFaultCracksWithTheFateAsync()
    {
        var risky = Fire();
        risky.Number = 8;
        risky.OnFail = StarSmithOnFail.ItemDestroyed;
        risky.Risky = true;
        var (player, plugIn, _) = await SmithAsync(risky, ingredients: 2, faults: 1).ConfigureAwait(false);
        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<IStarSmithingViewPlugIn>()!);
        await plugIn.StartForgeAsync(player, TargetSlot, [IngredientSlot, 14], []).ConfigureAwait(false);
        var fault = player.StarSmithForge!.Forge.Faults[0];
        var start = (X: 0.02, Y: 0.07);
        var points = StarSmithGeometryTest.Draw(risky.Geometry!, 0, 0.08, false, false, start.X, start.Y).Take(3).ToList();
        foreach (var point in points)
        {
            this._now += Interval;
            await plugIn.PlaceForgeNodeAsync(player, point).ConfigureAwait(false);
        }

        Assume.That(player.StarSmithForge, Is.Not.Null, "no early crack");
        this._now += Interval;
        await plugIn.PlaceForgeNodeAsync(player, new((ushort)((fault.A.X + fault.B.X) / 2), (ushort)((fault.A.Y + fault.B.Y) / 2))).ConfigureAwait(false);

        view.Verify(v => v.SendForgeNodeAsync(It.Is<StarSmithFigureAnswer>(a => a.Result == StarSmithFigureResult.Cracked && a.Flags == StarSmithFigureFlags.Fault)), Times.Once);
        view.Verify(v => v.SendForgeResultAsync(StarSmithForgeOutcome.Cracked, It.Is<StarSmithForgeResult>(r => r.Fail == StarSmithOnFail.ItemDestroyed && r.FateRecipe!.Number == 8), 12, false), Times.Once);
        Assert.That(player.Inventory!.GetItem(TargetSlot), Is.Null, "the item is destroyed");
        Assert.That(player.StarSmithForge, Is.Null);
    }

    /// <summary>
    /// A downgrade (a crack on a downgrading constellation's way) clears the record and its layers.
    /// </summary>
    [Test]
    public async ValueTask ADowngradeClearsTheLayersAsync()
    {
        var falling = Fire();
        falling.OnFail = StarSmithOnFail.ItemDowngraded;
        var (player, plugIn, _) = await SmithAsync(falling, ingredients: 2, faults: 1).ConfigureAwait(false);
        var id = ((MUnique.OpenMU.Persistence.IIdentifiable)player.Inventory!.GetItem(TargetSlot)!).Id;
        var record = player.PersistenceContext.CreateNew<DataModel.Entities.ItemCustomization>();
        record.ItemId = id;
        record.Layers = "113:2:13:100,100,200,100,200,200";
        record.Rarity = 2;
        await player.PersistenceContext.SaveChangesAsync().ConfigureAwait(false);

        await plugIn.StartForgeAsync(player, TargetSlot, [IngredientSlot, 14], []).ConfigureAwait(false);
        var fault = player.StarSmithForge!.Forge.Faults[0];
        var start = (X: 0.02, Y: 0.07);
        foreach (var point in StarSmithGeometryTest.Draw(falling.Geometry!, 0, 0.08, false, false, start.X, start.Y).Take(3))
        {
            this._now += Interval;
            await plugIn.PlaceForgeNodeAsync(player, point).ConfigureAwait(false);
        }

        Assume.That(player.StarSmithForge, Is.Not.Null, "no early crack");
        this._now += Interval;
        await plugIn.PlaceForgeNodeAsync(player, fault.A).ConfigureAwait(false);
        var after = await CustomizationAsync(player).ConfigureAwait(false);
        Assert.Multiple(() =>
        {
            Assert.That(after!.Layers, Is.Null);
            Assert.That(after.Rarity, Is.EqualTo(0));
        });
    }

    /// <summary>
    /// A refused node is answered and changes nothing; outside an attempt everything is refused.
    /// </summary>
    [Test]
    public async ValueTask RefusalsChangeNothingAsync()
    {
        var (player, plugIn, _) = await SmithAsync(Fire()).ConfigureAwait(false);
        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<IStarSmithingViewPlugIn>()!);
        await plugIn.SealForgeAsync(player).ConfigureAwait(false);
        await plugIn.AbandonForgeAsync(player).ConfigureAwait(false);
        view.Verify(v => v.SendForgeResultAsync(StarSmithForgeOutcome.Refused, null, 0, false), Times.Exactly(2));
        await plugIn.PlaceForgeNodeAsync(player, new(100, 100)).ConfigureAwait(false);
        await plugIn.StashForgeAsync(player).ConfigureAwait(false);
        view.Verify(v => v.SendForgeNodeAsync(It.Is<StarSmithFigureAnswer>(a => a.Result == StarSmithFigureResult.NoAttempt)), Times.Exactly(2));

        await plugIn.StartForgeAsync(player, TargetSlot, [IngredientSlot], []).ConfigureAwait(false);
        var attempt = player.StarSmithForge!;
        this._now += Interval;
        await plugIn.PlaceForgeNodeAsync(player, new(30000, 30000)).ConfigureAwait(false);
        this._now += Interval;
        await plugIn.PlaceForgeNodeAsync(player, new(30100, 30000)).ConfigureAwait(false);
        view.Verify(v => v.SendForgeNodeAsync(It.Is<StarSmithFigureAnswer>(a => a.Result == StarSmithFigureResult.TooNear && a.StrikesLeft == 2)), Times.Once);
        Assert.That(attempt.Forge.Figure, Has.Count.EqualTo(1));
        await plugIn.StartForgeAsync(player, TargetSlot, [], []).ConfigureAwait(false);
        view.Verify(v => v.SendForgeAttemptAsync(It.Is<StarSmithForgeAttemptMessage>(m => m.Result == StarSmithResultCode.AttemptRunning)), Times.Once);
    }

    /// <summary>
    /// Abandon: the ingredients are lost, the item stays and nothing is written; an idle attempt times out alike.
    /// </summary>
    [Test]
    public async ValueTask AbandonAndTimeoutKeepTheItemAsync()
    {
        var (player, plugIn, _) = await SmithAsync(Fire()).ConfigureAwait(false);
        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<IStarSmithingViewPlugIn>()!);
        await plugIn.StartForgeAsync(player, TargetSlot, [IngredientSlot], []).ConfigureAwait(false);
        this._now += Interval;
        await plugIn.PlaceForgeNodeAsync(player, new(30000, 30000)).ConfigureAwait(false);
        await plugIn.AbandonForgeAsync(player).ConfigureAwait(false);
        view.Verify(v => v.SendForgeResultAsync(StarSmithForgeOutcome.Abandoned, It.Is<StarSmithForgeResult>(r => r.Fail == StarSmithOnFail.IngredientsLost), 0, false), Times.Once);
        Assert.That(player.Inventory!.GetItem(TargetSlot), Is.Not.Null);
        Assert.That(await CustomizationAsync(player).ConfigureAwait(false), Is.Null, "nothing written");

        await AddAsync(player, IngredientSlot, 14, 13).ConfigureAwait(false);
        this._now = 0;
        await plugIn.StartForgeAsync(player, TargetSlot, [IngredientSlot], []).ConfigureAwait(false);
        this._now = 59_999;
        await plugIn.TimeOutIdleForgeAsync(player).ConfigureAwait(false);
        Assert.That(player.StarSmithForge, Is.Not.Null);
        this._now = 60_000;
        await plugIn.TimeOutIdleForgeAsync(player).ConfigureAwait(false);
        Assert.That(player.StarSmithForge, Is.Null);
        view.Verify(v => v.SendForgeResultAsync(StarSmithForgeOutcome.TimedOut, It.IsAny<StarSmithForgeResult>(), 0, false), Times.Once);
    }

    /// <summary>
    /// The start checks its slots; catalysts alone are no ingredients (14).
    /// </summary>
    [Test]
    public async ValueTask TheStartChecksItsSlotsAsync()
    {
        var (player, plugIn, _) = await SmithAsync(Fire()).ConfigureAwait(false);
        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<IStarSmithingViewPlugIn>()!);

        await plugIn.StartForgeAsync(player, TargetSlot, [], []).ConfigureAwait(false);
        await plugIn.StartForgeAsync(player, TargetSlot, [], [IngredientSlot]).ConfigureAwait(false);
        await plugIn.StartForgeAsync(player, TargetSlot, [IngredientSlot, IngredientSlot], []).ConfigureAwait(false);
        await plugIn.StartForgeAsync(player, TargetSlot, [TargetSlot], []).ConfigureAwait(false);
        await plugIn.StartForgeAsync(player, 40, [IngredientSlot], []).ConfigureAwait(false);

        view.Verify(v => v.SendForgeAttemptAsync(It.Is<StarSmithForgeAttemptMessage>(m => m.Result == StarSmithResultCode.NoIngredients)), Times.Exactly(2));
        view.Verify(v => v.SendForgeAttemptAsync(It.Is<StarSmithForgeAttemptMessage>(m => m.Result == StarSmithResultCode.WrongSlot)), Times.Exactly(2));
        view.Verify(v => v.SendForgeAttemptAsync(It.Is<StarSmithForgeAttemptMessage>(m => m.Result == StarSmithResultCode.NoItem)), Times.Once);
        Assert.That(player.StarSmithForge, Is.Null);
        Assert.That(player.Inventory!.GetItem(IngredientSlot), Is.Not.Null, "nothing is spent by a refused start");
    }

    /// <summary>
    /// Ingredients no constellation takes still start; a seal is the attunement: no layer, at most Uncommon, 10
    /// experience and one small boost of the ingredients' family (the skill orbs: damage).
    /// </summary>
    [Test]
    public async ValueTask NoMatchingConstellationAttunesAsync()
    {
        var (player, plugIn, _) = await SmithAsync(Fire()).ConfigureAwait(false);
        await AddAsync(player, 14, 12, 7).ConfigureAwait(false);
        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<IStarSmithingViewPlugIn>()!);
        await plugIn.StartForgeAsync(player, TargetSlot, [14], []).ConfigureAwait(false);
        this._now += Interval;
        await plugIn.PlaceForgeNodeAsync(player, new(30000, 30000)).ConfigureAwait(false);
        await plugIn.SealForgeAsync(player).ConfigureAwait(false);

        view.Verify(v => v.SendForgeResultAsync(StarSmithForgeOutcome.Sealed, It.Is<StarSmithForgeResult>(r => r.Attunement && r.Rarity <= StarSmithRarity.Uncommon), 10, false), Times.Once);
        var record = await CustomizationAsync(player).ConfigureAwait(false);
        var boosts = StarSmithBoosts.Parse(record?.Boosts);
        Assert.Multiple(() =>
        {
            Assert.That(boosts.Select(b => b.Stat), Is.EqualTo(new[] { StarSmithBoostStat.Damage }));
            Assert.That(record!.Layers, Is.Null);
            Assert.That(player.SelectedCharacter!.StarSmithExperience, Is.EqualTo(10));
        });
        StarSmithBoostBook.Forget(record!.ItemId);
    }

    private static StarSmithForgeRules NoFaults() => new() { FaultCount = 0 };

    private static StarSmithRecipe Fire() => new()
    {
        Number = 101,
        Name = "Fire",
        Look = StarSmithLook.Fire,
        Experience = 40,
        Targets = StarSmithTargetKind.Weapon,
        Ingredients = [new StarSmithIngredient { Group = 14, Number = 13, Minimum = 0, Maximum = 10 }],
        Geometry = StarSmithGeometry.Of(3, true, [120, 120], [1, 1, 1]),
    };

    private static List<StarSmithPoint> FireAt(double x, double y)
    {
        var points = StarSmithGeometryTest.Draw(Fire().Geometry!, 0, 0.1, false, false, x, y);
        points[3] = new StarSmithPoint((ushort)(points[3].X + 300), points[3].Y);
        return points;
    }

    private static async ValueTask<DataModel.Entities.ItemCustomization?> CustomizationAsync(Player player)
    {
        var item = player.Inventory!.GetItem(TargetSlot);
        if (item is null)
        {
            return null;
        }

        var id = ((MUnique.OpenMU.Persistence.IIdentifiable)item).Id;
        return (await player.PersistenceContext.GetItemCustomizationsAsync([id]).ConfigureAwait(false)).FirstOrDefault();
    }

    private static async ValueTask<byte[]> BytesAsync(string method, params object[] arguments)
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(GameServer.BabylonClient.Version);
        var type = Type.GetType(PlugIn);
        Assert.That(type, Is.Not.Null);
        var send = type!.GetMethod(method);
        Assert.That(send, Is.Not.Null);
        await ((ValueTask)send!.Invoke(Activator.CreateInstance(type, player), arguments)!).ConfigureAwait(false);
        return output.ToArray();
    }

    private static async ValueTask<Item> AddAsync(Player player, byte slot, byte group, short number, byte durability = 1)
    {
        var item = player.PersistenceContext.CreateNew<Item>();
        item.Definition = new MUnique.OpenMU.Persistence.BasicModel.ItemDefinition { Group = group, Number = number, Width = 1, Height = 1, Durability = durability };
        item.Durability = durability;
        await player.Inventory!.AddItemAsync(slot, item).ConfigureAwait(false);
        return item;
    }

    private ValueTask<(Player Player, StarSmithingFeaturePlugIn PlugIn, Item Hammer)> SmithAsync(StarSmithRecipe recipe, int ingredients = 1, int faults = 0)
        => this.SmithAsync([recipe], ingredients, faults);

    private async ValueTask<(Player Player, StarSmithingFeaturePlugIn PlugIn, Item Hammer)> SmithAsync(List<StarSmithRecipe> recipes, int ingredients = 1, int faults = 0)
    {
        this._now = 0;
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        var hammer = await AddAsync(player, HammerSlot, 2, 1000, 255).ConfigureAwait(false);
        await AddAsync(player, TargetSlot, 0, 1).ConfigureAwait(false);
        await AddAsync(player, IngredientSlot, 14, 13).ConfigureAwait(false);
        for (var i = 1; i < ingredients; i++)
        {
            await AddAsync(player, (byte)(IngredientSlot + i), 14, 13).ConfigureAwait(false);
        }

        var plugIn = new StarSmithingFeaturePlugIn
        {
            Configuration = new StarSmithingConfiguration { Recipes = recipes, Forge = new StarSmithForgeRules { FaultCount = faults } },
            Clock = () => this._now,
            NewSeed = () => 4711,
        };
        return (player, plugIn, hammer);
    }
}
