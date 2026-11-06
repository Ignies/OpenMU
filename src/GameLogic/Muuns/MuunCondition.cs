// <copyright file="MuunCondition.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Muuns;

/// <summary>
/// The condition under which an option of a Muun applies.
/// </summary>
public class MuunCondition
{
    /// <summary>
    /// The bit mask which means every day of the week.
    /// </summary>
    internal const int EveryDay = -1;

    private const int MinutesPerHour = 60;

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunCondition_Type_Name))]
    public MuunConditionType Type { get; set; }

    /// <summary>
    /// Gets or sets the first value: the map, the lowest level, the first hour or the days of the week.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunCondition_Value1_Name))]
    public int Value1 { get; set; }

    /// <summary>
    /// Gets or sets the second value: the highest level or the last hour; -1 for none.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunCondition_Value2_Name))]
    public int Value2 { get; set; } = -1;

    /// <summary>
    /// Determines whether the condition is met.
    /// </summary>
    /// <param name="mapNumber">The number of the map of the character.</param>
    /// <param name="totalLevel">The total level of the character (level and master level).</param>
    /// <param name="utcNow">The current point in time (UTC).</param>
    /// <param name="playedMinutes">The minutes for which the Muun was equipped while its owner played.</param>
    /// <returns><c>true</c>, if the option applies.</returns>
    public bool IsMet(int mapNumber, int totalLevel, DateTime utcNow, int playedMinutes)
    {
        switch (this.Type)
        {
            case MuunConditionType.Map:
                return this.Value1 < 0 || mapNumber == this.Value1;
            case MuunConditionType.CharacterLevel:
                return totalLevel >= this.Value1 && (this.Value2 < 0 || totalLevel <= this.Value2);
            case MuunConditionType.Hours:
                var hour = utcNow.Hour + 1;
                return this.Value1 < 0 || (hour >= this.Value1 && (this.Value2 < 0 || hour <= this.Value2));
            case MuunConditionType.PlayTime:
                return this.Value1 <= 0 || playedMinutes >= this.Value1 * MinutesPerHour;
            case MuunConditionType.Weekdays:
                return this.Value1 == EveryDay || (this.Value1 & (1 << (6 - (int)utcNow.DayOfWeek))) != 0;
            default:
                return true;
        }
    }
}
