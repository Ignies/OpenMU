// <copyright file="StarSmithCap.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The cap of a stat, in its unit: per item, and in total across the worn items.
/// </summary>
/// <param name="PerItem">The cap per item.</param>
/// <param name="Total">The cap in total.</param>
public sealed record StarSmithCap(double PerItem, double Total);
