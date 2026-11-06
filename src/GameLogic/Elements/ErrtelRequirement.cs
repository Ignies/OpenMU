// <copyright file="ErrtelRequirement.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Elements;

/// <summary>
/// An errtel a pentagram option needs in a socket: of at least this rank, with its option of that rank at least
/// this level (PentagramOption.xml, <c>TypeN</c>, <c>RankN</c>, <c>LevelN</c>).
/// </summary>
public class ErrtelRequirement
{
    /// <summary>
    /// Gets or sets the socket: 0 anger, 1 blessing, 2 integrity, 3 divinity, 4 radiance.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ErrtelRequirement_Socket_Name))]
    public int Socket { get; set; }

    /// <summary>
    /// Gets or sets the rank the errtel needs at least.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ErrtelRequirement_Rank_Name))]
    public int Rank { get; set; }

    /// <summary>
    /// Gets or sets the level its option of that rank needs at least.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ErrtelRequirement_Level_Name))]
    public int Level { get; set; }

    /// <inheritdoc />
    public override string ToString() => $"Socket {this.Socket}: rank {this.Rank}, level {this.Level}";
}
