// <copyright file="Season21EventsFeaturePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Holds the definitions of the events of Season 21 which run as mini games (Tormented Square, Labyrinth of
/// Dimensions, Protector of Acheron, Boss Battle Together). Each event is switched by its start plugin or entrance.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.Season21EventsFeaturePlugIn_Name), Description = nameof(PlugInResources.Season21EventsFeaturePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("EF98A1E3-573C-4F97-BA98-72C2AF87D685")]
public class Season21EventsFeaturePlugIn : IFeaturePlugIn, ISupportCustomConfiguration<Season21EventsConfiguration>, ISupportDefaultCustomConfiguration
{
    /// <inheritdoc />
    public Season21EventsConfiguration? Configuration { get; set; }

    /// <summary>
    /// Gets the configuration of the events of a game, or an empty one.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The configuration.</returns>
    public static Season21EventsConfiguration GetConfiguration(IGameContext gameContext)
        => gameContext.FeaturePlugIns.GetPlugIn<Season21EventsFeaturePlugIn>()?.Configuration ?? new Season21EventsConfiguration();

    /// <inheritdoc />
    public object CreateDefaultConfig() => new Season21EventsConfiguration();
}
