using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class DelveRevivesAndReroll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RerollFloor",
                table: "DelveRuns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Revives",
                table: "DelveRuns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Уже воскрешённый забег — одно воскрешение использовано
            migrationBuilder.Sql("UPDATE [DelveRuns] SET [Revives] = 1 WHERE [Revived] = 1");

            migrationBuilder.DropColumn(
                name: "Revived",
                table: "DelveRuns");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Revived",
                table: "DelveRuns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE [DelveRuns] SET [Revived] = 1 WHERE [Revives] > 0");

            migrationBuilder.DropColumn(
                name: "RerollFloor",
                table: "DelveRuns");

            migrationBuilder.DropColumn(
                name: "Revives",
                table: "DelveRuns");
        }
    }
}
