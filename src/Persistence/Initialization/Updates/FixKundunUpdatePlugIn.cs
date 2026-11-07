// <copyright file="FixKundunUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.Persistence.Initialization.Properties;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Gives Kundun (275), the boss of Kalima 7, its own numbers instead of a copy of the Illusion of Kundun of Kalima 6
/// (338): level 147, 5,000,000 health (S21Emu Monster.xml, muonlinefanz.com Kundun). Each value is replaced only while
/// it is still the copied one, so a value an operator changed is kept.
/// </summary>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.FixKundunUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.FixKundunUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("16A2C053-F6A8-40B6-BA61-DD6C5ABC78DC")]
public class FixKundunUpdatePlugIn : UpdatePlugInBase
{
    private const short KundunNumber = 275;

    private const string CopiedDesignation = "Illusion of Kundun 7";

    private static readonly (AttributeDefinition Attribute, float Copied, float Boss)[] Attributes =
    [
        (Stats.Level, 140, 147),
        (Stats.MaximumHealth, 140000, 5000000),
        (Stats.MinimumPhysBaseDmg, 1070, 2000),
        (Stats.MaximumPhysBaseDmg, 1105, 2500),
        (Stats.DefenseBase, 892, 1500),
        (Stats.AttackRatePvm, 1200, 2000),
        (Stats.DefenseRatePvm, 450, 1000),
        (Stats.PoisonResistance, 60f / 255, 254f / 255),
        (Stats.IceResistance, 60f / 255, 100f / 255),
        (Stats.LightningResistance, 60f / 255, 100f / 255),
        (Stats.FireResistance, 60f / 255, 100f / 255),
    ];

    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.FixKundunUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.FixKundunUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 07, 12, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        if (gameConfiguration.Monsters.FirstOrDefault(m => m.Number == KundunNumber) is not { } kundun)
        {
            return ValueTask.CompletedTask;
        }

        foreach (var (attribute, copied, boss) in Attributes)
        {
            if (kundun.Attributes.FirstOrDefault(a => a.AttributeDefinition == attribute) is { } value
                && Math.Abs(value.Value - copied) < 0.0001f)
            {
                value.Value = boss;
            }
        }

        if (kundun.RespawnDelay == TimeSpan.FromSeconds(10800))
        {
            kundun.RespawnDelay = TimeSpan.FromSeconds(43200);
        }

        if (kundun.AttackDelay == TimeSpan.FromMilliseconds(1700))
        {
            kundun.AttackDelay = TimeSpan.FromMilliseconds(2000);
        }

        if (kundun.AttackRange == 10 && kundun.ViewRange == 7)
        {
            kundun.AttackRange = 15;
            kundun.ViewRange = 10;
        }

        if (kundun.Designation.ValueInNeutralLanguage == CopiedDesignation)
        {
            kundun.Designation = LocalizedString.FromResource(() => MonsterNames.Kundun);
        }

        return ValueTask.CompletedTask;
    }
}
