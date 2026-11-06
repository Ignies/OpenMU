// <copyright file="SoulConverterActivation.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.SoulConverter;

/// <summary>
/// The result of the activation of a converter.
/// </summary>
public enum SoulConverterActivation
{
    /// <summary>
    /// The converter is active.
    /// </summary>
    Activated,

    /// <summary>
    /// The item is no converter of the configuration.
    /// </summary>
    NotAConverter,

    /// <summary>
    /// The total level of the character is below the one of the tier.
    /// </summary>
    LevelTooLow,

    /// <summary>
    /// The character already has an active converter.
    /// </summary>
    AlreadyActive,

    /// <summary>
    /// The character activated all converters of the week.
    /// </summary>
    WeeklyLimitReached,
}
