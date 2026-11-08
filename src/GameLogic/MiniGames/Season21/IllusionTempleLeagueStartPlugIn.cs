// <copyright file="IllusionTempleLeagueStartPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.PlugIns.PeriodicTasks;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Opens the temples of the Illusion Temple League by the schedule of S21Emu <c>IllusionTempleLeague.xml</c> (13:00 to
/// 22:00 hourly, the entry 5 minutes before), on the days of the league (<see cref="IllusionTempleLeaguePlugIn"/>). Off
/// by default; the update of the league switches it on.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.IllusionTempleLeagueStartPlugIn_Name), Description = nameof(PlugInResources.IllusionTempleLeagueStartPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("9E4A1C62-7B35-4D80-A6F2-3D58C9E17B04")]
public sealed class IllusionTempleLeagueStartPlugIn : MiniGameStartBasePlugIn<Season21EventStartConfiguration, Season21EventGameServerState>, IDisabledByDefault
{
    /// <inheritdoc />
    public override MiniGameType Key => MiniGameType.IllusionTempleLeague;

    /// <inheritdoc />
    public override object CreateDefaultConfig() => new Season21EventStartConfiguration
    {
        PreStartMessageDelay = TimeSpan.Zero,
        TaskDuration = TimeSpan.FromMinutes(22),
        Timetable = [.. Enumerable.Range(12, 10).Select(hour => new TimeOnly(hour, 55))],
    };

    /// <inheritdoc />
    protected override bool IsItTimeToStart(IGameContext gameContext)
        => base.IsItTimeToStart(gameContext)
           && IllusionTempleLeaguePlugIn.Get(gameContext) is { } league
           && league.MayStart(league.GetLocalNow(gameContext));

    /// <inheritdoc />
    protected override Season21EventGameServerState CreateState(IGameContext gameContext) => new(gameContext, "Illusion Temple League");
}
