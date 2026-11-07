// <copyright file="AbilityCardOption.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.AbilityCore;

using MUnique.OpenMU.GameLogic.Classes;

/// <summary>
/// An option which an ability card can carry. The card carries it as an item option of the type "Ability Card Option"
/// with the same number; that item option holds the values by card level.
/// </summary>
public class AbilityCardOption
{
    /// <summary>
    /// Gets or sets the number: the static option index of S21Emu <c>AbilityOption.xml</c> (<c>OptionIndex</c>).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCardOption_Number_Name))]
    public short Number { get; set; }

    /// <summary>
    /// Gets or sets the base classes for which the option takes effect, a bit per <see cref="BaseClassCode"/>
    /// (S21Emu <c>AbilityParticleBook.xml</c>).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCardOption_BaseClasses_Name))]
    public int BaseClasses { get; set; } = 0xFFFF;

    /// <summary>
    /// Gets or sets the chance in millionths to get this option when a skill card gets its option (S21Emu
    /// <c>AbilityAddOption.xml</c>); 0 for an even chance.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCardOption_Chance_Name))]
    public int Chance { get; set; }

    /// <summary>
    /// Determines whether the option takes effect for a base class.
    /// </summary>
    /// <param name="baseClass">The base class.</param>
    /// <returns><c>true</c>, if it takes effect.</returns>
    public bool AppliesTo(BaseClassCode baseClass) => (this.BaseClasses & (1 << (int)baseClass)) != 0;
}
