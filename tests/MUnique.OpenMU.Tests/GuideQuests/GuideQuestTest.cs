// <copyright file="GuideQuestTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.GuideQuests;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Quests;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.GuideQuests;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.Views.Quest;
using MUnique.OpenMU.Pathfinding;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;
using MonsterDefinition = MUnique.OpenMU.Persistence.BasicModel.MonsterDefinition;

/// <summary>
/// Tests the guide quests of Season 21 (<see cref="GuideQuestPlugIn"/>): the chain of a class, accepting, the
/// objectives, the completion with its rewards and the end of the chain.
/// </summary>
[TestFixture]
public class GuideQuestTest
{
    private const short Spider = 3;

    /// <summary>
    /// A Dark Wizard gets the steps of its class in their order; the first is offered, and accepting it makes it active.
    /// </summary>
    [Test]
    public async ValueTask TheFirstStepOfTheClassIsOfferedAndAcceptedAsync()
    {
        var (plugIn, player) = await CreateAsync().ConfigureAwait(false);

        var view = plugIn.GetView(player);
        Assert.That((view.Status, view.Step?.Number, view.Index, view.Count), Is.EqualTo((GuideQuestStatus.Offered, (short?)11, 1, 4)), "Noria's step 12 is the elf's.");

        await plugIn.AcceptAsync(player, 11).ConfigureAwait(false);

        Assert.That(plugIn.GetView(player).Status, Is.EqualTo(GuideQuestStatus.Active));
        VerifyShown(player, GuideQuestResult.Accepted);
    }

    /// <summary>
    /// A move step is ready once the character stands near its spot on its map.
    /// </summary>
    [Test]
    public async ValueTask TheMoveStepIsReadyNearItsSpotAsync()
    {
        var (plugIn, player) = await CreateAsync().ConfigureAwait(false);
        await plugIn.AcceptAsync(player, 11).ConfigureAwait(false);

        player.Position = new Point(150, 120);
        plugIn.AttackableMoved(player);
        Assert.That(plugIn.GetView(player).Status, Is.EqualTo(GuideQuestStatus.Active), "Too far away.");

        player.Position = new Point(165, 131);
        plugIn.AttackableMoved(player);
        Assert.That(plugIn.GetView(player).Status, Is.EqualTo(GuideQuestStatus.Ready));
    }

    /// <summary>
    /// The completion is refused while the objective isn't met; the step stays active.
    /// </summary>
    [Test]
    public async ValueTask CompletionIsRefusedUntilTheObjectiveIsMetAsync()
    {
        var (plugIn, player) = await CreateAsync().ConfigureAwait(false);
        await plugIn.AcceptAsync(player, 11).ConfigureAwait(false);

        await plugIn.CompleteAsync(player, 11).ConfigureAwait(false);

        Assert.That(plugIn.GetView(player).Status, Is.EqualTo(GuideQuestStatus.Active));
        VerifyShown(player, GuideQuestResult.NotReady);
    }

    /// <summary>
    /// Kills of the step's monster count up to its number; the completion gives the experience and the money, and the
    /// next step of the class is offered.
    /// </summary>
    [Test]
    public async ValueTask TheHuntStepCountsKillsAndRewardsAsync()
    {
        var (plugIn, player) = await CreateAsync().ConfigureAwait(false);
        await FinishMoveStepAsync(plugIn, player).ConfigureAwait(false);
        await plugIn.AcceptAsync(player, 14).ConfigureAwait(false);
        var money = player.Money;
        var experience = player.SelectedCharacter!.Experience;

        await plugIn.AttackableGotKilledAsync(CreateMonster(player, 26), player).ConfigureAwait(false);
        for (var i = 0; i < 4; i++)
        {
            await plugIn.AttackableGotKilledAsync(CreateMonster(player, Spider), player).ConfigureAwait(false);
        }

        var view = plugIn.GetView(player);
        Assert.That((view.Status, view.Progress), Is.EqualTo((GuideQuestStatus.Ready, 3)), "Other monsters don't count, the count stops at the goal.");

        await plugIn.CompleteAsync(player, 14).ConfigureAwait(false);

        Assert.That(player.Money - money, Is.EqualTo(2000));
        Assert.That(player.SelectedCharacter.Experience - experience, Is.EqualTo(300));
        Assert.That((plugIn.GetView(player).Status, plugIn.GetView(player).Step?.Number), Is.EqualTo((GuideQuestStatus.Offered, (short?)49)));
        VerifyShown(player, GuideQuestResult.Completed);
    }

    /// <summary>
    /// A level step counts the character's level; a step above the character's level can't be accepted yet.
    /// </summary>
    [Test]
    public async ValueTask LevelStepAndTheMinimumLevelAsync()
    {
        var (plugIn, player) = await CreateAsync().ConfigureAwait(false);
        await FinishMoveStepAsync(plugIn, player).ConfigureAwait(false);
        await FinishHuntStepAsync(plugIn, player).ConfigureAwait(false);
        await plugIn.AcceptAsync(player, 49).ConfigureAwait(false);
        player.Attributes![Stats.Level] = 12;
        Assert.That((plugIn.GetView(player).Status, plugIn.GetView(player).Progress), Is.EqualTo((GuideQuestStatus.Active, 12)));

        player.Attributes[Stats.Level] = 20;
        Assert.That(plugIn.GetView(player).Status, Is.EqualTo(GuideQuestStatus.Ready));
        await plugIn.CompleteAsync(player, 49).ConfigureAwait(false);

        Assert.That(plugIn.GetView(player).Status, Is.EqualTo(GuideQuestStatus.LevelTooLow), "Step 119 starts at 150.");
        await plugIn.AcceptAsync(player, 119).ConfigureAwait(false);
        Assert.That(plugIn.GetView(player).Status, Is.EqualTo(GuideQuestStatus.LevelTooLow));
        VerifyShown(player, GuideQuestResult.LevelTooLow);
    }

    /// <summary>
    /// A class quest step is met by the finished Season 6 class quest; after the last step the chain is finished.
    /// </summary>
    [Test]
    public async ValueTask ClassQuestStepAndTheEndOfTheChainAsync()
    {
        var (plugIn, player) = await CreateAsync().ConfigureAwait(false);
        await FinishMoveStepAsync(plugIn, player).ConfigureAwait(false);
        await FinishHuntStepAsync(plugIn, player).ConfigureAwait(false);
        player.Attributes![Stats.Level] = 150;
        await plugIn.AcceptAsync(player, 49).ConfigureAwait(false);
        await plugIn.CompleteAsync(player, 49).ConfigureAwait(false);
        await plugIn.AcceptAsync(player, 119).ConfigureAwait(false);
        Assert.That(plugIn.GetView(player).Status, Is.EqualTo(GuideQuestStatus.Active));

        var questStates = new List<CharacterQuestState> { new() { Group = 0, LastFinishedQuest = new QuestDefinition { Group = 0, Number = 1 } } };
        Mock.Get(player.SelectedCharacter!).Setup(c => c.QuestStates).Returns(questStates);
        Assert.That(plugIn.GetView(player).Status, Is.EqualTo(GuideQuestStatus.Ready));

        await plugIn.CompleteAsync(player, 119).ConfigureAwait(false);
        Assert.That(plugIn.GetView(player).Status, Is.EqualTo(GuideQuestStatus.Finished));
    }

    /// <summary>
    /// A request for another step than the current one is ignored (a late click).
    /// </summary>
    [Test]
    public async ValueTask ARequestForAnotherStepIsIgnoredAsync()
    {
        var (plugIn, player) = await CreateAsync().ConfigureAwait(false);

        await plugIn.AcceptAsync(player, 14).ConfigureAwait(false);

        Assert.That(plugIn.GetView(player).Status, Is.EqualTo(GuideQuestStatus.Offered));
    }

    private static void VerifyShown(Player player, GuideQuestResult result)
        => Mock.Get(player.ViewPlugIns.GetPlugIn<IGuideQuestViewPlugIn>()!)
            .Verify(v => v.ShowGuideQuestAsync(It.IsAny<GuideQuestView>(), result), Times.AtLeastOnce);

    private static async ValueTask FinishMoveStepAsync(GuideQuestPlugIn plugIn, Player player)
    {
        await plugIn.AcceptAsync(player, 11).ConfigureAwait(false);
        player.Position = new Point(167, 129);
        plugIn.AttackableMoved(player);
        await plugIn.CompleteAsync(player, 11).ConfigureAwait(false);
    }

    private static async ValueTask FinishHuntStepAsync(GuideQuestPlugIn plugIn, Player player)
    {
        await plugIn.AcceptAsync(player, 14).ConfigureAwait(false);
        for (var i = 0; i < 3; i++)
        {
            await plugIn.AttackableGotKilledAsync(CreateMonster(player, Spider), player).ConfigureAwait(false);
        }

        await plugIn.CompleteAsync(player, 14).ConfigureAwait(false);
    }

    private static async ValueTask<(GuideQuestPlugIn PlugIn, Player Player)> CreateAsync()
    {
        // The experience table is made with the context, so the maximum level comes first.
        var gameConfiguration = PlayerTestHelper.CreateGameConfiguration();
        gameConfiguration.MaximumLevel = 400;
        gameConfiguration.MaximumInventoryMoney = int.MaxValue;
        var mapInitializer = new MapInitializer(gameConfiguration, new NullLogger<MapInitializer>(), NullDropGenerator.Instance, null);
        var gameContext = new GameContext(gameConfiguration, new InMemoryPersistenceContextProvider(), mapInitializer, new NullLoggerFactory(), new PlugInManager(null, new NullLoggerFactory(), null, null), NullDropGenerator.Instance, new ConfigurationChangeMediator());
        mapInitializer.PlugInManager = gameContext.PlugInManager;
        mapInitializer.PathFinderPool = gameContext.PathFinderPool;
        var player = await PlayerTestHelper.CreatePlayerAsync(gameContext).ConfigureAwait(false);
        player.CurrentMap = await player.GameContext.GetMapAsync(0).ConfigureAwait(false);
        player.SelectedCharacter!.Id = Guid.NewGuid();
        player.Attributes![Stats.Level] = 1;
        var plugIn = new GuideQuestPlugIn
        {
            Configuration = new GuideQuestConfiguration
            {
                StateFile = string.Empty,
                Steps =
                [
                    new() { Number = 11, Name = "Move to Lorencia", MinimumLevel = 1, Classes = [BaseClassCode.DarkWizard, BaseClassCode.DarkKnight], Objective = GuideQuestObjective.MoveTo, Map = 0, X = 167, Y = 129, Experience = 85, Money = 100 },
                    new() { Number = 12, Name = "Move to Noria", MinimumLevel = 1, Classes = [BaseClassCode.FairyElf], Objective = GuideQuestObjective.MoveTo, Map = 3, X = 168, Y = 86 },
                    new() { Number = 14, Name = "Hunt Lorencia Spider", MinimumLevel = 1, Objective = GuideQuestObjective.HuntMonster, Target = Spider, Count = 3, Map = 0, X = 193, Y = 126, Experience = 300, Money = 2000 },
                    new() { Number = 49, Name = "Reach Lv. 20", MinimumLevel = 1, Objective = GuideQuestObjective.ReachLevel, Target = 20 },
                    new() { Number = 119, Name = "1st Class Upgrade Quest Completed", MinimumLevel = 150, Objective = GuideQuestObjective.CompleteQuest, Target = 1 },
                ],
            },
        };
        return (plugIn, player);
    }

    private static Monster CreateMonster(Player player, short number)
        => new(new MonsterSpawnArea { GameMap = player.CurrentMap!.Definition }, new MonsterDefinition { Number = number }, player.CurrentMap, NullDropGenerator.Instance, new Mock<INpcIntelligence>().Object, player.GameContext.PlugInManager, player.GameContext.PathFinderPool);
}
