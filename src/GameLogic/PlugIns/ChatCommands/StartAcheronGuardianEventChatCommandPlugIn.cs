// <copyright file="StartAcheronGuardianEventChatCommandPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns.ChatCommands;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// A chat command plugin which handles the startag command: starts the protector of acheron of Season 21.
/// If an event is already running, it is stopped (all players are removed from it) and
/// a new event starts with fresh instances.
/// </summary>
[Guid("B5D27F96-3E18-4A6C-8D41-0F9A6C2E7B53")]
[PlugIn]
[Display(Name = nameof(PlugInResources.StartAcheronGuardianEventChatCommandPlugIn_Name), Description = nameof(PlugInResources.StartAcheronGuardianEventChatCommandPlugIn_Description), ResourceType = typeof(PlugInResources))]
[ChatCommandHelp(Command, CharacterStatus.GameMaster)]
public class StartAcheronGuardianEventChatCommandPlugIn : StartMiniGameEventChatCommandPlugInBase
{
    private const string Command = "/startag";

    /// <inheritdoc />
    public override string Key => Command;

    /// <inheritdoc />
    protected override MiniGameType MiniGameType => MiniGameType.AcheronGuardian;
}
