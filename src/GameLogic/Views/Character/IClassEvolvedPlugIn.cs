// <copyright file="IClassEvolvedPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.Character;

/// <summary>
/// Interface of a view whose implementation shows that a character evolved into its next class.
/// </summary>
/// <remarks>
/// The Babylon client gets C1 EE 13; the Season 6 client shows a fourth and fifth evolution as the third one.
/// </remarks>
public interface IClassEvolvedPlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows that the player evolved.
    /// </summary>
    /// <param name="evolvedPlayer">The player which evolved.</param>
    /// <param name="reason">The reason of the evolution.</param>
    ValueTask ClassEvolvedAsync(Player evolvedPlayer, ClassEvolutionReason reason);
}
