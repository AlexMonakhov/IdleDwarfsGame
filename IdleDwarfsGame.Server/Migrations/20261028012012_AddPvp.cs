using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddPvp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PvpBattles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AttackerUserId = table.Column<string>(type: "TEXT", nullable: false),
                    DefenderUserId = table.Column<string>(type: "TEXT", nullable: false),
                    FoughtAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Day = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AttackerWon = table.Column<bool>(type: "INTEGER", nullable: false),
                    AttackerPower = table.Column<int>(type: "INTEGER", nullable: false),
                    DefenderPower = table.Column<int>(type: "INTEGER", nullable: false),
                    Points = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PvpBattles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PvpBattles_Users_AttackerUserId",
                        column: x => x.AttackerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PvpBattles_Users_DefenderUserId",
                        column: x => x.DefenderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PvpSeasonScores",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Start = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Points = table.Column<int>(type: "INTEGER", nullable: false),
                    Wins = table.Column<int>(type: "INTEGER", nullable: false),
                    Losses = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PvpSeasonScores", x => new { x.UserId, x.Type, x.Start });
                    table.ForeignKey(
                        name: "FK_PvpSeasonScores_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PvpBattles_AttackerUserId_DefenderUserId_Day",
                table: "PvpBattles",
                columns: new[] { "AttackerUserId", "DefenderUserId", "Day" },
                unique: true,
                filter: "\"AttackerWon\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_PvpBattles_AttackerUserId_FoughtAt",
                table: "PvpBattles",
                columns: new[] { "AttackerUserId", "FoughtAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PvpBattles_DefenderUserId_FoughtAt",
                table: "PvpBattles",
                columns: new[] { "DefenderUserId", "FoughtAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PvpSeasonScores_Type_Start_Points",
                table: "PvpSeasonScores",
                columns: new[] { "Type", "Start", "Points" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PvpBattles");

            migrationBuilder.DropTable(
                name: "PvpSeasonScores");
        }
    }
}
