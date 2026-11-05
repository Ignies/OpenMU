// <copyright file="Season21ItemCatalog.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Every item of the Season 21 server data, generated into the embedded resource <c>Resources/Season21/Items.json</c>
/// by <c>Generator/season21-items.mjs</c>, see Readme.md in this folder.
/// </summary>
internal static class Season21ItemCatalog
{
    private const string ResourceName = "MUnique.OpenMU.Persistence.Initialization.Resources.Season21.Items.json";

    private static readonly Lazy<IReadOnlyList<Season21CatalogItem>> LazyItems = new(Load);

    /// <summary>
    /// Gets the items, ordered by group and number.
    /// </summary>
    public static IReadOnlyList<Season21CatalogItem> Items => LazyItems.Value;

    private static IReadOnlyList<Season21CatalogItem> Load()
    {
        using var stream = typeof(Season21ItemCatalog).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"The resource {ResourceName} is missing.");
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };
        return JsonSerializer.Deserialize<List<Season21CatalogItem>>(stream, options)
            ?? throw new InvalidOperationException($"The resource {ResourceName} is empty.");
    }
}
