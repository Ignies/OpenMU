// <copyright file="EnhanceTreeDebuff.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EnhanceTree;

/// <summary>
/// A debuff of the skill enhancement tree, as S21Emu <c>EnhanceSkillTreeMagicField.xml</c> (MagicFieldBuffInfo) defines it.
/// </summary>
public class EnhanceTreeDebuff
{
    /// <summary>
    /// Gets or sets the kind.
    /// </summary>
    public EnhanceTreeDebuffKind Kind { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether it's the upgraded debuff of its kind.
    /// </summary>
    public bool Improved { get; set; }

    /// <summary>
    /// Gets or sets the number of the magic effect (<c>ID</c>).
    /// </summary>
    public short EffectNumber { get; set; }

    /// <summary>
    /// Gets or sets the seconds it lasts (<c>EffectTimeSec</c>).
    /// </summary>
    public int Seconds { get; set; }

    /// <summary>
    /// Gets or sets the damage counts over its seconds (<c>EffectCountBase</c>): the poison ticks, the bleeding hits.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets the success rate (<c>EffectRateBase</c>), of <see cref="EnhanceTreeConfiguration.DebuffRateScale"/>.
    /// </summary>
    public int SuccessRate { get; set; }

    /// <summary>
    /// Gets or sets the value: the poison damage (<c>PoisonDamage</c>), the defense decrease (<c>Defense</c>) or the
    /// percentage of the hit which bleeds (<c>DamagePer</c>).
    /// </summary>
    public float Value { get; set; }

    /// <summary>
    /// Gets or sets the random range added to the poison damage (<c>PoisonDamageRange</c>).
    /// </summary>
    public float ValueRange { get; set; }

    /// <summary>
    /// Gets or sets the multiplier of the basic debuff's damage (<c>PoisonMultiplier</c>, <c>DamageMultiplier</c>).
    /// </summary>
    public float Multiplier { get; set; }

    /// <summary>
    /// Gets or sets the attack speed decrease in percent (<c>AttackSpeed</c>).
    /// </summary>
    public float AttackSpeed { get; set; }
}
