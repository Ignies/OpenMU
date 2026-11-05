// <copyright file="SiteActorContext.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Web.AdminPanel.Operations.Site;

using System.Threading;

/// <summary>The caller of one site call: the website's admin with the role the website granted.</summary>
/// <param name="actor">The actor.</param>
internal sealed class SiteActorContext(AdminActor actor) : IAdminOperationContext
{
    /// <inheritdoc />
    public ValueTask<AdminActor> GetActorAsync(CancellationToken cancellationToken) => ValueTask.FromResult(actor);
}
