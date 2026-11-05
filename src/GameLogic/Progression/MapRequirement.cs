// <copyright file="MapRequirement.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// The requirement to enter a map: a normal level, a master level and a star count.
/// </summary>
/// <param name="Level">The required normal level.</param>
/// <param name="MasterLevel">The required master level.</param>
/// <param name="Stars">The required stars.</param>
public readonly record struct MapRequirement(int Level, int MasterLevel, int Stars);
