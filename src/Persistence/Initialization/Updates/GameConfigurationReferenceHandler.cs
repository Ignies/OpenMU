// <copyright file="GameConfigurationReferenceHandler.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// A reference handler for plugin configurations which reference objects of the game configuration (monsters, drop
/// item groups, items, maps): it writes them by their id and reads them back from the game configuration.
/// </summary>
/// <param name="gameConfiguration">The game configuration.</param>
internal sealed class GameConfigurationReferenceHandler(GameConfiguration gameConfiguration) : ReferenceHandler
{
    /// <inheritdoc />
    public override ReferenceResolver CreateResolver() => new Resolver(gameConfiguration);

    private sealed class Resolver(GameConfiguration gameConfiguration) : ReferenceResolver
    {
        private readonly Dictionary<string, object> _added = [];

        /// <inheritdoc />
        public override void AddReference(string referenceId, object value)
        {
            // Objects without an id (e.g. the configuration itself) are written with an empty one.
            if (referenceId.Length > 0)
            {
                this._added[referenceId] = value;
            }
        }

        /// <inheritdoc />
        public override string GetReference(object value, out bool alreadyExists)
        {
            if (value is IIdentifiable identifiable)
            {
                alreadyExists = true;
                return identifiable.Id.ToString();
            }

            alreadyExists = false;
            return string.Empty;
        }

        /// <inheritdoc />
        public override object ResolveReference(string referenceId)
        {
            if (this._added.TryGetValue(referenceId, out var added))
            {
                return added;
            }

            IEnumerable<object> candidates = gameConfiguration.Monsters.Cast<object>()
                .Concat(gameConfiguration.DropItemGroups)
                .Concat(gameConfiguration.Items)
                .Concat(gameConfiguration.Maps);
            return candidates.FirstOrDefault(o => o is IIdentifiable identifiable && identifiable.Id.ToString() == referenceId)
                   ?? throw new JsonException($"The object '{referenceId}' is not in the configuration.");
        }
    }
}
