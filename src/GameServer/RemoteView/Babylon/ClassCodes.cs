// <copyright file="ClassCodes.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.Babylon;

/// <summary>
/// The one byte class code of the Babylon packets: <c>base &lt;&lt; 3 | evolution</c> (docs/babylon.md, "Class code").
/// </summary>
/// <remarks>
/// The Season 6 packets carry the class in 5 bits (base &lt;&lt; 2 | generation), which leaves no room for the
/// classes and evolutions of later seasons. The bases are numbered as in Season 21 (0 Dark Wizard .. 15 Crusader;
/// the first seven are the Season 6 ones in the same order), the evolutions 0 to 4 are the 1st to 5th class.
/// The Season 6 classes keep their <see cref="DataModel.Configuration.CharacterClass.Number"/>; every other class
/// and evolution has the number <see cref="NewClassNumberOffset"/> + its code, so stored characters never change.
/// Magic Gladiator, Dark Lord and Rage Fighter have no 2nd class, so their master class is the 3rd evolution.
/// </remarks>
public static class ClassCodes
{
    /// <summary>
    /// The feature id (<see cref="GameLogic.ForkFeatures"/>) of the class codes.
    /// </summary>
    public const string FeatureId = "class_codes";

    /// <summary>
    /// The class number of the class code 0; the numbers below are the Season 6 ones.
    /// </summary>
    public const byte NewClassNumberOffset = 128;

    private const int Season6NumberLimit = 28;

    private const int EvolutionBits = 3;

    private const int EvolutionMask = (1 << EvolutionBits) - 1;

    private const int MaximumCode = NewClassNumberOffset - 1;

    private const int MagicGladiatorBase = 3;

    private const int DarkLordBase = 4;

    private const int RageFighterBase = 6;

    private const int Season6BaseLimit = 7;

    private const int MasterEvolution = 2;

    /// <summary>
    /// Gets the class code of a class number.
    /// </summary>
    /// <param name="classNumber">The <see cref="DataModel.Configuration.CharacterClass.Number"/>.</param>
    /// <returns>The class code.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The number belongs to no class.</exception>
    public static byte ToCode(byte classNumber)
    {
        if (classNumber >= NewClassNumberOffset)
        {
            return (byte)(classNumber - NewClassNumberOffset);
        }

        if (classNumber >= Season6NumberLimit)
        {
            throw new ArgumentOutOfRangeException(nameof(classNumber), classNumber, "The number belongs to no class.");
        }

        var evolution = (classNumber & 0b11) switch
        {
            0 => 0,
            2 => 1,
            _ => MasterEvolution,
        };

        return (byte)(((classNumber >> 2) << EvolutionBits) | evolution);
    }

    /// <summary>
    /// Gets the class number of a class code.
    /// </summary>
    /// <param name="classCode">The class code.</param>
    /// <returns>The <see cref="DataModel.Configuration.CharacterClass.Number"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The code has the reserved bit 7 set.</exception>
    public static byte FromCode(byte classCode)
    {
        if (classCode > MaximumCode)
        {
            throw new ArgumentOutOfRangeException(nameof(classCode), classCode, "Bit 7 of a class code is reserved.");
        }

        var baseClass = classCode >> EvolutionBits;
        var evolution = classCode & EvolutionMask;
        var skipsSecond = baseClass is MagicGladiatorBase or DarkLordBase or RageFighterBase;
        int? generation = (evolution, skipsSecond) switch
        {
            (0, _) => 0,
            (1, false) => 2,
            (MasterEvolution, false) => 3,
            (MasterEvolution, true) => 1,
            _ => null,
        };

        return baseClass < Season6BaseLimit && generation is { } g
            ? (byte)((baseClass << 2) | g)
            : (byte)(NewClassNumberOffset + classCode);
    }
}
