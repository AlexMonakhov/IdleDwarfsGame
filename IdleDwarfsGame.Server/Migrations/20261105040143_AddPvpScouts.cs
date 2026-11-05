using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddPvpScouts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PvpScouts",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Day = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Mode = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    OpponentUserId = table.Column<string>(type: "TEXT", nullable: false),
                    WinChance = table.Column<double>(type: "REAL", nullable: false),
                    PlayerPower = table.Column<int>(type: "INTEGER", nullable: false),
                    EnemyPower = table.Column<int>(type: "INTEGER", nullable: false),
                    ScoutedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PvpScouts", x => new { x.UserId, x.Day, x.Mode, x.OpponentUserId });
                    table.ForeignKey(
                        name: "FK_PvpScouts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PvpScouts");
        }
    }
}
