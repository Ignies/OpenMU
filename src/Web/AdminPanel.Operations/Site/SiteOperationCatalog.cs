// <copyright file="SiteOperationCatalog.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Web.AdminPanel.Operations.Site;

using System.Reflection;

/// <summary>
/// The operations the website may call: every <see cref="AdminOperationAttribute"/> method of the listed classes, named
/// <c>area.method</c>. The single-object configuration writes stay out: the site writes through changesets
/// (<see cref="SiteConfigOperations"/>), so every site change is previewed, audited and revertible the same way.
/// </summary>
internal static class SiteOperationCatalog
{
    private static readonly (Type Service, string Area)[] Services =
    [
        (typeof(ConfigurationReadOperations), "config"),
        (typeof(SiteConfigOperations), "config"),
        (typeof(PluginReadOperations), "plugins"),
        (typeof(PluginWriteOperations), "plugins"),
        (typeof(ServerReadOperations), "servers"),
        (typeof(AccountReadOperations), "accounts"),
        (typeof(GuildReadOperations), "guilds"),
        (typeof(MiniGameReadOperations), "minigames"),
        (typeof(AuditReadOperations), "audit"),
    ];

    /// <summary>Gets every operation by name.</summary>
    public static IReadOnlyDictionary<string, SiteOperation> All { get; } = Build();

    private static Dictionary<string, SiteOperation> Build()
    {
        var all = new Dictionary<string, SiteOperation>(StringComparer.Ordinal);
        foreach (var (service, area) in Services)
        {
            foreach (var method in service.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (method.GetCustomAttribute<AdminOperationAttribute>() is not { } operation)
                {
                    continue;
                }

                var name = method.Name.EndsWith("Async", StringComparison.Ordinal) ? method.Name[..^5] : method.Name;
                var key = $"{area}.{char.ToLowerInvariant(name[0])}{name[1..]}";
                all.Add(key, new SiteOperation(key, service, method, operation.Role, operation.ReadOnly));
            }
        }

        return all;
    }
}
