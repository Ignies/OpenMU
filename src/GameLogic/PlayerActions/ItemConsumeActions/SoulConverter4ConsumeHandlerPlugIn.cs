// <copyright file="SoulConverter4ConsumeHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Activates the monster soul converter of tier 4 (14, 463).
/// </summary>
[Guid("D15E7C28-4A3B-4F96-9C0D-2E8A7B5F1C47")]
[PlugIn]
[Display(Name = nameof(PlugInResources.SoulConverter4ConsumeHandlerPlugIn_Name), Description = nameof(PlugInResources.SoulConverterConsumeHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
public class SoulConverter4ConsumeHandlerPlugIn : SoulConverterConsumeHandlerPlugInBase
{
    /// <inheritdoc />
    public override ItemIdentifier Key => new(463, 14);
}
