// <copyright file="ConfigurationStaleException.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Web.AdminPanel.Operations.Site;

using System.Text.Json.Nodes;

/// <summary>A change was based on a version of an object that is no longer the current one.</summary>
/// <param name="at">The row of the change.</param>
/// <param name="version">The current version.</param>
/// <param name="now">The current values.</param>
internal sealed class ConfigurationStaleException(int at, string version, JsonObject now)
    : Exception($"Row {at} was based on an older version; it changed since. Read it again.")
{
    /// <summary>Gets the row.</summary>
    public int At { get; } = at;

    /// <summary>Gets the current version.</summary>
    public string Version { get; } = version;

    /// <summary>Gets the current values.</summary>
    public JsonObject Now { get; } = now;
}
