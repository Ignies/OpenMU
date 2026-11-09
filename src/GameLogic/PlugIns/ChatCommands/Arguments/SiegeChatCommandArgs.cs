// <copyright file="SiegeChatCommandArgs.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns.ChatCommands.Arguments;

/// <summary>
/// The arguments of the <see cref="SiegeChatCommandPlugIn"/>.
/// </summary>
public class SiegeChatCommandArgs : ArgumentsBase
{
    /// <summary>
    /// Gets or sets the state to force, or <c>crown</c> for the crown test.
    /// </summary>
    [Argument("state")]
    [ValidValues(SiegeChatCommandPlugIn.Register, SiegeChatCommandPlugIn.Mark, SiegeChatCommandPlugIn.Notify, SiegeChatCommandPlugIn.Ready, SiegeChatCommandPlugIn.Start, SiegeChatCommandPlugIn.End, SiegeChatCommandPlugIn.Crown)]
    public string? State { get; set; }
}
