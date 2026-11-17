// <copyright file="ItemCustomizationReportResult.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ItemCustomization;

/// <summary>
/// The answer to a report (EE 38).
/// </summary>
public enum ItemCustomizationReportResult : byte
{
    /// <summary>The report was stored; an admin sees it on the website.</summary>
    Sent,

    /// <summary>This player reported this item at this version already.</summary>
    Already,

    /// <summary>The item is not there, or has no name or lore.</summary>
    NotFound,

    /// <summary>Less than ten seconds after this player's last report.</summary>
    TooFast,
}
