// <copyright file="ElementMultiplier.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Elements;

/// <summary>
/// One cell of the element wheel: the elemental damage multiplier of an attacker element against a defender element.
/// </summary>
public class ElementMultiplier
{
    /// <summary>
    /// Gets or sets the element of the attacker.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ElementMultiplier_Attacker_Name))]
    public Element Attacker { get; set; }

    /// <summary>
    /// Gets or sets the element of the defender.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ElementMultiplier_Defender_Name))]
    public Element Defender { get; set; }

    /// <summary>
    /// Gets or sets the multiplier, e.g. 1.2 for 120 %.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ElementMultiplier_Multiplier_Name))]
    [Range(0, 10)]
    public float Multiplier { get; set; } = 1;

    /// <inheritdoc />
    public override string ToString() => $"{this.Attacker} -> {this.Defender}: {this.Multiplier:0.##}";
}
