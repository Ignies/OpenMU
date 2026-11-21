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
