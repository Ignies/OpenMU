// <copyright file="SiteOperation.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Web.AdminPanel.Operations.Site;

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;

/// <summary>One operation: its method, the role it needs, and its arguments bound from JSON by parameter name.</summary>
/// <param name="Name">The name.</param>
/// <param name="Service">The class that has it.</param>
/// <param name="Method">The method.</param>
/// <param name="Role">The role it needs.</param>
/// <param name="ReadOnly">Whether it only reads.</param>
internal sealed record SiteOperation(string Name, Type Service, MethodInfo Method, string Role, bool ReadOnly)
{
    private static readonly NullabilityInfoContext Nullability = new();

    /// <summary>Describes the operation for the site.</summary>
    /// <param name="allowed">Whether the caller's role reaches it.</param>
    /// <returns>Its name, role, whether it only reads, and its arguments.</returns>
    public JsonObject Describe(bool allowed) => new()
    {
        ["name"] = this.Name,
        ["role"] = this.Role,
        ["readOnly"] = this.ReadOnly,
        ["allowed"] = allowed,
        ["arguments"] = new JsonArray(this.Method.GetParameters().Where(p => p.ParameterType != typeof(CancellationToken))
            .Select(p => (JsonNode)new JsonObject { ["name"] = p.Name, ["type"] = (Nullable.GetUnderlyingType(p.ParameterType) ?? p.ParameterType).Name, ["optional"] = IsOptional(p) }).ToArray()),
    };

    /// <summary>Runs the operation on a built service.</summary>
    /// <param name="service">The service.</param>
    /// <param name="arguments">The arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result as JSON.</returns>
    public async Task<JsonNode?> InvokeAsync(object service, JsonObject arguments, CancellationToken cancellationToken)
    {
        var values = this.Method.GetParameters().Select(p => Bind(p, arguments, cancellationToken)).ToArray();
        object? result;
        try
        {
            result = this.Method.Invoke(service, values);
            if (result is not null && this.Method.ReturnType.IsGenericType && this.Method.ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
            {
                result = this.Method.ReturnType.GetMethod(nameof(ValueTask<int>.AsTask))!.Invoke(result, null);
            }

            if (result is Task task)
            {
                await task.ConfigureAwait(false);
                #pragma warning disable VSTHRD103 // the task has completed: it was awaited above
                result = task.GetType().GetProperty(nameof(Task<int>.Result))?.GetValue(task);
#pragma warning restore VSTHRD103
            }
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }

        return result as JsonNode ?? JsonSerializer.SerializeToNode(result, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    private static bool IsOptional(ParameterInfo parameter)
        => parameter.HasDefaultValue || Nullable.GetUnderlyingType(parameter.ParameterType) is not null
           || (!parameter.ParameterType.IsValueType && Nullability.Create(parameter).WriteState == NullabilityState.Nullable);

    private static object? Bind(ParameterInfo parameter, JsonObject arguments, CancellationToken cancellationToken)
    {
        if (parameter.ParameterType == typeof(CancellationToken))
        {
            return cancellationToken;
        }

        var node = arguments[parameter.Name!];
        if (node is null)
        {
            return parameter.HasDefaultValue ? parameter.DefaultValue
                : IsOptional(parameter) ? null
                : throw new ArgumentException($"The argument {parameter.Name} is missing.", parameter.Name);
        }

        if (parameter.ParameterType == typeof(string) && node is JsonObject or JsonArray)
        {
            return node.ToJsonString();
        }

        try
        {
            return node.Deserialize(parameter.ParameterType, ConfigurationPatch.Options);
        }
        catch (Exception ex) when (ex is JsonException or FormatException or InvalidOperationException)
        {
            throw new ArgumentException($"The argument {parameter.Name} must be a {(Nullable.GetUnderlyingType(parameter.ParameterType) ?? parameter.ParameterType).Name}.", parameter.Name, ex);
        }
    }
}
