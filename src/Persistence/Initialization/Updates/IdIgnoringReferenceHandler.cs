// <copyright file="IdIgnoringReferenceHandler.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Text.Json.Serialization;

/// <summary>
/// A reference handler which reads a stored plugin configuration without references: it accepts the <c>$id</c>
/// metadata which the data initialization writes (empty ids for objects without one) and ignores it.
/// </summary>
internal sealed class IdIgnoringReferenceHandler : ReferenceHandler
{
    /// <inheritdoc />
    public override ReferenceResolver CreateResolver() => new Resolver();

    private sealed class Resolver : ReferenceResolver
    {
        /// <inheritdoc />
        public override void AddReference(string referenceId, object value)
        {
            // The ids are ignored: a configuration without references never resolves one.
        }

        /// <inheritdoc />
        public override string GetReference(object value, out bool alreadyExists)
        {
            alreadyExists = false;
            return string.Empty;
        }

        /// <inheritdoc />
        public override object ResolveReference(string referenceId) => throw new NotSupportedException($"The configuration has no references, but '{referenceId}' was referenced.");
    }
}
