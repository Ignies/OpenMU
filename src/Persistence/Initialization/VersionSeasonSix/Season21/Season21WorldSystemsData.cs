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

    /// <summary>
    /// Gets the helper plus data (<c>Character/MuHelperPlus.xml</c>).
    /// </summary>
    public HelperPlusRows HelperPlus { get; init; } = new();

    /// <summary>
    /// Gets the season pass data (<c>Event/BattlePass/BattlePass.xml</c>).
    /// </summary>
    public SeasonPassRows SeasonPass { get; init; } = new();

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

    /// <summary>The season pass data.</summary>
    internal sealed class SeasonPassRows
    {
        /// <summary>Gets a value indicating whether the gold pass is on.</summary>
        public bool Gold { get; init; }

        /// <summary>Gets a value indicating whether the premium pass is on.</summary>
        public bool Premium { get; init; }

        /// <summary>Gets the experience of a pass ticket.</summary>
        public int TicketExperience { get; init; }

        /// <summary>Gets the seasons.</summary>
        public List<PassSeasonRow> Schedule { get; init; } = [];

        /// <summary>Gets the experience of the levels.</summary>
        public List<int> Levels { get; init; } = [];

        /// <summary>Gets the daily missions.</summary>
        public List<PassMissionRow> Missions { get; init; } = [];

        /// <summary>Gets the special missions.</summary>
        public List<PassMissionRow> Special { get; init; } = [];

        /// <summary>Gets the rewards.</summary>
        public List<PassRewardRow> Rewards { get; init; } = [];

        /// <summary>Gets the reward lists of the passes.</summary>
        public List<PassListRow> Lists { get; init; } = [];

        /// <summary>Gets the buffs.</summary>
        public List<PassBuffRow> Buffs { get; init; } = [];
    }

    /// <summary>A season.</summary>
    /// <param name="Id">The number.</param>
    /// <param name="Start">The start, <c>yyyy-MM-dd HH:mm</c>.</param>
    /// <param name="End">The end, <c>yyyy-MM-dd HH:mm</c>.</param>
    internal sealed record PassSeasonRow(ushort Id, string Start, string End);

    /// <summary>A mission.</summary>
    /// <param name="Index">The index.</param>
    /// <param name="Experience">The experience of a completion.</param>
    /// <param name="Required">The count of a completion.</param>
    /// <param name="Repeat">The completions.</param>
    internal sealed record PassMissionRow(byte Index, int Experience, int Required, int Repeat);

    /// <summary>A reward.</summary>
    /// <param name="Id">The id.</param>
    /// <param name="Type">0 a buff, 1 an item.</param>
    /// <param name="Effect">The effect index of a buff.</param>
    /// <param name="G">The item group.</param>
    /// <param name="I">The item number.</param>
    /// <param name="N">The item count.</param>
    /// <param name="Name">The name.</param>
    internal sealed record PassRewardRow(int Id, int Type, int Effect, int G, int I, int N, string Name);

    /// <summary>The reward list of a pass.</summary>
    /// <param name="Pass">The pass.</param>
    /// <param name="Levels">The reward ids per level, from level 1.</param>
    internal sealed record PassListRow(int Pass, List<List<int>> Levels);

    /// <summary>A buff.</summary>
    /// <param name="Effect">The effect index.</param>
    /// <param name="Values">The values by level.</param>
    internal sealed record PassBuffRow(int Effect, List<int> Values);

    /// <summary>The helper plus data.</summary>
    internal sealed class HelperPlusRows
    {
        /// <summary>Gets the hunting grounds.</summary>
        public List<HelperGroundRow> Grounds { get; init; } = [];

        /// <summary>Gets the hunting zones.</summary>
        public List<HelperZoneRow> Zones { get; init; } = [];
    }

    /// <summary>A hunting ground.</summary>
    /// <param name="Index">The index.</param>
    /// <param name="Move">The index of its warp (<c>MoveReqIndex</c>).</param>
    /// <param name="Name">The name.</param>
    internal sealed record HelperGroundRow(short Index, int Move, string Name);

    /// <summary>A hunting zone.</summary>
    /// <param name="Index">The index.</param>
    /// <param name="Ground">The ground.</param>
    /// <param name="Map">The map.</param>
    /// <param name="Element">The element.</param>
    /// <param name="X">The x coordinate of the hunting point.</param>
    /// <param name="Y">The y coordinate of the hunting point.</param>
    internal sealed record HelperZoneRow(short Index, short Ground, short Map, byte Element, byte X, byte Y);

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
