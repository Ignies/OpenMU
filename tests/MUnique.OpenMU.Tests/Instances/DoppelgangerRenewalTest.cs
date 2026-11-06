// <copyright file="DoppelgangerRenewalTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Instances;

using System.Reflection;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.MiniGames;
using MUnique.OpenMU.GameLogic.MiniGames.Doppelganger;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlayerActions.MiniGames;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.Updates;

/// <summary>
/// Runs the Doppelganger of Season 21 (S21Emu <c>Event/DoubleGoerRenewal</c>): seven rooms by total level with their
/// monster multipliers, experience and chests, at least two players.
/// </summary>
[TestFixture]
public class DoppelgangerRenewalTest
{
    private Season21EventWorld _world = null!;

    private DoppelgangerFeaturePlugIn _plugIn = null!;

    /// <summary>
    /// Initializes the world with the renewal.
    /// </summary>
    [OneTimeSetUp]
    public async ValueTask SetupAsync()
    {
        this._world = await Season21EventWorld.CreateAsync(new AddSeason21DoppelgangerUpdatePlugIn()).ConfigureAwait(false);
        this._plugIn = new DoppelgangerFeaturePlugIn
        {
            Configuration = this._world.GetStoredConfiguration<DoppelgangerEventDefinition>(typeof(DoppelgangerFeaturePlugIn).GUID),
        };
        this._world.AddFeaturePlugIn(this._plugIn);
    }

    /// <summary>
    /// The rooms of Season 21 scale the monsters by total level: the room from 1500 on with its multipliers, its
    /// experience and its golden chest.
    /// </summary>
    [Test]
    public void RoomsScaleByTotalLevel()
    {
        var configuration = this._plugIn.Configuration!;
        Assert.That(configuration.MonsterScalings, Has.Count.EqualTo(7));
        var room = configuration.GetMonsterScaling(1500)!;
        Assert.That((room.LevelMultipliers.Single(), room.HealthMultipliers.Single(), room.DamageMultipliers.Single(), room.DefenseMultipliers.Single()), Is.EqualTo((6.79f, 59f, 13.82f, 21.2f)));
        Assert.That(room.RewardExperience, Is.EqualTo(240_000));
        Assert.That(room.FinalChestItems?.PossibleItems.Select(i => (i.Group, i.Number)), Does.Contain(((byte)14, (short)13)));
        Assert.That(room.InterimChestItems?.PossibleItems, Is.Not.Empty);
        Assert.That(configuration.GetMonsterScaling(60)!.RewardExperience, Is.EqualTo(60_000));
    }

    /// <summary>
    /// Every total level may enter, and a game needs two players.
    /// </summary>
    [Test]
    public void EventIsOpenToEveryLevelAndNeedsTwoPlayers()
    {
        var definitions = this._world.Configuration.MiniGameDefinitions.Where(d => d.Type == MiniGameType.Doppelganger).ToList();
        Assert.That(definitions, Is.Not.Empty);
        Assert.That(definitions.Select(d => d.MaximumCharacterLevel), Has.All.EqualTo(int.MaxValue));
        Assert.That(definitions.Select(d => d.MinimumCharacterLevel), Has.All.EqualTo(50));
        Assert.That(definitions.Select(d => d.MinimumPlayerCount), Has.All.EqualTo(2));
    }

    /// <summary>
    /// A party of two at total level 1500 enters, defends the circle, gets the experience of its room and opens the
    /// golden chest of the room.
    /// </summary>
    [Test]
    public async ValueTask PartyWinsTheRoomRewardsAsync()
    {
        var first = await this._world.CreatePlayerAsync(CharacterClassNumber.BladeMaster, 400, 1100).ConfigureAwait(false);
        var second = await this._world.CreatePlayerAsync(CharacterClassNumber.GrandMaster, 400, 1100).ConfigureAwait(false);
        await Season21EventWorld.CreatePartyAsync(first, second).ConfigureAwait(false);
        var level = this._world.Configuration.MiniGameDefinitions.First(d => d.Type == MiniGameType.Doppelganger).GameLevel;
        foreach (var player in new[] { first, second })
        {
            await new EnterMiniGameAction().TryEnterMiniGameAsync(player, MiniGameType.Doppelganger, level, 0xFF).ConfigureAwait(false);
            await Season21EventWorld.CompleteMapChangeAsync(player).ConfigureAwait(false);
        }

        var game = (DoppelgangerContext)first.CurrentMiniGame!;
        Assert.That(second.CurrentMiniGame, Is.SameAs(game));
        await Season21EventWorld.SkipUntilPlayingAsync(game).ConfigureAwait(false);
        var experienceBefore = first.SelectedCharacter!.MasterExperience;

        typeof(MiniGameContext).GetMethod("FinishEvent", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(game, null);
        await Season21EventWorld.WaitUntilAsync(() => first.SelectedCharacter.MasterExperience > experienceBefore).ConfigureAwait(false);

        var chestNumber = this._plugIn.Configuration!.FinalRewardChest!.Number;
        Destructible? chest = null;
        await Season21EventWorld.WaitUntilAsync(() => (chest = game.Map.GetAttackablesInRange(first.Position, 10).OfType<Destructible>().FirstOrDefault(d => d.Definition.Number == chestNumber)) is not null).ConfigureAwait(false);
        await game.OpenRewardChestAsync(first, chest!).ConfigureAwait(false);

        var room = this._plugIn.Configuration.GetMonsterScaling(1500)!;
        var drops = game.Map.GetDropsInRange(chest!.Position, 5).OfType<DroppedItem>().ToList();
        Assert.That(drops, Is.Not.Empty);
        Assert.That(drops.Select(d => d.Item.Definition), Has.All.Matches<ItemDefinition>(d => room.FinalChestItems!.PossibleItems.Contains(d)));
    }
}
