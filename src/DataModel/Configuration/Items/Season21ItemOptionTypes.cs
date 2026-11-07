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
    /// Gets the element option type: the element of a pentagram or an errtel (the option number, 1 fire to 5 darkness).
    /// </summary>
    public static ItemOptionType Element { get; } = new() { Name = "Element", Id = new Guid("{2ADE9FF2-960F-4AD9-B9A5-F69F666F9DE0}"), IsVisible = true };

    /// <summary>
    /// Gets the errtel rank option type: an option of a rank of an errtel (the sub option type is the rank, the link
    /// level the level). On an errtel, the link index is the rank minus 1; in a pentagram, it is the socket.
    /// </summary>
    public static ItemOptionType ErrtelRank { get; } = new() { Name = "Errtel Rank Option", Id = new Guid("{6BBC206A-ADE2-4AEB-B9D4-AF5237063DF1}"), IsVisible = true };

    /// <summary>
    /// Gets the earring option type: a random option of an earring (the option number is the Season 21 option index).
    /// </summary>
    public static ItemOptionType EarringOption { get; } = new() { Name = "Earring Option", Id = new Guid("{4A6375C2-3641-4A7C-A5C6-B284EFA6A9FA}"), IsVisible = true };

    /// <summary>
    /// Gets the pentagram option type: an option of a pentagram which is active while its sockets hold the errtels it
    /// needs (the option number is the Season 21 option number).
    /// </summary>
    public static ItemOptionType PentagramOption { get; } = new() { Name = "Pentagram Option", Id = new Guid("{6742FCE3-016B-499E-80BD-85FBD314674F}"), IsVisible = true };

    /// <summary>
    /// Gets the errtel set option type: the set of a mastery errtel (the option number, 0 fighting spirit, 1 passionate).
    /// </summary>
    public static ItemOptionType ErrtelSet { get; } = new() { Name = "Errtel Set Option", Id = new Guid("{77D80CC4-D162-44D4-B5D8-1A326B5158AD}"), IsVisible = true };

    /// <summary>
    /// Gets the errtel set bonus type: a bonus of a mastery pentagram whose sockets hold enough errtels of one set
    /// (the option number divided by 10 is the set, the sub option type the errtels it needs).
    /// </summary>
    public static ItemOptionType ErrtelSetBonus { get; } = new() { Name = "Errtel Set Bonus", Id = new Guid("{577BD27D-F1F1-42B3-B418-C6AAF051F5C9}"), IsVisible = true };

    /// <summary>
    /// Gets the Muun evolution target option type: the Muun which an evolution stone evolves (the link level is the
    /// item number of the Muun in the item group 16).
    /// </summary>
    public static ItemOptionType MuunEvolutionTarget { get; } = new() { Name = "Muun Evolution Target", Id = new Guid("{9D3F6B1E-5A27-4C80-B4E9-2F1A7C6D8E53}"), IsVisible = true };

    /// <summary>
    /// Gets the ability card option type: the option an ability card of the Ability Enhancement Core carries (the number
    /// is the option index of the S21Emu data, the value follows the card level).
    /// </summary>
    public static ItemOptionType AbilityCardOption { get; } = new() { Name = "Ability Card Option", Id = new Guid("{3C9A7E15-0B6D-4F42-A8E3-71D5C2B9F604}"), IsVisible = true };

    /// <summary>
    /// Gets all types of this class.
    /// </summary>
    public static IReadOnlyList<ItemOptionType> All { get; } = [Mastery, MasteryBonus, WingGrade, WideFormat, Element, ErrtelRank, EarringOption, PentagramOption, ErrtelSet, ErrtelSetBonus, MuunEvolutionTarget, AbilityCardOption];
}
