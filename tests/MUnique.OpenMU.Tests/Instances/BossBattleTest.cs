// <copyright file="BossBattleTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Instances;

using System.Reflection;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.MiniGames;
using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.GameLogic.PlayerActions.MiniGames;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using Moq;

/// <summary>
/// Runs the Boss Battle Together of Season 21 (S21Emu <c>Event/BossBattleEvent.xml</c>) on map 139: one boss, the damage
/// of every player, rewards for damage, the top damage and the last hit.
/// </summary>
[TestFixture]
public class BossBattleTest
{
    private Season21EventWorld _world = null!;

    private Season21EventsFeaturePlugIn _plugIn = null!;

    /// <summary>
    /// Initializes the world with the event and the wide items, which the boxes are.
    /// </summary>
    [OneTimeSetUp]
    public async ValueTask SetupAsync()
    {
        this._world = await Season21EventWorld.CreateAsync(new ActivateSeason21WideItemsUpdatePlugIn(), new AddSeason21BossBattleUpdatePlugIn()).ConfigureAwait(false);
        this._plugIn = new Season21EventsFeaturePlugIn
        {
            Configuration = this._world.GetStoredConfiguration<Season21EventsConfiguration>(typeof(Season21EventsFeaturePlugIn).GUID),
        };
        this._world.AddFeaturePlugIn(this._plugIn);
        this._world.Register<GameLogic.PlugIns.IAttackableGotHitPlugIn>(new BossBattleDamagePlugIn());
    }

    /// <summary>
    /// The event is on map 139 from total level 800 for 10,000,000 zen, with the bosses and rewards of the data.
    /// </summary>
    [Test]
    public void EventHasTheBossesOfTheData()
    {
        var definition = this._world.Configuration.MiniGameDefinitions.Single(d => d.Type == MiniGameType.BossBattle);
        Assert.That((definition.Entrance?.Map?.Number, definition.MinimumCharacterLevel, definition.EntranceFee, definition.MaximumPlayerCount), Is.EqualTo(((short?)139, 800, 10_000_000, 100)));
        var battle = this._plugIn.Configuration!.BossBattle!;
        Assert.That(battle.Bosses, Is.EquivalentTo(new short[] { 919, 921, 922, 923, 924 }));
        Assert.That(battle.Bosses, Has.All.Matches<short>(n => this._world.Configuration.Monsters.Any(m => m.Number == n)));
        Assert.That((battle.Health, battle.DamageForRewards), Is.EqualTo((80_000_000L, 1_000_000L)));
    }

    /// <summary>
    /// Two players hit the boss; the one with enough damage gets the damage rewards and the top damage reward, the killer
    /// the last hit reward.
    /// </summary>
    [Test]
    public async ValueTask DamageTopDamageAndLastHitGiveTheirRewardsAsync()
    {
        var heavy = await this._world.CreatePlayerAsync(CharacterClassNumber.BladeMaster, 400, 500).ConfigureAwait(false);
        var light = await this._world.CreatePlayerAsync(CharacterClassNumber.GrandMaster, 400, 500).ConfigureAwait(false);
        foreach (var player in new[] { heavy, light })
        {
            player.Money = 20_000_000;
            await new EnterMiniGameAction().TryEnterMiniGameAsync(player, MiniGameType.BossBattle, 1, 0xFF).ConfigureAwait(false);
            await Season21EventWorld.CompleteMapChangeAsync(player).ConfigureAwait(false);
        }

        var game = (BossBattleContext)heavy.CurrentMiniGame!;
        Assert.That(light.CurrentMiniGame, Is.SameAs(game));
        Assert.That(heavy.Money, Is.EqualTo(10_000_000), "the entrance fee");
        await Season21EventWorld.SkipUntilPlayingAsync(game).ConfigureAwait(false);
        await Season21EventWorld.WaitUntilAsync(() => game.Boss is not null).ConfigureAwait(false);
        var boss = game.Boss!;
        Assert.That(boss.Health, Is.EqualTo(80_000_000), "no bonus below three players");

        await boss.ApplyPoisonDamageAsync(light, 1000).ConfigureAwait(false);
        await boss.ApplyPoisonDamageAsync(heavy, (uint)boss.Health - 5).ConfigureAwait(false);
        Assert.That((game.GetDamage(heavy), game.GetDamage(light)), Is.EqualTo((80_000_000L - 1005, 1000L)));

        await Season21EventWorld.KillAsync(boss, light).ConfigureAwait(false);
        await Season21EventWorld.WaitUntilAsync(() => game.State is MiniGameState.Ended or MiniGameState.Disposed).ConfigureAwait(false);
        await Season21EventWorld.WaitUntilAsync(() => Boxes(light) > 0).ConfigureAwait(false);

        Assert.That(Items(heavy, 78), Is.EqualTo(1), "the damage reward");
        Assert.That(Items(heavy, 110), Is.EqualTo(1 + 2), "the damage and top damage rewards");
        Assert.That(Items(light, 110), Is.EqualTo(2), "the last hit reward");
        Mock.Get(light.ViewPlugIns.GetPlugIn<ISeason21EventViewPlugIn>()!).Verify(v => v.ShowResultAsync(MiniGameType.BossBattle, true, It.IsAny<int>(), It.IsAny<int>(), 0), Times.Once);
    }

    /// <summary>
    /// A battle whose time runs out with the boss alive is no success, although the end of the game clears the boss
    /// from the map before the results.
    /// </summary>
    [Test]
    public async ValueTask TimeOutWithTheBossAliveIsNoSuccessAsync()
    {
        // A world of its own: the battle of the other test may still hold the map.
        var world = await Season21EventWorld.CreateAsync(new ActivateSeason21WideItemsUpdatePlugIn(), new AddSeason21BossBattleUpdatePlugIn()).ConfigureAwait(false);
        world.AddFeaturePlugIn(new Season21EventsFeaturePlugIn { Configuration = world.GetStoredConfiguration<Season21EventsConfiguration>(typeof(Season21EventsFeaturePlugIn).GUID) });
        world.Register<GameLogic.PlugIns.IAttackableGotHitPlugIn>(new BossBattleDamagePlugIn());
        var player = await world.CreatePlayerAsync(CharacterClassNumber.BladeMaster, 400, 500).ConfigureAwait(false);
        player.Money = 20_000_000;
        await new EnterMiniGameAction().TryEnterMiniGameAsync(player, MiniGameType.BossBattle, 1, 0xFF).ConfigureAwait(false);
        await Season21EventWorld.CompleteMapChangeAsync(player).ConfigureAwait(false);
        var game = (BossBattleContext)player.CurrentMiniGame!;
        await Season21EventWorld.SkipUntilPlayingAsync(game).ConfigureAwait(false);
        await Season21EventWorld.WaitUntilAsync(() => game.Boss is not null).ConfigureAwait(false);

        typeof(MiniGameContext).GetMethod("FinishEvent", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(game, null);
        await Season21EventWorld.WaitUntilAsync(() => game.State is MiniGameState.Ended or MiniGameState.Disposed).ConfigureAwait(false);

        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<ISeason21EventViewPlugIn>()!);
        await Season21EventWorld.WaitUntilAsync(() => view.Invocations.Any(i => i.Method.Name == nameof(ISeason21EventViewPlugIn.ShowResultAsync))).ConfigureAwait(false);
        view.Verify(v => v.ShowResultAsync(MiniGameType.BossBattle, false, It.IsAny<int>(), It.IsAny<int>(), 0), Times.Once);
    }

    private static int Boxes(Player player) => Items(player, 110);

    private static int Items(Player player, short number)
        => player.Inventory!.Items.Count(i => i.Definition is { Group: 20 } definition && definition.Number == number);
}
