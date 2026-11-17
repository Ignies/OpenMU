// <copyright file="ItemCustomizationReportExtensions.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.EntityFramework.Extensions.ModelBuilder;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MUnique.OpenMU.Persistence.EntityFramework.Model;

/// <summary>
/// Extensions for the <see cref="EntityTypeBuilder{TEntity}"/> of the <see cref="ItemCustomizationReport"/>.
/// </summary>
internal static class ItemCustomizationReportExtensions
{
    /// <summary>
    /// Applies the settings for the <see cref="ItemCustomizationReport"/> entity: one per reporter, item and version,
    /// gone with the item.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public static void Apply(this EntityTypeBuilder<ItemCustomizationReport> builder)
    {
        builder.HasIndex(report => new { report.ItemId, report.Reporter, report.Version }).IsUnique();
        builder.HasIndex(report => report.CreatedAt);
        builder.HasOne<Item>()
            .WithMany()
            .HasForeignKey(report => report.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(report => report.Reporter).HasMaxLength(16);
        builder.Property(report => report.Name).HasMaxLength(24);
        builder.Property(report => report.Lore).HasMaxLength(140);
    }
}
