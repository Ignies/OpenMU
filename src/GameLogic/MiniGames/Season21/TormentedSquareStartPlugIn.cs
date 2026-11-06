// <copyright file="TormentedSquareStartPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.PlugIns.PeriodicTasks;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Opens the Tormented Square of Season 21 by its timetable. Off by default; the update of the event switches it on.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.TormentedSquareStartPlugIn_Name), Description = nameof(PlugInResources.TormentedSquareStartPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("55873D42-8805-4664-8F06-4F0899137687")]
public sealed class TormentedSquareStartPlugIn : MiniGameStartBasePlugIn<Season21EventStartConfiguration, Season21EventGameServerState>, IDisabledByDefault
{
    /// <inheritdoc />
    public override MiniGameType Key => MiniGameType.TormentedSquare;

    /// <inheritdoc />
    public override object CreateDefaultConfig() => new Season21EventStartConfiguration
    {
        PreStartMessageDelay = TimeSpan.Zero,
        TaskDuration = TimeSpan.FromMinutes(30),
        Timetable = [new(11, 44), new(17, 30), new(19, 0), new(20, 30), new(22, 0), new(23, 30)],
    };

    /// <inheritdoc />
    protected override Season21EventGameServerState CreateState(IGameContext gameContext) => new(gameContext, "Tormented Square");
}
