// <copyright file="RuudBox400ConsumeHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Opens the Ruud box [400] (14, 489).
/// </summary>
[Guid("A8D27327-3C44-5A12-98A4-9BFCC886C361")]
[PlugIn]
[Display(Name = nameof(PlugInResources.RuudBox400ConsumeHandlerPlugIn_Name), Description = nameof(PlugInResources.RuudBoxConsumeHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
public class RuudBox400ConsumeHandlerPlugIn : RuudBoxConsumeHandlerPlugInBase
{
    /// <inheritdoc />
    public override ItemIdentifier Key => new(489, 14);
}
