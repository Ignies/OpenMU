// <copyright file="ShowCharacterListBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.Character;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.Network;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The character list of the Babylon client: the Season 6 list with every character, followed by the class code of
/// each one (<see cref="CharacterClassCodes"/>), as the Season 6 appearance has 5 bits for the class, and by the classes
/// the creation offers with the state of <see cref="ClassCreationRule"/> (<see cref="CreatableClasses"/>).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.ShowCharacterListBabylonPlugIn_Name), Description = nameof(PlugInResources.ShowCharacterListBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("D9AA4904-A71E-4015-BD0E-83375626CB66")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class ShowCharacterListBabylonPlugIn : ShowCharacterListPlugIn
{
    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShowCharacterListBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public ShowCharacterListBabylonPlugIn(RemotePlayer player)
        : base(player)
    {
        this._player = player;
    }

    /// <inheritdoc/>
    protected override bool IsShown(Character character) => true;

    /// <inheritdoc/>
    protected override async ValueTask OnCharacterListSentAsync(IConnection connection, IReadOnlyList<Character> characters)
    {
        var codes = characters
            .Select(c => (c.CharacterSlot, Code: c.CharacterClass?.GetClassCode()))
            .Where(c => c.Code is not null)
            .ToList();

        int Write()
        {
            var size = CharacterClassCodesRef.GetRequiredSize(codes.Count);
            var span = connection.Output.GetSpan(size)[..size];
            var packet = new CharacterClassCodesRef(span) { CharacterCount = (byte)codes.Count };
            for (var i = 0; i < codes.Count; i++)
            {
                var entry = packet[i];
                entry.SlotIndex = codes[i].CharacterSlot;
                entry.ClassCode = codes[i].Code!.Value;
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
        await this.SendCreatableClassesAsync(connection).ConfigureAwait(false);
    }

    private async ValueTask SendCreatableClassesAsync(IConnection connection)
    {
        if (this._player.Account is not { } account)
        {
            return;
        }

        var classes = this._player.GameContext.Configuration.CharacterClasses
            .Where(c => c.CanGetCreated)
            .Select(c => (Class: c, Code: c.GetClassCode()))
            .Where(c => c.Code is not null)
            .OrderBy(c => c.Code)
            .Select(c => (Code: c.Code!.Value, State: ClassCreationRule.GetState(account, c.Class), Level: (ushort)Math.Max((short)0, c.Class.LevelRequirementByCreation)))
            .ToList();

        int Write()
        {
            var size = CreatableClassesRef.GetRequiredSize(classes.Count);
            var span = connection.Output.GetSpan(size)[..size];
            var packet = new CreatableClassesRef(span) { ClassCount = (byte)classes.Count };
            for (var i = 0; i < classes.Count; i++)
            {
                var entry = packet[i];
                entry.ClassCode = classes[i].Code;
                entry.State = (byte)classes[i].State;
                entry.Level = classes[i].Level;
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }
}
