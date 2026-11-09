// <copyright file="StartTormentedSquareEventChatCommandPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns.ChatCommands;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// A chat command plugin which handles the startts command: starts the tormented square of Season 21.
/// If an event is already running, it is stopped (all players are removed from it) and
/// a new event starts with fresh instances.
/// </summary>
[Guid("8F3B6E21-4C7A-4D95-B0E3-2A6D9C51F7B4")]
[PlugIn]
[Display(Name = nameof(PlugInResources.StartTormentedSquareEventChatCommandPlugIn_Name), Description = nameof(PlugInResources.StartTormentedSquareEventChatCommandPlugIn_Description), ResourceType = typeof(PlugInResources))]
[ChatCommandHelp(Command, CharacterStatus.GameMaster)]
public class StartTormentedSquareEventChatCommandPlugIn : StartMiniGameEventChatCommandPlugInBase
{
    private const string Command = "/startts";

    /// <inheritdoc />
    public override string Key => Command;

    /// <inheritdoc />
    protected override MiniGameType MiniGameType => MiniGameType.TormentedSquare;
}
