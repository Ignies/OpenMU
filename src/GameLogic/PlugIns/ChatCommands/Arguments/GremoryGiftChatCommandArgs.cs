// <copyright file="GremoryGiftChatCommandArgs.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns.ChatCommands.Arguments;

/// <summary>
/// The arguments of the <see cref="GremoryGiftChatCommandPlugIn"/>.
/// </summary>
public class GremoryGiftChatCommandArgs : ArgumentsBase
{
    /// <summary>
    /// Gets or sets the name of the character.
    /// </summary>
    [Argument("character")]
    [ValueReference(ChatCommandValueReference.CharacterName)]
    public string? CharacterName { get; set; }

    /// <summary>
    /// Gets or sets the case: 1 for the account, 2 for the character, as in Season 21.
    /// </summary>
    [Argument("case")]
    [Range(1, 2)]
    public byte Case { get; set; }

    /// <summary>
    /// Gets or sets the group of the item.
    /// </summary>
    [Argument("group")]
    public byte Group { get; set; }

    /// <summary>
    /// Gets or sets the number of the item.
    /// </summary>
    [Argument("number")]
    public short Number { get; set; }

    /// <summary>
    /// Gets or sets the level of the item.
    /// </summary>
    [Argument("lvl", false)]
    public byte Level { get; set; }
}
