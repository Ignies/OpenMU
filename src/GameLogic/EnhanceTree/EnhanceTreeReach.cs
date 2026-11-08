// <copyright file="EnhanceTreeReach.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EnhanceTree;

/// <summary>
/// What the learned nodes of the skill enhancement tree add to the shape of a skill.
/// </summary>
/// <param name="Range">The tiles added to the range of the skill (the distance to its target).</param>
/// <param name="Radius">The tiles added to the area of the skill.</param>
/// <param name="Targets">The targets added to a skill which hits a limited number of them.</param>
public readonly record struct EnhanceTreeReach(int Range, int Radius, int Targets);
