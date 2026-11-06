// <copyright file="Pentagrams.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Elements;

using MUnique.OpenMU.DataModel.Configuration.Items;

/// <summary>
/// The rules of pentagrams and errtels (see Readme.md, "Pentagrams and errtels").
/// </summary>
public static class Pentagrams
{
    /// <summary>
    /// The number of sockets of a pentagram: anger, blessing, integrity, divinity, radiance.
    /// </summary>
    public const int Sockets = 5;

    /// <summary>
    /// The highest rank of an errtel.
    /// </summary>
    public const int MaximumRank = 5;

    /// <summary>
    /// The size of a block of errtel option numbers of one kind (S21 <c>PentagramJewelOption.xml</c>: anger 0-21,
    /// blessing 50-71, integrity 100-121, divinity 150-171, radiance 200-224).
    /// </summary>
    private const int OptionBlockSize = 50;

    /// <summary>
    /// The first option number of the errtels for beginners (250 anger to 253 divinity, one per kind).
    /// </summary>
    private const int FirstBeginnerOptionNumber = 250;

    /// <summary>
    /// Determines whether the item is a pentagram: an item which is worn in the <see cref="InventoryConstants.PentagramSlot"/>.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns><c>true</c>, if it's a pentagram.</returns>
    public static bool IsPentagram(this Item item) => item.Definition?.ItemSlot?.ItemSlots.Contains(InventoryConstants.PentagramSlot) is true;

    /// <summary>
    /// Determines whether the item is an errtel: an item with errtel rank options.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns><c>true</c>, if it's an errtel.</returns>
    public static bool IsErrtel(this Item item) => item.Definition is { } definition && GetErrtelSocket(definition) is not null;

    /// <summary>
    /// Gets the element of a pentagram or an errtel.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The element; <see cref="Element.None"/> if it has none.</returns>
    public static Element GetElement(this Item item)
        => item.ItemOptions.FirstOrDefault(o => o.ItemOption?.OptionType == Season21ItemOptionTypes.Element)?.ItemOption is { } option
            ? (Element)option.Number
            : Element.None;

    /// <summary>
    /// Gets the socket of the pentagram an errtel of this definition goes into.
    /// </summary>
    /// <param name="errtel">The errtel definition.</param>
    /// <returns>The socket (0 anger to 4 radiance), or <c>null</c> if it's no errtel.</returns>
    public static int? GetErrtelSocket(ItemDefinition errtel)
        => errtel.PossibleItemOptions.SelectMany(o => o.PossibleOptions).FirstOrDefault(o => o.OptionType == Season21ItemOptionTypes.ErrtelRank) is { } option
            ? GetSocketOfOption(option.Number)
            : null;

    /// <summary>
    /// Gets the socket of the errtel kind an errtel option belongs to.
    /// </summary>
    /// <param name="optionNumber">The number of the errtel option.</param>
    /// <returns>The socket.</returns>
    public static int GetSocketOfOption(int optionNumber)
        => optionNumber >= FirstBeginnerOptionNumber ? optionNumber - FirstBeginnerOptionNumber : optionNumber / OptionBlockSize;

    /// <summary>
    /// Gets the rank options of an errtel, or of the errtel in a socket of a pentagram, ordered by rank.
    /// </summary>
    /// <param name="item">The errtel or the pentagram.</param>
    /// <param name="socket">The socket of the pentagram; <c>null</c> for an errtel.</param>
    /// <returns>The links of the rank options.</returns>
    public static IEnumerable<ItemOptionLink> GetRankOptions(this Item item, int? socket = null)
        => item.ItemOptions
            .Where(o => o.ItemOption?.OptionType == Season21ItemOptionTypes.ErrtelRank && (socket is null || o.Index == socket))
            .OrderBy(o => o.ItemOption!.SubOptionType);

    /// <summary>
    /// Finds the errtel definition a rank option belongs to, e.g. to give an errtel back out of a pentagram.
    /// </summary>
    /// <param name="configuration">The game configuration.</param>
    /// <param name="rankOption">The rank option.</param>
    /// <returns>The errtel definition, or <c>null</c>.</returns>
    public static ItemDefinition? FindErrtelDefinition(this GameConfiguration configuration, IncreasableItemOption? rankOption)
        => rankOption is null ? null : configuration.Items.FirstOrDefault(d => d.PossibleItemOptions.Any(o => o.PossibleOptions.Contains(rankOption)));

    /// <summary>
    /// Gets the level of an errtel: the level of its highest rank option.
    /// </summary>
    /// <param name="rankOptions">The rank options of the errtel.</param>
    /// <returns>The level.</returns>
    public static int GetErrtelLevel(this IEnumerable<ItemOptionLink> rankOptions) => rankOptions.LastOrDefault()?.Level ?? 0;
}
