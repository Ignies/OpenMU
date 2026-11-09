// <copyright file="ItemBagRuud.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.WideItems;

/// <summary>
/// The Ruud which an item bag gives when it's opened: a random amount between <see cref="Minimum"/> and <see cref="Maximum"/>.
/// </summary>
public class ItemBagRuud
{
    /// <summary>
    /// Gets or sets the item group of the bag.
    /// </summary>
    public byte Group { get; set; }

    /// <summary>
    /// Gets or sets the item number of the bag.
    /// </summary>
    public short Number { get; set; }

    /// <summary>
    /// Gets or sets the least Ruud.
    /// </summary>
    public int Minimum { get; set; }

    /// <summary>
    /// Gets or sets the most Ruud.
    /// </summary>
    public int Maximum { get; set; }
}
