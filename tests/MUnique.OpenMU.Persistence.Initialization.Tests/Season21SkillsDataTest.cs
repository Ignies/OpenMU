// <copyright file="Season21SkillsDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.EnhanceTree;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Tests the <see cref="AddSeason21SkillsUpdatePlugIn"/>: the skills, master tree nodes, books and the skill enhancement tree of Season 21.
/// </summary>
[TestFixture]
internal class Season21SkillsDataTest
{
    private const short SpinStep = 271;
    private const short SpinStepImproved = 687;
    private const short MeteorStormGale = 2037;
    private const short ExtSkillDamage = 1001;
    private const short Poison = 1;

    /// <summary>
    /// A new database has none of the skills, and the update stays pending.
    /// </summary>
    [Test]
    public async Task NewDatabaseLeavesTheUpdatePendingAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);

        Assert.That(gameConfiguration.Skills.Any(s => s.Number == SpinStep), Is.False);
        var installed = (await context.GetAsync<ConfigurationUpdate>().ConfigureAwait(false)).Select(u => u.Key);
        Assert.That(installed, Has.None.EqualTo(new AddSeason21SkillsUpdatePlugIn().Key));
    }

    /// <summary>
    /// A skill of a new class keeps the numbers of the data, and every evolution of the class may use it.
    /// </summary>
    [Test]
    public async Task NewClassSkillHasTheSourceValuesAsync()
    {
        var (_, gameConfiguration) = await CreateWithSkillsAsync().ConfigureAwait(false);

        var spinStep = gameConfiguration.Skills.Single(s => s.Number == SpinStep);
        Assert.That(spinStep.AttackDamage, Is.EqualTo(100));
        Assert.That(spinStep.Range, Is.EqualTo(2));
        Assert.That(spinStep.SkillType, Is.EqualTo(SkillType.AreaSkillAutomaticHits));
        Assert.That(spinStep.DamageType, Is.EqualTo(DamageType.Physical));
        Assert.That(spinStep.ConsumeRequirements.Single().MinimumValue, Is.EqualTo(12));
        Assert.That(spinStep.Requirements.Single().MinimumValue, Is.EqualTo(83));
        Assert.That(spinStep.Name.ValueInNeutralLanguage, Is.EqualTo("Spin Step"));
        Assert.That(spinStep.QualifiedCharacters.Select(c => ClassCodes.GetBaseClass(c.Number)), Has.All.EqualTo(BaseClassCode.GrowLancer));
        Assert.That(spinStep.QualifiedCharacters, Has.Count.EqualTo(4));
    }

    /// <summary>
    /// A fifth evolution skill is for the fifth evolution only, a fourth evolution skill from the fourth on.
    /// </summary>
    [Test]
    public async Task EvolutionSkillsGoToTheirEvolutionAsync()
    {
        var (_, gameConfiguration) = await CreateWithSkillsAsync().ConfigureAwait(false);

        var gale = gameConfiguration.Skills.Single(s => s.Number == MeteorStormGale);
        Assert.That(gale.QualifiedCharacters.Select(c => c.GetClassCode()), Is.EquivalentTo(new byte?[] { ClassCodes.GetCode(BaseClassCode.DarkWizard, ClassEvolution.Fifth) }));
        var extension = gameConfiguration.Skills.Single(s => s.Number == ExtSkillDamage);
        Assert.That(extension.SkillType, Is.EqualTo(SkillType.PassiveBoost));
        Assert.That(extension.QualifiedCharacters.Select(c => c.GetEvolution()), Has.All.GreaterThanOrEqualTo(ClassEvolution.Fourth));
        Assert.That(extension.QualifiedCharacters, Has.Count.EqualTo(32));
    }

    /// <summary>
    /// A Season 6 skill gets the new classes the data names and keeps its Season 6 classes.
    /// </summary>
    [Test]
    public async Task Season6SkillGetsTheNewClassesAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);
        await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        var poison = gameConfiguration.Skills.Single(s => s.Number == Poison);
        var season6Classes = poison.QualifiedCharacters.Where(c => c.Number < ClassCodes.ForkNumberOffset).ToList();

        await new AddSeason21SkillsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(poison.QualifiedCharacters.Where(c => c.Number < ClassCodes.ForkNumberOffset), Is.EquivalentTo(season6Classes));
        Assert.That(poison.QualifiedCharacters.Select(c => c.GetClassCode()), Has.Member(ClassCodes.GetCode(BaseClassCode.RuneWizard, ClassEvolution.First)));
        Assert.That(poison.AttackDamage, Is.EqualTo(12));
    }

    /// <summary>
    /// A master tree node of a new class has its root, rank, levels, formula and the skill it improves.
    /// </summary>
    [Test]
    public async Task MasterNodeOfNewClassIsDefinedAsync()
    {
        var (_, gameConfiguration) = await CreateWithSkillsAsync().ConfigureAwait(false);

        var improved = gameConfiguration.Skills.Single(s => s.Number == SpinStepImproved);
        Assert.That(improved.MasterDefinition, Is.Not.Null);
        Assert.That(improved.MasterDefinition!.Root, Is.Not.Null);
        Assert.That(improved.MasterDefinition.Rank, Is.EqualTo(2));
        Assert.That(improved.MasterDefinition.MaximumLevel, Is.EqualTo(20));
        Assert.That(improved.MasterDefinition.ReplacedSkill?.Number, Is.EqualTo(SpinStep));
        Assert.That(improved.MasterDefinition.ValueFormula, Does.Contain("level"));
        Assert.That(gameConfiguration.MasterSkillRoots, Has.Count.EqualTo(3));
    }

    /// <summary>
    /// The update fills and turns on the skill enhancement tree.
    /// </summary>
    [Test]
    public async Task EnhanceTreeIsConfiguredAsync()
    {
        var (_, gameConfiguration) = await CreateWithSkillsAsync().ConfigureAwait(false);

        var plugIn = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(EnhanceTreeFeaturePlugIn).GUID);
        var configuration = plugIn.GetConfiguration<EnhanceTreeConfiguration>(new IdIgnoringReferenceHandler())!;
        Assert.That(plugIn.IsActive, Is.True);
        Assert.That(configuration.Nodes, Has.Count.EqualTo(2548));
        Assert.That(configuration.Passives, Has.Count.EqualTo(128));
        Assert.That(configuration.MagicFields, Has.Count.EqualTo(3));
        Assert.That(configuration.Nodes.Select(n => n.Skill), Has.All.Matches<short>(n => gameConfiguration.Skills.Any(s => s.Number == n)));
        Assert.That(configuration.FindNode(BaseClassCode.GrowLancer, 1118)!.Requirements.Single().Points, Is.EqualTo(10));
    }

    /// <summary>
    /// The books of the item catalog teach their skills.
    /// </summary>
    [Test]
    public async Task BooksTeachTheirSkillsAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);
        await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        await new AddSeason21SkillsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var book = gameConfiguration.Items.Single(i => i.Group == 12 && i.Number == SpinStep);
        Assert.That(book.Skill?.Number, Is.EqualTo(SpinStep));
    }

    /// <summary>
    /// Applying the update twice changes nothing, and an operator's change is kept.
    /// </summary>
    [Test]
    public async Task UpdateIsIdempotentAndKeepsOperatorEditsAsync()
    {
        var (context, gameConfiguration) = await CreateWithSkillsAsync().ConfigureAwait(false);
        var spinStep = gameConfiguration.Skills.Single(s => s.Number == SpinStep);
        spinStep.AttackDamage = 123;
        var skills = gameConfiguration.Skills.Count;
        var qualifications = gameConfiguration.Skills.Sum(s => s.QualifiedCharacters.Count);
        var effects = gameConfiguration.MagicEffects.Count;

        await new AddSeason21SkillsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(gameConfiguration.Skills, Has.Count.EqualTo(skills));
        Assert.That(gameConfiguration.Skills.Sum(s => s.QualifiedCharacters.Count), Is.EqualTo(qualifications));
        Assert.That(gameConfiguration.MagicEffects, Has.Count.EqualTo(effects));
        Assert.That(spinStep.AttackDamage, Is.EqualTo(123));
    }

    /// <summary>
    /// The explosion skills of the enhance tree (1125-1127, every class) deal physical damage: as wizardry, a class without
    /// a staff rise multiplies it by zero. Version 5 moves the imported wizardry ones; an operator's other type stays.
    /// </summary>
    [Test]
    public async Task ExplosionsArePhysicalAsync()
    {
        var (context, gameConfiguration) = await CreateWithSkillsAsync().ConfigureAwait(false);
        var explosions = gameConfiguration.Skills.Where(s => s.Number is >= 1125 and <= 1127).OrderBy(s => s.Number).ToList();
        Assert.That(explosions.Select(s => s.DamageType), Is.EqualTo(new[] { DamageType.Physical, DamageType.Physical, DamageType.Physical }));

        explosions[0].DamageType = DamageType.Wizardry;
        explosions[1].DamageType = DamageType.Curse;
        var updates = await context.GetAsync<ConfigurationUpdate>().ConfigureAwait(false);
        updates.Single(u => u.Key == new AddSeason21SkillsUpdatePlugIn().Key).Version = 4;
        await new AddSeason21SkillsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(explosions[0].DamageType, Is.EqualTo(DamageType.Physical));
        Assert.That(explosions[1].DamageType, Is.EqualTo(DamageType.Curse), "the operator's");
    }

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateWithSkillsAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);
        await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21SkillsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        return (context, gameConfiguration);
    }

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateSeason6Async()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        var dataInitialization = new VersionSeasonSix.DataInitialization(contextProvider, new NullLoggerFactory());
        await dataInitialization.CreateInitialDataAsync(1, true).ConfigureAwait(false);
        var context = contextProvider.CreateNewContext();
        var gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        return (context, gameConfiguration);
    }
}
