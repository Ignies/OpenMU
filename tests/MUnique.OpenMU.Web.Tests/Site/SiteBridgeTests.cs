// <copyright file="SiteBridgeTests.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Web.Tests.Site;

using System.Text.Json.Nodes;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.Persistence;
using MUnique.OpenMU.Persistence.AdminAuth;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.Web.AdminPanel.Operations;
using MUnique.OpenMU.Web.AdminPanel.Operations.Site;
using MUnique.OpenMU.Web.Tests.Mcp;

/// <summary>The website's way into the configuration: catalog, roles, schema, versioned changes, bulk edits, revert.</summary>
[TestFixture]
public sealed class SiteBridgeTests
{
    private ServiceProvider _services = null!;
    private GameConfiguration _config = null!;
    private MonsterDefinition _monster = null!;
    private MonsterAttribute _health = null!;
    private AttributeDefinition _healthDefinition = null!;
    private GameMapDefinition _map = null!;
    private Mock<IConfigurationChangeMediatorListener> _changes = null!;
    private InMemoryAdminAuditRepository _audit = null!;

    /// <summary>A configuration with a monster (100 HP) spawned on a map, the operations and an in-memory audit.</summary>
    [SetUp]
    public async Task SetUpAsync()
    {
        var persistence = new InMemoryPersistenceContextProvider();
        using var context = persistence.CreateNewContext();
        this._config = context.CreateNew<GameConfiguration>();
        this._config.ExperienceRate = 3;
        this._healthDefinition = context.CreateNew<AttributeDefinition>(Guid.NewGuid(), "Maximum Health", string.Empty);
        this._config.Attributes.Add(this._healthDefinition);
        this._monster = context.CreateNew<MonsterDefinition>();
        this._monster.Number = 7;
        this._monster.Designation = "Spider||de=Spinne||@src=MonsterNames/Spider";
        this._health = context.CreateNew<MonsterAttribute>();
        this._health.AttributeDefinition = this._healthDefinition;
        this._health.Value = 100;
        this._monster.Attributes.Add(this._health);
        this._config.Monsters.Add(this._monster);
        this._map = context.CreateNew<GameMapDefinition>();
        this._map.Number = 0;
        this._map.Name = "Lorencia";
        var spawn = context.CreateNew<MonsterSpawnArea>();
        spawn.MonsterDefinition = this._monster;
        spawn.GameMap = this._map;
        spawn.Quantity = 5;
        this._map.MonsterSpawns.Add(spawn);
        this._config.Maps.Add(this._map);
        var drops = context.CreateNew<DropItemGroup>();
        drops.Description = "Jewels";
        drops.Chance = 0.25;
        this._config.DropItemGroups.Add(drops);
        await context.SaveChangesAsync().ConfigureAwait(false);

        var configurations = new Mock<IDataSource<GameConfiguration>>();
        configurations.Setup(c => c.GetOwnerAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(this._config);
        this._changes = new Mock<IConfigurationChangeMediatorListener>();
        this._audit = new InMemoryAdminAuditRepository();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAdminReadOperations();
        services.AddSingleton<IAdminAuditRepository>(this._audit);
        services.AddSingleton(configurations.Object);
        services.AddSingleton<IPersistenceContextProvider>(persistence);
        services.AddSingleton<IAdminReadModel, TestReadModel>();
        services.AddSingleton(this._changes.Object);
        services.AddSingleton(new SiteBridgeOptions(SavesPublishChanges: false));
        this._services = services.BuildServiceProvider();
    }

    /// <summary>Disposes the services.</summary>
    [TearDown]
    public async Task TearDownAsync() => await this._services.DisposeAsync().ConfigureAwait(false);

    /// <summary>The catalog names every site operation once; the upstream single-object writes stay out (one write path).</summary>
    [Test]
    public async Task CatalogListsOperationsAsync()
    {
        var (status, body) = await this.CallAsync("ops", new JsonObject()).ConfigureAwait(false);
        var names = body["result"]!["operations"]!.AsArray().Select(o => o!["name"]!.GetValue<string>()).ToList();
        Assert.That(status, Is.EqualTo(200));
        Assert.That(names, Does.Contain("config.schema").And.Contain("config.change").And.Contain("config.bulk").And.Contain("config.search").And.Contain("plugins.list"));
        Assert.That(names, Does.Not.Contain("config.update"));
        Assert.That(names, Is.Unique);
    }

    /// <summary>A viewer reads but cannot change; an unknown operation or role is refused.</summary>
    [Test]
    public async Task RolesAreCheckedAsync()
    {
        Assert.That((await this.CallAsync("config.schema", new JsonObject(), AdminRoles.Viewer).ConfigureAwait(false)).Status, Is.EqualTo(200));
        Assert.That((await this.CallAsync("config.change", new JsonObject { ["rows"] = "[]", ["apply"] = false }, AdminRoles.Viewer).ConfigureAwait(false)).Status, Is.EqualTo(403));
        Assert.That((await this.CallAsync("config.nothing", new JsonObject()).ConfigureAwait(false)).Status, Is.EqualTo(404));
        Assert.That((await SiteBridge.InvokeAsync(this._services, "config.schema", new JsonObject(), "admin", "Root", default).ConfigureAwait(false)).Status, Is.EqualTo(400));
    }

    /// <summary>The schema carries kinds, ranges and when a field applies.</summary>
    [Test]
    public async Task SchemaDescribesFieldsAsync()
    {
        var (_, body) = await this.CallAsync("config.schema", new JsonObject()).ConfigureAwait(false);
        var types = body["result"]!["types"]!.AsArray();
        JsonNode Field(string type, string field) => types.Single(t => t!["name"]!.GetValue<string>() == type)!["fields"]!.AsArray().Single(f => f!["name"]!.GetValue<string>() == field)!;
        Assert.That(Field("GameConfiguration", "ExperienceRate")["kind"]!.GetValue<string>(), Is.EqualTo("scalar"));
        Assert.That(Field("GameConfiguration", "ExperienceRate")["applies"]!.GetValue<string>(), Is.EqualTo("live"));
        Assert.That(Field("GameConfiguration", "ExperienceRate")["min"]!.GetValue<double>(), Is.EqualTo(0));
        Assert.That(Field("MonsterDefinition", "Attributes")["kind"]!.GetValue<string>(), Is.EqualTo("children"));
        Assert.That(Field("MonsterDefinition", "Attributes")["type"]!.GetValue<string>(), Is.EqualTo("MonsterAttribute"));
        Assert.That(Field("MonsterSpawnArea", "MonsterDefinition")["kind"]!.GetValue<string>(), Is.EqualTo("reference"));
        Assert.That(Field("DropItemGroup", "Chance")["max"]!.GetValue<double>(), Is.EqualTo(1));
        Assert.That(Field("GameServerDefinition", "ExperienceRate")["kind"]!.GetValue<string>(), Is.EqualTo("scalar"));
    }

    /// <summary>A change is previewed, then applied with its token; a stale version is refused with what is there now.</summary>
    [Test]
    public async Task VersionedChangeAsync()
    {
        var (_, read) = await this.CallAsync("config.read", new JsonObject { ["type"] = "GameConfiguration", ["id"] = this._config.GetId().ToString() }).ConfigureAwait(false);
        var version = read["result"]!["item"]!["version"]!.GetValue<string>();
        var rows = new JsonArray(new JsonObject { ["op"] = "update", ["type"] = "GameConfiguration", ["id"] = this._config.GetId().ToString(), ["version"] = version, ["fields"] = new JsonObject { ["ExperienceRate"] = 5 } });

        var applied = await this.PreviewAndApplyAsync("config.change", new JsonObject { ["rows"] = rows.ToJsonString() }).ConfigureAwait(false);
        Assert.That(applied["result"]!["status"]!.GetValue<string>(), Is.EqualTo("applied"));
        Assert.That(this._config.ExperienceRate, Is.EqualTo(5));
        Assert.That((await this._audit.ReadAsync(0, 10).ConfigureAwait(false)).Items.Single().Operation, Is.EqualTo("config_changeset"));

        var (status, stale) = await this.CallAsync("config.change", new JsonObject { ["rows"] = rows.ToJsonString(), ["apply"] = false }).ConfigureAwait(false);
        Assert.That(status, Is.EqualTo(409));
        Assert.That(stale["error"]!.GetValue<string>(), Is.EqualTo("stale"));
        Assert.That(stale["now"]!["ExperienceRate"]!.GetValue<float>(), Is.EqualTo(5));
    }

    /// <summary>A monster attribute's change tells the game about the attribute and its monster, so spawned ones reload.</summary>
    [Test]
    public async Task ChangeNotifiesOwnersAsync()
    {
        var rows = new JsonArray(new JsonObject { ["op"] = "update", ["type"] = "MonsterAttribute", ["id"] = this._health.GetId().ToString(), ["fields"] = new JsonObject { ["Value"] = 150 } });
        await this.PreviewAndApplyAsync("config.change", new JsonObject { ["rows"] = rows.ToJsonString() }).ConfigureAwait(false);
        Assert.That(this._health.Value, Is.EqualTo(150));
        this._changes.Verify(c => c.HandleConfigurationChangedAsync(It.IsAny<Type>(), this._monster.GetId(), It.IsAny<object>()), Times.Once);
        this._changes.Verify(c => c.HandleConfigurationChangedAsync(It.IsAny<Type>(), this._health.GetId(), It.IsAny<object>()), Times.Once);
    }

    /// <summary>"Every monster on Lorencia x0.8 HP": a bulk edit by owner and reference filter, then reverted from its audit row.</summary>
    [Test]
    public async Task BulkEditAndRevertAsync()
    {
        var args = new JsonObject
        {
            ["type"] = "MonsterAttribute",
            ["where"] = new JsonArray(new JsonObject { ["field"] = "AttributeDefinition", ["op"] = "eq", ["value"] = this._healthDefinition.GetId().ToString() }).ToJsonString(),
            ["owner"] = new JsonObject { ["type"] = "MonsterDefinition", ["ids"] = new JsonArray(this._monster.GetId().ToString()) }.ToJsonString(),
            ["field"] = "Value",
            ["mode"] = "multiply",
            ["value"] = 0.8,
        };
        await this.PreviewAndApplyAsync("config.bulk", args).ConfigureAwait(false);
        Assert.That(this._health.Value, Is.EqualTo(80));

        var entry = (await this._audit.ReadAsync(0, 10).ConfigureAwait(false)).Items.Single();
        Assert.That(entry.Operation, Is.EqualTo("config_bulk"));
        await this.PreviewAndApplyAsync("config.revert", new JsonObject { ["auditId"] = entry.Id.ToString() }).ConfigureAwait(false);
        Assert.That(this._health.Value, Is.EqualTo(100));
    }

    /// <summary>A spawn area is created and deleted through changesets; the game hears of both.</summary>
    [Test]
    public async Task CreateAndDeleteAsync()
    {
        var create = new JsonArray(new JsonObject
        {
            ["op"] = "create", ["parentType"] = "GameMapDefinition", ["parentId"] = this._map.GetId().ToString(), ["collection"] = "MonsterSpawns",
            ["fields"] = new JsonObject { ["Quantity"] = 3, ["MonsterDefinition"] = new JsonObject { ["$ref"] = this._monster.GetId().ToString() }, ["GameMap"] = new JsonObject { ["$ref"] = this._map.GetId().ToString() } },
        });
        await this.PreviewAndApplyAsync("config.change", new JsonObject { ["rows"] = create.ToJsonString() }).ConfigureAwait(false);
        var added = this._map.MonsterSpawns.Single(s => s.Quantity == 3);
        this._changes.Verify(c => c.HandleConfigurationAddedAsync(It.IsAny<Type>(), added.GetId(), It.IsAny<object>()), Times.Once);

        var delete = new JsonArray(new JsonObject { ["op"] = "delete", ["type"] = "MonsterSpawnArea", ["id"] = added.GetId().ToString() });
        await this.PreviewAndApplyAsync("config.change", new JsonObject { ["rows"] = delete.ToJsonString() }).ConfigureAwait(false);
        Assert.That(this._map.MonsterSpawns, Has.Count.EqualTo(1));
        this._changes.Verify(c => c.HandleConfigurationRemovedAsync(It.IsAny<Type>(), added.GetId()), Times.Once);
    }

    /// <summary>An exported monster imported after an edit is a changeset that puts the exported values back.</summary>
    [Test]
    public async Task ExportImportAsync()
    {
        var (_, export) = await this.CallAsync("config.export", new JsonObject { ["type"] = "MonsterDefinition", ["id"] = this._monster.GetId().ToString() }).ConfigureAwait(false);
        var data = export["result"]!.ToJsonString();
        Assert.That(export["result"]!["objects"]!.AsArray(), Has.Count.EqualTo(2));
        this._health.Value = 1;

        var (_, preview) = await this.CallAsync("config.import", new JsonObject { ["data"] = data, ["apply"] = false }).ConfigureAwait(false);
        Assert.That(preview["result"]!["change"]!["rows"]!.AsArray(), Has.Count.EqualTo(1));
        await this.PreviewAndApplyAsync("config.import", new JsonObject { ["data"] = data }).ConfigureAwait(false);
        Assert.That(this._health.Value, Is.EqualTo(100));
    }

    /// <summary>A list pages, searches and filters by owner.</summary>
    [Test]
    public async Task ListFiltersAsync()
    {
        var (_, all) = await this.CallAsync("config.list", new JsonObject { ["type"] = "MonsterSpawnArea", ["owner"] = new JsonObject { ["type"] = "GameMapDefinition", ["ids"] = new JsonArray(this._map.GetId().ToString()) }.ToJsonString() }).ConfigureAwait(false);
        Assert.That(all["result"]!["total"]!.GetValue<int>(), Is.EqualTo(1));
        Assert.That(all["result"]!["items"]![0]!["fields"]!["MonsterDefinition"]!["caption"]!.GetValue<string>(), Is.EqualTo("Spider"));
        var (_, found) = await this.CallAsync("config.list", new JsonObject { ["type"] = "MonsterDefinition", ["text"] = "spid" }).ConfigureAwait(false);
        Assert.That(found["result"]!["total"]!.GetValue<int>(), Is.EqualTo(1));
        var (_, none) = await this.CallAsync("config.list", new JsonObject { ["type"] = "MonsterDefinition", ["where"] = "[{\"field\":\"Number\",\"op\":\"gt\",\"value\":7}]" }).ConfigureAwait(false);
        Assert.That(none["result"]!["total"]!.GetValue<int>(), Is.EqualTo(0));
    }

    /// <summary>One search over every object of the configuration finds what a caption names, with its owner.</summary>
    [Test]
    public async Task FindAcrossEverythingAsync()
    {
        var (_, found) = await this.CallAsync("config.find", new JsonObject { ["text"] = "maximum health" }).ConfigureAwait(false);
        var hits = found["result"]!["items"]!.AsArray();
        Assert.That(hits.Select(h => h!["type"]!.GetValue<string>()), Does.Contain("MonsterAttribute").And.Contain("AttributeDefinition"));
        Assert.That(hits.Single(h => h!["type"]!.GetValue<string>() == "MonsterAttribute")!["owner"]!["caption"]!.GetValue<string>(), Is.EqualTo("Spider"));
    }

    /// <summary>Owned objects outside OpenMU's own type list (a mix's settings) are editable, and their owners hear of it.</summary>
    [Test]
    public async Task NestedChildrenAreEditableAsync()
    {
        using (var context = this._services.GetRequiredService<IPersistenceContextProvider>().CreateNewContext())
        {
            var crafting = context.CreateNew<MUnique.OpenMU.DataModel.Configuration.ItemCrafting.ItemCrafting>();
            crafting.Name = "Chaos Weapon";
            crafting.SimpleCraftingSettings = context.CreateNew<MUnique.OpenMU.DataModel.Configuration.ItemCrafting.SimpleCraftingSettings>();
            crafting.SimpleCraftingSettings.SuccessPercent = 50;
            this._monster.ItemCraftings.Add(crafting);
            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        var (_, schema) = await this.CallAsync("config.schema", new JsonObject()).ConfigureAwait(false);
        Assert.That(schema["result"]!["types"]!.AsArray().Select(t => t!["name"]!.GetValue<string>()), Does.Contain("SimpleCraftingSettings").And.Contain("ItemCraftingRequiredItem").And.Not.Contain("Item").And.Not.Contain("PlugInConfiguration"));
        var (_, list) = await this.CallAsync("config.list", new JsonObject { ["type"] = "SimpleCraftingSettings" }).ConfigureAwait(false);
        var settings = list["result"]!["items"]![0]!;
        var rows = new JsonArray(new JsonObject { ["op"] = "update", ["type"] = "SimpleCraftingSettings", ["id"] = settings["id"]!.GetValue<string>(), ["fields"] = new JsonObject { ["SuccessPercent"] = 70 } });
        await this.PreviewAndApplyAsync("config.change", new JsonObject { ["rows"] = rows.ToJsonString() }).ConfigureAwait(false);
        Assert.That(this._monster.ItemCraftings.Single().SimpleCraftingSettings!.SuccessPercent, Is.EqualTo(70));
        this._changes.Verify(c => c.HandleConfigurationChangedAsync(It.IsAny<Type>(), this._monster.GetId(), It.IsAny<object>()), Times.Once);
    }

    private Task<(int Status, JsonObject Body)> CallAsync(string operation, JsonObject arguments, string role = "Administrator")
        => SiteBridge.InvokeAsync(this._services, operation, arguments, "devgm", role, default);

    private async Task<JsonObject> PreviewAndApplyAsync(string operation, JsonObject arguments)
    {
        arguments["apply"] = false;
        var (status, preview) = await this.CallAsync(operation, arguments).ConfigureAwait(false);
        Assert.That(status, Is.EqualTo(200), preview.ToJsonString());
        arguments["apply"] = true;
        arguments["confirmationToken"] = preview["result"]!["confirmationToken"]!.GetValue<string>();
        var (applyStatus, applied) = await this.CallAsync(operation, arguments).ConfigureAwait(false);
        Assert.That(applyStatus, Is.EqualTo(200), applied.ToJsonString());
        return applied;
    }
}
