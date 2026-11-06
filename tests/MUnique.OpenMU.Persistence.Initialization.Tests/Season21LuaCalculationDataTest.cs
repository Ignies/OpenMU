// <copyright file="Season21LuaCalculationDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;

/// <summary>
/// Tests the shield, recovery, per skill damage rates and elemental stats against the S21Emu calculation
/// (<c>Data/Scripts/LuaCalc/Character/Calculations</c> SD, AutoRecuperation, Elemental and
/// <c>LuaCalc/Skill/SkillDamageCalc.lua</c>, version S21.1-1).
/// </summary>
[TestFixture]
internal class Season21LuaCalculationDataTest
{
    private const float Strength = 1000;
    private const float Agility = 600;
    private const float Vitality = 600;
    private const float Energy = 900;
    private const float Level = 400;

    private const float StandardElementalAttackRate = (Level * 5) + (Strength / 4) + (Agility * 1.5f);

    private const float AgilityElementalAttackRate = (Level * 5) + (Agility * 4 / 3) + (Strength / 2);

    private static readonly IReadOnlyDictionary<BaseClassCode, float[]> ElementalValues = new Dictionary<BaseClassCode, float[]>
    {
        [BaseClassCode.DarkWizard] = [Agility / 4, Energy / 9, Energy / 6, StandardElementalAttackRate, Agility / 3],
        [BaseClassCode.DarkKnight] = [Agility / 3, Strength / 6, Strength / 4, StandardElementalAttackRate, Agility / 3],
        [BaseClassCode.FairyElf] = [Agility / 10, (Strength / 14) + (Agility / 8), (Strength / 8) + (Agility / 4), StandardElementalAttackRate, Agility / 4],
        [BaseClassCode.MagicGladiator] = [Agility / 5, (Strength / 10) + (Energy / 14), (Strength / 8) + (Energy / 4), StandardElementalAttackRate, Agility / 3],
        [BaseClassCode.DarkLord] = [Agility / 7, (Strength / 10) + (Energy / 14), (Strength / 6) + (Energy / 10), StandardElementalAttackRate, Agility / 7],
        [BaseClassCode.Summoner] = [Agility / 3, Energy / 10, Energy / 6, StandardElementalAttackRate, Agility / 4],
        [BaseClassCode.RageFighter] = [Agility / 8, (Strength / 10) + (Vitality / 20), (Strength / 6) + (Vitality / 15), StandardElementalAttackRate, Agility / 10],
        [BaseClassCode.GrowLancer] = [Agility / 7, (Strength / 10) + (Agility / 5), (Strength / 6) + (Agility / 3), AgilityElementalAttackRate, Agility / 4],
        [BaseClassCode.RuneWizard] = [Agility / 4, Energy / 9, Energy / 6, StandardElementalAttackRate, Agility / 3],
        [BaseClassCode.Slayer] = [Agility / 10, (Strength / 10) + (Agility / 5), (Strength / 6) + (Agility / 3), AgilityElementalAttackRate, Agility / 4],
        [BaseClassCode.GunCrusher] = [Agility / 4, Energy / 9, Energy / 6, StandardElementalAttackRate, Agility / 3],
        [BaseClassCode.LightWizard] = [Agility / 4, Energy / 9, Energy / 6, StandardElementalAttackRate, Agility / 3],
        [BaseClassCode.LemuriaMage] = [Agility / 4, Energy / 9, Energy / 6, StandardElementalAttackRate, Agility / 3],
        [BaseClassCode.IllusionKnight] = [Agility / 10, (Strength / 10) + (Agility / 5), (Strength / 6) + (Agility / 3), AgilityElementalAttackRate, Agility / 4],
        [BaseClassCode.Alchemist] = [Agility / 3, Energy / 6, Energy / 4, StandardElementalAttackRate, Agility / 3],
        [BaseClassCode.Crusader] = [Agility / 3, Strength / 7, Strength / 5, StandardElementalAttackRate, Agility / 3],
    };

    /// <summary>
    /// The skills with an own rate in the source: the skill number, the class which uses it and the multiplier.
    /// </summary>
    private static readonly (short Skill, BaseClassCode Class, float Multiplier)[] SkillRates =
    [
        (2101, BaseClassCode.RuneWizard, 1.7f + (Energy / 4000)), // rune sphere
        (2102, BaseClassCode.RuneWizard, 1.7f + (Energy / 4000)), // rune praise
        (240, BaseClassCode.RuneWizard, 2), // magical shot
        (240, BaseClassCode.Alchemist, 2),
        (240, BaseClassCode.LightWizard, 2 + (Energy / 5000)),
        (241, BaseClassCode.LightWizard, 2 + (Energy / 5000)), // shining bird
        (243, BaseClassCode.LightWizard, 2 + (Energy / 5000)), // spear storm
        (245, BaseClassCode.LemuriaMage, 1.4f + (Energy / 10000)), // marvel burst
        (247, BaseClassCode.LemuriaMage, 1.4f + (Energy / 10000)), // ultimate force
        (271, BaseClassCode.GrowLancer, 1 + (Agility / 1000)), // spin step
        (274, BaseClassCode.GrowLancer, (1 + (Agility / 1000)) * 0.8f), // magic pin, first hit
        (275, BaseClassCode.GrowLancer, 1), // clash
        (276, BaseClassCode.GrowLancer, 1 + (Strength / 1000)), // harsh strike, first hit
        (277, BaseClassCode.GrowLancer, (2 + (Strength / 1000) + (Agility / 1000)) * 0.8f), // shining peak
        (279, BaseClassCode.GrowLancer, (1 + (Strength / 1000)) * 0.5f), // breche
        (2085, BaseClassCode.GrowLancer, 1 + (Strength / 1000)), // wild breche
        (2036, BaseClassCode.GrowLancer, 1 + (Agility / 1000)), // oversting
        (282, BaseClassCode.RageFighter, 0.5f + (Vitality / 1000)), // spirit hook
        (739, BaseClassCode.RageFighter, 0.5f + (Agility / 1000)), // dark phoenix shot
        (2012, BaseClassCode.MagicGladiator, 0.5f + (Strength / 1000)), // chaos blade
        (2013, BaseClassCode.MagicGladiator, 2 + (Energy / 2500)), // havok spear
        (737, BaseClassCode.DarkLord, 2 + (Energy / 2500)), // wind soul
    ];

    /// <summary>
    /// Every evolution of the nine classes has the shield of SD.lua: 1.2 times the four stats, the total level squared
    /// by 30 and half the defense.
    /// </summary>
    [Test]
    public async Task ShieldFollowsTheSourceAsync()
    {
        var (_, gameConfiguration) = await CreateSeason21Async().ConfigureAwait(false);

        foreach (var characterClass in Season21Classes(gameConfiguration))
        {
            var attributes = Evaluate(gameConfiguration, characterClass);
            var expected = (1.2f * (Strength + Agility + Vitality + Energy)) + (Level * Level / 30) + (attributes[Stats.DefenseBase] / 2);
            Assert.That(attributes[Stats.MaximumShield], Is.EqualTo(expected).Within(0.1), characterClass.Name.ValueInNeutralLanguage);
        }
    }

    /// <summary>
    /// The recovery of AutoRecuperation.lua: one shield point on top of each shield recovery, and 3 % of the maximum
    /// ability, the 5 % being the dark knight's only.
    /// </summary>
    [Test]
    public async Task RecoveryFollowsTheSourceAsync()
    {
        var (_, gameConfiguration) = await CreateSeason21Async().ConfigureAwait(false);

        AssertRecovery(gameConfiguration);
    }

    /// <summary>
    /// A skill with an own rate in SkillDamageCalc.lua takes it instead of the class multiplier.
    /// </summary>
    [Test]
    public async Task SkillsTakeTheirOwnRateAsync()
    {
        var (_, gameConfiguration) = await CreateSeason21Async().ConfigureAwait(false);

        AssertSkillRates(gameConfiguration);
    }

    /// <summary>
    /// The spirit hook of the rage fighter adds a tenth of the energy to the damage before its rate.
    /// </summary>
    [Test]
    public async Task SpiritHookAddsEnergyAsync()
    {
        var (_, gameConfiguration) = await CreateSeason21Async().ConfigureAwait(false);
        var skill = gameConfiguration.Skills.Single(s => s.Number == 282);
        var attributes = Evaluate(gameConfiguration, skill.QualifiedCharacters.First());

        var skillAttributes = new SkillEntry { Skill = skill }.EnsureSkillAttributes(attributes)!;

        Assert.That(skillAttributes[Stats.SkillFinalDamageBonus], Is.EqualTo(Energy / 10).Within(0.001));
    }

    /// <summary>
    /// A skill rate stops at 500 %, as in the source.
    /// </summary>
    [Test]
    public async Task SkillRatesStopAtFiveAsync()
    {
        var (_, gameConfiguration) = await CreateSeason21Async().ConfigureAwait(false);
        var harshStrike = gameConfiguration.Skills.Single(s => s.Number == 276);
        var lancer = gameConfiguration.FindByClassCode(ClassCodes.GetCode(BaseClassCode.GrowLancer, ClassEvolution.First))!;

        Assert.That(GetSkillMultiplier(gameConfiguration, lancer, harshStrike, strength: 10000), Is.EqualTo(5).Within(0.001));
    }

    /// <summary>
    /// Databases with the previous versions of the class and skill updates get the recovery and the skill rates, once.
    /// </summary>
    [Test]
    public async Task PreviousVersionsGetTheSourceValuesAsync()
    {
        var (context, gameConfiguration) = await CreateSeason21Async().ConfigureAwait(false);
        var counts = gameConfiguration.CharacterClasses.ToDictionary(c => c.Number, c => (c.AttributeCombinations.Count, c.BaseAttributeValues.Count));
        var skillCounts = gameConfiguration.Skills.ToDictionary(s => s.Number, s => s.AttributeRelationships.Count);
        foreach (var characterClass in Season21Classes(gameConfiguration))
        {
            characterClass.BaseAttributeValues.Remove(characterClass.BaseAttributeValues.Single(v => v.Definition == Stats.ShieldRecoveryAbsolute));
            if (ClassCodes.GetBaseClass(characterClass.Number) is BaseClassCode.GrowLancer or BaseClassCode.Crusader)
            {
                var recovery = characterClass.BaseAttributeValues.Single(v => v.Definition == Stats.AbilityRecoveryMultiplier);
                characterClass.BaseAttributeValues.Remove(recovery);
                characterClass.BaseAttributeValues.Add(context.CreateNew<ConstValueAttribute>(0.05f, recovery.Definition));
            }
        }

        foreach (var number in SkillRates.Select(r => r.Skill).Distinct())
        {
            gameConfiguration.Skills.Single(s => s.Number == number).AttributeRelationships.Clear();
        }

        var updates = await context.GetAsync<ConfigurationUpdate>().ConfigureAwait(false);
        updates.Single(u => u.Key == new AddSeason21ClassesUpdatePlugIn().Key).Version = 2;
        updates.Single(u => u.Key == new AddSeason21SkillsUpdatePlugIn().Key).Version = 1;

        await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21SkillsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(gameConfiguration.CharacterClasses.ToDictionary(c => c.Number, c => (c.AttributeCombinations.Count, c.BaseAttributeValues.Count)), Is.EqualTo(counts));
        Assert.That(gameConfiguration.Skills.ToDictionary(s => s.Number, s => s.AttributeRelationships.Count), Is.EqualTo(skillCounts));
        AssertRecovery(gameConfiguration);
        AssertSkillRates(gameConfiguration);
    }

    /// <summary>
    /// The skills of Season 6 keep their relationships: the rates are on the skills only Season 21 adds.
    /// </summary>
    [Test]
    public async Task Season6SkillsStayAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);
        var counts = gameConfiguration.Skills.ToDictionary(s => s.Number, s => s.AttributeRelationships.Count);

        await ApplySeason21Async(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(counts.Where(c => gameConfiguration.Skills.Single(s => s.Number == c.Key).AttributeRelationships.Count != c.Value), Is.Empty);
    }

    /// <summary>
    /// Every class, Season 6 ones included, has the elemental stats of Elemental.lua once the elements are added.
    /// </summary>
    [Test]
    public async Task ElementalStatsFollowTheSourceAsync()
    {
        var (context, gameConfiguration) = await CreateSeason21Async().ConfigureAwait(false);

        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21ElementsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        foreach (var characterClass in gameConfiguration.CharacterClasses)
        {
            var expected = ElementalValues[ClassCodes.GetBaseClass(characterClass.Number)!.Value];
            var attributes = Evaluate(gameConfiguration, characterClass);
            var actual = new[] { Stats.ElementDefense, Stats.ElementDamageMin, Stats.ElementDamageMax, Stats.ElementAttackRate, Stats.ElementDefenseRate }.Select(a => attributes[a]);
            Assert.That(actual, Is.EqualTo(expected).Within(0.01), characterClass.Name.ValueInNeutralLanguage);
        }
    }

    private static void AssertRecovery(GameConfiguration gameConfiguration)
    {
        foreach (var characterClass in Season21Classes(gameConfiguration))
        {
            var attributes = Evaluate(gameConfiguration, characterClass);
            var name = characterClass.Name.ValueInNeutralLanguage;
            Assert.That(attributes[Stats.ShieldRecoveryAbsolute], Is.EqualTo(1), $"{name} shield recovery");
            Assert.That(attributes[Stats.AbilityRecoveryMultiplier], Is.EqualTo(0.03f).Within(0.0001), $"{name} ability recovery");
        }
    }

    private static void AssertSkillRates(GameConfiguration gameConfiguration)
    {
        foreach (var (number, baseClass, multiplier) in SkillRates)
        {
            var skill = gameConfiguration.Skills.Single(s => s.Number == number);
            var characterClass = skill.QualifiedCharacters.First(c => ClassCodes.GetBaseClass(c.Number) == baseClass);
            Assert.That(GetSkillMultiplier(gameConfiguration, characterClass, skill), Is.EqualTo(multiplier).Within(0.001), $"skill {number} of {characterClass.Name.ValueInNeutralLanguage}");
        }
    }

    private static IEnumerable<CharacterClass> Season21Classes(GameConfiguration gameConfiguration)
        => gameConfiguration.CharacterClasses.Where(c => ClassCodes.GetBaseClass(c.Number) > BaseClassCode.RageFighter);

    private static float GetSkillMultiplier(GameConfiguration gameConfiguration, CharacterClass characterClass, Skill skill, float strength = Strength)
    {
        var attributes = Evaluate(gameConfiguration, characterClass, strength);
        var skillAttributes = new SkillEntry { Skill = skill }.EnsureSkillAttributes(attributes);
        var final = skillAttributes?[Stats.SkillFinalMultiplier] ?? 0;
        return final > 0 ? final : attributes[Stats.SkillMultiplier];
    }

    private static AttributeSystem Evaluate(GameConfiguration gameConfiguration, CharacterClass characterClass, float strength = Strength)
    {
        var stats = characterClass.StatAttributes.Select(s => (IAttribute)new StatAttribute(s.Attribute!, s.BaseValue)).ToList();
        var attributes = new AttributeSystem(
            stats,
            characterClass.BaseAttributeValues.Concat(gameConfiguration.GlobalBaseAttributeValues),
            characterClass.AttributeCombinations.Concat(gameConfiguration.GlobalAttributeCombinations));
        attributes.SetStatAttribute(Stats.Level, Level);
        attributes.SetStatAttribute(Stats.BaseStrength, strength);
        attributes.SetStatAttribute(Stats.BaseAgility, Agility);
        attributes.SetStatAttribute(Stats.BaseVitality, Vitality);
        attributes.SetStatAttribute(Stats.BaseEnergy, Energy);
        if (characterClass.StatAttributes.Any(s => s.Attribute == Stats.BaseLeadership))
        {
            attributes.SetStatAttribute(Stats.BaseLeadership, 0);
        }

        return attributes;
    }

    private static async Task ApplySeason21Async(IContext context, GameConfiguration gameConfiguration)
    {
        await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21SkillsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
    }

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateSeason21Async()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);
        await ApplySeason21Async(context, gameConfiguration).ConfigureAwait(false);
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
