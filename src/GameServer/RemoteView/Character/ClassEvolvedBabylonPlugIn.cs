// <copyright file="ClassEvolvedBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.Character;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.Character;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Shows the evolution of a character to the Babylon client (<see cref="ClassEvolved"/>).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.ClassEvolvedBabylonPlugIn_Name), Description = nameof(PlugInResources.ClassEvolvedBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("A1C7F2E4-5B83-4E0D-9A16-2F7B3D8C4E95")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class ClassEvolvedBabylonPlugIn : IClassEvolvedPlugIn
{
    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassEvolvedBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public ClassEvolvedBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc/>
    public async ValueTask ClassEvolvedAsync(Player evolvedPlayer, ClassEvolutionReason reason)
    {
        if (this._player.Connection is not { } connection
            || evolvedPlayer.SelectedCharacter?.CharacterClass?.GetClassCode() is not { } classCode)
        {
            return;
        }

        await connection.SendClassEvolvedAsync(evolvedPlayer.GetId(this._player), classCode, (byte)reason).ConfigureAwait(false);
    }
}
