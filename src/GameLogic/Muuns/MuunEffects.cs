// <copyright file="MuunEffects.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Muuns;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// Maps the option types of the Muuns to the attributes of the owner.
/// </summary>
/// <remarks>
/// The option types are the ones of the Season 21 data (MuunOption.xml); this is where they meet our stats, so it can't be
/// configuration data. Attack power options add to the final damage: against players
/// (<see cref="Stats.FinalDamageIncreasePvp"/>) for the PvP ones, against monsters (<see cref="Stats.FinalDamageBonusPvm"/>)
/// for the others, and against both once their Muun is evolved ("applies to PvP when evolved"; the evolved premium Muuns
/// of the data carry the PvP type of the option of their first form). OpenMU has no excellent,
/// critical or skill damage bonus for monsters only, so those Non-PvP options apply against players as well. Not applied:
/// the repair cost (no attribute), the attack skills of the Muuns (the Muun doesn't cast).
/// </remarks>
public static class MuunEffects
{
    private const float Percent = 0.01f;

    /// <summary>
    /// Gets the attribute and the factor of an option type.
    /// </summary>
    /// <param name="type">The option type.</param>
    /// <param name="isEvolved">If set to <c>true</c>, the Muun is evolved.</param>
    /// <returns>The attribute and the factor of the value; <c>null</c>, if the option has no effect on the owner.</returns>
    public static (AttributeDefinition Attribute, float Factor)? GetEffect(MuunOptionType type, bool isEvolved)
        => type switch
        {
            MuunOptionType.AttackPowerPvp or MuunOptionType.SkillAttackPowerPvp or MuunOptionType.MaximumAttackPowerPvp => (isEvolved ? Stats.FinalDamageBonus : Stats.FinalDamageIncreasePvp, 1),
            MuunOptionType.AttackPower or MuunOptionType.MaximumAttackPower => (isEvolved ? Stats.FinalDamageBonus : Stats.FinalDamageBonusPvm, 1),
            MuunOptionType.SkillAttackPower => (Stats.SkillDamageBonus, 1),
            MuunOptionType.Defense => (Stats.DefenseBase, 1),
            MuunOptionType.DefensePvm => (Stats.DefensePvm, 1),
            MuunOptionType.ExcellentDamage or MuunOptionType.ExcellentDamagePvm => (Stats.ExcellentDamageBonus, 1),
            MuunOptionType.CriticalDamage or MuunOptionType.CriticalDamagePvm => (Stats.CriticalDamageBonus, 1),
            MuunOptionType.Money => (Stats.MoneyAmountRate, Percent),
            MuunOptionType.ElementalDamage => (Stats.ElementDamageBonus, 1),
            MuunOptionType.ElementalDefense or MuunOptionType.ElementalDefensePvm => (Stats.ElementDefense, 1),
            _ => null,
        };
}
