// <copyright file="IllusionTempleRenewalStatueTalkPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.MiniGames;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles talking to the statues of the renewed Illusion Temple: the Cursed Statue (658) gives the Cursed Hollow, a
/// Captured Stone Statue (659 to 668) seals or unseals it.
/// </summary>
[Guid("2F6B8D14-9C3E-4A75-B0E1-58A7D2C49F36")]
[PlugIn]
[Display(Name = nameof(PlugInResources.IllusionTempleRenewalStatueTalkPlugIn_Name), Description = nameof(PlugInResources.IllusionTempleRenewalStatueTalkPlugIn_Description), ResourceType = typeof(PlugInResources))]
public class IllusionTempleRenewalStatueTalkPlugIn : IPlayerTalkToNpcPlugIn
{
    private const short CursedStatueNumber = 658;

    private const short FirstStoneNumber = 659;

    private const short LastStoneNumber = 668;

    /// <inheritdoc />
    public async ValueTask PlayerTalksToNpcAsync(Player player, NonPlayerCharacter npc, NpcTalkEventArgs eventArgs)
    {
        if (player.CurrentMiniGame is not IllusionTempleContext illusionTemple
            || npc.Definition.Number is < CursedStatueNumber or > LastStoneNumber)
        {
            return;
        }

        eventArgs.HasBeenHandled = true;
        if (npc.Definition.Number >= FirstStoneNumber)
        {
            await illusionTemple.TalkToNpcCapturedStoneAsync(player, npc).ConfigureAwait(false);
        }
        else
        {
            await illusionTemple.TalkToNpcStoneStatueAsync(player).ConfigureAwait(false);
        }
    }
}
