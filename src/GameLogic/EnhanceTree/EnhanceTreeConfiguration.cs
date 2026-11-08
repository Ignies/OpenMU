// <copyright file="EnhanceTreeConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EnhanceTree;

using MUnique.OpenMU.GameLogic.Classes;

/// <summary>
/// Configuration of the <see cref="EnhanceTreeFeaturePlugIn"/>: the skill enhancement tree of the fourth evolution.
/// </summary>
/// <remarks>
/// The update "Add the skills of Season 21" fills the nodes from the S21Emu data. The tree's points are computed from the
/// level, see <see cref="EnhanceTreePoints"/>; nothing about them is stored.
/// </remarks>
public class EnhanceTreeConfiguration
{
    /// <summary>
    /// Gets or sets the Season 21 total level (level plus master level) above which each level gives one point.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EnhanceTreeConfiguration_PointsStartAt_Name))]
    [Range(0, 2000)]
    public int PointsStartAt { get; set; } = 800;

    /// <summary>
    /// Gets or sets the most points a character gets (S21Emu <c>EnhanceSkillTreeMaxLevel</c>).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EnhanceTreeConfiguration_MaximumPoints_Name))]
    [Range(0, 5000)]
    public int MaximumPoints { get; set; } = 950;

    /// <summary>
    /// Gets or sets the nodes of every class.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EnhanceTreeConfiguration_Nodes_Name))]
    public List<EnhanceTreeNode> Nodes { get; set; } = [];

    /// <summary>
    /// Gets or sets the passives of every class.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EnhanceTreeConfiguration_Passives_Name))]
    public List<EnhanceTreePassive> Passives { get; set; } = [];

    /// <summary>
    /// Gets or sets the debuff fields.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EnhanceTreeConfiguration_MagicFields_Name))]
    public List<EnhanceTreeMagicField> MagicFields { get; set; } = [];

    /// <summary>
    /// Gets or sets the debuffs which the circles place (S21Emu <c>EnhanceSkillTreeMagicField.xml</c>, MagicFieldBuffInfo).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EnhanceTreeConfiguration_Debuffs_Name))]
    public List<EnhanceTreeDebuff> Debuffs { get; set; } = EnhanceTreeDebuffs.CreateDefaultDebuffs();

    /// <summary>
    /// Gets or sets the targets a debuff circle takes before the target nodes add to them (no source: one).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EnhanceTreeConfiguration_DebuffCircleTargets_Name))]
    [Range(0, 100)]
    public int DebuffCircleTargets { get; set; } = 1;

    /// <summary>
    /// Gets or sets the radius in tiles of a debuff circle before the range nodes add to it (no source: one).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EnhanceTreeConfiguration_DebuffCircleRange_Name))]
    [Range(0, 20)]
    public int DebuffCircleRange { get; set; } = 1;

    /// <summary>
    /// Gets or sets the scale of the debuff success rates (<see cref="EnhanceTreeDebuff.SuccessRate"/> and the chance
    /// nodes): a rate of this value always succeeds (no source: per mille, so the base 200 is 20 %).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EnhanceTreeConfiguration_DebuffRateScale_Name))]
    [Range(1, 1000000)]
    public int DebuffRateScale { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the movement slow of Chilling in percent; Frostbite adds its improved value nodes (no source has the
    /// amount: 50, like OpenMU's iced slow).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EnhanceTreeConfiguration_ChillMovementSlow_Name))]
    [Range(0, 100)]
    public int ChillMovementSlow { get; set; } = 50;

    /// <summary>
    /// Gets or sets the damage a Mastery DMG hit adds in percent of the hit. The node "Add Ext Skill Extra Damage Rate"
    /// (1007) gives its chance; no source has the amount, so it is 0 by default.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EnhanceTreeConfiguration_MasteryDamagePercent_Name))]
    [Range(0, 1000)]
    public int MasteryDamagePercent { get; set; }

    /// <summary>
    /// Gets or sets the explosion skills (muonlinefanz.com tree tool, sheet "Debuff").
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EnhanceTreeConfiguration_Explosions_Name))]
    public List<EnhanceTreeExplosion> Explosions { get; set; } = EnhanceTreeExplosions.CreateDefaultExplosions();

    /// <summary>
    /// Gets or sets the value formulas by their id (S21Emu <c>Formula.txt</c>, section SkillTree), in the syntax of
    /// <see cref="MasterSkillDefinition.ValueFormula"/>.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EnhanceTreeConfiguration_Formulas_Name))]
    public Dictionary<short, string> Formulas { get; set; } = [];

    /// <summary>
    /// Gets the node of a class's tree which teaches a skill.
    /// </summary>
    /// <param name="baseClass">The base class.</param>
    /// <param name="skill">The skill number.</param>
    /// <returns>The node, or <c>null</c>.</returns>
    public EnhanceTreeNode? FindNode(BaseClassCode baseClass, short skill)
        => this.Nodes.FirstOrDefault(n => n.BaseClass == (byte)baseClass && n.Skill == skill);

    /// <summary>
    /// Gets the passive of a class's tree for a skill.
    /// </summary>
    /// <param name="baseClass">The base class.</param>
    /// <param name="skill">The skill number.</param>
    /// <returns>The passive, or <c>null</c>.</returns>
    public EnhanceTreePassive? FindPassive(BaseClassCode baseClass, short skill)
        => this.Passives.FirstOrDefault(p => p.BaseClass == (byte)baseClass && p.Skill == skill);

    /// <summary>
    /// Gets the skills of a class's tree, nodes and passives.
    /// </summary>
    /// <param name="baseClass">The base class.</param>
    /// <returns>The skill numbers.</returns>
    public HashSet<short> GetTreeSkills(BaseClassCode baseClass)
        => this.Nodes.Where(n => n.BaseClass == (byte)baseClass).Select(n => n.Skill)
            .Concat(this.Passives.Where(p => p.BaseClass == (byte)baseClass).Select(p => p.Skill))
            .ToHashSet();
}
