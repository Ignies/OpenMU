// <copyright file="GoldPassTicketConsumeHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.SeasonPass;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Uses the Gold Pass Ticket: the character gets the gold pass of the running season (20, 123).
/// </summary>
[Guid("2B8D5F03-4C69-4E27-904B-7F3EAC6D4B82")]
[PlugIn]
[Display(Name = nameof(PlugInResources.GoldPassTicketConsumeHandlerPlugIn_Name), Description = nameof(PlugInResources.GoldPassTicketConsumeHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
public class GoldPassTicketConsumeHandlerPlugIn : SeasonPassTicketConsumeHandlerPlugInBase
{
    /// <inheritdoc />
    public override ItemIdentifier Key => new(123, 20);

    /// <inheritdoc />
    protected override ValueTask<bool> ApplyAsync(SeasonPassPlugIn plugIn, Player player) => plugIn.UnlockAsync(player, SeasonPassKind.Gold);
}
