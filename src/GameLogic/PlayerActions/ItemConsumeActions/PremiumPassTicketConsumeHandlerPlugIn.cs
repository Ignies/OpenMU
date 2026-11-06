// <copyright file="PremiumPassTicketConsumeHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.SeasonPass;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Uses the Premium Pass Ticket: a character with the gold pass gets the premium pass of the running season (20, 124).
/// </summary>
[Guid("3C9E6014-5D7A-4F38-A15C-80F4BD7E5C93")]
[PlugIn]
[Display(Name = nameof(PlugInResources.PremiumPassTicketConsumeHandlerPlugIn_Name), Description = nameof(PlugInResources.PremiumPassTicketConsumeHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
public class PremiumPassTicketConsumeHandlerPlugIn : SeasonPassTicketConsumeHandlerPlugInBase
{
    /// <inheritdoc />
    public override ItemIdentifier Key => new(124, 20);

    /// <inheritdoc />
    protected override ValueTask<bool> ApplyAsync(SeasonPassPlugIn plugIn, Player player) => plugIn.UnlockAsync(player, SeasonPassKind.Premium);
}
