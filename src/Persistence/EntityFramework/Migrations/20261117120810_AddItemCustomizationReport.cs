using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MUnique.OpenMU.Persistence.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddItemCustomizationReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemCustomizationReport",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reporter = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                    Lore = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemCustomizationReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemCustomizationReport_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "data",
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemCustomizationReport_CreatedAt",
                schema: "data",
                table: "ItemCustomizationReport",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ItemCustomizationReport_ItemId_Reporter_Version",
                schema: "data",
                table: "ItemCustomizationReport",
                columns: new[] { "ItemId", "Reporter", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemCustomizationReport",
                schema: "data");
        }
    }
}
