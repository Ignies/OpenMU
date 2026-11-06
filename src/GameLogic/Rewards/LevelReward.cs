// <copyright file="LevelReward.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Rewards;

using MUnique.OpenMU.GameLogic.Classes;

/// <summary>
/// The reward of reaching a total level (level plus master level), Season 21's "item compensation level".
/// </summary>
public class LevelReward : RewardItem
{
    /// <summary>
    /// The highest <see cref="Id"/>: the claimed rewards are bits of a 64 bit number (<see cref="Character.ClaimedLevelRewards"/>).
    /// </summary>
    public const int MaximumId = 64;

    /// <summary>
    /// Gets or sets the id of the reward, 1 to <see cref="MaximumId"/>, which records that a character got it.
    /// It's a contract with the characters which got it: never reuse an id for another reward.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.LevelReward_Id_Name))]
    [Range(1, MaximumId)]
    public byte Id { get; set; }

    /// <summary>
    /// Gets or sets the Season 21 total level which gives the reward. The world's progression mode maps it like the
    /// evolution levels (original: the total level; normalized: level plus master level; stars: level and stars).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.LevelReward_TotalLevel_Name))]
    public int TotalLevel { get; set; }

    /// <summary>
    /// Gets or sets the base classes which get it; empty for every class.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.LevelReward_Classes_Name))]
    public List<BaseClassCode> Classes { get; set; } = [];
}
