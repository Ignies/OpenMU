// <copyright file="EvolveChatCommandPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns.ChatCommands;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.PlayerActions.Character;
using MUnique.OpenMU.GameLogic.Views.Character;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// A chat command of game masters which evolves a character into its next class, without the level rules
/// (see <see cref="EvolveCharacterAction"/>).
/// </summary>
[Guid("01D1F968-0EDF-41A7-A366-7C9864D09BB4")]
[PlugIn]
[Display(Name = nameof(PlugInResources.EvolveChatCommandPlugIn_Name), Description = nameof(PlugInResources.EvolveChatCommandPlugIn_Description), ResourceType = typeof(PlugInResources))]
[ChatCommandHelp(Command, typeof(Arguments), MinimumStatus)]
public class EvolveChatCommandPlugIn : ChatCommandPlugInBase<EvolveChatCommandPlugIn.Arguments>, IDisabledByDefault
{
    private const string Command = "/evolve";
    private const CharacterStatus MinimumStatus = CharacterStatus.GameMaster;

    /// <inheritdoc />
    public override string Key => Command;

    /// <inheritdoc />
    public override CharacterStatus MinCharacterStatusRequirement => MinimumStatus;

    /// <inheritdoc />
    protected override async ValueTask DoHandleCommandAsync(Player player, Arguments arguments)
    {
        var targetPlayer = player;
        if (arguments?.CharacterName is { } characterName)
        {
            targetPlayer = player.GameContext.GetPlayerByCharacterName(characterName);
            if (targetPlayer?.SelectedCharacter is null
                || !targetPlayer.SelectedCharacter.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase))
            {
                await player.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.CharacterNotFound), characterName).ConfigureAwait(false);
                return;
            }
        }

        if (targetPlayer.SelectedCharacter is not { } character)
        {
            return;
        }

        if (await new EvolveCharacterAction().TryEvolveAsync(targetPlayer, false, ClassEvolutionReason.GameMaster).ConfigureAwait(false))
        {
            await player.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.CharacterEvolved), character.Name, character.CharacterClass?.Name.ToString() ?? string.Empty).ConfigureAwait(false);
        }
        else
        {
            await player.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.CharacterCannotEvolve), character.Name).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Arguments for the evolve chat command.
    /// </summary>
    public class Arguments : ArgumentsBase
    {
        /// <summary>
        /// Gets or sets the name of the character to evolve; the own one if empty.
        /// </summary>
        [ValueReference(ChatCommandValueReference.CharacterName)]
        public string? CharacterName { get; set; }
    }
}
