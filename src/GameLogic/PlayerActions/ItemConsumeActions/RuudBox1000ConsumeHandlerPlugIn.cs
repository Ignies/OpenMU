// <copyright file="RuudBox1000ConsumeHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Opens the Ruud box [1,000] (14, 352).
/// </summary>
[Guid("0067928C-782F-5C3C-8988-F2231D4964F2")]
[PlugIn]
[Display(Name = nameof(PlugInResources.RuudBox1000ConsumeHandlerPlugIn_Name), Description = nameof(PlugInResources.RuudBoxConsumeHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
public class RuudBox1000ConsumeHandlerPlugIn : RuudBoxConsumeHandlerPlugInBase
{
    /// <inheritdoc />
    public override ItemIdentifier Key => new(352, 14);
}
