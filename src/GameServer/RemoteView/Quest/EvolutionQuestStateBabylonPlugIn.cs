// <copyright file="EvolutionQuestStateBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.Quest;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.EvolutionQuests;
using MUnique.OpenMU.GameLogic.Views.Quest;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Shows the state towards the next evolution to the Babylon client (<see cref="Network.Packets.Babylon.EvolutionQuestState"/>).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.EvolutionQuestStateBabylonPlugIn_Name), Description = nameof(PlugInResources.EvolutionQuestStateBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("5D2E8B71-C4A9-4F36-8E0B-7A1D9C3F6B24")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class EvolutionQuestStateBabylonPlugIn : IEvolutionQuestStatePlugIn
{
    /// <summary>
    /// The value of <see cref="Network.Packets.Babylon.EvolutionQuestState.NextEvolution"/> when no quest leads further.
    /// </summary>
    internal const byte NoEvolution = 0xFF;

    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="EvolutionQuestStateBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public EvolutionQuestStateBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc/>
    public async ValueTask ShowEvolutionQuestStateAsync(GameLogic.EvolutionQuests.EvolutionQuestState state)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        await connection.SendEvolutionQuestStateAsync(
            state.NextEvolution is { } evolution ? (byte)evolution : NoEvolution,
            state.RuleMet,
            state.ExperienceHeld,
            (byte)state.LevelKey,
            (ushort)Math.Clamp(state.RequiredLevel, 0, ushort.MaxValue)).ConfigureAwait(false);
    }
}
