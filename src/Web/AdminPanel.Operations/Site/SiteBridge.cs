// <copyright file="SiteBridge.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Web.AdminPanel.Operations.Site;

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MUnique.OpenMU.Persistence.AdminAuth;

/// <summary>
/// The website's way into the admin operations (Readme.md): the game bridge plugin of the website calls
/// <see cref="InvokeAsync(string, string, string, string, CancellationToken)"/> by reflection, so its signature only uses
/// framework types and the plugin still builds against an OpenMU without it. The site signs its admins in and checks
/// their flags; this checks the role it names against each operation's own.
/// </summary>
public static partial class SiteBridge
{
    private static IServiceProvider? _services;

    /// <summary>Gets a value indicating whether the bridge is attached to the running host.</summary>
    public static bool IsAttached => _services is not null;

    /// <summary>Attaches the bridge to the host's services (Startup, when the admin panel's operations are registered).</summary>
    /// <param name="services">The root service provider.</param>
    public static void Attach(IServiceProvider services) => _services = services;

    /// <summary>Runs one operation for the website.</summary>
    /// <param name="operation">The operation name, e.g. <c>config.change</c>; <c>ops</c> lists them.</param>
    /// <param name="argumentsJson">The arguments as a JSON object, by parameter name.</param>
    /// <param name="actor">The website login of the admin.</param>
    /// <param name="role">The admin role the website grants him: Viewer, Operator or Administrator.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The answer as JSON: <c>{"status": http status, "body": {ok, result | error, message}}</c>.</returns>
    public static async Task<string> InvokeAsync(string operation, string argumentsJson, string actor, string role, CancellationToken cancellationToken)
    {
        if (_services is not { } services)
        {
            return Wrap(503, Failure("unavailable", "The admin operations are not running in this process."));
        }

        JsonObject arguments;
        try
        {
            arguments = JsonNode.Parse(argumentsJson, documentOptions: new JsonDocumentOptions { MaxDepth = 16 }) as JsonObject ?? new JsonObject();
        }
        catch (JsonException)
        {
            return Wrap(400, Failure("invalid", "The arguments are not a JSON object."));
        }

        var (status, body) = await InvokeAsync(services, operation, arguments, actor, role, cancellationToken).ConfigureAwait(false);
        return Wrap(status, body);
    }

    /// <summary>Runs one operation in its own scope, as the given website admin.</summary>
    /// <param name="services">The root service provider.</param>
    /// <param name="operation">The operation name.</param>
    /// <param name="arguments">The arguments by parameter name.</param>
    /// <param name="actor">The website login.</param>
    /// <param name="role">The granted role.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The HTTP status and the answer.</returns>
    internal static async Task<(int Status, JsonObject Body)> InvokeAsync(IServiceProvider services, string operation, JsonObject arguments, string actor, string role, CancellationToken cancellationToken)
    {
        if (!LoginPattern().IsMatch(actor) || !AdminRoles.All.Contains(role, StringComparer.Ordinal))
        {
            return (400, Failure("invalid", "The site names an unknown admin or role."));
        }

        var granted = AdminRoles.GetEffectiveRoles(role).ToHashSet(StringComparer.Ordinal);
        if (operation == "ops")
        {
            return (200, Success(new JsonObject { ["operations"] = new JsonArray(SiteOperationCatalog.All.Values.OrderBy(o => o.Name, StringComparer.Ordinal).Select(o => (JsonNode)o.Describe(granted.Contains(o.Role))).ToArray()) }));
        }

        if (!SiteOperationCatalog.All.TryGetValue(operation, out var known))
        {
            return (404, Failure("unknown_operation", "No such operation. Call ops for the list."));
        }

        if (!granted.Contains(known.Role))
        {
            return (403, Failure("forbidden", $"The operation needs the {known.Role} role."));
        }

        await using var scope = services.CreateAsyncScope();
        var caller = new SiteActorContext(new AdminActor("site:" + actor, actor, "site", [role]));
        try
        {
            var built = new SiteServices(scope.ServiceProvider, caller);
            var result = await known.InvokeAsync(built.Get(known.Service), arguments, cancellationToken).ConfigureAwait(false);
            return (200, Success(result));
        }
        catch (ConfigurationStaleException stale)
        {
            var body = Failure("stale", stale.Message);
            body["at"] = stale.At;
            body["version"] = stale.Version;
            body["now"] = stale.Now;
            return (409, body);
        }
        catch (AdminResourceNotFoundException ex)
        {
            return (404, Failure("not_found", ex.Message));
        }
        catch (AdminOperationException ex)
        {
            return (ex.Kind switch { AdminOperationErrorKind.InvalidInput => 400, AdminOperationErrorKind.Forbidden => 403, AdminOperationErrorKind.Unavailable => 503, AdminOperationErrorKind.Uncertain => 500, _ => 409 },
                Failure(ex.Kind switch { AdminOperationErrorKind.InvalidInput => "invalid", _ => ex.Kind.ToString().ToLowerInvariant() }, ex.Message));
        }
        catch (ArgumentException ex)
        {
            return (400, Failure("invalid", ex.Message));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(SiteBridge)).LogError(ex, "Site bridge operation {Operation} for {Actor} failed.", operation, actor);
            return (500, Failure("failed", "The operation failed; the server log has the details."));
        }
    }

    private static JsonObject Success(JsonNode? result) => new() { ["ok"] = true, ["result"] = result };

    private static JsonObject Failure(string error, string message) => new() { ["ok"] = false, ["error"] = error, ["message"] = message };

    private static string Wrap(int status, JsonObject body) => new JsonObject { ["status"] = status, ["body"] = body }.ToJsonString();

    [GeneratedRegex("^[A-Za-z0-9_]{1,20}$")]
    private static partial Regex LoginPattern();
}
