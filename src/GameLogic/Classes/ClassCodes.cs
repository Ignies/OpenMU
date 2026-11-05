// <copyright file="ClassCodes.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Classes;

/// <summary>
/// The class code of fork worlds, <c>base &lt;&lt; 3 | evolution</c> (<see cref="BaseClassCode"/>,
/// <see cref="ClassEvolution"/>), and its relation to <see cref="CharacterClass.Number"/>.
/// </summary>
/// <remarks>
/// The number stays the database key. The Season 6 classes keep theirs (base * 4 + generation, which the Season 6
/// protocol sends in 5 bits); every other class and evolution is numbered <see cref="ForkNumberOffset"/> + code, so
/// no stored character changes and no number collides. The code is computed from the number, it isn't stored.
/// </remarks>
public static class ClassCodes
{
    /// <summary>
    /// The number of the first class which the Season 6 numbering can't express.
    /// </summary>
    public const byte ForkNumberOffset = 128;

    private const int EvolutionBits = 3;
    private const int EvolutionMask = (1 << EvolutionBits) - 1;
    private const int Season6GenerationBits = 2;
    private const byte LastSeason6Base = (byte)BaseClassCode.RageFighter;

    /// <summary>
    /// Determines whether the base class has a second evolution. Magic gladiator, dark lord, rage fighter and grow
    /// lancer go from the first evolution straight to the third.
    /// </summary>
    /// <param name="baseClass">The base class.</param>
    /// <returns><c>true</c>, if the base class has a second evolution.</returns>
    public static bool HasSecondEvolution(BaseClassCode baseClass)
        => baseClass is not (BaseClassCode.MagicGladiator or BaseClassCode.DarkLord or BaseClassCode.RageFighter or BaseClassCode.GrowLancer);

    /// <summary>
    /// Gets the class code.
    /// </summary>
    /// <param name="baseClass">The base class.</param>
    /// <param name="evolution">The evolution.</param>
    /// <returns>The class code.</returns>
    public static byte GetCode(BaseClassCode baseClass, ClassEvolution evolution) => (byte)((int)baseClass << EvolutionBits | (int)evolution);

    /// <summary>
    /// Gets the class number of a class code.
    /// </summary>
    /// <param name="code">The class code.</param>
    /// <returns>The class number, or <c>null</c> if no class has the code.</returns>
    public static byte? FromCode(byte code)
    {
        if (code >= ForkNumberOffset)
        {
            return null;
        }

        var baseClass = (BaseClassCode)(code >> EvolutionBits);
        var evolution = (ClassEvolution)(code & EvolutionMask);
        if (evolution > ClassEvolution.Fifth
            || (evolution == ClassEvolution.Second && !HasSecondEvolution(baseClass)))
        {
            return null;
        }

        if ((byte)baseClass <= LastSeason6Base && evolution <= ClassEvolution.Third)
        {
            return (byte)(((int)baseClass << Season6GenerationBits) | GetSeason6Generation(baseClass, evolution));
        }

        return (byte)(ForkNumberOffset + code);
    }

    /// <summary>
    /// Gets the class code of a class number.
    /// </summary>
    /// <param name="number">The class number.</param>
    /// <returns>The class code, or <c>null</c> if the number belongs to no class of the sixteen.</returns>
    public static byte? ToCode(byte number)
    {
        byte? code = number >= ForkNumberOffset
            ? (byte)(number - ForkNumberOffset)
            : GetSeason6Code(number);
        return code is { } value && FromCode(value) == number ? value : null;
    }

    /// <summary>
    /// Gets the base class of a class number.
    /// </summary>
    /// <param name="number">The class number.</param>
    /// <returns>The base class, or <c>null</c> if the number belongs to no class of the sixteen.</returns>
    public static BaseClassCode? GetBaseClass(byte number) => ToCode(number) is { } code ? (BaseClassCode)(code >> EvolutionBits) : null;

    /// <summary>
    /// Gets the evolution of a class number.
    /// </summary>
    /// <param name="number">The class number.</param>
    /// <returns>The evolution, or <c>null</c> if the number belongs to no class of the sixteen.</returns>
    public static ClassEvolution? GetEvolution(byte number) => ToCode(number) is { } code ? (ClassEvolution)(code & EvolutionMask) : null;

    /// <summary>
    /// Gets the number which a Season 6 client gets for a class: the fourth and fifth evolutions of the Season 6 classes
    /// show as their third, the other classes keep their number.
    /// </summary>
    /// <param name="number">The class number.</param>
    /// <returns>The number for a Season 6 client, or <c>null</c> for a class which a Season 6 client doesn't know.</returns>
    public static byte? GetSeason6Number(byte number)
    {
        if (ToCode(number) is not { } code)
        {
            return number < ForkNumberOffset ? number : null;
        }

        var baseClass = (BaseClassCode)(code >> EvolutionBits);
        if ((byte)baseClass > LastSeason6Base)
        {
            return null;
        }

        var evolution = (ClassEvolution)(code & EvolutionMask);
        return evolution > ClassEvolution.Third ? FromCode(GetCode(baseClass, ClassEvolution.Third)) : number;
    }

    /// <summary>
    /// Gets the class code of the character class.
    /// </summary>
    /// <param name="characterClass">The character class.</param>
    /// <returns>The class code, or <c>null</c> if it's no class of the sixteen.</returns>
    public static byte? GetClassCode(this CharacterClass characterClass) => ToCode(characterClass.Number);

    /// <summary>
    /// Gets the evolution of the character class.
    /// </summary>
    /// <param name="characterClass">The character class.</param>
    /// <returns>The evolution, or <c>null</c> if it's no class of the sixteen.</returns>
    public static ClassEvolution? GetEvolution(this CharacterClass characterClass) => GetEvolution(characterClass.Number);

    /// <summary>
    /// Finds the character class of a class code.
    /// </summary>
    /// <param name="gameConfiguration">The game configuration.</param>
    /// <param name="code">The class code.</param>
    /// <returns>The character class, or <c>null</c>.</returns>
    public static CharacterClass? FindByClassCode(this GameConfiguration gameConfiguration, byte code)
        => FromCode(code) is { } number ? gameConfiguration.CharacterClasses.FirstOrDefault(c => c.Number == number) : null;

    private static int GetSeason6Generation(BaseClassCode baseClass, ClassEvolution evolution) => evolution switch
    {
        ClassEvolution.First => 0,
        ClassEvolution.Second => 2,
        _ => HasSecondEvolution(baseClass) ? 3 : 1,
    };

    private static byte? GetSeason6Code(byte number)
    {
        var baseClass = (BaseClassCode)(number >> Season6GenerationBits);
        var evolution = (number & ((1 << Season6GenerationBits) - 1)) switch
        {
            0 => ClassEvolution.First,
            2 => ClassEvolution.Second,
            _ => ClassEvolution.Third,
        };
        return (byte)baseClass <= LastSeason6Base ? GetCode(baseClass, evolution) : null;
    }
}
