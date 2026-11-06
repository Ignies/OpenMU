// <copyright file="SeasonPassTicketConsumeHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.SeasonPass;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Uses the Exp Pass Ticket: the season pass gets the experience of a ticket (20, 122).
/// </summary>
[Guid("1A7C4E92-3B58-4D16-8F0A-6E2D9B5C3A71")]
[PlugIn]
[Display(Name = nameof(PlugInResources.SeasonPassTicketConsumeHandlerPlugIn_Name), Description = nameof(PlugInResources.SeasonPassTicketConsumeHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
public class SeasonPassTicketConsumeHandlerPlugIn : SeasonPassTicketConsumeHandlerPlugInBase
{
    /// <inheritdoc />
    public override ItemIdentifier Key => new(122, 20);

    /// <inheritdoc />
    protected override ValueTask<bool> ApplyAsync(SeasonPassPlugIn plugIn, Player player) => plugIn.AddExperienceAsync(player, plugIn.Configuration?.TicketExperience ?? 0);
}
