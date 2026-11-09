// <copyright file="IGuideQuestViewPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.Quest;

using MUnique.OpenMU.GameLogic.GuideQuests;

/// <summary>
/// Interface of a view whose implementation shows the guide quest step of the character.
/// </summary>
/// <remarks>
/// The Babylon client gets C1 EE C0; the Season 6 client has no such view.
/// </remarks>
public interface IGuideQuestViewPlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows the guide quest step.
    /// </summary>
    /// <param name="view">What to show.</param>
    /// <param name="result">The answer to the request which caused it, if any.</param>
    ValueTask ShowGuideQuestAsync(GuideQuestView view, GuideQuestResult result);
}
