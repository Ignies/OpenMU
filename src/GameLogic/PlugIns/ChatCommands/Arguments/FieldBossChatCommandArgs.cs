// <copyright file="FieldBossChatCommandArgs.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns.ChatCommands.Arguments;

/// <summary>
/// Arguments used by the <see cref="FieldBossChatCommandPlugIn"/>.
/// </summary>
public class FieldBossChatCommandArgs : ArgumentsBase
{
    /// <summary>
    /// Gets or sets the name of the boss; without one, the command lists the bosses.
    /// </summary>
    [Argument("name", false)]
    public string? Name { get; set; }
}
