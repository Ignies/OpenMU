// <copyright file="LabyrinthChatCommandArgs.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns.ChatCommands.Arguments;

/// <summary>
/// The arguments of the <see cref="LabyrinthChatCommandPlugIn"/>.
/// </summary>
public class LabyrinthChatCommandArgs : ArgumentsBase
{
    /// <summary>
    /// Gets or sets the floor to play; 0 for the next one.
    /// </summary>
    [Argument("floor", false)]
    [Range(0, byte.MaxValue)]
    public int Floor { get; set; }
}
