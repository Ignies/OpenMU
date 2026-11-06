// <copyright file="HelperPlusZone.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.HelperPlus;

/// <summary>
/// A hunting zone of the helper plus (S21Emu <c>Character/MuHelperPlus.xml</c>, <c>HuntingZone/Info</c>).
/// </summary>
public class HelperPlusZone
{
    /// <summary>
    /// Gets or sets the index of the zone, which the client asks for.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HelperPlusZone_Index_Name))]
    public short Index { get; set; }

    /// <summary>
    /// Gets or sets the index of the hunting ground of the zone.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HelperPlusZone_Ground_Name))]
    public short Ground { get; set; }

    /// <summary>
    /// Gets or sets the map.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HelperPlusZone_MapNumber_Name))]
    public short MapNumber { get; set; }

    /// <summary>
    /// Gets or sets the element of the monsters of the zone (1 fire, 2 water, 3 earth, 4 wind, 5 darkness).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HelperPlusZone_Element_Name))]
    public byte Element { get; set; }

    /// <summary>
    /// Gets or sets the x coordinate at which the character starts to hunt.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HelperPlusZone_X_Name))]
    public byte X { get; set; }

    /// <summary>
    /// Gets or sets the y coordinate at which the character starts to hunt.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HelperPlusZone_Y_Name))]
    public byte Y { get; set; }

    /// <inheritdoc />
    public override string ToString() => $"{this.Index}: map {this.MapNumber} ({this.X}, {this.Y})";
}
