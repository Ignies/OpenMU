// <copyright file="StartBossBattleEventChatCommandPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns.ChatCommands;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// A chat command plugin which handles the startbb command: starts the boss battle together of Season 21.
/// If an event is already running, it is stopped (all players are removed from it) and
/// a new event starts with fresh instances.
/// </summary>
[Guid("1C9E4A73-6B2D-4F80-9A15-E7D3B0C84F26")]
[PlugIn]
[Display(Name = nameof(PlugInResources.StartBossBattleEventChatCommandPlugIn_Name), Description = nameof(PlugInResources.StartBossBattleEventChatCommandPlugIn_Description), ResourceType = typeof(PlugInResources))]
[ChatCommandHelp(Command, CharacterStatus.GameMaster)]
public class StartBossBattleEventChatCommandPlugIn : StartMiniGameEventChatCommandPlugInBase
{
    private const string Command = "/startbb";

    /// <inheritdoc />
    public override string Key => Command;

    /// <inheritdoc />
    protected override MiniGameType MiniGameType => MiniGameType.BossBattle;
}
