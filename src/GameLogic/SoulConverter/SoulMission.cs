// <copyright file="SoulMission.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.SoulConverter;

/// <summary>
/// A soul mission a converter of a tier may draw (S21Emu <c>MonsterSoulConversionMissionInfo.txt</c>).
/// </summary>
public class SoulMission
{
    /// <summary>
    /// Gets or sets the map of the monster.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SoulMission_MapNumber_Name))]
    public short MapNumber { get; set; }

    /// <summary>
    /// Gets or sets the monster whose souls count.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SoulMission_MonsterNumber_Name))]
    public short MonsterNumber { get; set; }

    /// <summary>
    /// Gets or sets the souls the mission needs.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SoulMission_RequiredSouls_Name))]
    public int RequiredSouls { get; set; }

    /// <summary>
    /// Gets or sets the chance of a kill to absorb a soul, in 1/1000000.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SoulMission_AbsorbRate_Name))]
    public int AbsorbRate { get; set; }

    /// <inheritdoc />
    public override string ToString() => $"{this.MonsterNumber} on {this.MapNumber}: {this.RequiredSouls}";
}
