// <copyright file="Season21ClassFormulasDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;

/// <summary>
/// Tests the formulas of the nine new classes of Season 21 against the S21Emu character calculation
/// (<c>Data/Scripts/LuaCalc/Character/Calculations</c>, version S21.1-1).
/// </summary>
[TestFixture]
internal class Season21ClassFormulasDataTest
{
    private const float Strength = 1000;
    private const float Agility = 600;
    private const float Vitality = 600;
    private const float Energy = 900;
    private const float Level = 400;

    private static readonly IReadOnlyDictionary<BaseClassCode, Expected> ExpectedValues = new Dictionary<BaseClassCode, Expected>
    {
        [BaseClassCode.GrowLancer] = new((Strength / 8) + (Agility / 10), (Strength / 4) + (Agility / 6), 0, 0, Agility / 7, Agility / 4, (Level * 2) + (Agility / 5), (Level * 3) + Agility + (Strength / 4), (Level * 3) + (Agility * 2), Agility / 20, Agility / 20, (Strength * 0.15f) + (Agility * 0.2f) + (Vitality * 0.3f) + Energy, 2),
        [BaseClassCode.RuneWizard] = new(Strength / 6, Strength / 4, Energy / 9, Energy / 4, Agility / 4, Agility / 3, (Level * 2) + (Agility / 4), (Level * 5) + (Agility * 1.5f) + (Strength / 4), (Level * 3) + (Agility * 4), Agility / 20, Agility / 10, (Strength * 0.2f) + (Agility * 0.4f) + (Vitality * 0.3f) + (Energy * 0.2f), 1),
        [BaseClassCode.Slayer] = new((Strength / 9) + (Agility / 20), (Strength / 5) + (Agility / 14), 0, 0, Agility / 10, Agility / 3, (Level * 2) + (Agility / 3), (Level * 5) + (Agility * 1.5f) + (Strength / 4), (Level * 3) + (Agility * 2.5f), Agility / 15, Agility / 20, (Strength * 0.15f) + (Agility * 0.2f) + (Vitality * 0.3f) + Energy, 1.2f + (Strength / 800) + (Agility / 2800)),
        [BaseClassCode.GunCrusher] = new(Strength / 8, Strength / 5, Energy / 8, Energy / 4, Agility / 4, Agility / 3, (Level * 2) + (Agility / 4), (Level * 5) + (Agility * 1.5f) + (Strength / 4), (Level * 3) + (Agility * 4), Agility / 20, Agility / 10, (Strength * 0.2f) + (Agility * 0.4f) + (Vitality * 0.3f) + (Energy * 0.2f), 1),
        [BaseClassCode.LightWizard] = new(Strength / 8, Strength / 4, Energy / 5, Energy / 3, Agility / 3, Agility / 3, (Level * 2) + (Agility * 4), (Level * 5) + (Agility * 1.5f) + (Strength / 4), (Level * 3) + (Agility * 4), Agility / 4, Agility / 10, (Strength * 0.2f) + (Agility * 0.4f) + (Vitality * 0.3f) + (Energy * 0.2f), 1),
        [BaseClassCode.LemuriaMage] = new(Strength / 8, Strength / 4, Energy / 5, Energy / 3, Agility / 4, Agility / 3, (Level * 2) + (Agility * 4), (Level * 5) + (Agility * 1.5f) + (Strength / 2), (Level * 3) + (Agility * 4), Agility / 4, Agility / 10, (Strength * 0.2f) + (Agility * 0.4f) + (Vitality * 0.3f) + (Energy * 0.2f), 1),
        [BaseClassCode.IllusionKnight] = new((Agility / 9) + (Strength / 11), (Agility / 6) + (Strength / 9), 0, 0, (Agility / 10) + (Strength / 5), Agility / 3, (Level * 2) + (Agility / 4), (Level * 5) + (Agility * 1.5f) + (Strength / 4), (Level * 3) + (Agility * 2.5f), Agility / 10, Agility / 15, (Strength * 0.35f) + (Agility * 0.4f) + (Vitality * 0.3f) + (Energy * 0.2f), 2 + (Strength / 3000) + (Agility / 3000)),
        [BaseClassCode.Alchemist] = new(Strength / 8, Strength / 4, Energy / 4, Energy / 3, Agility / 2, Agility / 3, (Level * 2) + (Agility / 3), (Level * 5) + (Agility * 1.5f) + (Strength / 4), (Level * 3) + (Agility * 4), Agility / 10, Agility / 15, (Strength * 0.2f) + (Agility * 0.4f) + (Vitality * 0.3f) + (Energy * 0.2f), 1),
        [BaseClassCode.Crusader] = new(Strength / 6, Strength / 4, 0, 0, Agility / 2, Agility / 3, (Level * 2) + (Agility / 3), (Level * 5) + (Agility * 0.75f) + (Strength / 4), (Level * 3) + (Agility * 0.4f), Agility / 10, Agility / 10, (Strength * 0.15f) + (Agility * 0.4f) + (Vitality * 0.3f) + (Energy * 0.2f), 2 + (Energy / 1000)),
    };

    /// <summary>
    /// Every evolution of the nine classes computes damage, defense, rates, speeds, ability and the skill multiplier as
    /// the S21Emu calculation does.
    /// </summary>
    [Test]
    public async Task FormulasFollowTheSourceAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);

        await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        AssertFormulas(gameConfiguration);
    }

    /// <summary>
    /// The skill damage multiplier stops at 500 %, as in the source.
    /// </summary>
    [Test]
    public async Task SkillMultiplierStopsAtFiveAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);

        await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var crusader = Evaluate(gameConfiguration, gameConfiguration.FindByClassCode(ClassCodes.GetCode(BaseClassCode.Crusader, ClassEvolution.First))!, energy: 10000);
        Assert.That(crusader[Stats.SkillMultiplier], Is.EqualTo(5).Within(0.001));
    }

    /// <summary>
    /// A database with the first version of the update gets the formulas of the source: the formulas of the old
    /// archetypes are replaced, nothing is doubled.
    /// </summary>
    [Test]
    public async Task FirstVersionGetsTheSourceFormulasAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);
        var update = new AddSeason21ClassesUpdatePlugIn();
        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        var counts = gameConfiguration.CharacterClasses.ToDictionary(c => c.Number, c => (c.AttributeCombinations.Count, c.BaseAttributeValues.Count));
        var crusader = gameConfiguration.FindByClassCode(ClassCodes.GetCode(BaseClassCode.Crusader, ClassEvolution.Third))!;
        var defense = crusader.AttributeCombinations.Single(r => r.TargetAttribute == Stats.DefenseBase && r.InputAttribute == Stats.TotalAgility);
        defense.InputOperand = 1.0f / 3;
        var entry = (await context.GetAsync<ConfigurationUpdate>().ConfigureAwait(false)).Single(u => u.Key == update.Key);
        entry.Version = 1;

        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        AssertFormulas(gameConfiguration);
        Assert.That(gameConfiguration.CharacterClasses.ToDictionary(c => c.Number, c => (c.AttributeCombinations.Count, c.BaseAttributeValues.Count)), Is.EqualTo(counts));
        Assert.That(entry.Version, Is.EqualTo(update.Version));
    }

    /// <summary>
    /// Applying the current version again keeps a formula which the operator changed.
    /// </summary>
    [Test]
    public async Task CurrentVersionKeepsTheOperatorFormulasAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);
        var update = new AddSeason21ClassesUpdatePlugIn();
        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        var crusader = gameConfiguration.FindByClassCode(ClassCodes.GetCode(BaseClassCode.Crusader, ClassEvolution.First))!;
        var defense = crusader.AttributeCombinations.Single(r => r.TargetAttribute == Stats.DefenseBase && r.InputAttribute == Stats.TotalAgility);
        defense.InputOperand = 0.75f;

        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(crusader.AttributeCombinations.Single(r => r.TargetAttribute == Stats.DefenseBase && r.InputAttribute == Stats.TotalAgility).InputOperand, Is.EqualTo(0.75f));
    }

    private static void AssertFormulas(GameConfiguration gameConfiguration)
    {
        foreach (var (baseClass, expected) in ExpectedValues)
        {
            foreach (var characterClass in gameConfiguration.CharacterClasses.Where(c => ClassCodes.GetBaseClass(c.Number) == baseClass))
            {
                var attributes = Evaluate(gameConfiguration, characterClass);
                var name = characterClass.Name.ValueInNeutralLanguage;
                Assert.That(attributes[Stats.MinimumPhysBaseDmg], Is.EqualTo(expected.MinimumPhysical).Within(0.01), $"{name} min physical");
                Assert.That(attributes[Stats.MaximumPhysBaseDmg], Is.EqualTo(expected.MaximumPhysical).Within(0.01), $"{name} max physical");
                Assert.That(attributes[Stats.MinimumWizBaseDmg], Is.EqualTo(expected.MinimumWizardry).Within(0.01), $"{name} min wizardry");
                Assert.That(attributes[Stats.MaximumWizBaseDmg], Is.EqualTo(expected.MaximumWizardry).Within(0.01), $"{name} max wizardry");
                Assert.That(attributes[Stats.DefenseBase], Is.EqualTo(expected.Defense).Within(0.01), $"{name} defense");
                Assert.That(attributes[Stats.DefenseRatePvm], Is.EqualTo(expected.DefenseRate).Within(0.01), $"{name} defense rate");
                Assert.That(attributes[Stats.DefenseRatePvp], Is.EqualTo(expected.DefenseRatePvp).Within(0.01), $"{name} defense rate pvp");
                Assert.That(attributes[Stats.AttackRatePvm], Is.EqualTo(expected.AttackRate).Within(0.01), $"{name} attack rate");
                Assert.That(attributes[Stats.AttackRatePvp], Is.EqualTo(expected.AttackRatePvp).Within(0.01), $"{name} attack rate pvp");
                Assert.That(attributes[Stats.AttackSpeed], Is.EqualTo(expected.AttackSpeed + attributes[Stats.AttackSpeedAny]).Within(0.01), $"{name} attack speed");
                Assert.That(attributes[Stats.MagicSpeed], Is.EqualTo(expected.MagicSpeed + attributes[Stats.AttackSpeedAny]).Within(0.01), $"{name} magic speed");
                Assert.That(attributes[Stats.MaximumAbility], Is.EqualTo(expected.Ability).Within(0.01), $"{name} ability");
                Assert.That(attributes[Stats.SkillMultiplier], Is.EqualTo(expected.SkillMultiplier).Within(0.001), $"{name} skill multiplier");
            }
        }
    }

    private static AttributeSystem Evaluate(GameConfiguration gameConfiguration, CharacterClass characterClass, float energy = Energy)
    {
        var stats = characterClass.StatAttributes.Select(s => (IAttribute)new StatAttribute(s.Attribute!, s.BaseValue)).ToList();
        var attributes = new AttributeSystem(
            stats,
            characterClass.BaseAttributeValues.Concat(gameConfiguration.GlobalBaseAttributeValues),
            characterClass.AttributeCombinations.Concat(gameConfiguration.GlobalAttributeCombinations));
        attributes.SetStatAttribute(Stats.Level, Level);
        attributes.SetStatAttribute(Stats.BaseStrength, Strength);
        attributes.SetStatAttribute(Stats.BaseAgility, Agility);
        attributes.SetStatAttribute(Stats.BaseVitality, Vitality);
        attributes.SetStatAttribute(Stats.BaseEnergy, energy);
        return attributes;
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

    private sealed record Expected(
        float MinimumPhysical,
        float MaximumPhysical,
        float MinimumWizardry,
        float MaximumWizardry,
        float Defense,
        float DefenseRate,
        float DefenseRatePvp,
        float AttackRate,
        float AttackRatePvp,
        float AttackSpeed,
        float MagicSpeed,
        float Ability,
        float SkillMultiplier);
}
