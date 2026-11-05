// <copyright file="ShowCreatedCharacterWidePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.Character;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.Views.Character;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Shows a created character to the Babylon client with its class code (<see cref="CharacterCreatedWide"/>).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.ShowCreatedCharacterWidePlugIn_Name), Description = nameof(PlugInResources.ShowCreatedCharacterWidePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("677F8B92-92C3-449B-93DB-2B47D5A8A44B")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class ShowCreatedCharacterWidePlugIn : IShowCreatedCharacterPlugIn
{
    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShowCreatedCharacterWidePlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public ShowCreatedCharacterWidePlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc/>
    public async ValueTask ShowCreatedCharacterAsync(Character character)
    {
        if (this._player.Connection is not { } connection || character.CharacterClass?.GetClassCode() is not { } classCode)
        {
            return;
        }

        await connection.SendCharacterCreatedWideAsync(character.Name, character.CharacterSlot, classCode).ConfigureAwait(false);
    }
}
