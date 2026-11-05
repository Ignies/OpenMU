// <copyright file="ShowCharacterListPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.Character;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.Views.Character;
using MUnique.OpenMU.GameServer.RemoteView.Guild;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.Network;
using MUnique.OpenMU.Network.Packets.ServerToClient;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The default implementation of the <see cref="IShowCharacterListPlugIn"/> which is forwarding everything to the game client with specific data packets.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.ShowCharacterListPlugIn_Name), Description = nameof(PlugInResources.ShowCharacterListPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("9563dd2c-85cc-4b23-aa95-9d1a18582032")]
[MinimumClient(5, 0, ClientLanguage.Invariant)]
public class ShowCharacterListPlugIn : IShowCharacterListPlugIn
{
    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShowCharacterListPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public ShowCharacterListPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc/>
    public async ValueTask ShowCharacterListAsync()
    {
        var connection = this._player.Connection;
        if (connection is null || this._player.Account is not { } account)
        {
            return;
        }

        var unlockFlags = CreateUnlockFlags(account);
        var characters = account.Characters.Where(this.IsShown).OrderBy(c => c.CharacterSlot).ToList();
        await this.SendCharacterListAsync(connection, account, characters, unlockFlags).ConfigureAwait(false);
        if (unlockFlags > CharacterCreationUnlockFlags.None)
        {
            await connection.SendCharacterClassCreationUnlockAsync(unlockFlags).ConfigureAwait(false);
        }

        await this.OnCharacterListSentAsync(connection, characters).ConfigureAwait(false);
    }

    /// <summary>
    /// Determines whether the list shows the character. A Season 6 client gets no character of a class which it doesn't
    /// know (<see cref="ClassCodes.GetSeason6Number"/>).
    /// </summary>
    /// <param name="character">The character.</param>
    /// <returns><c>true</c>, if the character is in the list.</returns>
    protected virtual bool IsShown(Character character)
        => character.CharacterClass is null || ClassCodes.GetSeason6Number(character.CharacterClass.Number) is not null;

    /// <summary>
    /// Called after the character list was sent, e.g. to send more about the characters.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="characters">The characters of the list, in slot order.</param>
    /// <returns>The task.</returns>
    protected virtual ValueTask OnCharacterListSentAsync(IConnection connection, IReadOnlyList<Character> characters) => ValueTask.CompletedTask;

    private static CharacterCreationUnlockFlags CreateUnlockFlags(Account account)
    {
        byte aggregatedFlags = 0;
        var result = account.UnlockedCharacterClasses?
            .Where(c => c.IsActive)
            .Select(c => c.CreationAllowedFlag)
            .Aggregate(aggregatedFlags, (current, flag) => (byte)(current | flag)) ?? 0;
        return (CharacterCreationUnlockFlags)result;
    }

    private async ValueTask SendCharacterListAsync(IConnection connection, Account account, IReadOnlyList<Character> characters, CharacterCreationUnlockFlags unlockFlags)
    {
        // The positions are written by index, so both loops go through the same list in slot order.
        var guildPositions = new GuildPosition?[characters.Count];
        int i = 0;
        foreach (var character in characters)
        {
            guildPositions[i] = await this._player.GameServerContext.GuildServer.GetGuildPositionAsync(character.Id).ConfigureAwait(false);
            i++;
        }

        var appearanceSerializer = this._player.AppearanceSerializer;
        int Write()
        {
            var size = CharacterListRef.GetRequiredSize(characters.Count);
            var span = connection.Output.GetSpan(size)[..size];
            var packet = new CharacterListRef(span)
            {
                UnlockFlags = unlockFlags,
                CharacterCount = (byte)characters.Count,
                IsVaultExtended = account.IsVaultExtended,
            };

            var j = 0;
            foreach (var character in characters)
            {
                var characterData = packet[j];
                characterData.SlotIndex = character.CharacterSlot;
                characterData.Name = character.Name;
                characterData.Level = (ushort)(character.Attributes.FirstOrDefault(s => s.Definition == Stats.Level)?.Value ?? 1);
                characterData.Status = character.CharacterStatus.Convert();
                characterData.GuildPosition = guildPositions[j].Convert();
                appearanceSerializer.WriteAppearanceData(characterData.Appearance, new CharacterAppearanceDataAdapter(character), false);
                j++;
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }
}