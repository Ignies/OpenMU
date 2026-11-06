// <copyright file="Season21SkillRow.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

/// <summary>
/// A row of S21Emu <c>Skill/Skill.xml</c>.
/// </summary>
internal sealed class Season21SkillRow
{
    /// <summary>
    /// Gets the skill number.
    /// </summary>
    public short Number { get; init; }

    /// <summary>
    /// Gets the damage, or the formula id of a master node (use types 3 and 4).
    /// </summary>
    public int Damage { get; init; }

    /// <summary>
    /// Gets the mana it uses.
    /// </summary>
    public int Mana { get; init; }

    /// <summary>
    /// Gets the ability (AG) it uses.
    /// </summary>
    public int Ability { get; init; }

    /// <summary>
    /// Gets the range.
    /// </summary>
    public short Range { get; init; }

    /// <summary>
    /// Gets the radius of the area around the target; <c>0</c> for a single target.
    /// </summary>
    public short Radius { get; init; }

    /// <summary>
    /// Gets the delay between two uses in milliseconds.
    /// </summary>
    public int Delay { get; init; }

    /// <summary>
    /// Gets the number of the effect it applies (<c>Effect.txt</c>), <c>0</c> for none.
    /// </summary>
    public short Effect { get; init; }

    /// <summary>
    /// Gets the required level.
    /// </summary>
    public int Level { get; init; }

    /// <summary>
    /// Gets the required energy.
    /// </summary>
    public int Energy { get; init; }

    /// <summary>
    /// Gets the required command.
    /// </summary>
    public int Leadership { get; init; }

    /// <summary>
    /// Gets the required kill count (castle siege).
    /// </summary>
    public int KillCount { get; init; }

    /// <summary>
    /// Gets the stat its damage is specialized by: 1 strength, 2 agility, 3 vitality, 4 energy, 5 command, -1 none.
    /// </summary>
    public int DivisionStat { get; init; }

    /// <summary>
    /// Gets the kind (<c>SkillDamageType</c>): 0 attack, 1 buff, 2 debuff, 3 effect removal, 4 summon, 5 movement, 6 heal, 7 attack.
    /// </summary>
    public int Kind { get; init; }

    /// <summary>
    /// Gets the use type (<c>SkillUseType</c>), see Readme.md.
    /// </summary>
    public int Use { get; init; }

    /// <summary>
    /// Gets per base class (Season 21 order) the first evolution which may use it: 1 to 5, 0 none.
    /// </summary>
    public int[] Classes { get; init; } = [];
}
