using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MUnique.OpenMU.Persistence.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddStarSmithBoosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Boosts",
                schema: "data",
                table: "ItemCustomization",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "SmithLevel",
                schema: "data",
                table: "ItemCustomization",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Boosts",
                schema: "data",
                table: "ItemCustomization");

            migrationBuilder.DropColumn(
                name: "SmithLevel",
                schema: "data",
                table: "ItemCustomization");
        }
    }
}
