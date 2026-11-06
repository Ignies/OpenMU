// <copyright file="MasteryMixMaterial.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.WideItems;

/// <summary>
/// A material of a <see cref="MasteryMixRecipe"/>. An item index is group * 512 + number.
/// </summary>
public class MasteryMixMaterial
{
    /// <summary>
    /// Gets or sets the lowest item index.
    /// </summary>
    public int IndexMin { get; set; }

    /// <summary>
    /// Gets or sets the highest item index.
    /// </summary>
    public int IndexMax { get; set; }

    /// <summary>
    /// Gets or sets the lowest item level.
    /// </summary>
    public int LevelMin { get; set; }

    /// <summary>
    /// Gets or sets the highest item level.
    /// </summary>
    public int LevelMax { get; set; } = byte.MaxValue;

    /// <summary>
    /// Gets or sets the lowest value of the option (+4 per option level, e.g. 16 is +16).
    /// </summary>
    public int OptionMin { get; set; }

    /// <summary>
    /// Gets or sets the highest value of the option.
    /// </summary>
    public int OptionMax { get; set; } = byte.MaxValue;

    /// <summary>
    /// Gets or sets the lowest count; each piece of a stack counts.
    /// </summary>
    public int CountMin { get; set; } = 1;

    /// <summary>
    /// Gets or sets the highest count.
    /// </summary>
    public int CountMax { get; set; } = 1;
}
