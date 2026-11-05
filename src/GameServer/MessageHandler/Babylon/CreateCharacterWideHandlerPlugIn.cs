// <copyright file="CreateCharacterWideHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.PlayerActions.Character;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles the <see cref="CreateCharacterWide"/> of the Babylon client: a character creation with a class code, so
/// every class of the sixteen can be asked for.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.CreateCharacterWideHandlerPlugIn_Name), Description = nameof(PlugInResources.CreateCharacterWideHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("73B7E763-3CC4-4032-850D-D21E6338AA3C")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class CreateCharacterWideHandlerPlugIn : ISubPacketHandlerPlugIn
{
    private readonly CreateCharacterAction _action = new();

    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => CreateCharacterWide.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < CreateCharacterWide.Length)
        {
            return;
        }

        CreateCharacterWide message = packet;
        int classNumber = ClassCodes.FromCode(message.ClassCode) is { } number ? number : -1;
        await this._action.CreateCharacterAsync(player, message.Name, classNumber).ConfigureAwait(false);
    }
}
