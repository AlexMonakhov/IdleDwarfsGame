using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DwarfMiningIdleGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddChestTypeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChestType",
                table: "ChestDropTables",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChestType",
                table: "ChestDropTables");
        }
    }
}
