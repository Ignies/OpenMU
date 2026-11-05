// <copyright file="GoldenSet.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Progression;

/// <summary>
/// A Season 21 map set row of the progression design.
/// </summary>
/// <param name="Id">The set id.</param>
/// <param name="Requirement">The Season 21 entry level R.</param>
/// <param name="PowerLevel">T(R), to one decimal.</param>
/// <param name="Level">The normalized level.</param>
/// <param name="MasterLevel">The normalized master level.</param>
/// <param name="Stars">The stars in the stars mode.</param>
/// <param name="StarLevel">The level in the stars mode.</param>
/// <param name="Zen">The normalized warp zen.</param>
/// <param name="AnchorLevel">The anchor monster level M, rounded.</param>
public sealed record GoldenSet(string Id, int Requirement, double PowerLevel, int Level, int MasterLevel, int Stars, int StarLevel, int Zen, int AnchorLevel);
