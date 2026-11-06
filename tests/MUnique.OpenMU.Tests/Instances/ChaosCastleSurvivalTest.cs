// <copyright file="ChaosCastleSurvivalTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Instances;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.MiniGames;
using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.GameLogic.PlayerActions.MiniGames;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.Updates;

/// <summary>
/// Tests the tournament of the Chaos Castle Survival of the Fittest (S21Emu <c>Event/ChaosCastleSurvival.dat</c>, webzen
/// guide 161): the stage of the day, the points of a game (monster kill 1, player kill 2, winner 3), the qualification of
/// the best 20 of the try-outs for the semi-finals and of the best 10 of those for the final, and the champion.
/// </summary>
[TestFixture]
public class ChaosCastleSurvivalTest
{
    private static readonly DateTime TryOutDay = new(2026, 11, 8, 15, 0, 0);

    /// <summary>
    /// Try-outs on the 8th to 10th, semi-finals on the 11th and 12th, the final on the 13th at 1:25 only.
    /// </summary>
    [Test]
    public void StagesFollowTheDaysOfTheData()
    {
        var plugIn = new ChaosCastleSurvivalPlugIn { Configuration = new ChaosCastleSurvivalConfiguration() };

        Assert.That(plugIn.GetStage(new DateTime(2026, 11, 7, 15, 0, 0)), Is.EqualTo(ChaosCastleSurvivalStage.None));
        Assert.That(plugIn.GetStage(TryOutDay), Is.EqualTo(ChaosCastleSurvivalStage.TryOut));
        Assert.That(plugIn.GetStage(new DateTime(2026, 11, 12, 15, 0, 0)), Is.EqualTo(ChaosCastleSurvivalStage.SemiFinal));
        Assert.That(plugIn.GetStage(new DateTime(2026, 11, 13, 1, 25, 0)), Is.EqualTo(ChaosCastleSurvivalStage.Final));
        Assert.That(plugIn.MayStart(new DateTime(2026, 11, 13, 1, 25, 0)), Is.True);
        Assert.That(plugIn.MayStart(new DateTime(2026, 11, 13, 5, 0, 0)), Is.False, "the final runs once");
        Assert.That(plugIn.MayStart(TryOutDay), Is.True);
        Assert.That(plugIn.MayStart(new DateTime(2026, 11, 14, 15, 0, 0)), Is.False);
    }

    /// <summary>
    /// The points of the try-outs add up over the days; the best ones go to the semi-finals, the best of those to the
    /// final; a new month starts over.
    /// </summary>
    [Test]
    public void BestOfAStageQualifyForTheNext()
    {
        var plugIn = new ChaosCastleSurvivalPlugIn { Configuration = new ChaosCastleSurvivalConfiguration { StateFile = string.Empty, SemiFinalists = 1, Finalists = 1 } };
        var (a, b) = (Guid.NewGuid(), Guid.NewGuid());

        Assert.That(plugIn.Record(ChaosCastleSurvivalStage.TryOut, [new(a, 5, 1, true), new(b, 3, 0, false)], TryOutDay), Is.Null);
        plugIn.Record(ChaosCastleSurvivalStage.TryOut, [new(b, 4, 0, false)], TryOutDay.AddDays(1));

        Assert.That(plugIn.GetPoints(a, ChaosCastleSurvivalStage.TryOut, TryOutDay.AddDays(1)), Is.EqualTo(10));
        Assert.That(plugIn.GetPoints(b, ChaosCastleSurvivalStage.TryOut, TryOutDay.AddDays(1)), Is.EqualTo(7));
        var semiDay = TryOutDay.AddDays(3);
        Assert.That(plugIn.IsQualified(a, ChaosCastleSurvivalStage.SemiFinal, semiDay), Is.True);
        Assert.That(plugIn.IsQualified(b, ChaosCastleSurvivalStage.SemiFinal, semiDay), Is.False);
        Assert.That(plugIn.IsQualified(b, ChaosCastleSurvivalStage.TryOut, TryOutDay), Is.True, "every one may try out");

        plugIn.Record(ChaosCastleSurvivalStage.SemiFinal, [new(a, 1, 0, false)], semiDay);
        var finalDay = new DateTime(2026, 11, 13, 1, 25, 0);
        Assert.That(plugIn.IsQualified(a, ChaosCastleSurvivalStage.Final, finalDay), Is.True);
        Assert.That(plugIn.Record(ChaosCastleSurvivalStage.Final, [new(a, 0, 0, true)], finalDay), Is.EqualTo(a), "the winner of the final is the champion");

        Assert.That(plugIn.IsQualified(a, ChaosCastleSurvivalStage.SemiFinal, semiDay.AddMonths(1)), Is.False, "a new month starts over");
    }

    /// <summary>
    /// The update adds the castle of map 97 as an event of its own; on a semi-final day only the best of the try-outs get
    /// in, into a Chaos Castle game.
    /// </summary>
    [Test]
    public async ValueTask SemiFinalLetsOnlyTheQualifiedInAsync()
    {
        var world = await Season21EventWorld.CreateAsync(new AddSeason21ChaosCastleSurvivalUpdatePlugIn()).ConfigureAwait(false);
        var semiDay = new DateTime(2026, 11, 11, 15, 0, 0, DateTimeKind.Utc);
        var plugIn = new ChaosCastleSurvivalPlugIn { Configuration = new ChaosCastleSurvivalConfiguration { StateFile = string.Empty }, UtcNow = () => semiDay };
        world.AddFeaturePlugIn(plugIn);
        var definition = world.Configuration.MiniGameDefinitions.Single(d => d.Type == MiniGameType.ChaosCastleSurvival);
        Assert.That(definition.Entrance?.Map?.Number, Is.EqualTo(97));

        var outsider = await world.CreatePlayerAsync(CharacterClassNumber.BladeMaster, 400, 100).ConfigureAwait(false);
        var qualified = await world.CreatePlayerAsync(CharacterClassNumber.GrandMaster, 400, 100).ConfigureAwait(false);
        plugIn.Record(ChaosCastleSurvivalStage.TryOut, [new(qualified.SelectedCharacter!.Id, 3, 0, false)], semiDay.AddDays(-2));

        await new EnterMiniGameAction().TryEnterMiniGameAsync(outsider, MiniGameType.ChaosCastleSurvival, 1, 0xFF).ConfigureAwait(false);
        await new EnterMiniGameAction().TryEnterMiniGameAsync(qualified, MiniGameType.ChaosCastleSurvival, 1, 0xFF).ConfigureAwait(false);

        Assert.That(outsider.CurrentMiniGame, Is.Null);
        Assert.That(qualified.CurrentMiniGame, Is.InstanceOf<ChaosCastleContext>());
    }
}
