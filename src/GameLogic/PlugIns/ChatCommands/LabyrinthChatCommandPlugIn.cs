// <copyright file="LabyrinthChatCommandPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns.ChatCommands;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.GameLogic.PlayerActions.MiniGames;
using MUnique.OpenMU.GameLogic.PlugIns.ChatCommands.Arguments;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// A chat command for game masters to test the Labyrinth of Dimensions: outside of it, <c>/labyrinth [floor]</c> enters
/// a run of his own without a ticket, at the floor when one is given; inside, it clears the floor he is on and goes on
/// with the given floor, or the next one.
/// </summary>
[Guid("8D2C5A41-7E93-4B06-A1F8-3C6E9B04D752")]
[PlugIn]
[Display(Name = nameof(PlugInResources.LabyrinthChatCommandPlugIn_Name), Description = nameof(PlugInResources.LabyrinthChatCommandPlugIn_Description), ResourceType = typeof(PlugInResources))]
[ChatCommandHelp(Command, typeof(LabyrinthChatCommandArgs), CharacterStatus.GameMaster)]
public class LabyrinthChatCommandPlugIn : ChatCommandPlugInBase<LabyrinthChatCommandArgs>
{
    private const string Command = "/labyrinth";

    /// <inheritdoc />
    public override string Key => Command;

    /// <inheritdoc />
    public override CharacterStatus MinCharacterStatusRequirement => CharacterStatus.GameMaster;

    /// <inheritdoc />
    protected override async ValueTask DoHandleCommandAsync(Player gameMaster, LabyrinthChatCommandArgs arguments)
    {
        if (gameMaster.CurrentMiniGame is Season21RoundsContext { Definition.Type: MiniGameType.LabyrinthOfDimensions } run)
        {
            run.Advance(arguments.Floor);
            await gameMaster.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.LabyrinthAdvanced), arguments.Floor > 0 ? arguments.Floor : run.Round + 1).ConfigureAwait(false);
            return;
        }

        if (gameMaster.CurrentMiniGame is not null)
        {
            await gameMaster.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.MiniGameEnterFailed)).ConfigureAwait(false);
            return;
        }

        if (gameMaster.GameContext.Configuration.MiniGameDefinitions.FirstOrDefault(d => d.Type == MiniGameType.LabyrinthOfDimensions && d.IsAvailable()) is not { Entrance: { } entrance } definition
            || await gameMaster.GameContext.MiniGames.GetOrCreateAsync(definition, gameMaster).ConfigureAwait(false) is not Season21RoundsContext labyrinth)
        {
            await gameMaster.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.MiniGameNotConfigured)).ConfigureAwait(false);
            return;
        }

        if (await labyrinth.TryEnterAsync(gameMaster).ConfigureAwait(false) is not EnterResult.Success)
        {
            await gameMaster.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.MiniGameEnterFailed)).ConfigureAwait(false);
            return;
        }

        if (arguments.Floor > 0)
        {
            labyrinth.Advance(arguments.Floor);
        }

        await gameMaster.WarpToAsync(entrance).ConfigureAwait(false);
    }
}
