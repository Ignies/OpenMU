// <copyright file="SetRuudChatCommandArgs.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns.ChatCommands.Arguments;

/// <summary>
/// The arguments of the <see cref="SetRuudChatCommandPlugIn"/>.
/// </summary>
public class SetRuudChatCommandArgs : ArgumentsBase
{
    /// <summary>
    /// Gets or sets the name of the character.
    /// </summary>
    [Argument("character")]
    [ValueReference(ChatCommandValueReference.CharacterName)]
    public string? CharacterName { get; set; }

    /// <summary>
    /// Gets or sets the amount of Ruud.
    /// </summary>
    [Argument("amount")]
    [Range(0, int.MaxValue)]
    public int Amount { get; set; }
}
