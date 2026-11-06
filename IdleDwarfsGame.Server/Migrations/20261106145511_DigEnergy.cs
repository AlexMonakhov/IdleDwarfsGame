using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class DigEnergy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DigEnergy",
                table: "Players",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DigEnergyAt",
                table: "Players",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DigProgress",
                table: "Mines",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "Id",
                keyValue: "a1d2f3e4-0000-4000-8000-000000000001",
                columns: new[] { "DigEnergy", "DigEnergyAt" },
                values: new object[] { 0.0, null });

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "Id",
                keyValue: "b0b0b0b0-0000-4000-8000-000000000004",
                columns: new[] { "DigEnergy", "DigEnergyAt" },
                values: new object[] { 0.0, null });

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "Id",
                keyValue: "c0c0c0c0-0000-4000-8000-000000000003",
                columns: new[] { "DigEnergy", "DigEnergyAt" },
                values: new object[] { 0.0, null });

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "Id",
                keyValue: "e0e0e0e0-0000-4000-8000-000000000002",
                columns: new[] { "DigEnergy", "DigEnergyAt" },
                values: new object[] { 0.0, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DigEnergy",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "DigEnergyAt",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "DigProgress",
                table: "Mines");
        }
    }
}
