// <copyright file="ForkFeatures.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic;

/// <summary>
/// The features of the Babylon protocol as bits, which the client asks for and the server answers with
/// (docs/babylon.md, "Protocol").
/// </summary>
/// <remarks>
/// A bit index is a contract with every released client: bits are appended, never renumbered or reused.
/// A bit is available when <see cref="ForkInfo"/> carries the feature id of the same name.
/// </remarks>
public static class ForkFeatures
{
    /// <summary>
    /// The version of the Babylon protocol which this server speaks.
    /// </summary>
    public const ushort WireVersion = 1;

    /// <summary>
    /// The feature id which says that the fork speaks the Babylon protocol at all.
    /// </summary>
    public const string ProtocolFeatureId = "fork_protocol";

    private static readonly string[] Ids =
    [
        "wire_w32",
        "class_codes",
        "classes_s21",
        "evolutions_45",
        "enhance_tree",
        "ability_core",
        "mastery",
        "wing_grades",
        "sockets_10",
        "pentagram_elements",
        "earrings",
        "muun",
        "gremory",
        "ruud",
        "mining",
        "elite_zones",
        "world_bosses",
        "events_s21",
        "muquest",
        "helper_plus",
        "ranking",
        "battle_pass",
        "artifacts",
        "legendary_craft",
        "evolution_quests",
    ];

    /// <summary>
    /// Gets the feature ids by their bit index.
    /// </summary>
    public static IReadOnlyList<string> BitIds => Ids;

    /// <summary>
    /// Gets the feature bits which this fork carries.
    /// </summary>
    public static uint Available { get; } = GetBits(ForkInfo.Has);

    /// <summary>
    /// Gets the bit of a feature id.
    /// </summary>
    /// <param name="featureId">The feature id.</param>
    /// <returns>The bit, or 0 when the id has no bit.</returns>
    public static uint BitOf(string featureId)
    {
        var index = Array.IndexOf(Ids, featureId);
        return index < 0 ? 0 : 1u << index;
    }

    /// <summary>
    /// Gets the features which are active for a client which wants <paramref name="wantedFeatures"/>.
    /// </summary>
    /// <param name="wantedFeatures">The features the client wants.</param>
    /// <returns>The wanted features which this fork carries.</returns>
    public static uint Negotiate(uint wantedFeatures) => wantedFeatures & Available;

    /// <summary>
    /// Gets the bits of every feature id for which <paramref name="isCarried"/> is true.
    /// </summary>
    /// <param name="isCarried">Says if a feature id is carried.</param>
    /// <returns>The bits.</returns>
    internal static uint GetBits(Func<string, bool> isCarried)
    {
        uint bits = 0;
        for (var i = 0; i < Ids.Length; i++)
        {
            if (isCarried(Ids[i]))
            {
                bits |= 1u << i;
            }
        }

        return bits;
    }
}
