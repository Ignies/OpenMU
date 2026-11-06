// <copyright file="EnhanceTreePassive.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EnhanceTree;

/// <summary>
/// A passive of the skill enhancement tree, unlocked in stages by the points spent in the tree, as S21Emu
/// <c>Skill/EnhanceSkillTree/EnhanceSkillTreePassive.xml</c> defines it.
/// </summary>
public class EnhanceTreePassive
{
    /// <summary>
    /// Gets or sets the base class (<see cref="Classes.BaseClassCode"/>).
    /// </summary>
    public byte BaseClass { get; set; }

    /// <summary>
    /// Gets or sets the group.
    /// </summary>
    public byte Group { get; set; }

    /// <summary>
    /// Gets or sets the index in the group.
    /// </summary>
    public byte Index { get; set; }

    /// <summary>
    /// Gets or sets the number of the skill.
    /// </summary>
    public short Skill { get; set; }

    /// <summary>
    /// Gets or sets the stages, in their order.
    /// </summary>
    public List<EnhanceTreePassiveStage> Stages { get; set; } = [];
}
