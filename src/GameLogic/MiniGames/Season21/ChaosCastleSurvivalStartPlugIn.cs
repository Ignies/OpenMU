// <copyright file="ChaosCastleSurvivalStartPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.PlugIns.PeriodicTasks;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Opens the Chaos Castle Survival of the Fittest by the timetable of S21Emu <c>Event/ChaosCastleSurvival.dat</c>, on the
/// days of the tournament (<see cref="ChaosCastleSurvivalPlugIn"/>). Off by default; the update of the event switches it on.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.ChaosCastleSurvivalStartPlugIn_Name), Description = nameof(PlugInResources.ChaosCastleSurvivalStartPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("B6E05F38-2C17-4A9D-8E63-F41A07D25C98")]
public sealed class ChaosCastleSurvivalStartPlugIn : MiniGameStartBasePlugIn<Season21EventStartConfiguration, Season21EventGameServerState>, IDisabledByDefault
{
    /// <inheritdoc />
    public override MiniGameType Key => MiniGameType.ChaosCastleSurvival;

    /// <inheritdoc />
    public override object CreateDefaultConfig() => new Season21EventStartConfiguration
    {
        PreStartMessageDelay = TimeSpan.Zero,
        TaskDuration = TimeSpan.FromMinutes(20),
        Timetable = [new(1, 25), new(4, 35), new(5, 0), new(7, 0), new(9, 0), new(11, 0), new(13, 0), new(15, 0), new(17, 0), new(19, 0), new(21, 4), new(23, 0)],
    };

    /// <inheritdoc />
    protected override bool IsItTimeToStart(IGameContext gameContext)
        => base.IsItTimeToStart(gameContext)
           && ChaosCastleSurvivalPlugIn.Get(gameContext) is { } tournament
           && tournament.MayStart(tournament.GetLocalNow(gameContext));

    /// <inheritdoc />
    protected override Season21EventGameServerState CreateState(IGameContext gameContext) => new(gameContext, "Chaos Castle Survival");
}
