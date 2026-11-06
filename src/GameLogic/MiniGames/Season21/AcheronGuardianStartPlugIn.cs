// <copyright file="AcheronGuardianStartPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Opens the Protector of Acheron of Season 21 by its timetable. Off by default; the update of the event switches it on.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.AcheronGuardianStartPlugIn_Name), Description = nameof(PlugInResources.AcheronGuardianStartPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("611615E9-C969-40BE-B149-0A9F665C8FD3")]
public sealed class AcheronGuardianStartPlugIn : PlugIns.PeriodicTasks.MiniGameStartBasePlugIn<Season21EventStartConfiguration, Season21EventGameServerState>, IDisabledByDefault
{
    /// <inheritdoc />
    public override MiniGameType Key => MiniGameType.AcheronGuardian;

    /// <inheritdoc />
    public override object CreateDefaultConfig() => new Season21EventStartConfiguration
    {
        PreStartMessageDelay = TimeSpan.Zero,
        TaskDuration = TimeSpan.FromMinutes(37),
        Timetable = [new(4, 29), new(12, 0), new(23, 6)],
    };

    /// <inheritdoc />
    protected override Season21EventGameServerState CreateState(IGameContext gameContext) => new(gameContext, "Protector of Acheron");
}
