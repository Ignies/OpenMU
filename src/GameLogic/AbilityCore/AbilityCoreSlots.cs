// <copyright file="AbilityCoreSlots.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.AbilityCore;

/// <summary>
/// The slots of the Ability Enhancement Core (<see cref="Character.AbilityCore"/>): the extra damage slot, three skill
/// slots, three rows of five option slots (blue, green, purple) and an elite slot per row.
/// </summary>
public static class AbilityCoreSlots
{
    /// <summary>
    /// The extra damage slot.
    /// </summary>
    public const byte Extra = 0;

    /// <summary>
    /// The first skill slot.
    /// </summary>
    public const byte FirstSkill = 1;

    /// <summary>
    /// The number of skill slots.
    /// </summary>
    public const byte SkillSlots = 3;

    /// <summary>
    /// The first option slot.
    /// </summary>
    public const byte FirstOption = FirstSkill + SkillSlots;

    /// <summary>
    /// The number of option rows.
    /// </summary>
    public const byte Rows = 3;

    /// <summary>
    /// The option slots of a row, one per option card type.
    /// </summary>
    public const byte OptionsPerRow = 5;

    /// <summary>
    /// The elite slot of the first row; the other rows follow.
    /// </summary>
    public const byte FirstElite = FirstOption + (Rows * OptionsPerRow);

    /// <summary>
    /// The number of slots.
    /// </summary>
    public const byte Count = FirstElite + Rows;

    /// <summary>
    /// Gets the kind of card a slot takes.
    /// </summary>
    /// <param name="slot">The slot.</param>
    /// <returns>The kind, <see cref="AbilityCardKind.None"/> for no slot.</returns>
    public static AbilityCardKind GetKind(byte slot) => slot switch
    {
        Extra => AbilityCardKind.Extra,
        < FirstOption => AbilityCardKind.Skill,
        < FirstElite => AbilityCardKind.Option,
        < Count => AbilityCardKind.Elite,
        _ => AbilityCardKind.None,
    };

    /// <summary>
    /// Gets the row (0 to 2) of an option or elite slot.
    /// </summary>
    /// <param name="slot">The slot.</param>
    /// <returns>The row, or -1 for another slot.</returns>
    public static int GetRow(byte slot) => GetKind(slot) switch
    {
        AbilityCardKind.Option => (slot - FirstOption) / OptionsPerRow,
        AbilityCardKind.Elite => slot - FirstElite,
        _ => -1,
    };

    /// <summary>
    /// Gets the option slots of a row.
    /// </summary>
    /// <param name="row">The row.</param>
    /// <returns>The slots.</returns>
    public static IEnumerable<byte> GetOptionSlots(int row)
        => Enumerable.Range(FirstOption + (row * OptionsPerRow), OptionsPerRow).Select(s => (byte)s);
}
