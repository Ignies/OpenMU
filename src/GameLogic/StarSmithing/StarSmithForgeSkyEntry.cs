// <copyright file="StarSmithForgeSkyEntry.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// One entry of the sky the client gets (EE 9D): a star, a link or a hint.
/// </summary>
/// <param name="Kind">0 a star, 1 a link, 2 a hint.</param>
/// <param name="A">A star: its index; a link or hint: one star.</param>
/// <param name="B">A star: X, 0 to 255; a link or hint: the other star.</param>
/// <param name="C">A star: Y, 0 to 255; a hint: its clarity, 0 to 255; a link: 0.</param>
/// <param name="D">A star: 1 when it is a void a Jewel of Guardian reveals; else 0.</param>
public readonly record struct StarSmithForgeSkyEntry(byte Kind, byte A, byte B, byte C, byte D);
