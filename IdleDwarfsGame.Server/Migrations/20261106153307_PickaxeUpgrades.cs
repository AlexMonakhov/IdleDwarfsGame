using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class PickaxeUpgrades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PickaxeCapacityLevel",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "PickaxeRecoveryLevel",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "PickaxeStrengthLevel",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "Id",
                keyValue: "a1d2f3e4-0000-4000-8000-000000000001",
                columns: new[] { "PickaxeCapacityLevel", "PickaxeRecoveryLevel", "PickaxeStrengthLevel" },
                values: new object[] { 1, 1, 1 });

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "Id",
                keyValue: "b0b0b0b0-0000-4000-8000-000000000004",
                columns: new[] { "PickaxeCapacityLevel", "PickaxeRecoveryLevel", "PickaxeStrengthLevel" },
                values: new object[] { 1, 1, 1 });

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "Id",
                keyValue: "c0c0c0c0-0000-4000-8000-000000000003",
                columns: new[] { "PickaxeCapacityLevel", "PickaxeRecoveryLevel", "PickaxeStrengthLevel" },
                values: new object[] { 1, 1, 1 });

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "Id",
                keyValue: "e0e0e0e0-0000-4000-8000-000000000002",
                columns: new[] { "PickaxeCapacityLevel", "PickaxeRecoveryLevel", "PickaxeStrengthLevel" },
                values: new object[] { 1, 1, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PickaxeCapacityLevel",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "PickaxeRecoveryLevel",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "PickaxeStrengthLevel",
                table: "Players");
        }
    }
}
