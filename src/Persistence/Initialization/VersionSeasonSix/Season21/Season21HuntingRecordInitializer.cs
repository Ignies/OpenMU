// <copyright file="Season21HuntingRecordInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.GameLogic.HuntingRecord;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Configures the <see cref="HuntingRecordPlugIn"/> with the maps and excluded monsters of <c>HuntingRecord.txt</c>.
/// </summary>
internal class Season21HuntingRecordInitializer
{
    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21HuntingRecordInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Season21HuntingRecordInitializer(IContext context, GameConfiguration gameConfiguration)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
    }

    /// <summary>
    /// Applies the hunting record.
    /// </summary>
    public void Initialize()
    {
        var configuration = Season21PlugInConfigurations.GetOrCreate<HuntingRecordConfiguration>(this._context, this._gameConfiguration, typeof(HuntingRecordPlugIn).GUID, out var plugIn);
        if (configuration is null || configuration.Maps.Count > 0)
        {
            // A configuration which can't be read, or one which already has its maps, is the operator's.
            return;
        }

        var data = Season21WorldSystemsData.Instance.HuntingRecord;
        configuration.Maps = [.. data.Maps];
        configuration.ExcludedMonsters = [.. data.Excluded];

        // The first application turns the feature on; later ones leave the operator's switch alone.
        plugIn.IsActive = true;
        plugIn.SetConfiguration(configuration, null);
    }
}
