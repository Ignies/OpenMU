// <copyright file="ImperialGuardianReworkTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Instances;

using Moq;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic.PlayerActions.MiniGames;
using MUnique.OpenMU.GameLogic.MiniGames.ImperialGuardian;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.Updates;

/// <summary>
/// Tests the Season 18 rework of the Imperial Guardian for Season 21: the entry level and the completion rewards.
/// </summary>
[TestFixture]
public class ImperialGuardianReworkTest
{
    private Season21EventWorld _world = null!;

    /// <summary>
    /// Initializes the world with the rework and the wide items, which the rewards are.
    /// </summary>
    [OneTimeSetUp]
    public async ValueTask SetupAsync()
    {
        this._world = await Season21EventWorld.CreateAsync(new ActivateSeason21WideItemsUpdatePlugIn(), new AddSeason21ImperialGuardianReworkUpdatePlugIn()).ConfigureAwait(false);
    }

    /// <summary>
    /// Every day of the event asks for total level 1000.
    /// </summary>
    [Test]
    public void EveryDayStartsAtTotalLevel1000()
    {
        var days = this._world.Configuration.MiniGameDefinitions.Where(d => d.Type == MiniGameType.ImperialGuardian).ToList();
        Assert.That(days, Has.Count.EqualTo(7));
        Assert.That(days.Select(d => (d.MinimumCharacterLevel, d.MaximumCharacterLevel)), Has.All.EqualTo((1000, int.MaxValue)));
    }

    /// <summary>
    /// The completion rewards of the data are configured, and the event keeps the rest of its configuration.
    /// </summary>
    [Test]
    public void CompletionRewardsAreConfigured()
    {
        var configuration = this._world.GetStoredConfiguration<ImperialGuardianEventDefinition>(typeof(ImperialGuardianFeaturePlugIn).GUID)!;
        Assert.That(configuration.CompletionRewards.Select(r => (r.Group, r.Number, r.Count, r.Chance)), Is.EqualTo(new[] { ((byte)20, (short)94, 4, 20), ((byte)20, (short)94, 2, 40), ((byte)20, (short)95, 1, 40) }));
        Assert.That(configuration.ZoneDuration, Is.EqualTo(TimeSpan.FromMinutes(10)));
    }

    /// <summary>
    /// The roll picks a reward by its chance.
    /// </summary>
    /// <param name="roll">The roll.</param>
    /// <param name="count">The count of the picked reward.</param>
    [TestCase(0, 4)]
    [TestCase(19, 4)]
    [TestCase(20, 2)]
    [TestCase(59, 2)]
    [TestCase(60, 1)]
    [TestCase(99, 1)]
    public void RollPicksByChance(int roll, int count)
    {
        var configuration = this._world.GetStoredConfiguration<ImperialGuardianEventDefinition>(typeof(ImperialGuardianFeaturePlugIn).GUID)!;
        Assert.That(ImperialGuardianItemReward.Pick(configuration.CompletionRewards, roll)?.Count, Is.EqualTo(count));
    }

    /// <summary>
    /// A player who completed the event gets one of the rewards with its count.
    /// </summary>
    [Test]
    public async ValueTask CompletionGivesOneRewardAsync()
    {
        var configuration = this._world.GetStoredConfiguration<ImperialGuardianEventDefinition>(typeof(ImperialGuardianFeaturePlugIn).GUID)!;
        var player = await this._world.CreatePlayerAsync(CharacterClassNumber.BladeMaster, 400, 600).ConfigureAwait(false);

        await ImperialGuardianItemReward.GiveOneAsync(player, configuration.CompletionRewards).ConfigureAwait(false);

        var items = player.Inventory!.Items.Where(i => i.Definition?.Group == 20).Select(i => i.Definition!.Number).ToList();
        Assert.That(items.Count, Is.AnyOf(1, 2, 4));
        Assert.That(items.Distinct().Single(), Is.AnyOf((short)94, (short)95));
    }

    /// <summary>
    /// The grade of total level 1000 is checked against level and master level, not the normal level alone.
    /// </summary>
    [Test]
    public async ValueTask TotalLevelPassesTheGradeAsync()
    {
        var leader = await this._world.CreatePlayerAsync(CharacterClassNumber.BladeMaster, 400, 600).ConfigureAwait(false);
        var mate = await this._world.CreatePlayerAsync(CharacterClassNumber.BladeMaster, 400, 600).ConfigureAwait(false);
        await Season21EventWorld.CreatePartyAsync(leader, mate).ConfigureAwait(false);
        foreach (var number in new short[] { 102, 109 })
        {
            var item = leader.PersistenceContext.CreateNew<Item>();
            item.Definition = this._world.Configuration.Items.Single(i => i.Group == 14 && i.Number == number);
            item.Durability = 1;
            Assert.That(await leader.Inventory!.AddItemAsync(item).ConfigureAwait(false), Is.True);
        }

        await new EnterImperialGuardianAction().TryEnterAsync(leader).ConfigureAwait(false);

        var view = Mock.Get(leader.ViewPlugIns.GetPlugIn<IImperialGuardianViewPlugIn>()!);
        view.Verify(
            v => v.ShowEnterResultAsync(ImperialGuardianEnterResult.CharacterLevelTooLow, It.IsAny<byte>(), It.IsAny<int>(), It.IsAny<ImperialGuardianWeather>(), It.IsAny<TimeSpan>()),
            Times.Never);
    }
}
