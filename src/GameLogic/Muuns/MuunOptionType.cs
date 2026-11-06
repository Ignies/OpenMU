// <copyright file="MuunOptionType.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Muuns;

/// <summary>
/// The kind of the option of a Muun (MuunOption.xml <c>OptionType</c>).
/// </summary>
public enum MuunOptionType : byte
{
    /// <summary>
    /// Increases the attack, wizardry and curse power against players.
    /// </summary>
    AttackPowerPvp = 1,

    /// <summary>
    /// Increases the defense.
    /// </summary>
    Defense = 2,

    /// <summary>
    /// Increases the excellent damage.
    /// </summary>
    ExcellentDamage = 3,

    /// <summary>
    /// Increases the critical damage.
    /// </summary>
    CriticalDamage = 4,

    /// <summary>
    /// Increases the skill attack power against players.
    /// </summary>
    SkillAttackPowerPvp = 5,

    /// <summary>
    /// Increases the maximum attack, wizardry and curse power against players.
    /// </summary>
    MaximumAttackPowerPvp = 7,

    /// <summary>
    /// Increases the attack, wizardry and curse power against monsters; against players too when evolved.
    /// </summary>
    AttackPower = 8,

    /// <summary>
    /// Increases the defense against monsters.
    /// </summary>
    DefensePvm = 9,

    /// <summary>
    /// Increases the excellent damage against monsters.
    /// </summary>
    ExcellentDamagePvm = 10,

    /// <summary>
    /// Increases the critical damage against monsters.
    /// </summary>
    CriticalDamagePvm = 11,

    /// <summary>
    /// Increases the skill attack power against monsters; against players too when evolved.
    /// </summary>
    SkillAttackPower = 12,

    /// <summary>
    /// Increases the maximum attack, wizardry and curse power against monsters; against players too when evolved.
    /// </summary>
    MaximumAttackPower = 14,

    /// <summary>
    /// Reduces the item repair cost by a percentage.
    /// </summary>
    RepairCost = 15,

    /// <summary>
    /// Increases the zen which monsters drop by a percentage.
    /// </summary>
    Money = 17,

    /// <summary>
    /// Increases the elemental damage.
    /// </summary>
    ElementalDamage = 18,

    /// <summary>
    /// Increases the elemental defense.
    /// </summary>
    ElementalDefense = 19,

    /// <summary>
    /// Increases the elemental defense against monsters.
    /// </summary>
    ElementalDefensePvm = 21,

    /// <summary>
    /// The Muun casts an attack skill of a level.
    /// </summary>
    AttackSkill = 50,

    /// <summary>
    /// The Muun casts an attack skill of a level against monsters.
    /// </summary>
    AttackSkillPvm = 53,
}
