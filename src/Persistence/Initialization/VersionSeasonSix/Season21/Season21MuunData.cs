// <copyright file="Season21MuunData.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// The Muun data of the Season 21 server data, generated into the embedded resource <c>Resources/Season21/Muuns.json</c>
/// by <c>Generator/season21-muuns.mjs</c>: the Muuns, their options, the energy and the exchange.
/// </summary>
internal sealed class Season21MuunData
{
    private const string ResourceName = "MUnique.OpenMU.Persistence.Initialization.Resources.Season21.Muuns.json";

    private static readonly Lazy<Season21MuunData> LazyData = new(Load);

    /// <summary>
    /// Gets the data.
    /// </summary>
    public static Season21MuunData Data => LazyData.Value;

    /// <summary>
    /// Gets or sets the energy of an evolution stone.
    /// </summary>
    [JsonPropertyName("stoneEnergy")]
    public int StoneEnergy { get; set; }

    /// <summary>
    /// Gets or sets the Muuns.
    /// </summary>
    [JsonPropertyName("muuns")]
    public List<MuunRow> Muuns { get; set; } = [];

    /// <summary>
    /// Gets or sets the options.
    /// </summary>
    [JsonPropertyName("options")]
    public List<OptionRow> Options { get; set; } = [];

    /// <summary>
    /// Gets or sets the additional options.
    /// </summary>
    [JsonPropertyName("addOptions")]
    public List<AddOptionRow> AddOptions { get; set; } = [];

    /// <summary>
    /// Gets or sets the energy by rank.
    /// </summary>
    [JsonPropertyName("rankEnergy")]
    public List<RankEnergyRow> RankEnergy { get; set; } = [];

    /// <summary>
    /// Gets or sets the energy by level.
    /// </summary>
    [JsonPropertyName("levelEnergy")]
    public List<LevelEnergyRow> LevelEnergy { get; set; } = [];

    /// <summary>
    /// Gets or sets the offers of the exchange NPC.
    /// </summary>
    [JsonPropertyName("exchange")]
    public List<ExchangeRow> Exchange { get; set; } = [];

    /// <summary>
    /// Gets or sets the places of the exchange NPC.
    /// </summary>
    [JsonPropertyName("monica")]
    public List<NpcRow> Monica { get; set; } = [];

    private static Season21MuunData Load()
    {
        using var stream = typeof(Season21MuunData).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"The resource {ResourceName} is missing.");
        return JsonSerializer.Deserialize<Season21MuunData>(stream)
            ?? throw new InvalidOperationException($"The resource {ResourceName} is empty.");
    }

    /// <summary>
    /// A Muun (MuunInfo.xml).
    /// </summary>
    /// <param name="Number">The item number in the group 16.</param>
    /// <param name="Type">The kind (1 normal, 4 mount, 5 hero).</param>
    /// <param name="SubType">The grade (0 normal, 1 premium, 2 tempest).</param>
    /// <param name="Rank">The rank.</param>
    /// <param name="OptionGroup">The option group.</param>
    /// <param name="AddOption">The additional option type.</param>
    /// <param name="Days">The days of the additional option.</param>
    /// <param name="Evolution">The number of the evolved Muun, -1 for none.</param>
    public sealed record MuunRow(
        [property: JsonPropertyName("n")] short Number,
        [property: JsonPropertyName("t")] byte Type,
        [property: JsonPropertyName("s")] byte SubType,
        [property: JsonPropertyName("r")] byte Rank,
        [property: JsonPropertyName("o")] short OptionGroup,
        [property: JsonPropertyName("a")] short AddOption,
        [property: JsonPropertyName("d")] int Days,
        [property: JsonPropertyName("e")] short Evolution);

    /// <summary>
    /// An option (MuunOption.xml).
    /// </summary>
    /// <param name="Group">The option group.</param>
    /// <param name="Type">The option type.</param>
    /// <param name="Minimum">The lowest total level.</param>
    /// <param name="Maximum">The highest total level, -1 for none.</param>
    /// <param name="Values">The values: evolved, levels 1-5.</param>
    /// <param name="Condition">The condition before the evolution: type, value 1, value 2.</param>
    /// <param name="EvolvedCondition">The condition of the evolved Muun.</param>
    /// <param name="Name">The name.</param>
    public sealed record OptionRow(
        [property: JsonPropertyName("g")] short Group,
        [property: JsonPropertyName("t")] byte Type,
        [property: JsonPropertyName("min")] int Minimum,
        [property: JsonPropertyName("max")] int Maximum,
        [property: JsonPropertyName("v")] int[] Values,
        [property: JsonPropertyName("c1")] int[] Condition,
        [property: JsonPropertyName("c2")] int[] EvolvedCondition,
        [property: JsonPropertyName("name")] string Name);

    /// <summary>
    /// An additional option (MuunAddOption.xml).
    /// </summary>
    /// <param name="Type">The type.</param>
    /// <param name="Minimum">The lowest total level.</param>
    /// <param name="Maximum">The highest total level, -1 for none.</param>
    /// <param name="Kind">The kind (1 multiplier, 2 bonus, 3 low multiplier).</param>
    /// <param name="Values">The values: evolved, levels 1-5.</param>
    public sealed record AddOptionRow(
        [property: JsonPropertyName("t")] short Type,
        [property: JsonPropertyName("min")] int Minimum,
        [property: JsonPropertyName("max")] int Maximum,
        [property: JsonPropertyName("k")] byte Kind,
        [property: JsonPropertyName("v")] int[] Values);

    /// <summary>
    /// The energy of a rank.
    /// </summary>
    /// <param name="Rank">The rank.</param>
    /// <param name="Points">The energy.</param>
    public sealed record RankEnergyRow([property: JsonPropertyName("r")] int Rank, [property: JsonPropertyName("p")] int Points);

    /// <summary>
    /// The energy of a level.
    /// </summary>
    /// <param name="Level">The level.</param>
    /// <param name="Points">The energy.</param>
    public sealed record LevelEnergyRow([property: JsonPropertyName("l")] int Level, [property: JsonPropertyName("p")] int Points);

    /// <summary>
    /// An offer of the exchange NPC.
    /// </summary>
    /// <param name="Index">The index.</param>
    /// <param name="CostGroup">The group of the price.</param>
    /// <param name="CostNumber">The number of the price.</param>
    /// <param name="CostCount">The count of the price.</param>
    /// <param name="Group">The group of the result.</param>
    /// <param name="Number">The number of the result.</param>
    /// <param name="TargetGroup">The group of the Muun of the evolution stone.</param>
    /// <param name="TargetNumber">The number of the Muun of the evolution stone.</param>
    public sealed record ExchangeRow(
        [property: JsonPropertyName("i")] int Index,
        [property: JsonPropertyName("cg")] byte CostGroup,
        [property: JsonPropertyName("cn")] short CostNumber,
        [property: JsonPropertyName("cc")] byte CostCount,
        [property: JsonPropertyName("g")] byte Group,
        [property: JsonPropertyName("n")] short Number,
        [property: JsonPropertyName("tg")] byte TargetGroup,
        [property: JsonPropertyName("tn")] short TargetNumber);

    /// <summary>
    /// A place of an NPC.
    /// </summary>
    /// <param name="Npc">The NPC number.</param>
    /// <param name="Map">The map number.</param>
    /// <param name="X">The x coordinate.</param>
    /// <param name="Y">The y coordinate.</param>
    /// <param name="Direction">The direction of the data.</param>
    public sealed record NpcRow(
        [property: JsonPropertyName("npc")] short Npc,
        [property: JsonPropertyName("map")] byte Map,
        [property: JsonPropertyName("x")] byte X,
        [property: JsonPropertyName("y")] byte Y,
        [property: JsonPropertyName("d")] byte Direction);
}
