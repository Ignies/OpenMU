// <copyright file="PartyInstanceTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Instances;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.EvolutionQuests;
using MUnique.OpenMU.GameLogic.MiniGames;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlayerActions.Quests;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using Moq;

/// <summary>
/// Runs the trial of the fourth class quest in the party instances of Season 21: every party gets its own copy of the
/// Trial Area with its own monsters, which is removed when the party left.
/// </summary>
[TestFixture]
public class PartyInstanceTest
{
    private const short Group = EvolutionQuestConfiguration.Season21QuestGroup;

    private Season21EventWorld _world = null!;

    private EvolutionQuestFeaturePlugIn _plugIn = null!;

    /// <summary>
    /// Initializes the Season 6 data with the Season 21 quests and the instances.
    /// </summary>
    [OneTimeSetUp]
    public async ValueTask SetupAsync()
    {
        this._world = await Season21EventWorld.CreateAsync(
            new AddSeason21EvolutionQuestsUpdatePlugIn(),
            new AddSeason21InstancesUpdatePlugIn()).ConfigureAwait(false);
        this._plugIn = new EvolutionQuestFeaturePlugIn();
        this._world.AddFeaturePlugIn(this._plugIn);
        this._world.Register<IPlayerTalkToNpcPlugIn>(this._plugIn);
        this._world.Register<IAttackableGotKilledPlugIn>(this._plugIn);
        this._world.Register<IObjectAddedToMapPlugIn>(this._plugIn);
        this._world.Register<IObjectRemovedFromMapPlugIn>(this._plugIn);
    }

    /// <summary>
    /// The update adds the trial as a party instance of the Trial Area for up to five players.
    /// </summary>
    [Test]
    public void TrialIsAPartyInstance()
    {
        var definition = this._world.Configuration.MiniGameDefinitions.SingleOrDefault(d => d.Type == MiniGameType.PartyInstance && d.Entrance?.Map?.Number == EvolutionQuestConfiguration.Season21TrialMap);
        Assert.That(definition, Is.Not.Null);
        Assert.That(definition!.MapCreationPolicy, Is.EqualTo(MiniGameMapCreationPolicy.OnePerParty));
        Assert.That(definition.MaximumPlayerCount, Is.EqualTo(5));
        Assert.That(definition.AllowParty, Is.True);
    }

    /// <summary>
    /// Two members of a party end up in the same copy of the Trial Area, which isn't the shared map.
    /// </summary>
    [Test]
    public async ValueTask PartyMembersShareTheirInstanceAsync()
    {
        var (owner, helper) = await this.CreatePartyAsync().ConfigureAwait(false);
        await this._world.TalkAsync(owner, 764).ConfigureAwait(false);
        await this._world.TalkAsync(helper, 764).ConfigureAwait(false);

        Assert.That(owner.CurrentMiniGame?.Definition.Type, Is.EqualTo(MiniGameType.PartyInstance));
        Assert.That(helper.CurrentMiniGame, Is.SameAs(owner.CurrentMiniGame));
        Assert.That(owner.CurrentMap, Is.SameAs(helper.CurrentMap));
        Assert.That(owner.CurrentMap?.Definition.Number, Is.EqualTo(EvolutionQuestConfiguration.Season21TrialMap));
        var sharedMap = await this._world.GameContext.GetMapAsync(EvolutionQuestConfiguration.Season21TrialMap).ConfigureAwait(false);
        Assert.That(owner.CurrentMap, Is.Not.SameAs(sharedMap));

        await LeaveAsync(owner, helper).ConfigureAwait(false);
    }

    /// <summary>
    /// Another party gets another copy, and the monsters of a trial step spawn in the copy of its party only.
    /// </summary>
    [Test]
    public async ValueTask EveryPartyHasItsOwnMonstersAsync()
    {
        var (owner, helper) = await this.CreatePartyAsync().ConfigureAwait(false);
        var (other, otherHelper) = await this.CreatePartyAsync().ConfigureAwait(false);
        await this._world.TalkAsync(owner, 764).ConfigureAwait(false);
        await this._world.TalkAsync(other, 764).ConfigureAwait(false);
        Assert.That(other.CurrentMap, Is.Not.SameAs(owner.CurrentMap));

        await this._world.TalkAsync(owner, 766).ConfigureAwait(false);
        await new QuestStartAction().StartQuestAsync(owner, Group, 7).ConfigureAwait(false);
        var trial = this._plugIn.GetTrial(owner)!;
        Assert.That(trial.Monsters.Select(m => m.CurrentMap), Has.All.SameAs(owner.CurrentMap));
        Assert.That(other.CurrentMap!.GetAttackablesInRange(other.Position, 255).OfType<Monster>(), Is.Empty);

        await LeaveAsync(owner, helper, other, otherHelper).ConfigureAwait(false);
    }

    /// <summary>
    /// The copy is removed when the last player of the party left it, and the party gets a new one next time.
    /// </summary>
    [Test]
    public async ValueTask InstanceIsRemovedWhenThePartyLeftAsync()
    {
        var (owner, helper) = await this.CreatePartyAsync().ConfigureAwait(false);
        await this._world.TalkAsync(owner, 764).ConfigureAwait(false);
        await this._world.TalkAsync(helper, 764).ConfigureAwait(false);
        var instance = owner.CurrentMiniGame!;

        await LeaveAsync(owner).ConfigureAwait(false);
        Assert.That(instance.IsDisposed, Is.False, "one member is still inside");
        await LeaveAsync(helper).ConfigureAwait(false);
        await Season21EventWorld.WaitUntilAsync(() => instance.IsDisposed).ConfigureAwait(false);

        await this._world.TalkAsync(owner, 764).ConfigureAwait(false);
        Assert.That(owner.CurrentMiniGame, Is.Not.Null.And.Not.SameAs(instance));
        await LeaveAsync(owner).ConfigureAwait(false);
    }

    /// <summary>
    /// The view of the events shows the player the running instance with its time.
    /// </summary>
    [Test]
    public async ValueTask ViewShowsTheRunningInstanceAsync()
    {
        var player = await this._world.CreatePlayerAsync(CharacterClassNumber.BladeMaster, 400, 400).ConfigureAwait(false);
        await this._world.TalkAsync(player, 764).ConfigureAwait(false);
        var instance = player.CurrentMiniGame!;
        await Season21EventWorld.SkipUntilPlayingAsync(instance).ConfigureAwait(false);

        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<ISeason21EventViewPlugIn>()!);
        await Season21EventWorld.WaitUntilAsync(() => view.Invocations.Any(i => i.Method.Name == nameof(ISeason21EventViewPlugIn.ShowStateAsync))).ConfigureAwait(false);
        view.Verify(v => v.ShowStateAsync(MiniGameType.PartyInstance, 2, MiniGameState.Playing, It.Is<uint>(s => s > 1700)), Times.Once);
        await LeaveAsync(player).ConfigureAwait(false);
    }

    private static async ValueTask LeaveAsync(params Player[] players)
    {
        foreach (var player in players.Where(p => p.CurrentMap is not null))
        {
            await player.WarpToSafezoneAsync().ConfigureAwait(false);
            await Season21EventWorld.CompleteMapChangeAsync(player).ConfigureAwait(false);
        }
    }

    private async ValueTask<(Player Owner, Player Helper)> CreatePartyAsync()
    {
        var owner = await this._world.CreatePlayerAsync(CharacterClassNumber.BladeMaster, 400, 400).ConfigureAwait(false);
        var helper = await this._world.CreatePlayerAsync(CharacterClassNumber.GrandMaster, 400, 400).ConfigureAwait(false);
        await Season21EventWorld.CreatePartyAsync(owner, helper).ConfigureAwait(false);
        return (owner, helper);
    }
}
