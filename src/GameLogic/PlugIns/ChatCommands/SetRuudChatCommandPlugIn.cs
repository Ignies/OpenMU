// <copyright file="SetRuudChatCommandPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns.ChatCommands;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.PlugIns.ChatCommands.Arguments;
using MUnique.OpenMU.GameLogic.Ruud;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Sets the Ruud of a character (Season 21: <c>/setruud &lt;character&gt; &lt;amount&gt;</c>).
/// </summary>
[Guid("5E9B3D17-0C6A-4F28-8D41-7B2E9C5A1F63")]
[PlugIn]
[Display(Name = nameof(PlugInResources.SetRuudChatCommandPlugIn_Name), Description = nameof(PlugInResources.SetRuudChatCommandPlugIn_Description), ResourceType = typeof(PlugInResources))]
[ChatCommandHelp(Command, typeof(SetRuudChatCommandArgs), CharacterStatus.GameMaster)]
public class SetRuudChatCommandPlugIn : ChatCommandPlugInBase<SetRuudChatCommandArgs>
{
    private const string Command = "/setruud";

    /// <inheritdoc />
    public override string Key => Command;

    /// <inheritdoc />
    public override CharacterStatus MinCharacterStatusRequirement => CharacterStatus.GameMaster;

    /// <inheritdoc />
    protected override async ValueTask DoHandleCommandAsync(Player gameMaster, SetRuudChatCommandArgs arguments)
    {
        if (RuudFeaturePlugIn.Get(gameMaster.GameContext) is null)
        {
            await gameMaster.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.RuudFeatureOff)).ConfigureAwait(false);
            return;
        }

        if (gameMaster.GameContext.GetPlayerByCharacterName(arguments.CharacterName ?? string.Empty) is not { SelectedCharacter: { } } target)
        {
            await gameMaster.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.CharacterNotFound), arguments.CharacterName).ConfigureAwait(false);
            return;
        }

        await target.SetRuudAsync(arguments.Amount).ConfigureAwait(false);
        await gameMaster.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.SetRuudResult), target.Name, target.GetRuud()).ConfigureAwait(false);
    }
}
