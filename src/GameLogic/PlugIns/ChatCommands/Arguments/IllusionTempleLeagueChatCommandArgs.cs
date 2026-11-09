// <copyright file="IllusionTempleLeagueChatCommandArgs.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns.ChatCommands.Arguments;

/// <summary>
/// The arguments of the <see cref="IllusionTempleLeagueChatCommandPlugIn"/>.
/// </summary>
public class IllusionTempleLeagueChatCommandArgs : ArgumentsBase
{
    /// <summary>
    /// Gets or sets the stage to force, or <c>off</c> to give the stages back to the days.
    /// </summary>
    [Argument("stage")]
    [ValidValues(IllusionTempleLeagueChatCommandPlugIn.Elimination, IllusionTempleLeagueChatCommandPlugIn.SemiFinal, IllusionTempleLeagueChatCommandPlugIn.Final, IllusionTempleLeagueChatCommandPlugIn.Reward, IllusionTempleLeagueChatCommandPlugIn.Off)]
    public string? Stage { get; set; }
}
