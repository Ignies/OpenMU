// <copyright file="LevelPenaltiesTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Progression;

using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// Tests <see cref="LevelPenalties"/>, the damage and experience penalties of S21Emu (<c>DamagePenalty.lua</c>,
/// <c>ExperienciePenalty.lua</c>): 10 % per level below the monster level + 600, at most 7 levels.
/// </summary>
[TestFixture]
public class LevelPenaltiesTest
{
    private const int MonsterLevel = 150;
    private const int Labyrinth = 115;
    private const int KardamahalTemple = 143;

    private readonly LevelPenaltyConfiguration _configuration = new();

    /// <summary>
    /// The defaults are the values of the Lua.
    /// </summary>
    [Test]
    public void DefaultsAreTheLuaValues()
    {
        Assert.That(this._configuration.MonsterLevelOffset, Is.EqualTo(600));
        Assert.That(this._configuration.PercentPerLevel, Is.EqualTo(10));
        Assert.That(this._configuration.MaximumLevels, Is.EqualTo(7));
        Assert.That(this._configuration.DamageMinimumEvolution, Is.EqualTo(ClassEvolution.Fourth));
        Assert.That(this._configuration.DamageLevelMaps, Is.EqualTo(new short[] { Labyrinth }));
        Assert.That(this._configuration.DamageLevelMapMinimumLevel, Is.EqualTo(400));
        Assert.That(this._configuration.ExperienceMaps, Is.EqualTo(new short[] { KardamahalTemple }));
        Assert.That(this._configuration.DamagePenaltyActive && this._configuration.MonsterDamageBonusActive && this._configuration.ExperiencePenaltyActive, Is.True);
    }

    /// <summary>
    /// Outside the labyrinth, a fourth evolution below the monster level + 600 loses 10 % per level, at most 70 %; a
    /// third evolution has no penalty.
    /// </summary>
    /// <param name="level">The level.</param>
    /// <param name="masterLevel">The master level.</param>
    /// <param name="percent">The expected penalty in percent.</param>
    [TestCase(400, 345, 50)]
    [TestCase(400, 343, 70)]
    [TestCase(400, 0, 70)]
    [TestCase(400, 349, 10)]
    [TestCase(400, 350, 0)]
    public void DamagePenaltyOfTheFourthEvolution(int level, int masterLevel, int percent)
    {
        Assert.That(this.Damage(0, ClassEvolution.Fourth, level, masterLevel), Is.EqualTo(percent));
        Assert.That(this.Damage(0, ClassEvolution.Fifth, level, masterLevel), Is.EqualTo(percent));
        Assert.That(this.Damage(0, ClassEvolution.Third, level, masterLevel), Is.Zero);
    }

    /// <summary>
    /// The Deep Dungeon renewal removed the hunting penalty there (Webzen Korea patch notes, muhelper.com Deep Dungeon
    /// renewal): no damage penalty on its five floors (116-120), the setting can bring it back.
    /// </summary>
    /// <param name="map">The map.</param>
    [TestCase((short)116)]
    [TestCase((short)120)]
    public void DeepDungeonHasNoDamagePenalty(short map)
    {
        Assert.That(this._configuration.PenaltyFreeMaps, Is.EqualTo(new short[] { 116, 117, 118, 119, 120 }));
        Assert.That(this.Damage(map, ClassEvolution.Fourth, 400, 0), Is.Zero);

        var withPenalty = new LevelPenaltyConfiguration { PenaltyFreeMaps = [] };
        Assert.That(LevelPenalties.GetDamagePenaltyPercent(withPenalty, null, map, ClassEvolution.Fourth, 400, 0, 0, MonsterLevel), Is.EqualTo(70));
    }

    /// <summary>
    /// In the labyrinth, any evolution from total level 400 on gets the penalty; below 400 none.
    /// </summary>
    [Test]
    public void DamagePenaltyInTheLabyrinth()
    {
        Assert.That(this.Damage(Labyrinth, ClassEvolution.Third, 400, 345), Is.EqualTo(50));
        Assert.That(this.Damage(Labyrinth, ClassEvolution.First, 400, 0), Is.EqualTo(70));
        Assert.That(this.Damage(Labyrinth, ClassEvolution.Fifth, 399, 0), Is.Zero);
    }

    /// <summary>
    /// The experience penalty is only for monsters of the Kardamahal temple, for every evolution.
    /// </summary>
    [Test]
    public void ExperiencePenaltyInTheTemple()
    {
        Assert.That(LevelPenalties.GetExperiencePenaltyPercent(this._configuration, null, KardamahalTemple, 400, 346, 0, MonsterLevel), Is.EqualTo(40));
        Assert.That(LevelPenalties.GetExperiencePenaltyPercent(this._configuration, null, KardamahalTemple, 300, 0, 0, MonsterLevel), Is.EqualTo(70));
        Assert.That(LevelPenalties.GetExperiencePenaltyPercent(this._configuration, null, 0, 300, 0, 0, MonsterLevel), Is.Zero);
    }

    /// <summary>
    /// In the normalized mode the threshold maps to the mode's level like every Season 21 total, and the difference
    /// counts levels of the mode: a capped level 400 character is not 350 Season 21 levels short.
    /// </summary>
    [Test]
    public void NormalizedModeCountsItsOwnLevels()
    {
        var progression = new ProgressionConfiguration { Mode = ProgressionMode.Normalized };
        var threshold = EvolutionRules.GetRequiredLevel(MonsterLevel + 600, progression);
        Assert.That(threshold, Is.LessThan(400));

        Assert.That(this.Damage(0, ClassEvolution.Fourth, 400, 0, progression), Is.Zero);
        Assert.That(this.Damage(0, ClassEvolution.Fourth, threshold - 3, 0, progression), Is.EqualTo(30));
        Assert.That(this.Damage(0, ClassEvolution.Fourth, 200, threshold - 200 - 9, progression), Is.EqualTo(70));
        Assert.That(LevelPenalties.GetExperiencePenaltyPercent(this._configuration, progression, KardamahalTemple, threshold - 1, 0, 0, MonsterLevel), Is.EqualTo(10));
    }

    /// <summary>
    /// In the stars mode the character's level is its power level (level and star worth).
    /// </summary>
    [Test]
    public void StarsModeCountsThePowerLevel()
    {
        var progression = new ProgressionConfiguration { Mode = ProgressionMode.Stars };
        var threshold = EvolutionRules.GetRequiredLevel(MonsterLevel + 600, progression);
        var worth = StarRules.GetStarWorth(1, progression);

        Assert.That(this.Damage(0, ClassEvolution.Fourth, threshold - worth - 2, 0, progression, stars: 1), Is.EqualTo(20));
        Assert.That(this.Damage(0, ClassEvolution.Fourth, threshold - worth, 0, progression, stars: 1), Is.Zero);
        Assert.That(this.Damage(Labyrinth, ClassEvolution.Third, threshold - worth - 2, 0, progression, stars: 1), Is.EqualTo(20), "the labyrinth's 400 maps too");
    }

    /// <summary>
    /// The damage changes like the Lua's floor: a player loses the percent rounded up, a monster gains it rounded down.
    /// </summary>
    [Test]
    public void DamageIsRoundedLikeTheLua()
    {
        Assert.That(LevelPenalties.ApplyToDamage(1000, 70, true), Is.EqualTo(300));
        Assert.That(LevelPenalties.ApplyToDamage(1000, 70, false), Is.EqualTo(1700));
        Assert.That(LevelPenalties.ApplyToDamage(15, 10, true), Is.EqualTo(13));
        Assert.That(LevelPenalties.ApplyToDamage(15, 10, false), Is.EqualTo(16));
        Assert.That(LevelPenalties.ApplyToDamage(15, 0, true), Is.EqualTo(15));
    }

    /// <summary>
    /// The switches turn each part off.
    /// </summary>
    [Test]
    public void SwitchesTurnThePartsOff()
    {
        var configuration = new LevelPenaltyConfiguration { DamagePenaltyActive = false, ExperiencePenaltyActive = false };

        Assert.That(LevelPenalties.GetDamagePenaltyPercent(configuration, null, 0, ClassEvolution.Fifth, 400, 0, 0, MonsterLevel), Is.Zero);
        Assert.That(LevelPenalties.GetExperiencePenaltyPercent(configuration, null, KardamahalTemple, 400, 0, 0, MonsterLevel), Is.Zero);
    }

    private int Damage(short map, ClassEvolution evolution, int level, int masterLevel, ProgressionConfiguration? progression = null, int stars = 0)
        => LevelPenalties.GetDamagePenaltyPercent(this._configuration, progression, map, evolution, level, masterLevel, stars, MonsterLevel);
}
