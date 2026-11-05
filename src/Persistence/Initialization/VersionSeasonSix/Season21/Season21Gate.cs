// <copyright file="Season21Gate.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

/// <summary>
/// A gate of a <see cref="Season21MapSet"/>, its coordinates are inclusive.
/// </summary>
/// <param name="Number">The number of the gate.</param>
/// <param name="Kind">The kind of the gate.</param>
/// <param name="Map">The number of the map of the gate.</param>
/// <param name="X1">The first x coordinate.</param>
/// <param name="Y1">The first y coordinate.</param>
/// <param name="X2">The last x coordinate.</param>
/// <param name="Y2">The last y coordinate.</param>
/// <param name="Target">The number of the gate which an entrance leads to.</param>
/// <param name="Direction">The direction a character faces when it arrives at a target gate.</param>
internal sealed record Season21Gate(short Number, Season21GateKind Kind, byte Map, byte X1, byte Y1, byte X2, byte Y2, short Target = 0, Direction Direction = Direction.Undefined);
