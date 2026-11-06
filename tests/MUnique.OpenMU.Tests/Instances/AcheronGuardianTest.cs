// <copyright file="AcheronGuardianTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Instances;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.MiniGames;
using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlayerActions.MiniGames;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.Updates;

/// <summary>
/// Runs the Protector of Acheron of Season 21 (S21Emu <c>Event/AcheronGuardian.dat</c>): waves of monsters in the event
/// copies of Acheron and Deventer, the reward bag for enough points.
/// </summary>
[TestFixture]
public class AcheronGuardianTest
{
    private Season21EventWorld _world = null!;

    private Season21EventsFeaturePlugIn _plugIn = null!;

    /// <summary>
    /// Initializes the world with the event and the wide items of its bag.
    /// </summary>
    [OneTimeSetUp]
    public async ValueTask SetupAsync()
    {
        this._world = await Season21EventWorld.CreateAsync(new ActivateSeason21WideItemsUpdatePlugIn(), new AddSeason21AcheronGuardianUpdatePlugIn()).ConfigureAwait(false);
        this._plugIn = new Season21EventsFeaturePlugIn
        {
            Configuration = this._world.GetStoredConfiguration<Season21EventsConfiguration>(typeof(Season21EventsFeaturePlugIn).GUID),
        };
        this._world.AddFeaturePlugIn(this._plugIn);
    }

    /// <summary>
    /// Both maps have their game level with 30 waves of ten monsters an area and the reward bag.
    /// </summary>
    [Test]
    public void LevelsHaveTheirAreasAndWaves()
    {
        var definitions = this._world.Configuration.MiniGameDefinitions.Where(d => d.Type == MiniGameType.AcheronGuardian).OrderBy(d => d.GameLevel).ToList();
        Assert.That(definitions.Select(d => d.Entrance?.Map?.Number), Is.EqualTo(new short?[] { 92, 96 }));
        Assert.That(definitions.Select(d => d.MinimumCharacterLevel), Has.All.EqualTo(450));
        Assert.That(definitions.SelectMany(d => d.Rewards).Select(r => r.ItemReward?.PossibleItems.Count), Has.All.GreaterThan(0));
        var acheron = this._plugIn.Configuration!.AcheronGuardian[0];
        Assert.That(acheron.Rounds, Has.Count.EqualTo(30));
        Assert.That(acheron.Rounds[0].Count + acheron.Rounds[0].BonusMonsters.Sum(b => b.Count), Is.EqualTo(20), "ten monsters in each of the two areas");
        Assert.That(this._plugIn.Configuration.AcheronGuardian[1].Rounds[0].BonusMonsters.Sum(b => b.Count) + 4, Is.EqualTo(10));
    }

    /// <summary>
    /// A player kills the monsters of two waves and gets an item of the bag at the end.
    /// </summary>
    [Test]
    public async ValueTask KillsGiveTheRewardAsync()
    {
        var original = this._plugIn.Configuration!.AcheronGuardian[0];
        this._plugIn.Configuration.AcheronGuardian[0] = new RoundsEventDefinition
        {
            SpawnPoints = original.SpawnPoints,
            RoundDuration = TimeSpan.FromSeconds(5),
            PauseAfterRound = TimeSpan.Zero,
            PartyIsATeam = false,
            MinimumPointsForRewards = 20,
            Rounds =
            [
                new EventRound { MonsterNumber = 586, Count = 1, Points = 10 },
                new EventRound { MonsterNumber = 589, Count = 1, Points = 10, SpawnKind = 1 },
            ],
        };
        try
        {
            var player = await this._world.CreatePlayerAsync(CharacterClassNumber.BladeMaster, 400, 100).ConfigureAwait(false);
            await new EnterMiniGameAction().TryEnterMiniGameAsync(player, MiniGameType.AcheronGuardian, 1, 0xFF).ConfigureAwait(false);
            await Season21EventWorld.CompleteMapChangeAsync(player).ConfigureAwait(false);
            var game = (Season21RoundsContext)player.CurrentMiniGame!;
            Assert.That(game.Map.Definition.Number, Is.EqualTo(92));
            await Season21EventWorld.SkipUntilPlayingAsync(game).ConfigureAwait(false);
            var itemsBefore = player.Inventory!.Items.Count();

            for (var wave = 1; wave <= 2; wave++)
            {
                var number = wave;
                await Season21EventWorld.WaitUntilAsync(() => game.Round == number && Monsters(game).Count == 1).ConfigureAwait(false);
                await Season21EventWorld.KillAsync(Monsters(game)[0], player).ConfigureAwait(false);
            }

            await Season21EventWorld.WaitUntilAsync(() => game.State is MiniGameState.Ended or MiniGameState.Disposed).ConfigureAwait(false);
            Assert.That(game.GetScore(player), Is.EqualTo(20));
            await Season21EventWorld.WaitUntilAsync(() => player.Inventory.Items.Count() > itemsBefore).ConfigureAwait(false);
        }
        finally
        {
            this._plugIn.Configuration.AcheronGuardian[0] = original;
        }
    }

    private static List<Monster> Monsters(MiniGameContext game)
        => game.Map.GetAttackablesInRange(new MUnique.OpenMU.Pathfinding.Point(128, 128), 255).OfType<Monster>().Where(m => m.IsAlive && m.SpawnArea.SpawnTrigger == SpawnTrigger.OnceAtWaveStart).ToList();
}
