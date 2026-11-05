// <copyright file="FixZombieFighterHealthUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Corrects the maximum health of the Zombie Fighter of Vulcanus (481), a digit slip of 5,000,000 for 500,000.
/// </summary>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.FixZombieFighterHealthUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.FixZombieFighterHealthUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("5D7A1C93-E46B-4F02-8B3D-91C6A2E5F047")]
public class FixZombieFighterHealthUpdatePlugIn : UpdatePlugInBase
{
    private const short ZombieFighterNumber = 481;

    private const float SlippedHealth = 5000000;

    /// <summary>
    /// The health of the Zombie Fighter, in line with its siblings of Vulcanus (513,000 to 530,000) and the Season 21
    /// server data.
    /// </summary>
    private const float Health = 500000;

    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.FixZombieFighterHealthUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.FixZombieFighterHealthUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 05, 14, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        var health = gameConfiguration.Monsters.FirstOrDefault(m => m.Number == ZombieFighterNumber)?
            .Attributes.FirstOrDefault(a => a.AttributeDefinition == Stats.MaximumHealth);
        if (health?.Value == SlippedHealth)
        {
            health.Value = Health;
        }

        return ValueTask.CompletedTask;
    }
}
