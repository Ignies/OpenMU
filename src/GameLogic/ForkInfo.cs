// <copyright file="ForkInfo.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic;

/// <summary>
/// The version marker of the Ignies/OpenMU fork, see <c>docs/babylon.md</c>; <c>fork.json</c> in the repository
/// root says the same for installers. Upstream OpenMU has no such type, so plugins detect the fork by looking it up.
/// </summary>
public static class ForkInfo
{
    /// <summary>
    /// The name of the fork.
    /// </summary>
    public const string Name = "Ignies/OpenMU";

    /// <summary>
    /// The shipped line of the fork.
    /// </summary>
    public const string Line = "master of Ignies/OpenMU";

    /// <summary>
    /// The version of the marker contract.
    /// </summary>
    public const int MarkerVersion = 1;

    /// <summary>
    /// Gets the ids of the features this fork carries.
    /// </summary>
    public static IReadOnlyList<string> Features { get; } =
    [
        "s21_maps",
        "level_caps",
        "s21_gear",
        "s21_drops",
        "s21_event_grades",
        "party_bonus",
        "lucky_items",
        "lucky_refinery_s21",
        "progression",
        "fork_protocol",
        "wire_w32",
        "class_codes",
        "s21_items_all",
        "config_api",
        "classes_s21",
        "evolutions_45",
        "s21_skills",
        "enhance_tree",
        "evolution_quests",
        "elite_zones",
        "world_bosses",
        "wide_items",
        "mastery",
        "wing_grades",
        "sockets_10",
        "item_expiry",
        "gremory",
        "ruud",
        "npc_shop_tabs",
        "s21_shops",
        "zen_sinks",
        "s21_mixes",
        "level_rewards",
        "event_contribution",
        "pentagram_elements",
        "earrings",
        "muun",
        "mining",
        "golden_invasions_s21",
        "ranking",
        "soul_converter",
        "helper_plus",
        "party_instances",
    ];

    /// <summary>
    /// Determines whether the fork carries the feature.
    /// </summary>
    /// <param name="feature">The feature id.</param>
    /// <returns><c>true</c>, if the fork carries the feature.</returns>
    public static bool Has(string feature) => Features.Contains(feature, StringComparer.Ordinal);
}
