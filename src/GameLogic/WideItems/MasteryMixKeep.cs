// <copyright file="MasteryMixKeep.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.WideItems;

/// <summary>
/// What the result of a mastery mix takes from its main material (S21Emu "OptKeepType", a sum of bits).
/// </summary>
[Flags]
public enum MasteryMixKeep
{
    /// <summary>
    /// Nothing.
    /// </summary>
    None = 0,

    /// <summary>
    /// The excellent and mastery options.
    /// </summary>
    Excellent = 1,

    /// <summary>
    /// The luck.
    /// </summary>
    Luck = 2,

    /// <summary>
    /// The set options: the mastery bonus and the ancient bonus option.
    /// </summary>
    SetOptions = 4,

    /// <summary>
    /// A new mastery option and bonus option are rolled.
    /// </summary>
    NewSetOption = 8,

    /// <summary>
    /// A new option (+4 to +16) is rolled.
    /// </summary>
    NewOption = 16,

    /// <summary>
    /// Everything: every option, the item level, the skill and the sockets.
    /// </summary>
    CopyAll = 128,
}
