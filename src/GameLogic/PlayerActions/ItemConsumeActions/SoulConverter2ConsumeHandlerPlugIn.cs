// <copyright file="SoulConverter2ConsumeHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Activates the monster soul converter of tier 2 (14, 461).
/// </summary>
[Guid("7C2F5A93-1E6B-4D88-A3F0-9B4E2D6C8A25")]
[PlugIn]
[Display(Name = nameof(PlugInResources.SoulConverter2ConsumeHandlerPlugIn_Name), Description = nameof(PlugInResources.SoulConverterConsumeHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
public class SoulConverter2ConsumeHandlerPlugIn : SoulConverterConsumeHandlerPlugInBase
{
    /// <inheritdoc />
    public override ItemIdentifier Key => new(461, 14);
}
