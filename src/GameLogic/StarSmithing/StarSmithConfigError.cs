// <copyright file="StarSmithConfigError.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// A refused field of the Star Smithing configuration.
/// </summary>
/// <param name="Field">The field, as its camelCase JSON path (recipes[2].outcomes[0].boosts[1].max).</param>
/// <param name="Message">Why, in English.</param>
public readonly record struct StarSmithConfigError(string Field, string Message);
