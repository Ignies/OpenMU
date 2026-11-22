// <copyright file="StarSmithGuidebookEntry.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// A found recipe of the smith's Guidebook (EE 90).
/// </summary>
/// <param name="Recipe">The recipe's number.</param>
/// <param name="Made">How often the smith made it.</param>
/// <param name="Name">The recipe's name.</param>
public readonly record struct StarSmithGuidebookEntry(short Recipe, int Made, string Name);
