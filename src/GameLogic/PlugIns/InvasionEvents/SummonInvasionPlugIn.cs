// <copyright file="SummonInvasionPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns.InvasionEvents;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The Summon invasion of Season 21: Golden Goblins on Tarkan, Aida, Kanturu, Karutan or Acheron.
/// </summary>
[PlugIn]
[Display(Name = nameof(MUnique.OpenMU.GameLogic.Properties.PlugInResources.SummonInvasionPlugIn_Name), Description = nameof(MUnique.OpenMU.GameLogic.Properties.PlugInResources.SummonInvasionPlugIn_Description), ResourceType = typeof(MUnique.OpenMU.GameLogic.Properties.PlugInResources))]
[Guid("B84F1C27-5A6E-4D93-A0C2-7F3E9D1B6A48")]
public sealed class SummonInvasionPlugIn : SimpleInvasionPlugIn, IDisabledByDefault
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SummonInvasionPlugIn"/> class.
    /// </summary>
    public SummonInvasionPlugIn()
        : base(() => InvasionConfigurationDefaults.Summon)
    {
    }

    /// <inheritdoc />
    protected override ushort? AnnouncedMonsterId => InvasionMonsters.SummonedGoldenGoblin;
}
