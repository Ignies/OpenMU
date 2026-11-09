// <copyright file="ItemChatCommandArgs.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns.ChatCommands.Arguments;

/// <summary>
/// Arguments used by ItemChatCommandPlugIn.
/// </summary>
public class ItemChatCommandArgs : ArgumentsBase
{
    /// <summary>
    /// Gets or sets the group.
    /// </summary>
    [Argument("group")]
    [ValueReference(ChatCommandValueReference.ItemGroup)]
    public byte Group { get; set; }

    /// <summary>
    /// Gets or sets the number.
    /// </summary>
    [Argument("number")]
    [ValueReference(ChatCommandValueReference.ItemNumber, GroupWith = nameof(Group))]
    public short Number { get; set; }

    /// <summary>
    /// Gets or sets the level.
    /// </summary>
    [Argument("lvl", false)]
    public byte Level { get; set; }

    /// <summary>
    /// Gets or sets the excellent number.
    /// </summary>
    [Argument("ex", false)]
    public byte ExcellentNumber { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the item contains skill.
    /// </summary>
    [Argument("sk", false)]
    public bool Skill { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the item contains luck.
    /// </summary>
    [Argument("lu", false)]
    public bool Luck { get; set; }

    /// <summary>
    /// Gets or sets the option.
    /// </summary>
    [Argument("opt", false)]
    public byte Opt { get; set; }

    /// <summary>
    /// Gets or sets the ancient set discriminator.
    /// When 0, it's not an ancient.
    /// When 1, the first ancient type of an item is applied; When 2, the second, if available.
    /// Example for a Dragon Set item: 1 will be Hyon, 2 will be Vicious..
    /// </summary>
    [Argument("anc", false)]
    [ValidValues("0", "1", "2")]
    public byte Ancient { get; set; }

    /// <summary>
    /// Gets or sets the ancient bonus option; Should be 1 or 2. Only applies, when <see cref="Ancient"/> is bigger than 0.
    /// </summary>
    [Argument("ancBonuslvl", false)]
    [ValidValues("1", "2")]
    public byte AncientBonusLevel { get; set; } = 1;

    /// <summary>
    /// Gets or sets the number of sockets; the default is the maximum of the item.
    /// </summary>
    [Argument("sck", false)]
    public byte Sockets { get; set; } = byte.MaxValue;

    /// <summary>
    /// Gets or sets the seeds of the sockets in order: <c>index[:level]</c> separated by commas, the index over all
    /// elements as the item data carries it (0-49), the sphere level 1-10 (default 1).
    /// </summary>
    [Argument("seeds", false)]
    public string? Seeds { get; set; }

    /// <summary>
    /// Gets or sets the errtels in the sockets of a pentagram: <c>socket:ranks[:level]</c> separated by commas, the socket
    /// 0 anger to 4 radiance, 1-5 ranks, the level 0-10 (default 0).
    /// </summary>
    [Argument("errtels", false)]
    public string? Errtels { get; set; }

    /// <summary>
    /// Gets or sets more options of the item: <c>kind:number[:level]</c> separated by commas; the kinds are
    /// <c>mastery</c>, <c>bonus</c> (mastery bonus), <c>element</c>, <c>grade</c> (wing grade) and <c>earring</c>.
    /// </summary>
    [Argument("opts", false)]
    public string? Options { get; set; }

    /// <summary>
    /// Gets or sets the days of the additional option of a Muun; 0 for none.
    /// </summary>
    [Argument("muun", false)]
    public byte MuunOptionDays { get; set; }
}