// <copyright file="StarRequirement.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// What the next star costs and asks for.
/// </summary>
/// <param name="NextStar">The star count after the reset.</param>
/// <param name="RequiredLevel">The required normal level.</param>
/// <param name="RequiredZen">The required zen.</param>
/// <param name="RequiredItemAmount">The amount of the reset item which is consumed.</param>
/// <param name="RequiresMasterClass">If set to <c>true</c>, the character needs its third class.</param>
/// <param name="LimitReached">If set to <c>true</c>, the next star is above the star limit.</param>
public readonly record struct StarRequirement(int NextStar, int RequiredLevel, int RequiredZen, int RequiredItemAmount, bool RequiresMasterClass, bool LimitReached);
