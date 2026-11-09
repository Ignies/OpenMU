// <copyright file="SocketOptionIndex.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic;

using MUnique.OpenMU.DataModel.Configuration.Items;

/// <summary>
/// The index of a socket option over all elements, as the item data of the client carries it.
/// </summary>
/// <remarks>
/// Webzen decided to put every possible socket option of each elemental seed type into one big list,
/// which may contain up to <see cref="Count"/> elements.
/// I couldn't figure out a pattern, but found these index offsets by trial and error.
/// Their list contains holes, so expect that index 9 doesn't define an option.
/// </remarks>
public static class SocketOptionIndex
{
    /// <summary>
    /// The number of socket option indexes.
    /// </summary>
    public const byte Count = 50;

    /// <summary>
    /// The socket seed index offsets, where the key is the numerical value of a socket sub option type (the element
    /// of the seed) and the value is the first index of this corresponding elemental seed.
    /// </summary>
    private static readonly byte[] Offsets = [0, 10, 16, 21, 29, 36];

    /// <summary>
    /// Gets the index of a socket option.
    /// </summary>
    /// <param name="option">The socket option.</param>
    /// <returns>The index over all elements.</returns>
    public static byte Of(ItemOption option) => (byte)(Offsets[option.SubOptionType] + option.Number);

    /// <summary>
    /// Finds the socket option of an index among the possible options of an item.
    /// </summary>
    /// <param name="definition">The item definition.</param>
    /// <param name="index">The index over all elements.</param>
    /// <returns>The socket option, or <c>null</c> if the item can't have it.</returns>
    public static IncreasableItemOption? Find(ItemDefinition definition, int index)
    {
        if (index is < 0 or >= Count)
        {
            return null;
        }

        var offset = Offsets.Last(o => o <= index);
        var elementType = Array.IndexOf(Offsets, offset);
        var number = index - offset;
        return definition.PossibleItemOptions
            .SelectMany(o => o.PossibleOptions)
            .FirstOrDefault(p => p.OptionType == ItemOptionTypes.SocketOption && p.SubOptionType == elementType && p.Number == number);
    }
}
