// <copyright file="AbilityCoreRules.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.AbilityCore;

using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// The slot rules of the Ability Enhancement Core: every slot needs the fifth evolution, the skill slots and the option
/// rows a Season 21 total level, read in the world's progression mode (<see cref="EvolutionRules"/>), never raw.
/// </summary>
public static class AbilityCoreRules
{
    /// <summary>
    /// Gets the Season 21 total level a slot asks for on top of the fifth evolution.
    /// </summary>
    /// <param name="slot">The slot.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The total level, 0 for none; <see cref="int.MaxValue"/> for a slot without a level in the configuration.</returns>
    public static int GetSeason21Level(byte slot, AbilityCoreConfiguration configuration) => AbilityCoreSlots.GetKind(slot) switch
    {
        AbilityCardKind.Extra => 0,
        AbilityCardKind.Skill => LevelAt(configuration.SkillSlotLevels, slot - AbilityCoreSlots.FirstSkill),
        AbilityCardKind.Option or AbilityCardKind.Elite => LevelAt(configuration.OptionRowLevels, AbilityCoreSlots.GetRow(slot)),
        _ => int.MaxValue,
    };

    /// <summary>
    /// Determines whether a slot is open for a character.
    /// </summary>
    /// <param name="slot">The slot.</param>
    /// <param name="evolution">The evolution of the character's class.</param>
    /// <param name="level">The normal level.</param>
    /// <param name="masterLevel">The master level.</param>
    /// <param name="stars">The stars (resets).</param>
    /// <param name="progression">The progression configuration, or <c>null</c> if the progression is off.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns><c>true</c>, if the slot is open.</returns>
    public static bool IsOpen(byte slot, ClassEvolution? evolution, int level, int masterLevel, int stars, ProgressionConfiguration? progression, AbilityCoreConfiguration configuration)
    {
        if (evolution != ClassEvolution.Fifth)
        {
            return false;
        }

        var required = GetSeason21Level(slot, configuration);
        return required != int.MaxValue && EvolutionRules.IsMet(required, level, masterLevel, stars, progression);
    }

    /// <summary>
    /// Gets the open slots as bits (bit n is the slot n).
    /// </summary>
    /// <param name="evolution">The evolution of the character's class.</param>
    /// <param name="level">The normal level.</param>
    /// <param name="masterLevel">The master level.</param>
    /// <param name="stars">The stars (resets).</param>
    /// <param name="progression">The progression configuration, or <c>null</c> if the progression is off.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The bits of the open slots.</returns>
    public static uint GetOpenSlots(ClassEvolution? evolution, int level, int masterLevel, int stars, ProgressionConfiguration? progression, AbilityCoreConfiguration configuration)
    {
        uint bits = 0;
        for (byte slot = 0; slot < AbilityCoreSlots.Count; slot++)
        {
            if (IsOpen(slot, evolution, level, masterLevel, stars, progression, configuration))
            {
                bits |= 1u << slot;
            }
        }

        return bits;
    }

    private static int LevelAt(List<int> levels, int index) => index >= 0 && index < levels.Count ? levels[index] : int.MaxValue;
}
