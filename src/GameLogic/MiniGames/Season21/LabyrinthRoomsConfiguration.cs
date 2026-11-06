// <copyright file="LabyrinthRoomsConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The configuration of the rooms of the Labyrinth of Dimensions (S21Emu <c>LabyrinthOfDimension.xml</c> and <c>LabyrinthOfDimension_Path.xml</c>).
/// </summary>
public class LabyrinthRoomsConfiguration
{
    /// <summary>
    /// Gets or sets the areas of other maps which the rooms show.
    /// </summary>
    public ICollection<LabyrinthArea> Areas { get; set; } = new List<LabyrinthArea>();

    /// <summary>
    /// Gets or sets the missions of the rooms.
    /// </summary>
    public ICollection<LabyrinthMission> Missions { get; set; } = new List<LabyrinthMission>();

    /// <summary>
    /// Gets or sets the paths; a floor plays one of them, chosen by chance.
    /// </summary>
    public IList<LabyrinthPath> Paths { get; set; } = new List<LabyrinthPath>();

    /// <summary>
    /// Gets or sets the goblin, which may appear in a room the character opens (its chance, count and points).
    /// </summary>
    public EventBonusMonster? Goblin { get; set; }

    /// <summary>
    /// Gets or sets the number of open rooms of a floor from which on the goblin no longer appears.
    /// </summary>
    public int GoblinMaximumOpenRooms { get; set; } = 10;

    /// <summary>
    /// Gets or sets the numbers of the health potions (item group 14) which a mission without health potions forbids.
    /// </summary>
    public ICollection<short> HealthPotions { get; set; } = new List<short>();

    /// <summary>
    /// Gets or sets the numbers of the mana potions (item group 14) which a mission without mana potions forbids.
    /// </summary>
    public ICollection<short> ManaPotions { get; set; } = new List<short>();
}
