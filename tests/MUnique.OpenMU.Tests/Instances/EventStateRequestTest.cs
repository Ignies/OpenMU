// <copyright file="EventStateRequestTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Instances;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.MiniGames;
using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.GameLogic.PlayerActions.MiniGames;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using Moq;

/// <summary>
/// The state of a Season 21 event which the entry window of the Babylon client asks for (EE A9): open with the seconds
/// until the entrance closes, playing, or closed.
/// </summary>
[TestFixture]
public class EventStateRequestTest
{
    private Season21EventWorld _world = null!;

    /// <summary>
    /// Initializes the world with the boss battle.
    /// </summary>
    [OneTimeSetUp]
    public async ValueTask SetupAsync()
    {
        this._world = await Season21EventWorld.CreateAsync(new ActivateSeason21WideItemsUpdatePlugIn(), new AddSeason21BossBattleUpdatePlugIn()).ConfigureAwait(false);
        this._world.AddFeaturePlugIn(new Season21EventsFeaturePlugIn
        {
            Configuration = this._world.GetStoredConfiguration<Season21EventsConfiguration>(typeof(Season21EventsFeaturePlugIn).GUID),
        });
    }

    /// <summary>
    /// Without a running game and without a start plugin the event is closed, with no known time.
    /// </summary>
    [Test]
    public async ValueTask ClosedWithoutAGameAsync()
    {
        var player = await this._world.CreatePlayerAsync(CharacterClassNumber.BladeMaster, 400, 500).ConfigureAwait(false);

        await new EventStateRequestAction().ShowStateAsync(player, MiniGameType.BossBattle, 1).ConfigureAwait(false);

        Mock.Get(player.ViewPlugIns.GetPlugIn<ISeason21EventViewPlugIn>()!).Verify(v => v.ShowStateAsync(MiniGameType.BossBattle, 1, MiniGameState.Closed, 0), Times.Once);
    }

    /// <summary>
    /// While its entrance is open the event is open, with the seconds until the entrance closes; then it plays.
    /// </summary>
    [Test]
    public async ValueTask OpenWithTheEntranceTimeThenPlayingAsync()
    {
        var entrant = await this._world.CreatePlayerAsync(CharacterClassNumber.BladeMaster, 400, 500).ConfigureAwait(false);
        entrant.Money = 20_000_000;
        await new EnterMiniGameAction().TryEnterMiniGameAsync(entrant, MiniGameType.BossBattle, 1, 0xFF).ConfigureAwait(false);
        await Season21EventWorld.CompleteMapChangeAsync(entrant).ConfigureAwait(false);
        var game = entrant.CurrentMiniGame!;
        var asker = await this._world.CreatePlayerAsync(CharacterClassNumber.GrandMaster, 400, 500).ConfigureAwait(false);
        var view = Mock.Get(asker.ViewPlugIns.GetPlugIn<ISeason21EventViewPlugIn>()!);

        await new EventStateRequestAction().ShowStateAsync(asker, MiniGameType.BossBattle, 1).ConfigureAwait(false);
        view.Verify(v => v.ShowStateAsync(MiniGameType.BossBattle, 1, MiniGameState.Open, It.IsInRange(1u, 300u, Moq.Range.Inclusive)), Times.Once);

        await Season21EventWorld.SkipUntilPlayingAsync(game).ConfigureAwait(false);
        await new EventStateRequestAction().ShowStateAsync(asker, MiniGameType.BossBattle, 1).ConfigureAwait(false);
        view.Verify(v => v.ShowStateAsync(MiniGameType.BossBattle, 1, MiniGameState.Playing, 0), Times.Once);
        await game.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// A level the event doesn't have gets no answer.
    /// </summary>
    [Test]
    public async ValueTask UnknownLevelGetsNoAnswerAsync()
    {
        var player = await this._world.CreatePlayerAsync(CharacterClassNumber.BladeMaster, 400, 500).ConfigureAwait(false);

        await new EventStateRequestAction().ShowStateAsync(player, MiniGameType.BossBattle, 9).ConfigureAwait(false);

        Mock.Get(player.ViewPlugIns.GetPlugIn<ISeason21EventViewPlugIn>()!).Verify(v => v.ShowStateAsync(It.IsAny<MiniGameType>(), It.IsAny<byte>(), It.IsAny<MiniGameState>(), It.IsAny<uint>()), Times.Never);
    }
}
