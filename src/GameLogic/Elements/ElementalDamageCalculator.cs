// <copyright file="ElementalDamageCalculator.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Elements;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// Calculates the elemental damage of a hit (see Readme.md, "Elemental damage").
/// </summary>
public static class ElementalDamageCalculator
{
    private const float Percent = 100f;

    private static readonly AttributeDefinition[] DamageAgainst =
    [
        Stats.ElementDamageAgainstFire, Stats.ElementDamageAgainstWater, Stats.ElementDamageAgainstEarth, Stats.ElementDamageAgainstWind, Stats.ElementDamageAgainstDarkness,
    ];

    private static readonly AttributeDefinition[] DefenseAgainst =
    [
        Stats.ElementDefenseAgainstFire, Stats.ElementDefenseAgainstWater, Stats.ElementDefenseAgainstEarth, Stats.ElementDefenseAgainstWind, Stats.ElementDefenseAgainstDarkness,
    ];

    /// <summary>
    /// Gets the element of an object: one of the five, or <see cref="Element.None"/>.
    /// </summary>
    /// <param name="attributes">The attributes of the object.</param>
    /// <param name="id">The id of the object on its map.</param>
    /// <returns>The element.</returns>
    /// <remarks>
    /// A monster of the <see cref="Element.Random"/> element takes one of the five by its object id, so it keeps its
    /// element while it lives and gets a new one with the id of its next spawn.
    /// </remarks>
    public static Element GetElement(this IAttributeSystem attributes, ushort id)
    {
        var element = (Element)(int)attributes[Stats.Element];
        return element switch
        {
            >= Element.Fire and <= Element.Darkness => element,
            Element.Random => (Element)((id % (int)Element.Darkness) + 1),
            _ => Element.None,
        };
    }

    /// <summary>
    /// Calculates the elemental damage of a hit, after the elemental defense of the defender.
    /// </summary>
    /// <param name="attacker">The attributes of the attacker.</param>
    /// <param name="element">The element of the attacker.</param>
    /// <param name="defender">The attributes of the defender.</param>
    /// <param name="defenderElement">The element of the defender.</param>
    /// <param name="isPvp">If set to <c>true</c>, the defender is a player.</param>
    /// <param name="imprintPercent">The element imprint bonus of the skill, in percent.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The elemental damage; 0 when the attacker has no element or the elemental hit fails.</returns>
    /// <remarks>
    /// The order is a design (S21Emu and Webzen give the factors, not the formula): the base damage between minimum
    /// and maximum, plus the bonuses, times the increase, the wheel, the imprint and the PvP multiplier, minus the
    /// elemental defense of the defender with its bonuses and increase. The elemental hit has its own success roll,
    /// elemental attack rate against elemental defense rate, shaped like the normal hit chance.
    /// </remarks>
    public static uint Calculate(IAttributeSystem attacker, Element element, IAttributeSystem defender, Element defenderElement, bool isPvp, int imprintPercent, ElementsConfiguration configuration)
    {
        if (element == Element.None || !Rand.NextRandomBool(GetHitChance(attacker, defender, configuration)))
        {
            return 0;
        }

        var minimum = (int)attacker[Stats.ElementDamageMin];
        var maximum = Math.Max(minimum, (int)attacker[Stats.ElementDamageMax]);
        float damage = Rand.NextInt(minimum, maximum + 1)
            + attacker[Stats.ElementDamageBonus]
            + (isPvp ? attacker[Stats.ElementDamagePvp] : attacker[Stats.ElementDamagePvm])
            + (defenderElement == Element.None ? 0 : attacker[DamageAgainst[(int)defenderElement - 1]]);
        damage *= (1 + attacker[Stats.ElementDamageIncrease])
                  * configuration.GetMultiplier(element, defenderElement)
                  * (imprintPercent / Percent)
                  * (isPvp ? configuration.PvpMultiplier : 1);

        var defense = (defender[Stats.ElementDefense]
                       + (isPvp ? defender[Stats.ElementDefensePvp] : defender[Stats.ElementDefensePvm])
                       + defender[DefenseAgainst[(int)element - 1]])
                      * (1 + defender[Stats.ElementDefenseIncrease]);
        return (uint)Math.Max(0, damage - defense);
    }

    private static float GetHitChance(IAttributeSystem attacker, IAttributeSystem defender, ElementsConfiguration configuration)
    {
        var defenseRate = defender[Stats.ElementDefenseRate];
        if (defenseRate <= 0)
        {
            return 1;
        }

        var attackRate = attacker[Stats.ElementAttackRate];
        return defenseRate < attackRate ? 1 - (defenseRate / attackRate) : configuration.MinimumHitChance;
    }
}
