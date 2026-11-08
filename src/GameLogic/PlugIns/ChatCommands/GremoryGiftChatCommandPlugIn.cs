// <copyright file="GremoryGiftChatCommandPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns.ChatCommands;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.Administration;
using MUnique.OpenMU.GameLogic.GremoryCase;
using MUnique.OpenMU.GameLogic.PlugIns.ChatCommands.Arguments;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Puts an item into a gremory case of a character (Season 21: <c>/gremgif &lt;character&gt; &lt;case&gt; &lt;group&gt; &lt;number&gt;</c>,
/// case 1 the account, 2 the character).
/// </summary>
[Guid("B82F4C61-3E7D-4A95-9C10-6D3A8E2F5B47")]
[PlugIn]
[Display(Name = nameof(PlugInResources.GremoryGiftChatCommandPlugIn_Name), Description = nameof(PlugInResources.GremoryGiftChatCommandPlugIn_Description), ResourceType = typeof(PlugInResources))]
[ChatCommandHelp(Command, typeof(GremoryGiftChatCommandArgs), CharacterStatus.GameMaster)]
public class GremoryGiftChatCommandPlugIn : ChatCommandPlugInBase<GremoryGiftChatCommandArgs>
{
    private const string Command = "/gremgif";

    private const byte AccountCase = 1;

    /// <inheritdoc />
    public override string Key => Command;

    /// <inheritdoc />
    public override CharacterStatus MinCharacterStatusRequirement => CharacterStatus.GameMaster;

    /// <inheritdoc />
    protected override async ValueTask DoHandleCommandAsync(Player gameMaster, GremoryGiftChatCommandArgs arguments)
    {
        if (gameMaster.GameContext.GetPlayerByCharacterName(arguments.CharacterName ?? string.Empty) is not { SelectedCharacter: { } } target)
        {
            await gameMaster.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.CharacterNotFound), arguments.CharacterName).ConfigureAwait(false);
            return;
        }

        if (gameMaster.GameContext.Configuration.Items.FirstOrDefault(d => d.Group == arguments.Group && d.Number == arguments.Number) is not { } definition)
        {
            await gameMaster.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.ItemUnknown)).ConfigureAwait(false);
            return;
        }

        var item = GameMasterItemFactory.CreateItem(definition, new ItemSpecification(arguments.Group, arguments.Number, arguments.Level));
        var kind = arguments.Case == AccountCase ? GremoryCaseKind.Account : GremoryCaseKind.Character;
        if (await GremoryCaseService.TryAddAsync(target, item, kind).ConfigureAwait(false) is null)
        {
            await gameMaster.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.GremoryGiftFailed), target.Name).ConfigureAwait(false);
            return;
        }

        await gameMaster.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.GremoryGiftSent), definition.Name.ToString(), target.Name).ConfigureAwait(false);
    }
}
