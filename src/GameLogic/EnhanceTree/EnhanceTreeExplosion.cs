// <copyright file="EnhanceTreeExplosion.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EnhanceTree;

/// <summary>
/// An explosion skill of the skill enhancement tree (Poison Storm, Frozen Slayer, Bloodying Hit): it explodes the upgraded
/// debuff of its kind on its target, see <see cref="EnhanceTreeExplosions"/>.
/// </summary>
public class EnhanceTreeExplosion
{
    /// <summary>
    /// Gets or sets the number of the explosion skill.
    /// </summary>
    public short Skill { get; set; }

    /// <summary>
    /// Gets or sets the kind of the debuff it explodes (its upgraded debuff).
    /// </summary>
    public EnhanceTreeDebuffKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the radius in tiles around the target which the explosion reaches (no source).
    /// </summary>
    public int Radius { get; set; }

    /// <summary>
    /// Gets or sets the value of the explosion before its effect node adds to it: the targets around which get the basic
    /// debuff (poison), the percentage of the skill damage which hits them (chill) or the defense decrease in percent of the
    /// target (bleed).
    /// </summary>
    public float Value { get; set; }

    /// <summary>
    /// Gets or sets the seconds of the defense decrease of a bleed explosion (no source).
    /// </summary>
    public int Seconds { get; set; }

    /// <summary>
    /// Gets or sets the number of the magic effect of the defense decrease of a bleed explosion.
    /// </summary>
    public short EffectNumber { get; set; }
}
