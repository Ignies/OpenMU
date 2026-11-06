// <copyright file="Season21WorldSystemsData.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using System.Text.Json;

/// <summary>
/// The world systems data of Season 21 (<c>Resources/Season21/WorldSystems.json</c>, written by <c>Generator/season21-world.mjs</c>).
/// </summary>
internal sealed class Season21WorldSystemsData
{
    private const string ResourceName = "MUnique.OpenMU.Persistence.Initialization.Resources.Season21.WorldSystems.json";

    private static readonly Lazy<Season21WorldSystemsData> LazyInstance = new(Load);

    /// <summary>
    /// Gets the data.
    /// </summary>
    public static Season21WorldSystemsData Instance => LazyInstance.Value;

    /// <summary>
    /// Gets the hunting record data (<c>HuntingRecord.txt</c>).
    /// </summary>
    public HuntingRecordRows HuntingRecord { get; init; } = new();

    /// <summary>
    /// Gets the monster soul converter data (<c>MonsterSoulConverter/</c>, <c>EventItemBag/</c> 288-355).
    /// </summary>
    public SoulConverterRows SoulConverter { get; init; } = new();

    private static Season21WorldSystemsData Load()
    {
        using var stream = typeof(Season21WorldSystemsData).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"The resource {ResourceName} is missing.");
        return JsonSerializer.Deserialize<Season21WorldSystemsData>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidOperationException($"The resource {ResourceName} is empty.");
    }

    /// <summary>The hunting record data.</summary>
    internal sealed class HuntingRecordRows
    {
        /// <summary>Gets the maps on which the hunting is recorded.</summary>
        public List<short> Maps { get; init; } = [];

        /// <summary>Gets the monsters whose kills don't count.</summary>
        public List<short> Excluded { get; init; } = [];
    }

    /// <summary>The monster soul converter data.</summary>
    internal sealed class SoulConverterRows
    {
        /// <summary>Gets the converters a week.</summary>
        public int Purchases { get; init; }

        /// <summary>Gets the tiers.</summary>
        public List<SoulTierRow> Tiers { get; init; } = [];
    }

    /// <summary>A converter tier.</summary>
    /// <param name="Tier">The tier.</param>
    /// <param name="G">The item group.</param>
    /// <param name="I">The item number.</param>
    /// <param name="Missions">The missions a converter draws.</param>
    /// <param name="Level">The total level.</param>
    /// <param name="Monsters">The missions of the tier.</param>
    /// <param name="Rewards">The rewards by kind.</param>
    internal sealed record SoulTierRow(byte Tier, byte G, short I, int Missions, int Level, List<SoulMissionRow> Monsters, List<SoulRewardRow> Rewards);

    /// <summary>A soul mission.</summary>
    /// <param name="Map">The map.</param>
    /// <param name="Monster">The monster.</param>
    /// <param name="Souls">The souls it needs.</param>
    /// <param name="Rate">The absorb chance in 1/1000000.</param>
    internal sealed record SoulMissionRow(short Map, short Monster, int Souls, int Rate);

    /// <summary>A reward of a kind.</summary>
    /// <param name="Kind">The kind: ruud, jewels, options or accessory.</param>
    /// <param name="Draws">The draws of its item bag.</param>
    internal sealed record SoulRewardRow(string Kind, List<SoulDrawRow> Draws);

    /// <summary>A draw of an item bag.</summary>
    /// <param name="C">The chance in 1/10000.</param>
    /// <param name="Items">The items of which it gives one.</param>
    /// <param name="RuudMin">The least Ruud.</param>
    /// <param name="RuudMax">The most Ruud.</param>
    internal sealed record SoulDrawRow(int C, List<BagItemRow> Items, int RuudMin, int RuudMax);

    /// <summary>An item of an item bag.</summary>
    /// <param name="G">The group.</param>
    /// <param name="I">The number.</param>
    /// <param name="L">The level.</param>
    /// <param name="D">The count (durability); 0 for the one of the definition.</param>
    internal sealed record BagItemRow(byte G, short I, byte L, byte D);
}
