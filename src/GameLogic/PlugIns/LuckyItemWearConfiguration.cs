// <copyright file="LuckyItemWearConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns;

/// <summary>
/// The configuration of the <see cref="LuckyItemWearPlugIn"/>.
/// </summary>
public class LuckyItemWearConfiguration
{
    /// <summary>
    /// Gets or sets the time for which a lucky item has to be worn to lose a point of durability.
    /// </summary>
    /// <remarks>
    /// The default of 40 minutes is the one of the Season 21 server data (LuckySystem.xml).
    /// </remarks>
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(40);
}
