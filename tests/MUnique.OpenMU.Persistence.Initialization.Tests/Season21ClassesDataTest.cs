// <copyright file="Season21ClassesDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;

/// <summary>
/// Tests the <see cref="AddSeason21ClassesUpdatePlugIn"/>: the fourth and fifth evolutions and the nine new classes.
/// </summary>
[TestFixture]
internal class Season21ClassesDataTest
{
    /// <summary>
    /// A new Season 6 database keeps its 18 classes, and the update stays pending.
    /// </summary>
    [Test]
    public async Task NewDatabaseKeepsTheSeason6ClassesAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);

        Assert.That(gameConfiguration.CharacterClasses, Has.Count.EqualTo(18));
        Assert.That(gameConfiguration.CharacterClasses.Single(c => c.Number == (byte)CharacterClassNumber.GrandMaster).NextGenerationClass, Is.Null);
        var installed = (await context.GetAsync<ConfigurationUpdate>().ConfigureAwait(false)).Select(u => u.Key);
        Assert.That(installed, Has.None.EqualTo(new AddSeason21ClassesUpdatePlugIn().Key));
    }

    /// <summary>
    /// The update adds every class of the sixteen and its evolutions: 76 classes, each number from its class code.
    /// </summary>
    [Test]
    public async Task UpdateAddsAllClassesAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);

        await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(gameConfiguration.CharacterClasses, Has.Count.EqualTo(76));
        Assert.That(gameConfiguration.CharacterClasses.Select(c => c.GetClassCode()), Has.None.Null);
        Assert.That(gameConfiguration.CharacterClasses.Select(c => c.Number), Is.Unique);
        Assert.That(gameConfiguration.CharacterClasses.Select(c => c.Name.ValueInNeutralLanguage), Has.None.Empty);
        Assert.That(gameConfiguration.CharacterClasses.Select(c => c.IntroducedIn), Has.None.EqualTo(GameVersion.Unknown));
        Assert.That(gameConfiguration.CharacterClasses.Where(c => c.Number >= ClassCodes.ForkNumberOffset).Select(c => c.IntroducedIn), Has.All.GreaterThan(GameVersion.Season6Episode3));
    }

    /// <summary>
    /// Each base class is one chain from its first to its fifth evolution; only the first can get created, the third and
    /// later are master classes.
    /// </summary>
    [Test]
    public async Task EveryClassIsOneChainAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);

        await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        for (var baseClass = BaseClassCode.DarkWizard; baseClass <= BaseClassCode.Crusader; baseClass++)
        {
            var first = gameConfiguration.FindByClassCode(ClassCodes.GetCode(baseClass, ClassEvolution.First))!;
            var chain = new List<CharacterClass>();
            for (var current = first; current is not null; current = current.NextGenerationClass)
            {
                chain.Add(current);
            }

            var expected = ClassCodes.HasSecondEvolution(baseClass) ? 5 : 4;
            Assert.That(chain, Has.Count.EqualTo(expected), baseClass.ToString());
            Assert.That(chain.Select(c => c.GetClassCode()! >> 3), Has.All.EqualTo((int)baseClass));
            Assert.That(chain.Last().GetEvolution(), Is.EqualTo(ClassEvolution.Fifth));
            Assert.That(chain.Select(c => c.CanGetCreated), Is.EqualTo(chain.Select(c => c == first)));
            Assert.That(chain.Where(c => c.IsMasterClass), Is.EqualTo(chain.Where(c => c.GetEvolution() >= ClassEvolution.Third)));
            Assert.That(chain.Where(c => c.IsMasterClass).Select(c => c.GetStatAttribute(Stats.MasterLevel)), Has.None.Null);
        }
    }

    /// <summary>
    /// The Season 6 classes keep their numbers, and their fourth and fifth evolutions follow the master class.
    /// </summary>
    [Test]
    public async Task Season6ClassesGetTheirFourthAndFifthEvolutionAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);

        await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var grandMaster = Class(gameConfiguration, CharacterClassNumber.GrandMaster);
        Assert.That(grandMaster.NextGenerationClass?.Number, Is.EqualTo((byte)CharacterClassNumber.SoulWizard));
        Assert.That(grandMaster.NextGenerationClass!.NextGenerationClass?.Number, Is.EqualTo((byte)CharacterClassNumber.DarknessWizard));
        Assert.That(grandMaster.NextGenerationClass.Name.ValueInNeutralLanguage, Is.EqualTo("Soul Wizard"));
        Assert.That(Class(gameConfiguration, CharacterClassNumber.DuelMaster).NextGenerationClass?.Number, Is.EqualTo((byte)CharacterClassNumber.MagicKnight));
        Assert.That(Class(gameConfiguration, CharacterClassNumber.FistMaster).NextGenerationClass?.Name.ValueInNeutralLanguage, Is.EqualTo("Fist Blazer"));

        var soulWizard = Class(gameConfiguration, CharacterClassNumber.SoulWizard);
        Assert.That(soulWizard.GetStatAttribute(Stats.BaseEnergy)?.BaseValue, Is.EqualTo(grandMaster.GetStatAttribute(Stats.BaseEnergy)?.BaseValue));
        Assert.That(soulWizard.AttributeCombinations, Has.Count.EqualTo(grandMaster.AttributeCombinations.Count));
    }

    /// <summary>
    /// A fourth and fifth evolution may use every item and skill of the third.
    /// </summary>
    [Test]
    public async Task FourthAndFifthEvolutionUseTheGearOfTheThirdAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);

        await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var bladeMaster = Class(gameConfiguration, CharacterClassNumber.BladeMaster);
        var evolutions = new[] { Class(gameConfiguration, CharacterClassNumber.DragonKnight), Class(gameConfiguration, CharacterClassNumber.IgnitionKnight) };
        var items = gameConfiguration.Items.Where(i => i.QualifiedCharacters.Contains(bladeMaster)).ToList();
        var skills = gameConfiguration.Skills.Where(s => s.QualifiedCharacters.Contains(bladeMaster)).ToList();
        Assert.That(items, Is.Not.Empty);
        Assert.That(skills, Is.Not.Empty);
        foreach (var evolution in evolutions)
        {
            Assert.That(gameConfiguration.Items.Where(i => i.QualifiedCharacters.Contains(evolution)), Is.EquivalentTo(items));
            Assert.That(gameConfiguration.Skills.Where(s => s.QualifiedCharacters.Contains(evolution)), Is.EquivalentTo(skills));
        }
    }

    /// <summary>
    /// The new classes take the base stats, points and health and mana of the S21Emu data, and can get created at level 1
    /// in Lorencia.
    /// </summary>
    [Test]
    public async Task NewClassesTakeTheSourceValuesAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);

        await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var growLancer = Class(gameConfiguration, CharacterClassNumber.GrowLancer);
        Assert.That(growLancer.Name.ValueInNeutralLanguage, Is.EqualTo("Grow Lancer"));
        Assert.That(growLancer.CanGetCreated, Is.True);
        Assert.That(growLancer.LevelRequirementByCreation, Is.Zero);
        Assert.That(growLancer.HomeMap?.Number, Is.EqualTo(0));
        Assert.That(growLancer.GetStatAttribute(Stats.BaseStrength)?.BaseValue, Is.EqualTo(30));
        Assert.That(growLancer.GetStatAttribute(Stats.BaseAgility)?.BaseValue, Is.EqualTo(30));
        Assert.That(growLancer.GetStatAttribute(Stats.BaseVitality)?.BaseValue, Is.EqualTo(25));
        Assert.That(growLancer.GetStatAttribute(Stats.BaseEnergy)?.BaseValue, Is.EqualTo(24));
        Assert.That(growLancer.GetStatAttribute(Stats.PointsPerLevelUp)?.BaseValue, Is.EqualTo(7));
        Assert.That(Class(gameConfiguration, CharacterClassNumber.RuneWizard).GetStatAttribute(Stats.BaseEnergy)?.BaseValue, Is.EqualTo(40));
        Assert.That(Class(gameConfiguration, CharacterClassNumber.Crusader).GetStatAttribute(Stats.PointsPerLevelUp)?.BaseValue, Is.EqualTo(5));
        Assert.That(Class(gameConfiguration, CharacterClassNumber.Crusader).IntroducedIn, Is.EqualTo(GameVersion.Season20Episode1));
        Assert.That(Season21ClassData.Classes.Single(c => c.IsUnfinishedInSource).BaseClass, Is.EqualTo(BaseClassCode.Crusader));

        foreach (var info in Season21ClassData.Classes)
        {
            var characterClass = gameConfiguration.FindByClassCode(ClassCodes.GetCode(info.BaseClass, ClassEvolution.First))!;
            Assert.That(Evaluate(characterClass, Stats.MaximumHealth, info.Vitality, info.Energy), Is.EqualTo(info.MaxLife).Within(0.01), info.BaseClass.ToString());
            Assert.That(Evaluate(characterClass, Stats.MaximumMana, info.Vitality, info.Energy), Is.EqualTo(info.MaxMana).Within(0.01), info.BaseClass.ToString());
            Assert.That(Evaluate(characterClass, Stats.MaximumHealth, info.Vitality + 10, info.Energy), Is.EqualTo(info.MaxLife + (10 * info.VitalityToLife)).Within(0.01), info.BaseClass.ToString());
        }
    }

    /// <summary>
    /// Applying the update a second time changes nothing, and it keeps the changes of the operator.
    /// </summary>
    [Test]
    public async Task UpdateTwiceKeepsTheOperatorChangesAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);
        var update = new AddSeason21ClassesUpdatePlugIn();

        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        var growLancer = Class(gameConfiguration, CharacterClassNumber.GrowLancer);
        growLancer.GetStatAttribute(Stats.BaseStrength)!.BaseValue = 99;
        growLancer.CanGetCreated = false;
        var dragonKnight = Class(gameConfiguration, CharacterClassNumber.DragonKnight);
        var removedFrom = gameConfiguration.Items.First(i => i.QualifiedCharacters.Contains(dragonKnight));
        removedFrom.QualifiedCharacters.Remove(dragonKnight);
        var qualifications = gameConfiguration.Items.Sum(i => i.QualifiedCharacters.Count);
        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(gameConfiguration.CharacterClasses, Has.Count.EqualTo(76));
        Assert.That(growLancer.GetStatAttribute(Stats.BaseStrength)!.BaseValue, Is.EqualTo(99));
        Assert.That(growLancer.CanGetCreated, Is.False);
        Assert.That(removedFrom.QualifiedCharacters, Does.Not.Contain(dragonKnight));
        Assert.That(gameConfiguration.Items.Sum(i => i.QualifiedCharacters.Count), Is.EqualTo(qualifications));
    }

    private static float Evaluate(CharacterClass characterClass, AttributeSystem.AttributeDefinition target, int vitality, int energy)
    {
        var inputs = new Dictionary<AttributeSystem.AttributeDefinition, float>
        {
            [Stats.TotalLevel] = 1,
            [Stats.TotalVitality] = vitality,
            [Stats.TotalEnergy] = energy,
        };
        var constant = characterClass.BaseAttributeValues
            .Where(v => v.Definition == target && v.AggregateType == AttributeSystem.AggregateType.AddRaw)
            .Sum(v => v.Value);
        var combined = characterClass.AttributeCombinations
            .Where(r => r.TargetAttribute == target && r.InputAttribute is { } input && inputs.ContainsKey(input))
            .Sum(r => r.InputOperand * inputs[r.InputAttribute!]);
        return constant + combined;
    }

    private static CharacterClass Class(GameConfiguration gameConfiguration, CharacterClassNumber number)
        => gameConfiguration.CharacterClasses.Single(c => c.Number == (byte)number);

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
