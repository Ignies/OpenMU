// <copyright file="ItemCustomizationExtensions.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.EntityFramework.Extensions.ModelBuilder;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MUnique.OpenMU.Persistence.EntityFramework.Model;

/// <summary>
/// Extensions for the <see cref="EntityTypeBuilder{TEntity}"/> of the <see cref="ItemCustomization"/>.
/// </summary>
internal static class ItemCustomizationExtensions
{
    /// <summary>
    /// Applies the settings for the <see cref="ItemCustomization"/> entity: one per item, gone with the item.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public static void Apply(this EntityTypeBuilder<ItemCustomization> builder)
    {
        builder.HasIndex(customization => customization.ItemId).IsUnique();
        builder.HasOne<Item>()
            .WithMany()
            .HasForeignKey(customization => customization.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(customization => customization.Name).HasMaxLength(24);
        builder.Property(customization => customization.Lore).HasMaxLength(140);
        builder.Property(customization => customization.CreatedBy).HasMaxLength(64);
    }
}
