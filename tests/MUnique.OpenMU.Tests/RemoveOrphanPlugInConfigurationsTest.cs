// <copyright file="RemoveOrphanPlugInConfigurationsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Tests the update which removes the configurations of plugins this server no longer has: a world moved from upstream
/// OpenMU 0.9.10 logged "Unknown plugin type for id" for five of them on every start.
/// </summary>
[TestFixture]
public class RemoveOrphanPlugInConfigurationsTest
{
    /// <summary>The PlasmaStormSkillPlugIn of upstream 0.9.10, which the fork does not have.</summary>
    private static readonly Guid PlasmaStorm = new("5ef7c564-b32b-4630-9380-0233becfa663");

    /// <summary>
    /// The listed orphans go; a plugin of this server and one this server does not know (an external assembly's) stay.
    /// </summary>
    [Test]
    public async Task RemovesOnlyTheListedOrphansAsync()
    {
        var (context, configuration) = await CreateSeason6Async().ConfigureAwait(false);
        using var _ = context;
        var known = configuration.PlugInConfigurations.First().TypeId;
        var external = Guid.NewGuid();
        foreach (var typeId in new[] { PlasmaStorm, external })
        {
            var row = context.CreateNew<PlugInConfiguration>();
            row.TypeId = typeId;
            configuration.PlugInConfigurations.Add(row);
        }

        var logged = new CapturingLoggerFactory();
        await new RemoveOrphanPlugInConfigurationsPlugInSeason6(logged).ApplyUpdateAsync(context, configuration).ConfigureAwait(false);

        // The plugin manager reads every row of the context, not the configuration's collection: unlinked is not removed.
        var left = (await context.GetAsync<PlugInConfiguration>().ConfigureAwait(false)).Select(c => c.TypeId).ToList();
        Assert.That(left, Does.Not.Contain(PlasmaStorm));
        Assert.That(left, Does.Contain(known));
        Assert.That(left, Does.Contain(external));
        Assert.That(configuration.PlugInConfigurations.Select(c => c.TypeId), Does.Not.Contain(PlasmaStorm));
        Assert.That(logged.Messages, Has.One.Contains(PlasmaStorm.ToString()));
    }

    /// <summary>
    /// A row an earlier version of the update only unlinked from the configuration (left without one) is deleted too.
    /// </summary>
    [Test]
    public async Task DeletesAnUnlinkedOrphanAsync()
    {
        var (context, configuration) = await CreateSeason6Async().ConfigureAwait(false);
        using var _ = context;
        var unlinked = context.CreateNew<PlugInConfiguration>();
        unlinked.TypeId = PlasmaStorm;

        await new RemoveOrphanPlugInConfigurationsPlugInSeason6().ApplyUpdateAsync(context, configuration).ConfigureAwait(false);

        Assert.That((await context.GetAsync<PlugInConfiguration>().ConfigureAwait(false)).Select(c => c.TypeId), Does.Not.Contain(PlasmaStorm));
    }

    /// <summary>
    /// It runs on a new database and on an installed one alike, without being asked.
    /// </summary>
    [Test]
    public void IsMandatoryAndRunsOnNewDatabases()
    {
        var update = new RemoveOrphanPlugInConfigurationsPlugInSeason6();
        Assert.That(update.IsMandatory, Is.True);
        Assert.That(update.RunsOnNewDatabase, Is.True);
    }

    /// <summary>
    /// No listed type is a plugin of this server: the update must never remove a configuration that is in use.
    /// </summary>
    [Test]
    public void ListsNoPlugInOfThisServer()
    {
        foreach (var file in Directory.GetFiles(AppContext.BaseDirectory, "MUnique.OpenMU.*.dll"))
        {
            try
            {
                AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
            }
            catch (BadImageFormatException)
            {
                // not a managed assembly
            }
        }

        var ours = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => assembly.GetName().Name?.StartsWith("MUnique.OpenMU", StringComparison.Ordinal) is true)
            .SelectMany(SafePlugInTypes)
            .Select(type => type.GUID)
            .ToHashSet();

        // A scan that sees almost nothing would pass for the wrong reason.
        Assert.That(ours, Has.Count.GreaterThan(500));
        Assert.That(RemoveOrphanPlugInConfigurationsPlugInBase.RemovedPlugInTypes.Intersect(ours), Is.Empty);
    }

    private static IEnumerable<Type> SafePlugInTypes(Assembly assembly)
    {
        try
        {
            return PlugInManager.DiscoverPlugInTypes(assembly).ToList();
        }
        catch (ReflectionTypeLoadException)
        {
            return [];
        }
    }

    private static async Task<(IContext Context, GameConfiguration Configuration)> CreateSeason6Async()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        var dataInitialization = new Persistence.Initialization.VersionSeasonSix.DataInitialization(contextProvider, new NullLoggerFactory());
        await dataInitialization.CreateInitialDataAsync(1, true).ConfigureAwait(false);
        var context = contextProvider.CreateNewContext();
        return (context, (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First());
    }

    /// <summary>Keeps every formatted log message.</summary>
    private sealed class CapturingLoggerFactory : ILoggerFactory, ILoggerProvider, ILogger
    {
        public List<string> Messages { get; } = [];

        public ILogger CreateLogger(string categoryName) => this;

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => this.Messages.Add(formatter(state, exception));

        public void Dispose()
        {
        }
    }
}
