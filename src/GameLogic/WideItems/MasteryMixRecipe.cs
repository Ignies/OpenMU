// <copyright file="MasteryMixRecipe.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.WideItems;

/// <summary>
/// A mastery item mix: the materials in the chaos machine and the item they give.
/// </summary>
public class MasteryMixRecipe
{
    /// <summary>
    /// Gets or sets the id of the mix in the data.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the success rate in percent.
    /// </summary>
    public byte SuccessRate { get; set; }

    /// <summary>
    /// Gets or sets the price in zen.
    /// </summary>
    public int Money { get; set; }

    /// <summary>
    /// Gets or sets what the result takes from the main material, the first one.
    /// </summary>
    public MasteryMixKeep Keep { get; set; }

    /// <summary>
    /// Gets or sets the materials; the first one is the main material.
    /// </summary>
    public List<MasteryMixMaterial> Materials { get; set; } = [];

    /// <summary>
    /// Gets or sets the item index of the result: group * 512 + number.
    /// </summary>
    public int ResultIndex { get; set; }

    /// <summary>
    /// Gets or sets the item level of the result.
    /// </summary>
    public byte ResultLevel { get; set; }

    /// <summary>
    /// Gets or sets the weights of 1, 2, ... sockets of the result, which the mix rolls; empty = the sockets of the main item.
    /// </summary>
    public List<int> SocketWeights { get; set; } = [];

    /// <summary>
    /// Gets or sets the chance in percent that the result has its skill; <c>null</c> = the skill of the main item or, when the
    /// options aren't copied, the skill of the definition.
    /// </summary>
    public byte? SkillPercent { get; set; }

    /// <summary>
    /// Gets or sets the chance in percent that the result gets the luck option.
    /// </summary>
    public byte LuckPercent { get; set; }
}
