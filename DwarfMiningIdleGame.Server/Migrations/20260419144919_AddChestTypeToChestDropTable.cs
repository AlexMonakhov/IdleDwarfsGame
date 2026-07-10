using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DwarfMiningIdleGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddChestTypeToChestDropTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubType",
                table: "DropEntry",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubType",
                table: "DropEntry");
        }
    }
}
