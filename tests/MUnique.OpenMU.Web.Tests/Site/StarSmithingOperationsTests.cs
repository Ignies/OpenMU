// <copyright file="StarSmithingOperationsTests.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Web.Tests.Site;

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.StarSmithing;
using MUnique.OpenMU.Persistence;
using MUnique.OpenMU.Persistence.AdminAuth;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;
using MUnique.OpenMU.Web.AdminPanel.Operations;
using MUnique.OpenMU.Web.AdminPanel.Operations.Site;
using MUnique.OpenMU.Web.Tests.Mcp;

/// <summary>
/// The website's dye editor (Admin > Dyes, Hidden-Babylon docs/star_smithing.md section 9): <c>smithing.dyes</c> reads the
/// dyes of the Star Smithing plugin with their item names, <c>smithing.setDyes</c> replaces them, previewed and applied,
/// keeping every other setting, and refuses an unknown item, a dye without colours or one listed twice.
/// </summary>
[TestFixture]
public sealed class StarSmithingOperationsTests
{
    private static readonly Guid StarSmithingPlugIn = new("27031977-58F3-4B43-A2EE-524CB1F01D4D");

    private ServiceProvider _services = null!;
    private InMemoryPersistenceContextProvider _persistence = null!;
    private PlugInConfiguration _plugin = null!;
    private InMemoryAdminAuditRepository _audit = null!;

    /// <summary>A configuration with two dye items and the plugin keeping one dye and its own recolour price.</summary>
    [SetUp]
    public async Task SetUpAsync()
    {
        this._persistence = new InMemoryPersistenceContextProvider();
        using var context = this._persistence.CreateNewContext();
        var config = context.CreateNew<GameConfiguration>();
        foreach (var (number, name) in new[] { ((short)1004, "Azure Dye"), ((short)1014, "Radiant Dye") })
        {
            var item = context.CreateNew<ItemDefinition>();
            item.Group = 14;
            item.Number = number;
            item.Name = name;
            config.Items.Add(item);
        }

        this._plugin = context.CreateNew<PlugInConfiguration>();
        this._plugin.TypeId = StarSmithingPlugIn;
        this._plugin.CustomConfiguration = """{"RecolorZen":300000,"Dyes":[{"Group":14,"Number":1004,"Swatches":[2003199],"MaximumStrength":85}]}""";
        config.PlugInConfigurations.Add(this._plugin);
        await context.SaveChangesAsync().ConfigureAwait(false);

        var configurations = new Mock<IDataSource<GameConfiguration>>();
        configurations.Setup(c => c.GetOwnerAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(config);
        this._audit = new InMemoryAdminAuditRepository();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAdminReadOperations();
        services.AddSingleton<IAdminAuditRepository>(this._audit);
        services.AddSingleton(configurations.Object);
        services.AddSingleton<IPersistenceContextProvider>(this._persistence);
        services.AddSingleton<IAdminReadModel, TestReadModel>();
        services.AddSingleton(new Mock<IConfigurationChangeMediatorListener>().Object);
        services.AddSingleton(new SiteBridgeOptions(SavesPublishChanges: false));
        this._services = services.BuildServiceProvider();
    }

    /// <summary>Disposes the services.</summary>
    [TearDown]
    public async Task TearDownAsync() => await this._services.DisposeAsync().ConfigureAwait(false);

    /// <summary>The dyes read with their item names and colours as <c>#rrggbb</c>.</summary>
    [Test]
    public async Task ReadsTheDyesAsync()
    {
        var (status, body) = await this.CallAsync("smithing.dyes", new JsonObject(), "Viewer").ConfigureAwait(false);
        Assert.That(status, Is.EqualTo(200), body.ToJsonString());
        var dye = body["result"]!["dyes"]!.AsArray().Single()!;
        Assert.That(
            (dye["number"]!.GetValue<int>(), dye["name"]!.GetValue<string>(), dye["swatches"]!.AsArray().Single()!.GetValue<string>(), dye["maximumStrength"]!.GetValue<int>()),
            Is.EqualTo((1004, "Azure Dye", "#1e90ff", 85)));
    }

    /// <summary>The dyes are replaced, previewed then applied and audited; the recolour price stays.</summary>
    [Test]
    public async Task ReplacesTheDyesAsync()
    {
        var dyes = new JsonArray(
            new JsonObject { ["group"] = 14, ["number"] = 1004, ["swatches"] = new JsonArray("#7FC4FF", "#1e90ff"), ["maximumStrength"] = 100, ["keep"] = true },
            new JsonObject { ["group"] = 14, ["number"] = 1014, ["swatches"] = new JsonArray(), ["maximumStrength"] = 0, ["radiant"] = true });
        await this.PreviewAndApplyAsync("smithing.setDyes", new JsonObject { ["dyes"] = dyes.ToJsonString() }).ConfigureAwait(false);

        var saved = JsonSerializer.Deserialize<StarSmithingConfiguration>(this._plugin.CustomConfiguration!)!;
        Assert.That(saved.RecolorZen, Is.EqualTo(300_000));
        Assert.That(saved.Dyes.Select(d => (d.Number, string.Join(",", d.Swatches), d.MaximumStrength, d.Keep, d.Radiant)), Is.EqualTo(new[] { ((short)1004, "8373503,2003199", (byte)100, true, false), ((short)1014, string.Empty, (byte)0, false, true) }));
        Assert.That((await this._audit.ReadAsync(0, 10).ConfigureAwait(false)).Items.Single().Operation, Is.EqualTo("smithing_set_dyes"));
    }

    /// <summary>An unknown item, a dye without colours or with too many, one listed twice, or a Viewer: refused.</summary>
    [Test]
    public async Task RefusesBadDyesAsync()
    {
        static JsonObject Dye(int number, params string[] swatches) => new() { ["group"] = 14, ["number"] = number, ["swatches"] = new JsonArray(swatches.Select(s => (JsonNode)s).ToArray()) };
        foreach (var dyes in new[]
                 {
                     new JsonArray(Dye(999, "#ffffff")),
                     new JsonArray(Dye(1004)),
                     new JsonArray(Dye(1004, Enumerable.Range(1, 9).Select(i => $"#00000{i}").ToArray())),
                     new JsonArray(Dye(1004, "#ffffff"), Dye(1004, "#000000")),
                     new JsonArray(Dye(1004, "blue")),
                 })
        {
            var (status, body) = await this.CallAsync("smithing.setDyes", new JsonObject { ["dyes"] = dyes.ToJsonString(), ["apply"] = false }).ConfigureAwait(false);
            Assert.That(status, Is.EqualTo(400), body.ToJsonString());
        }

        Assert.That((await this.CallAsync("smithing.setDyes", new JsonObject { ["dyes"] = new JsonArray(Dye(1004, "#ffffff")).ToJsonString(), ["apply"] = false }, "Viewer").ConfigureAwait(false)).Status, Is.EqualTo(403));
    }

    /// <summary>
    /// The recipes and the balance read in the bridge's camelCase shape: targets as a list, enums as names, the hint as
    /// one object, colours as <c>#rrggbb</c>, with the code ceilings.
    /// </summary>
    [Test]
    public async Task ReadsTheConfigAsync()
    {
        var (status, body) = await this.CallAsync("smithing.config", new JsonObject(), "Viewer").ConfigureAwait(false);
        Assert.That(status, Is.EqualTo(200), body.ToJsonString());
        var result = body["result"]!;
        var recipes = result["config"]!["recipes"]!.AsArray();
        Assert.That(recipes, Has.Count.EqualTo(11));
        Assert.That(recipes[0]!["targets"]!.AsArray().Select(t => t!.GetValue<string>()), Is.EqualTo(new[] { "weapon" }));
        Assert.That(recipes[0]!["onFail"]!.GetValue<string>(), Is.EqualTo("itemDowngraded"));
        Assert.That(recipes[0]!["outcomes"]![0]!["tier"]!.GetValue<string>(), Is.EqualTo("common"));
        Assert.That(recipes[0]!["outcomes"]![0]!["boosts"]![0]!["stat"]!.GetValue<string>(), Is.EqualTo("damage"));
        Assert.That(recipes[8]!["hint"]!["kind"]!.GetValue<string>(), Is.EqualTo("ingredient"));
        Assert.That(recipes[8]!["hint"]!["number"]!.GetValue<int>(), Is.EqualTo(7));
        var balance = result["config"]!["balance"]!;
        Assert.That(balance["tiers"]![0]!["color"]!.GetValue<string>(), Is.EqualTo("#ffffff"));
        Assert.That(balance["scaling"]!["factor"]!.GetValue<double>(), Is.EqualTo(0.5));
        Assert.That(balance["caps"]!["pvp"]!["damage"]!["perItem"]!.GetValue<double>(), Is.EqualTo(3));
        Assert.That(result["ceilings"]!["damage"]!.GetValue<double>(), Is.EqualTo(30));
        Assert.That(result["items"], Is.InstanceOf<JsonArray>());
    }

    /// <summary>
    /// A config written through the bridge, previewed then applied: the recipes and the balance change, every other
    /// setting stays; what is left out stays as it was.
    /// </summary>
    [Test]
    public async Task ReplacesTheConfigAsync()
    {
        var config = new JsonObject
        {
            ["recipes"] = new JsonArray(Recipe(1)),
            ["balance"] = (await this.CallAsync("smithing.config", new JsonObject(), "Viewer").ConfigureAwait(false)).Body["result"]!["config"]!["balance"]!.DeepClone(),
        };
        config["balance"]!["caps"]!["pve"]!["damage"]!["perItem"] = 5;
        await this.PreviewAndApplyAsync("smithing.setConfig", new JsonObject { ["config"] = config.ToJsonString() }).ConfigureAwait(false);

        var saved = JsonSerializer.Deserialize<StarSmithingConfiguration>(this._plugin.CustomConfiguration!)!;
        Assert.That(saved.RecolorZen, Is.EqualTo(300_000), "the other settings stay");
        Assert.That(saved.Dyes, Has.Count.EqualTo(1));
        var recipe = saved.Recipes.Single();
        Assert.That((recipe.Name, recipe.Targets, recipe.HintKind, recipe.Hint), Is.EqualTo(("Edited", StarSmithTargetKind.Armor | StarSmithTargetKind.Ring, StarSmithHintKind.Riddle, "A riddle.")));
        Assert.That(recipe.Outcomes.Single().Boosts.Single().Maximum, Is.EqualTo(2.5));
        Assert.That(saved.Balance.Pve[StarSmithBoostStat.Damage].PerItem, Is.EqualTo(5));
        Assert.That((await this._audit.ReadAsync(0, 10).ConfigureAwait(false)).Items.Single().Operation, Is.EqualTo("smithing_set_config"));

        await this.PreviewAndApplyAsync("smithing.setConfig", new JsonObject { ["config"] = new JsonObject { ["recipes"] = new JsonArray(Recipe(2)) }.ToJsonString() }).ConfigureAwait(false);
        saved = JsonSerializer.Deserialize<StarSmithingConfiguration>(this._plugin.CustomConfiguration!)!;
        Assert.That(saved.Recipes.Single().Number, Is.EqualTo(2));
        Assert.That(saved.Balance.Pve[StarSmithBoostStat.Damage].PerItem, Is.EqualTo(5), "a missing balance stays");
    }

    /// <summary>
    /// A refusal is a 400 that names every field at fault, as the camelCase path; a Viewer may not write.
    /// </summary>
    [Test]
    public async Task RefusesWithTheFieldsAsync()
    {
        var bad = Recipe(1);
        bad["ingredients"]![0]!["number"] = 7;
        bad["ingredients"]![0]!["group"] = 12;
        bad["outcomes"]![0]!["boosts"]![0]!["max"] = 99;
        bad["baseChance"] = "lots";
        var (status, body) = await this.CallAsync("smithing.setConfig", new JsonObject { ["config"] = new JsonObject { ["recipes"] = new JsonArray(bad) }.ToJsonString(), ["apply"] = false }).ConfigureAwait(false);

        Assert.That(status, Is.EqualTo(400), body.ToJsonString());
        Assert.That(body["error"]!.GetValue<string>(), Is.EqualTo("invalid"));
        var fields = body["fields"]!.AsArray().Select(f => f!["field"]!.GetValue<string>()).ToList();
        Assert.That(fields, Is.SupersetOf(new[] { "recipes[0].ingredients[0]", "recipes[0].outcomes[0].boosts[0].max", "recipes[0].baseChance" }));
        Assert.That(body["fields"]!.AsArray().All(f => f!["message"]!.GetValue<string>().Length > 0), Is.True);

        Assert.That((await this.CallAsync("smithing.setConfig", new JsonObject { ["config"] = "not json", ["apply"] = false }).ConfigureAwait(false)).Status, Is.EqualTo(400));
        Assert.That((await this.CallAsync("smithing.setConfig", new JsonObject { ["config"] = new JsonObject { ["recipes"] = new JsonArray(Recipe(1)) }.ToJsonString(), ["apply"] = false }, "Viewer").ConfigureAwait(false)).Status, Is.EqualTo(403));
    }

    private static JsonObject Recipe(int number) => new()
    {
        ["number"] = number,
        ["name"] = "Edited",
        ["experience"] = 10,
        ["targets"] = new JsonArray("armor", "ring"),
        ["minimumItemLevel"] = 0,
        ["excellent"] = null,
        ["ingredients"] = new JsonArray(new JsonObject { ["group"] = 14, ["number"] = 1004, ["minimum"] = 1, ["maximum"] = 2 }),
        ["baseChance"] = 5000,
        ["maximumChance"] = 9000,
        ["catalysts"] = new JsonArray(),
        ["onFail"] = "ingredientsLost",
        ["risky"] = false,
        ["shape"] = new JsonObject { ["stars"] = new JsonArray(0, 1, 2), ["ordered"] = true, ["match"] = "exact", ["extensionPercent"] = 10, ["maximumExtension"] = 3 },
        ["hint"] = new JsonObject { ["kind"] = "riddle", ["text"] = "A riddle.", ["group"] = 0, ["number"] = 0 },
        ["outcomes"] = new JsonArray(new JsonObject { ["weight"] = 1, ["tier"] = "common", ["boosts"] = new JsonArray(new JsonObject { ["stat"] = "defense", ["min"] = 1, ["max"] = 2.5 }) }),
    };

    private Task<(int Status, JsonObject Body)> CallAsync(string operation, JsonObject arguments, string role = "Operator")
        => SiteBridge.InvokeAsync(this._services, operation, arguments, "devgm", role, default);

    private async Task PreviewAndApplyAsync(string operation, JsonObject arguments)
    {
        arguments["apply"] = false;
        var (status, preview) = await this.CallAsync(operation, arguments).ConfigureAwait(false);
        Assert.That(status, Is.EqualTo(200), preview.ToJsonString());
        arguments["apply"] = true;
        arguments["confirmationToken"] = preview["result"]!["confirmationToken"]!.GetValue<string>();
        var (applyStatus, applied) = await this.CallAsync(operation, arguments).ConfigureAwait(false);
        Assert.That(applyStatus, Is.EqualTo(200), applied.ToJsonString());
    }
}
