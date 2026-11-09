// <copyright file="GetRuudChatCommandPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns.ChatCommands;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.Ruud;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Shows the Ruud of a character, the game master's own without a name (Season 21: <c>/getruud [character]</c>).
/// </summary>
[Guid("A3F86D20-5C17-4E94-8B3A-0D7E6C29F185")]
[PlugIn]
[Display(Name = nameof(PlugInResources.GetRuudChatCommandPlugIn_Name), Description = nameof(PlugInResources.GetRuudChatCommandPlugIn_Description), ResourceType = typeof(PlugInResources))]
[ChatCommandHelp(Command, typeof(Arguments), CharacterStatus.GameMaster)]
public class GetRuudChatCommandPlugIn : ChatCommandPlugInBase<GetRuudChatCommandPlugIn.Arguments>
{
    private const string Command = "/getruud";

    /// <inheritdoc />
    public override string Key => Command;

    /// <inheritdoc />
    public override CharacterStatus MinCharacterStatusRequirement => CharacterStatus.GameMaster;

    /// <inheritdoc />
    protected override async ValueTask DoHandleCommandAsync(Player gameMaster, Arguments arguments)
    {
        if (RuudFeaturePlugIn.Get(gameMaster.GameContext) is null)
        {
            await gameMaster.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.RuudFeatureOff)).ConfigureAwait(false);
            return;
        }

        var target = string.IsNullOrWhiteSpace(arguments.CharacterName) ? gameMaster : await this.GetPlayerByCharacterNameAsync(gameMaster, arguments.CharacterName).ConfigureAwait(false);
        if (target?.SelectedCharacter is null)
        {
            return;
        }

        await gameMaster.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.RuudInfo), target.Name, target.GetRuud()).ConfigureAwait(false);
    }

    /// <summary>
    /// The arguments of the <see cref="GetRuudChatCommandPlugIn"/>.
    /// </summary>
    public class Arguments : ArgumentsBase
    {
        /// <summary>
        /// Gets or sets the name of the character; the game master's own when it's empty.
        /// </summary>
        [ValueReference(ChatCommandValueReference.CharacterName)]
        public string? CharacterName { get; set; }
    }
}
