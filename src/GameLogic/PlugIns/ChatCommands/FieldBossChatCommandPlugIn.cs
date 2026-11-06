// <copyright file="FieldBossChatCommandPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns.ChatCommands;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.FieldBosses;
using MUnique.OpenMU.GameLogic.PlugIns.ChatCommands.Arguments;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// A chat command which spawns a field boss now, e.g. <c>/fieldboss nix</c>; without a name it lists the bosses and their state.
/// </summary>
[Guid("C7A3E915-4B2D-4F68-8E01-6D9B2F7A3C58")]
[PlugIn]
[Display(Name = nameof(PlugInResources.FieldBossChatCommandPlugIn_Name), Description = nameof(PlugInResources.FieldBossChatCommandPlugIn_Description), ResourceType = typeof(PlugInResources))]
[ChatCommandHelp(Command, typeof(FieldBossChatCommandArgs), CharacterStatus.GameMaster)]
public class FieldBossChatCommandPlugIn : ChatCommandPlugInBase<FieldBossChatCommandArgs>
{
    private const string Command = "/fieldboss";

    /// <inheritdoc />
    public override string Key => Command;

    /// <inheritdoc />
    public override CharacterStatus MinCharacterStatusRequirement => CharacterStatus.GameMaster;

    /// <inheritdoc />
    protected override async ValueTask DoHandleCommandAsync(Player gameMaster, FieldBossChatCommandArgs arguments)
    {
        if (FieldBossesFeaturePlugIn.Get(gameMaster.GameContext) is not { } plugIn)
        {
            await gameMaster.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.FieldBossFeatureOff)).ConfigureAwait(false);
            return;
        }

        if (string.IsNullOrWhiteSpace(arguments.Name) || plugIn.FindBoss(arguments.Name) is not { } boss)
        {
            var list = string.Join(", ", plugIn.GetStates(gameMaster.GameContext).Select(s => $"{s.Definition.Name} ({s.Status})"));
            await gameMaster.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.FieldBossList), list).ConfigureAwait(false);
            return;
        }

        var spawned = await plugIn.SpawnAsync(gameMaster.GameContext, boss).ConfigureAwait(false);
        await gameMaster.ShowLocalizedBlueMessageAsync(spawned ? nameof(PlayerMessage.FieldBossSpawnedByGameMaster) : nameof(PlayerMessage.FieldBossSpawnFailed), boss.Name).ConfigureAwait(false);
    }
}
