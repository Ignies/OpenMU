// <copyright file="SiteServices.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Web.AdminPanel.Operations.Site;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Builds the operations of one site call: the executor and every <c>*Operations</c> class of this assembly are made
/// here so that they see the site's caller; everything else (the shared confirmations, settings, persistence) comes
/// from the scope.
/// </summary>
/// <param name="scope">The call's scope.</param>
/// <param name="caller">The site's caller.</param>
internal sealed class SiteServices(IServiceProvider scope, IAdminOperationContext caller)
{
    private readonly Dictionary<Type, object> _built = [];

    /// <summary>Gets the service of a type for this call.</summary>
    /// <param name="type">The type.</param>
    /// <returns>The service.</returns>
    public object Get(Type type)
    {
        if (type == typeof(IAdminOperationContext))
        {
            return caller;
        }

        if (this._built.TryGetValue(type, out var known))
        {
            return known;
        }

        if (type.Assembly != typeof(SiteServices).Assembly || !(type == typeof(AdminOperationExecutor) || type.Name.EndsWith("Operations", StringComparison.Ordinal)))
        {
            return scope.GetRequiredService(type);
        }

        var constructor = type.GetConstructors().Single();
        var arguments = constructor.GetParameters().Select(p => p.HasDefaultValue ? scope.GetService(p.ParameterType) ?? p.DefaultValue : this.Get(p.ParameterType)).ToArray();
        var built = constructor.Invoke(arguments);
        this._built[type] = built;
        return built;
    }
}
