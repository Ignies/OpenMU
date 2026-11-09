// <copyright file="Season21GuideQuestsDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.GuideQuests;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Tests the optional update of the guide quests of Season 21 (S21Emu <c>QuestGuide.txt</c>).
/// </summary>
[TestFixture]
internal class Season21GuideQuestsDataTest
{
    /// <summary>
    /// The update sets the 186 steps of the data and switches the guide quests on; a class gets its own chain.
    /// </summary>
    [Test]
    public async Task UpdateSetsTheStepsOfTheDataAsync()
    {
        var (context, gameConfiguration) = await CreateAsync().ConfigureAwait(false);

        await new AddSeason21GuideQuestsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var configuration = GetConfiguration(gameConfiguration, out var plugIn);
        Assert.That(plugIn.IsActive, Is.True);
        Assert.That(configuration.Steps, Has.Count.EqualTo(186));
        var first = configuration.Steps[0];
        Assert.That((first.Number, first.Name, first.Objective, first.Map, first.X, first.Y), Is.EqualTo(((short)11, "Move to Lorencia", GuideQuestObjective.MoveTo, (short)0, (byte)167, (byte)129)));
        Assert.That(first.IsFor(BaseClassCode.DarkWizard) && !first.IsFor(BaseClassCode.FairyElf), Is.True, "The elf starts in Noria.");

        var spider = configuration.Steps.Single(s => s.Number == 14);
        Assert.That((spider.Objective, spider.Target, spider.Count, spider.Experience, spider.Money), Is.EqualTo((GuideQuestObjective.HuntMonster, 3, 3, 300, 2000)));
        var apple = configuration.Steps.Single(s => s.Number == 26);
        Assert.That((apple.Objective, apple.Target, apple.TargetNumber, apple.Count), Is.EqualTo((GuideQuestObjective.CollectItem, 14, (short)0, 5)));
        var level = configuration.Steps.Single(s => s.Number == 49);
        Assert.That((level.Objective, level.Target), Is.EqualTo((GuideQuestObjective.ReachLevel, 20)));
        var thirdClass = configuration.Steps.Single(s => s.Number == 196);
        Assert.That((thirdClass.Objective, thirdClass.Target, thirdClass.MinimumLevel), Is.EqualTo((GuideQuestObjective.CompleteQuest, 6, 400)));

        Assert.That(configuration.Steps.Count(s => s.IsFor(BaseClassCode.DarkWizard)), Is.EqualTo(164));
        Assert.That(configuration.Steps.SelectMany(s => s.Items).All(i => gameConfiguration.Items.Any(d => d.Group == i.Group && d.Number == i.Number)), Is.True, "Only items of the catalog.");
        Assert.That(configuration.Steps.SelectMany(s => s.Items), Is.Not.Empty);
        Assert.That(GameLogic.ForkInfo.Has("muquest"), Is.True);
    }

    /// <summary>
    /// Applied twice, the update keeps the steps and the switch of the operator; it is optional after the item catalog.
    /// </summary>
    [Test]
    public async Task UpdateTwiceKeepsTheOperatorsStepsAsync()
    {
        var (context, gameConfiguration) = await CreateAsync().ConfigureAwait(false);
        var update = new AddSeason21GuideQuestsUpdatePlugIn();
        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        var configuration = GetConfiguration(gameConfiguration, out var plugIn);
        configuration.Steps.RemoveAt(0);
        plugIn.SetConfiguration(configuration, null);
        plugIn.IsActive = false;

        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(GetConfiguration(gameConfiguration, out _).Steps, Has.Count.EqualTo(185));
        Assert.That(plugIn.IsActive, Is.False);
        Assert.That(update.IsMandatory, Is.False);
        Assert.That(update.DependsOn.Select(d => d.Key), Does.Contain(new AddSeason21ItemCatalogUpdatePlugIn().Key));
    }

    private static GuideQuestConfiguration GetConfiguration(GameConfiguration gameConfiguration, out PlugInConfiguration plugIn)
    {
        plugIn = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(GuideQuestPlugIn).GUID);
        return plugIn.GetConfiguration<GuideQuestConfiguration>(new IdIgnoringReferenceHandler())!;
    }

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        var dataInitialization = new VersionSeasonSix.DataInitialization(contextProvider, new NullLoggerFactory());
        await dataInitialization.CreateInitialDataAsync(1, true).ConfigureAwait(false);
        var context = contextProvider.CreateNewContext();
        var gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        return (context, gameConfiguration);
    }
}
