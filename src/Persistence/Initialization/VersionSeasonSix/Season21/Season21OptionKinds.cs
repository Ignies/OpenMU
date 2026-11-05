// <copyright file="Season21OptionKinds.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

/// <summary>
/// The options of its template which an item of Season 21 takes, when it takes options at all.
/// </summary>
[Flags]
internal enum Season21OptionKinds
{
    /// <summary>
    /// None of luck, the option and the excellent options; the other options of the template stay.
    /// </summary>
    None = 0,

    /// <summary>
    /// The luck.
    /// </summary>
    Luck = 1,

    /// <summary>
    /// The option.
    /// </summary>
    Option = 2,

    /// <summary>
    /// The excellent options.
    /// </summary>
    Excellent = 4,

    /// <summary>
    /// Luck, the option and the excellent options.
    /// </summary>
    All = Luck | Option | Excellent,
}
