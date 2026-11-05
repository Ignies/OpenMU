// <copyright file="SiteBridgeOptions.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Web.AdminPanel.Operations.Site;

/// <summary>How the host's persistence reports saves (Startup registers it).</summary>
/// <param name="SavesPublishChanges">Whether a save already notifies the game about the changed objects (a database does, the in-memory demo does not).</param>
public sealed record SiteBridgeOptions(bool SavesPublishChanges);
