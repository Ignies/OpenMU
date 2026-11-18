using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MUnique.OpenMU.Persistence.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddStarSmithing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Locked",
                schema: "data",
                table: "ItemCustomization",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockedAt",
                schema: "data",
                table: "ItemCustomization",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "StarSmithExperience",
                schema: "data",
                table: "Character",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<short>(
                name: "StarSmithLevel",
                schema: "data",
                table: "Character",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Locked",
                schema: "data",
                table: "ItemCustomization");

            migrationBuilder.DropColumn(
                name: "LockedAt",
                schema: "data",
                table: "ItemCustomization");

            migrationBuilder.DropColumn(
                name: "StarSmithExperience",
                schema: "data",
                table: "Character");

            migrationBuilder.DropColumn(
                name: "StarSmithLevel",
                schema: "data",
                table: "Character");
        }
    }
}
