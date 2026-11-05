// <copyright file="PartyExperienceBonusPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Holds the experience bonus of a party, see <see cref="PartyExperienceBonusConfiguration"/>. Without it, a party
/// gets the default bonus of 1.05 per additional member.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.PartyExperienceBonusPlugIn_Name), Description = nameof(PlugInResources.PartyExperienceBonusPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("3C1F4A8E-5B62-4D0B-9E43-7A1D2C6F8B95")]
public class PartyExperienceBonusPlugIn : IFeaturePlugIn, ISupportCustomConfiguration<PartyExperienceBonusConfiguration>, ISupportDefaultCustomConfiguration
{
    /// <inheritdoc />
    public PartyExperienceBonusConfiguration? Configuration { get; set; }

    /// <inheritdoc />
    public object CreateDefaultConfig() => new PartyExperienceBonusConfiguration();
}
