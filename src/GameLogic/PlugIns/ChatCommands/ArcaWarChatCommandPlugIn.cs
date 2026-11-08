// <copyright file="ArcaWarChatCommandPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns.ChatCommands;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.ArcaWar;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// A chat command plugin which handles the arcawar command: it opens the registration of the Arca War, or ends its
/// current stage, at the next tick of the war.
/// </summary>
[Guid("5573B33C-0842-48EA-9FBE-6DF5D5C49EB7")]
[PlugIn]
[Display(Name = nameof(PlugInResources.ArcaWarChatCommandPlugIn_Name), Description = nameof(PlugInResources.ArcaWarChatCommandPlugIn_Description), ResourceType = typeof(PlugInResources))]
[ChatCommandHelp(Command, CharacterStatus.GameMaster)]
public class ArcaWarChatCommandPlugIn : IChatCommandPlugIn
{
    private const string Command = "/arcawar";

    /// <inheritdoc />
    public string Key => Command;

    /// <inheritdoc/>
    public CharacterStatus MinCharacterStatusRequirement => CharacterStatus.GameMaster;

    /// <inheritdoc />
    public async ValueTask HandleCommandAsync(Player player, string command)
    {
        if (ArcaWarPlugIn.Get(player.GameContext) is not { Configuration: not null } arcaWar)
        {
            await player.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.MiniGameNotConfigured)).ConfigureAwait(false);
            return;
        }

        arcaWar.Advance(player.GameContext);
        await player.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.ArcaWarAdvanced)).ConfigureAwait(false);
    }
}
