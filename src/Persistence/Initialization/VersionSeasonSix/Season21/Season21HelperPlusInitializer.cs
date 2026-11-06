// <copyright file="Season21HelperPlusInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.GameLogic.HelperPlus;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Configures the <see cref="HelperPlusPlugIn"/> with the hunting grounds and zones of <c>Character/MuHelperPlus.xml</c>.
/// </summary>
internal class Season21HelperPlusInitializer
{
    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21HelperPlusInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Season21HelperPlusInitializer(IContext context, GameConfiguration gameConfiguration)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
    }

    /// <summary>
    /// Applies the MU Helper Plus.
    /// </summary>
    public void Initialize()
    {
        var configuration = Season21PlugInConfigurations.GetOrCreate<HelperPlusConfiguration>(this._context, this._gameConfiguration, typeof(HelperPlusPlugIn).GUID, out var plugIn);
        if (configuration is null || configuration.Zones.Count > 0)
        {
            // A configuration which can't be read, or one which already has its zones, is the operator's.
            return;
        }

        var data = Season21WorldSystemsData.Instance.HelperPlus;
        configuration.Grounds = data.Grounds.Select(g => new HelperPlusGround { Index = g.Index, WarpIndex = g.Move, Name = g.Name }).ToList();
        configuration.Zones = data.Zones.Select(z => new HelperPlusZone { Index = z.Index, Ground = z.Ground, MapNumber = z.Map, Element = z.Element, X = z.X, Y = z.Y }).ToList();

        // The first application turns the feature on; later ones leave the operator's switch alone.
        plugIn.IsActive = true;
        plugIn.SetConfiguration(configuration, null);
    }
}
