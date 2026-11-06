// <copyright file="IEvolutionQuestStatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.Quest;

using MUnique.OpenMU.GameLogic.EvolutionQuests;

/// <summary>
/// Interface of a view whose implementation shows the state of the character towards its next evolution.
/// </summary>
/// <remarks>
/// The Babylon client gets C1 EE C4; the Season 6 client has no such view.
/// </remarks>
public interface IEvolutionQuestStatePlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows the state.
    /// </summary>
    /// <param name="state">The state.</param>
    ValueTask ShowEvolutionQuestStateAsync(EvolutionQuestState state);
}
