// <copyright file="PentagramOptionRule.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Elements;

/// <summary>
/// The errtels a pentagram option needs: the option is active while the pentagram holds all of them.
/// </summary>
public class PentagramOptionRule
{
    /// <summary>
    /// Gets or sets the option number (PentagramOption.xml <c>OptionNum</c>).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.PentagramOptionRule_Number_Name))]
    public int Number { get; set; }

    /// <summary>
    /// Gets or sets the errtels it needs.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.PentagramOptionRule_Needs_Name))]
    public List<ErrtelRequirement> Needs { get; set; } = [];

    /// <inheritdoc />
    public override string ToString() => $"Option {this.Number}: {string.Join("; ", this.Needs)}";
}
