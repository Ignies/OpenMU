using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MUnique.OpenMU.Persistence.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddSeason21Economy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                schema: "data",
                table: "Item",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GremoryCaseId",
                schema: "data",
                table: "Character",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Ruud",
                schema: "data",
                table: "Character",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "GremoryCaseId",
                schema: "data",
                table: "Account",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MerchantStoreTab",
                schema: "config",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemsId = table.Column<Guid>(type: "uuid", nullable: true),
                    MonsterDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Index = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantStoreTab", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MerchantStoreTab_ItemStorage_ItemsId",
                        column: x => x.ItemsId,
                        principalSchema: "data",
                        principalTable: "ItemStorage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MerchantStoreTab_MonsterDefinition_MonsterDefinitionId",
                        column: x => x.MonsterDefinitionId,
                        principalSchema: "config",
                        principalTable: "MonsterDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Character_GremoryCaseId",
                schema: "data",
                table: "Character",
                column: "GremoryCaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Account_GremoryCaseId",
                schema: "data",
                table: "Account",
                column: "GremoryCaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MerchantStoreTab_ItemsId",
                schema: "config",
                table: "MerchantStoreTab",
                column: "ItemsId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MerchantStoreTab_MonsterDefinitionId",
                schema: "config",
                table: "MerchantStoreTab",
                column: "MonsterDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Account_ItemStorage_GremoryCaseId",
                schema: "data",
                table: "Account",
                column: "GremoryCaseId",
                principalSchema: "data",
                principalTable: "ItemStorage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Character_ItemStorage_GremoryCaseId",
                schema: "data",
                table: "Character",
                column: "GremoryCaseId",
                principalSchema: "data",
                principalTable: "ItemStorage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Account_ItemStorage_GremoryCaseId",
                schema: "data",
                table: "Account");

            migrationBuilder.DropForeignKey(
                name: "FK_Character_ItemStorage_GremoryCaseId",
                schema: "data",
                table: "Character");

            migrationBuilder.DropTable(
                name: "MerchantStoreTab",
                schema: "config");

            migrationBuilder.DropIndex(
                name: "IX_Character_GremoryCaseId",
                schema: "data",
                table: "Character");

            migrationBuilder.DropIndex(
                name: "IX_Account_GremoryCaseId",
                schema: "data",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                schema: "data",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "GremoryCaseId",
                schema: "data",
                table: "Character");

            migrationBuilder.DropColumn(
                name: "Ruud",
                schema: "data",
                table: "Character");

            migrationBuilder.DropColumn(
                name: "GremoryCaseId",
                schema: "data",
                table: "Account");
        }
    }
}
