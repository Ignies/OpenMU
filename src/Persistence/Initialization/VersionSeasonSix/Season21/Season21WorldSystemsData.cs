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
}
