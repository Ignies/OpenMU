// <copyright file="MuunEnergyPoints.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Muuns;

/// <summary>
/// The energy which a Muun of a rank or a level gives to a Muun energy converter (MuunExchange.txt, section 1).
/// </summary>
public class MuunEnergyPoints
{
    /// <summary>
    /// Gets or sets the rank or the level.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunEnergyPoints_Key_Name))]
    public int Key { get; set; }

    /// <summary>
    /// Gets or sets the energy.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunEnergyPoints_Points_Name))]
    public int Points { get; set; }
}
