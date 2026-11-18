// <copyright file="ItemModerationOperationsTests.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Web.Tests.Site;

using System.Text.Json.Nodes;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.ItemCustomization;
using MUnique.OpenMU.Persistence;
using MUnique.OpenMU.Persistence.AdminAuth;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;
using MUnique.OpenMU.Web.AdminPanel.Operations;
using MUnique.OpenMU.Web.AdminPanel.Operations.Site;
using MUnique.OpenMU.Web.Tests.Mcp;

/// <summary>
/// Star Smithing moderation from the website (docs/star_smithing.md "Player text"): the word list, the same text check
/// the game runs, and an admin's reset of a name or lore with a reason, audited.
/// </summary>
[TestFixture]
public sealed class ItemModerationOperationsTests
{
    /// <summary>The type id of the fork's <c>TextModerationPlugIn</c>, which keeps the word list.</summary>
    private static readonly Guid TextModerationPlugIn = new("BD3EC1B2-3B76-4772-B8A5-8DFA8E9A2140");

    private ServiceProvider _services = null!;
    private InMemoryPersistenceContextProvider _persistence = null!;
    private PlugInConfiguration _plugin = null!;
    private InMemoryAdminAuditRepository _audit = null!;
    private Guid _record;
    private Guid _item;

    /// <summary>A configuration with the text moderation plugin (one word) and a customised item.</summary>
    [SetUp]
    public async Task SetUpAsync()
    {
        this._persistence = new InMemoryPersistenceContextProvider();
        using var context = this._persistence.CreateNewContext();
        var config = context.CreateNew<GameConfiguration>();
        this._plugin = context.CreateNew<PlugInConfiguration>();
        this._plugin.TypeId = TextModerationPlugIn;
        this._plugin.IsActive = true;
        this._plugin.CustomConfiguration = """{"Words":[{"Word":"badword","Substring":true,"Language":""}]}""";
        config.PlugInConfigurations.Add(this._plugin);
        var record = context.CreateNew<ItemCustomization>();
        this._item = Guid.NewGuid();
        record.ItemId = this._item;
        record.Name = "Frost Edge";
        record.Lore = "Forged in ice.";
        record.Version = 3;
        this._record = record.Id;
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

    /// <summary>The site checks a name or lore with the game's rules and the saved word list.</summary>
    [Test]
    public async Task CheckTextAsync()
    {
        var (status, passed) = await this.CallAsync("items.checkText", new JsonObject { ["kind"] = "name", ["text"] = "  Frost Edge " }, "Viewer").ConfigureAwait(false);
        Assert.That(status, Is.EqualTo(200), passed.ToJsonString());
        Assert.That((passed["result"]!["ok"]!.GetValue<bool>(), passed["result"]!["text"]!.GetValue<string>()), Is.EqualTo((true, "Frost Edge")));

        var (_, refused) = await this.CallAsync("items.checkText", new JsonObject { ["kind"] = "lore", ["text"] = "a B4DW0RD here" }, "Viewer").ConfigureAwait(false);
        Assert.That((refused["result"]!["ok"]!.GetValue<bool>(), refused["result"]!["refusal"]!.GetValue<string>(), refused["result"]!["word"]!.GetValue<string>()), Is.EqualTo((false, "word", "badword")));
    }

    /// <summary>The word list is read and replaced (trimmed, empty entries and repeats dropped), audited.</summary>
    [Test]
    public async Task WordListAsync()
    {
        var (_, before) = await this.CallAsync("items.words", new JsonObject(), "Viewer").ConfigureAwait(false);
        Assert.That(before["result"]!["words"]!.AsArray().Select(w => w!["word"]!.GetValue<string>()), Is.EqualTo(new[] { "badword" }));

        var words = new JsonArray(
            new JsonObject { ["word"] = " ass ", ["substring"] = false, ["language"] = "en" },
            new JsonObject { ["word"] = "badword", ["substring"] = true },
            new JsonObject { ["word"] = "BADWORD", ["substring"] = true },
            new JsonObject { ["word"] = " " });
        await this.PreviewAndApplyAsync("items.setWords", new JsonObject { ["words"] = words.ToJsonString() }).ConfigureAwait(false);

        var (_, after) = await this.CallAsync("items.words", new JsonObject(), "Viewer").ConfigureAwait(false);
        Assert.That(after["result"]!["words"]!.AsArray().Select(w => $"{w!["word"]!.GetValue<string>()}:{w["substring"]!.GetValue<bool>()}:{w["language"]!.GetValue<string>()}"), Is.EqualTo(new[] { "ass:False:en", "badword:True:" }));
        Assert.That((await this._audit.ReadAsync(0, 10).ConfigureAwait(false)).Items.Single().Operation, Is.EqualTo("items_set_words"));
        var (_, check) = await this.CallAsync("items.checkText", new JsonObject { ["kind"] = "name", ["text"] = "Ass Blade" }, "Viewer").ConfigureAwait(false);
        Assert.That(check["result"]!["word"]!.GetValue<string>(), Is.EqualTo("ass"));
    }

    /// <summary>
    /// An admin resets a name with a reason: the name goes, the lore stays, the version goes up so clients ask again,
    /// the game's cache forgets the item, and the reset is an audit row with the reason.
    /// </summary>
    [Test]
    public async Task ResetNameAsync()
    {
        ItemCustomizationCache.Store(this._item, new ItemCustomization { ItemId = this._item, Name = "Frost Edge" }, long.MaxValue);
        await this.PreviewAndApplyAsync("items.resetText", new JsonObject { ["customizationId"] = this._record.ToString(), ["field"] = "name", ["reason"] = "Offensive" }).ConfigureAwait(false);

        using var context = this._persistence.CreateNewContext();
        var record = await context.GetByIdAsync<ItemCustomization>(this._record).ConfigureAwait(false);
        Assert.That((record!.Name, record.Lore, record.Version), Is.EqualTo(((string?)null, "Forged in ice.", 4)));
        Assert.That(ItemCustomizationCache.TryGet(this._item, 0, out _), Is.False);
        var entry = (await this._audit.ReadAsync(0, 10).ConfigureAwait(false)).Items.Single();
        Assert.That((entry.Operation, entry.ResultJson.Contains("Offensive", StringComparison.Ordinal), entry.ResultJson.Contains("Frost Edge", StringComparison.Ordinal)), Is.EqualTo(("items_reset_text", true, true)));
    }

    /// <summary>A reset needs a reason, a known field and the Operator role.</summary>
    [Test]
    public async Task ResetIsRefusedAsync()
    {
        var id = this._record.ToString();
        Assert.That((await this.CallAsync("items.resetText", new JsonObject { ["customizationId"] = id, ["field"] = "name", ["reason"] = " ", ["apply"] = false }).ConfigureAwait(false)).Status, Is.EqualTo(400));
        Assert.That((await this.CallAsync("items.resetText", new JsonObject { ["customizationId"] = id, ["field"] = "look", ["reason"] = "x", ["apply"] = false }).ConfigureAwait(false)).Status, Is.EqualTo(400));
        Assert.That((await this.CallAsync("items.resetText", new JsonObject { ["customizationId"] = id, ["field"] = "name", ["reason"] = "x", ["apply"] = false }, "Viewer").ConfigureAwait(false)).Status, Is.EqualTo(403));
    }

    private Task<(int Status, JsonObject Body)> CallAsync(string operation, JsonObject arguments, string role = "Operator")
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
