// <copyright file="GremoryCaseConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.GremoryCase;

/// <summary>
/// The configuration of the <see cref="GremoryCaseFeaturePlugIn"/>.
/// </summary>
public class GremoryCaseConfiguration
{
    /// <summary>
    /// Gets or sets the number of rewards the case of a character holds.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.GremoryCaseConfiguration_CharacterCapacity_Name))]
    [Range(1, byte.MaxValue)]
    public int CharacterCapacity { get; set; } = 50;

    /// <summary>
    /// Gets or sets the number of rewards the case of an account holds.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.GremoryCaseConfiguration_AccountCapacity_Name))]
    [Range(1, byte.MaxValue)]
    public int AccountCapacity { get; set; } = 50;

    /// <summary>
    /// Gets or sets the time in which a reward has to be claimed before it expires.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.GremoryCaseConfiguration_Duration_Name))]
    public TimeSpan Duration { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    /// Gets or sets a value indicating whether the item rewards of the mini games (blood castle, devil square, ...) go into the case.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.GremoryCaseConfiguration_MiniGameRewards_Name))]
    public bool MiniGameRewards { get; set; } = true;
}
