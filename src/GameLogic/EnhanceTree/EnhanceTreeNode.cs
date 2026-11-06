// <copyright file="EnhanceTreeNode.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EnhanceTree;

/// <summary>
/// A node of the skill enhancement tree of a class: a skill which takes points, as S21Emu
/// <c>Skill/EnhanceSkillTree/EnhanceSkillTree.xml</c> defines it.
/// </summary>
public class EnhanceTreeNode
{
    /// <summary>
    /// Gets or sets the base class (<see cref="Classes.BaseClassCode"/>) whose tree holds the node.
    /// </summary>
    public byte BaseClass { get; set; }

    /// <summary>
    /// Gets or sets the group: the sheet of the tree, one per main skill.
    /// </summary>
    public byte Group { get; set; }

    /// <summary>
    /// Gets or sets the index of the node on its sheet (the client's slot).
    /// </summary>
    public byte Index { get; set; }

    /// <summary>
    /// Gets or sets the number of the skill which the node teaches.
    /// </summary>
    public short Skill { get; set; }

    /// <summary>
    /// Gets or sets the column color of the node (0 main skill, 1 options, 2 debuffs).
    /// </summary>
    public byte Color { get; set; }

    /// <summary>
    /// Gets or sets the points the first step costs; the node is active from them on.
    /// </summary>
    public byte MinimumLevel { get; set; }

    /// <summary>
    /// Gets or sets the maximum points of the node.
    /// </summary>
    public byte MaximumLevel { get; set; }

    /// <summary>
    /// Gets or sets the id of the formula (S21Emu <c>Formula.txt</c>, section SkillTree) of the node's value by its level.
    /// </summary>
    public short FormulaId { get; set; }

    /// <summary>
    /// Gets or sets the skill which the node improves, <c>0</c> for none.
    /// </summary>
    public short RelatedSkill { get; set; }

    /// <summary>
    /// Gets or sets the skills the node requires.
    /// </summary>
    public List<EnhanceTreeRequirement> Requirements { get; set; } = [];
}
