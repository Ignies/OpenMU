// <copyright file="SoulConverter3ConsumeHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Activates the monster soul converter of tier 3 (14, 462).
/// </summary>
[Guid("A94D3B76-2C1F-4E59-B8A7-1D6F5C3E9B36")]
[PlugIn]
[Display(Name = nameof(PlugInResources.SoulConverter3ConsumeHandlerPlugIn_Name), Description = nameof(PlugInResources.SoulConverterConsumeHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
public class SoulConverter3ConsumeHandlerPlugIn : SoulConverterConsumeHandlerPlugInBase
{
    /// <inheritdoc />
    public override ItemIdentifier Key => new(462, 14);
}
