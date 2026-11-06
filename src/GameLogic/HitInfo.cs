// <copyright file="HitInfo.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic;

/// <summary>
/// The information about a hit.
/// </summary>
public record struct HitInfo(
    uint HealthDamage,
    uint ShieldDamage,
    DamageAttributes Attributes,
    uint ManaToll = 0,
    uint PetDamage = 0,
    SkillEntry? SkillEntry = null,
    bool? TriggersHitShock = false)
{
    /// <summary>
    /// Gets the total damage.
    /// </summary>
    public readonly uint TotalDamage => this.HealthDamage + this.ShieldDamage;

    /// <summary>
    /// Gets the part of the <see cref="TotalDamage"/> which is elemental damage (see <see cref="Elements.ElementalDamageCalculator"/>).
    /// </summary>
    public uint ElementalDamage { get; init; }

    /// <summary>
    /// Gets the element of the <see cref="ElementalDamage"/>.
    /// </summary>
    public Elements.Element Element { get; init; }
}