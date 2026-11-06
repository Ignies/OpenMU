// <copyright file="Season21PlugInConfigurations.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using System.Text.Json;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Sets the configuration of a feature plugin from a Season 21 update, keeping what the operator configured.
/// </summary>
internal static class Season21PlugInConfigurations
{
    /// <summary>
    /// Gets the configuration of a plugin, creating its row when the game has none.
    /// </summary>
    /// <typeparam name="T">The type of the configuration.</typeparam>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    /// <param name="typeId">The type id of the plugin.</param>
    /// <param name="plugIn">The plugin configuration row.</param>
    /// <returns>The stored configuration, a new one when none is stored, or <c>null</c> when the stored one can't be read.</returns>
    public static T? GetOrCreate<T>(IContext context, GameConfiguration gameConfiguration, Guid typeId, out PlugInConfiguration plugIn)
        where T : class, new()
    {
        if (gameConfiguration.PlugInConfigurations.FirstOrDefault(c => c.TypeId == typeId) is { } existing)
        {
            plugIn = existing;
        }
        else
        {
            plugIn = context.CreateNew<PlugInConfiguration>();
            plugIn.SetGuid(typeId);
            plugIn.TypeId = typeId;
            gameConfiguration.PlugInConfigurations.Add(plugIn);
        }

        try
        {
            return plugIn.GetConfiguration<T>(new IdIgnoringReferenceHandler()) ?? new T();
        }
        catch (JsonException)
        {
            // A configuration which can't be read is the operator's; it stays.
            return null;
        }
    }
}
