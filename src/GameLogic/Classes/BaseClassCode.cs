// <copyright file="BaseClassCode.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Classes;

/// <summary>
/// The base classes in the order of Season 21 (<c>Character/DefaultClassInfo.txt</c> of the S21Emu data), the high
/// bits of a class code, see <see cref="ClassCodes"/>.
/// </summary>
public enum BaseClassCode : byte
{
    /// <summary>
    /// The dark wizard.
    /// </summary>
    DarkWizard = 0,

    /// <summary>
    /// The dark knight.
    /// </summary>
    DarkKnight = 1,

    /// <summary>
    /// The fairy elf.
    /// </summary>
    FairyElf = 2,

    /// <summary>
    /// The magic gladiator.
    /// </summary>
    MagicGladiator = 3,

    /// <summary>
    /// The dark lord.
    /// </summary>
    DarkLord = 4,

    /// <summary>
    /// The summoner.
    /// </summary>
    Summoner = 5,

    /// <summary>
    /// The rage fighter.
    /// </summary>
    RageFighter = 6,

    /// <summary>
    /// The grow lancer.
    /// </summary>
    GrowLancer = 7,

    /// <summary>
    /// The rune wizard.
    /// </summary>
    RuneWizard = 8,

    /// <summary>
    /// The slayer.
    /// </summary>
    Slayer = 9,

    /// <summary>
    /// The gun crusher.
    /// </summary>
    GunCrusher = 10,

    /// <summary>
    /// The light wizard (white wizard, "Kundum Mephis" in the S21Emu data).
    /// </summary>
    LightWizard = 11,

    /// <summary>
    /// The lemuria mage.
    /// </summary>
    LemuriaMage = 12,

    /// <summary>
    /// The illusion knight.
    /// </summary>
    IllusionKnight = 13,

    /// <summary>
    /// The alchemist.
    /// </summary>
    Alchemist = 14,

    /// <summary>
    /// The crusader.
    /// </summary>
    Crusader = 15,
}
