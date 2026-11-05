// <copyright file="ClassEvolution.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Classes;

/// <summary>
/// The evolution of a class, the low bits of a class code, see <see cref="ClassCodes"/>.
/// </summary>
public enum ClassEvolution : byte
{
    /// <summary>
    /// The first evolution, the class as it gets created.
    /// </summary>
    First = 0,

    /// <summary>
    /// The second evolution. Magic gladiator, dark lord, rage fighter and grow lancer skip it.
    /// </summary>
    Second = 1,

    /// <summary>
    /// The third evolution, the master class.
    /// </summary>
    Third = 2,

    /// <summary>
    /// The fourth evolution, at a Season 21 total level of 800.
    /// </summary>
    Fourth = 3,

    /// <summary>
    /// The fifth evolution, at a Season 21 total level of 1200.
    /// </summary>
    Fifth = 4,
}
