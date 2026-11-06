// <copyright file="Season21ClassFormulas.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.CharacterClasses;

using MUnique.OpenMU.GameLogic.Classes;

/// <summary>
/// The stat formulas of a Season 21 class as the S21Emu character calculation defines them
/// (<c>Data/Scripts/LuaCalc/Character/Calculations</c>, version S21.1-1). Expressed as attribute relationships of the
/// total stats; the source rounds down each value, OpenMU keeps the fraction as it does for the Season 6 classes.
/// </summary>
/// <param name="MinimumPhysical">The minimum physical base damage.</param>
/// <param name="MaximumPhysical">The maximum physical base damage.</param>
/// <param name="MinimumWizardry">The minimum wizardry base damage of a caster.</param>
/// <param name="MaximumWizardry">The maximum wizardry base damage of a caster.</param>
/// <param name="Defense">The base defense.</param>
/// <param name="DefenseRate">The defense rate against monsters.</param>
/// <param name="DefenseRatePvp">The defense rate against players.</param>
/// <param name="AttackRate">The attack rate against monsters.</param>
/// <param name="AttackRatePvp">The attack rate against players.</param>
/// <param name="AttackSpeed">The attack speed.</param>
/// <param name="MagicSpeed">The magic speed.</param>
/// <param name="Ability">The maximum ability (AG).</param>
/// <param name="SkillMultiplier">The skill damage multiplier (2 = 200 %), at most <see cref="MaximumSkillMultiplier"/>.</param>
/// <param name="GeneralSkillRate">The rate of a caster's general skills, see <see cref="Season21SkillRates.GeneralRateId"/>.</param>
/// <param name="FirstSkillRate">The first own skill rate, see <see cref="Season21SkillRates.FirstRateId"/>.</param>
/// <param name="SecondSkillRate">The second own skill rate, see <see cref="Season21SkillRates.SecondRateId"/>.</param>
public sealed record Season21ClassFormulas(
    Season21StatFactors MinimumPhysical,
    Season21StatFactors MaximumPhysical,
    Season21StatFactors MinimumWizardry,
    Season21StatFactors MaximumWizardry,
    Season21StatFactors Defense,
    Season21StatFactors DefenseRate,
    Season21StatFactors DefenseRatePvp,
    Season21StatFactors AttackRate,
    Season21StatFactors AttackRatePvp,
    Season21StatFactors AttackSpeed,
    Season21StatFactors MagicSpeed,
    Season21StatFactors Ability,
    Season21StatFactors SkillMultiplier,
    Season21StatFactors GeneralSkillRate = default,
    Season21StatFactors FirstSkillRate = default,
    Season21StatFactors SecondSkillRate = default)
{
    /// <summary>
    /// The highest skill damage multiplier, 500 % in the source.
    /// </summary>
    public const float MaximumSkillMultiplier = 5;

    private static readonly Season21StatFactors None = default;

    private static readonly Season21StatFactors WizardBattlePoints = new(0.2f, 0.4f, 0.3f, 0.2f);

    private static readonly Season21StatFactors FighterBattlePoints = new(0.15f, 0.2f, 0.3f, 1);

    private static readonly Season21StatFactors AttackRateOfMost = new(0.25f, 1.5f, Level: 5);

    /// <summary>
    /// Gets the formulas of the nine classes which Season 21 adds.
    /// </summary>
    /// <remarks>
    /// The skill multiplier is the rate of the general physical skills of the source. Casters keep 1: OpenMU has one
    /// multiplier for every skill of a class, and their spells don't get the rate of the physical skills. The skills with
    /// a rate of their own take one of the three skill rates (<see cref="Season21SkillRates"/>).
    /// </remarks>
    public static IReadOnlyDictionary<BaseClassCode, Season21ClassFormulas> Classes { get; } = new Dictionary<BaseClassCode, Season21ClassFormulas>
    {
        [BaseClassCode.GrowLancer] = new(
            new(1.0f / 8, 1.0f / 10),
            new(1.0f / 4, 1.0f / 6),
            None,
            None,
            new(Agility: 1.0f / 7),
            new(Agility: 1.0f / 4),
            new(Agility: 1.0f / 5, Level: 2),
            new(0.25f, 1, Level: 3),
            new(Agility: 2, Level: 3),
            new(Agility: 1.0f / 20),
            new(Agility: 1.0f / 20),
            FighterBattlePoints,
            new(Constant: 2),
            FirstSkillRate: new(Strength: 1.0f / 1000, Constant: 1),
            SecondSkillRate: new(Agility: 1.0f / 1000, Constant: 1)),
        [BaseClassCode.RuneWizard] = new(
            new(1.0f / 6),
            new(1.0f / 4),
            new(Energy: 1.0f / 9),
            new(Energy: 1.0f / 4),
            new(Agility: 1.0f / 4),
            new(Agility: 1.0f / 3),
            new(Agility: 1.0f / 4, Level: 2),
            AttackRateOfMost,
            new(Agility: 4, Level: 3),
            new(Agility: 1.0f / 20),
            new(Agility: 1.0f / 10),
            WizardBattlePoints,
            new(Constant: 1),
            GeneralSkillRate: new(Constant: 2),
            FirstSkillRate: new(Energy: 1.0f / 4000, Constant: 1.7f)),
        [BaseClassCode.Slayer] = new(
            new(1.0f / 9, 1.0f / 20),
            new(1.0f / 5, 1.0f / 14),
            None,
            None,
            new(Agility: 1.0f / 10),
            new(Agility: 1.0f / 3),
            new(Agility: 1.0f / 3, Level: 2),
            AttackRateOfMost,
            new(Agility: 2.5f, Level: 3),
            new(Agility: 1.0f / 15),
            new(Agility: 1.0f / 20),
            FighterBattlePoints,
            new(1.0f / 800, 1.0f / 2800, Constant: 1.2f)),
        [BaseClassCode.GunCrusher] = new(
            new(1.0f / 8),
            new(1.0f / 5),
            new(Energy: 1.0f / 8),
            new(Energy: 1.0f / 4),
            new(Agility: 1.0f / 4),
            new(Agility: 1.0f / 3),
            new(Agility: 1.0f / 4, Level: 2),
            AttackRateOfMost,
            new(Agility: 4, Level: 3),
            new(Agility: 1.0f / 20),
            new(Agility: 1.0f / 10),
            WizardBattlePoints,
            new(Constant: 1),
            GeneralSkillRate: new(Constant: 2)),
        [BaseClassCode.LightWizard] = new(
            new(1.0f / 8),
            new(1.0f / 4),
            new(Energy: 1.0f / 5),
            new(Energy: 1.0f / 3),
            new(Agility: 1.0f / 3),
            new(Agility: 1.0f / 3),
            new(Agility: 4, Level: 2),
            AttackRateOfMost,
            new(Agility: 4, Level: 3),
            new(Agility: 1.0f / 4),
            new(Agility: 1.0f / 10),
            WizardBattlePoints,
            new(Constant: 1),
            GeneralSkillRate: new(Energy: 1.0f / 5000, Constant: 2)),
        [BaseClassCode.LemuriaMage] = new(
            new(1.0f / 8),
            new(1.0f / 4),
            new(Energy: 1.0f / 5),
            new(Energy: 1.0f / 3),
            new(Agility: 1.0f / 4),
            new(Agility: 1.0f / 3),
            new(Agility: 4, Level: 2),
            new(0.5f, 1.5f, Level: 5),
            new(Agility: 4, Level: 3),
            new(Agility: 1.0f / 4),
            new(Agility: 1.0f / 10),
            WizardBattlePoints,
            new(Constant: 1),
            GeneralSkillRate: new(Energy: 1.0f / 10000, Constant: 1.4f)),
        [BaseClassCode.IllusionKnight] = new(
            new(1.0f / 11, 1.0f / 9),
            new(1.0f / 9, 1.0f / 6),
            None,
            None,
            new(1.0f / 5, 1.0f / 10),
            new(Agility: 1.0f / 3),
            new(Agility: 1.0f / 4, Level: 2),
            AttackRateOfMost,
            new(Agility: 2.5f, Level: 3),
            new(Agility: 1.0f / 10),
            new(Agility: 1.0f / 15),
            new(0.35f, 0.4f, 0.3f, 0.2f),
            new(1.0f / 3000, 1.0f / 3000, Constant: 2)),
        [BaseClassCode.Alchemist] = new(
            new(1.0f / 8),
            new(1.0f / 4),
            new(Energy: 1.0f / 4),
            new(Energy: 1.0f / 3),
            new(Agility: 1.0f / 2),
            new(Agility: 1.0f / 3),
            new(Agility: 1.0f / 3, Level: 2),
            AttackRateOfMost,
            new(Agility: 4, Level: 3),
            new(Agility: 1.0f / 10),
            new(Agility: 1.0f / 15),
            WizardBattlePoints,
            new(Constant: 1),
            GeneralSkillRate: new(Constant: 2)),
        [BaseClassCode.Crusader] = new(
            new(1.0f / 6),
            new(1.0f / 4),
            None,
            None,
            new(Agility: 1.0f / 2),
            new(Agility: 1.0f / 3),
            new(Agility: 1.0f / 3, Level: 2),
            new(0.25f, 0.75f, Level: 5),
            new(Agility: 0.4f, Level: 3),
            new(Agility: 1.0f / 10),
            new(Agility: 1.0f / 10),
            new(0.15f, 0.4f, 0.3f, 0.2f),
            new(Energy: 1.0f / 1000, Constant: 2)),
    };
}
