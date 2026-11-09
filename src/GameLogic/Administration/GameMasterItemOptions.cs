// <copyright file="GameMasterItemOptions.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Administration;

using System.Globalization;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Elements;

/// <summary>
/// Gives a game master's item the options of the later seasons (<see cref="GameMasterItemExtras"/>).
/// </summary>
public static class GameMasterItemOptions
{
    private static readonly IReadOnlyDictionary<string, ItemOptionType> OptionKinds = new Dictionary<string, ItemOptionType>(StringComparer.OrdinalIgnoreCase)
    {
        ["mastery"] = Season21ItemOptionTypes.Mastery,
        ["bonus"] = Season21ItemOptionTypes.MasteryBonus,
        ["element"] = Season21ItemOptionTypes.Element,
        ["grade"] = Season21ItemOptionTypes.WingGrade,
        ["earring"] = Season21ItemOptionTypes.EarringOption,
    };

    /// <summary>
    /// Applies the extras to a new item.
    /// </summary>
    /// <param name="item">The item, as the <see cref="GameMasterItemFactory"/> made it.</param>
    /// <param name="extras">The extras.</param>
    /// <param name="configuration">The game configuration, which holds the errtels.</param>
    /// <param name="elements">The configuration of the elements, for the options a pentagram gets from its errtels.</param>
    /// <returns>The first entry which doesn't fit the item; <c>null</c> when all fit.</returns>
    public static string? Apply(Item item, GameMasterItemExtras extras, GameConfiguration configuration, ElementsConfiguration? elements)
    {
        var definition = item.Definition!;
        item.SocketCount = Math.Min(extras.Sockets, definition.MaximumSockets);

        var slot = 0;
        foreach (var entry in Entries(extras.Seeds))
        {
            if (slot >= item.SocketCount || item.IsPentagram()
                || Numbers(entry) is not [var index, .. var rest] || rest is [_, _, ..]
                || SocketOptionIndex.Find(definition, index) is not { } seed)
            {
                return entry;
            }

            item.ItemOptions.Add(new ItemOptionLink { ItemOption = seed, Index = slot++, Level = rest is [var level] ? level : 1 });
        }

        foreach (var entry in Entries(extras.Errtels))
        {
            if (!item.IsPentagram() || Numbers(entry) is not [var socket, var ranks, .. var rest] || rest is [_, _, ..]
                || socket >= item.SocketCount || ranks is < 1 or > Pentagrams.MaximumRank
                || item.GetRankOptions(socket).Any()
                || !TryAddErrtel(item, configuration, socket, ranks, rest is [var level] ? level : 0))
            {
                return entry;
            }
        }

        foreach (var entry in Entries(extras.Options))
        {
            var parts = entry.Split(':');
            if (parts.Length is < 2 or > 3 || !OptionKinds.TryGetValue(parts[0], out var kind)
                || Numbers(string.Join(':', parts.Skip(1))) is not [var number, .. var rest]
                || FindOption(definition, kind, number) is not { } option)
            {
                return entry;
            }

            item.ItemOptions.Add(new ItemOptionLink { ItemOption = option, Level = rest is [var level] ? level : 0 });
        }

        if (extras.MuunOptionDays > 0)
        {
            item.TimedOptionExpiresAt = DateTime.UtcNow.AddDays(extras.MuunOptionDays);
        }

        if (elements is not null && item.IsPentagram())
        {
            item.UpdateConditionalOptions(elements, () => new ItemOptionLink());
        }

        return null;
    }

    private static bool TryAddErrtel(Item pentagram, GameConfiguration configuration, int socket, int ranks, int level)
    {
        if (configuration.Items.FirstOrDefault(d => d.IsActive && Pentagrams.GetErrtelSocket(d) == socket) is not { } errtel)
        {
            return false;
        }

        var options = errtel.PossibleItemOptions.SelectMany(o => o.PossibleOptions).ToList();
        var rankOptions = Enumerable.Range(1, ranks)
            .Select(rank => options.FirstOrDefault(o => o.OptionType == Season21ItemOptionTypes.ErrtelRank && o.SubOptionType == rank))
            .ToList();
        if (rankOptions.Any(o => o is null))
        {
            return false;
        }

        foreach (var option in rankOptions)
        {
            pentagram.ItemOptions.Add(new ItemOptionLink { ItemOption = option, Index = socket, Level = level });
        }

        if (options.FirstOrDefault(o => o.OptionType == Season21ItemOptionTypes.ErrtelSet) is { } set)
        {
            pentagram.ItemOptions.Add(new ItemOptionLink { ItemOption = set, Index = socket });
        }

        return true;
    }

    private static IncreasableItemOption? FindOption(ItemDefinition definition, ItemOptionType kind, int number)
        => definition.PossibleItemOptions.SelectMany(o => o.PossibleOptions).FirstOrDefault(o => o.OptionType == kind && o.Number == number);

    private static string[] Entries(string? list)
        => string.IsNullOrWhiteSpace(list) ? [] : list.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static int[]? Numbers(string entry)
    {
        var parts = entry.Split(':');
        var numbers = new int[parts.Length];
        for (var i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i], NumberStyles.None, CultureInfo.InvariantCulture, out numbers[i]))
            {
                return null;
            }
        }

        return numbers;
    }
}
