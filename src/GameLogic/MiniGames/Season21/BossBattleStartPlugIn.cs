// <copyright file="BossBattleStartPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Opens the Boss Battle Together of Season 21 by its timetable. Off by default; the update of the event switches it on.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.BossBattleStartPlugIn_Name), Description = nameof(PlugInResources.BossBattleStartPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("957024AB-FE54-4D26-80BA-E3945C7DA137")]
public sealed class BossBattleStartPlugIn : PlugIns.PeriodicTasks.MiniGameStartBasePlugIn<Season21EventStartConfiguration, Season21EventGameServerState>, IDisabledByDefault
{
    /// <inheritdoc />
    public override MiniGameType Key => MiniGameType.BossBattle;

    /// <inheritdoc />
    public override object CreateDefaultConfig() => new Season21EventStartConfiguration
    {
        PreStartMessageDelay = TimeSpan.Zero,
        TaskDuration = TimeSpan.FromMinutes(12),
        Timetable = [new(0, 5)],
    };

    /// <inheritdoc />
    protected override Season21EventGameServerState CreateState(IGameContext gameContext) => new(gameContext, "Boss Battle Together");
}
