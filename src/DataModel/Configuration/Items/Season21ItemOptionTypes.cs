// <copyright file="Season21ItemOptionTypes.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.DataModel.Configuration.Items;

/// <summary>
/// The item option types of the later seasons which only the wide item format (W32) of the Babylon client carries.
/// </summary>
/// <remarks>
/// They are kept apart from <see cref="ItemOptionTypes"/>, whose types every new Season 6 configuration gets.
/// The optional update "Add the item options of Season 21" adds them to a configuration.
/// </remarks>
public static class Season21ItemOptionTypes
{
    /// <summary>
    /// Gets the mastery option type: the options of the mastery items which replace some excellent options.
    /// </summary>
    public static ItemOptionType Mastery { get; } = new() { Name = "Mastery Option", Id = new Guid("{8A61C0F2-3B57-4E1D-9C24-6F0B7D5E2A91}"), IsVisible = true };

    /// <summary>
    /// Gets the mastery bonus option type: the bonus of a mastery item, e.g. all stats +10.
    /// </summary>
    public static ItemOptionType MasteryBonus { get; } = new() { Name = "Mastery Bonus Option", Id = new Guid("{2D7E94B1-0C6A-4F38-A5E2-91B3C8D64F07}"), IsVisible = true };

    /// <summary>
    /// Gets the wing grade option type: the graded options of the fourth and fifth wings, the level is the grade + 1.
    /// </summary>
    public static ItemOptionType WingGrade { get; } = new() { Name = "Wing Grade Option", Id = new Guid("{C5F3A817-6E2B-49D0-B7A4-0E8D1F92C365}"), IsVisible = true };

    /// <summary>
    /// Gets the type which marks an item definition that only the wide item format may carry, e.g. a Muun or an
    /// earring: a Season 6 client has no such item. It has no effect on the item's power.
    /// </summary>
    public static ItemOptionType WideFormat { get; } = new() { Name = "Wide Item Format", Id = new Guid("{F09B2C64-81D5-4A7E-93C0-5B6E4D2A17F8}") };

    /// <summary>
    /// Gets all types of this class.
    /// </summary>
    public static IReadOnlyList<ItemOptionType> All { get; } = [Mastery, MasteryBonus, WingGrade, WideFormat];
}
