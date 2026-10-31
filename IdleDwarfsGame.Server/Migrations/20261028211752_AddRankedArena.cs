using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddRankedArena : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PvpSeasonScores",
                table: "PvpSeasonScores");

            migrationBuilder.DropIndex(
                name: "IX_PvpSeasonScores_Type_Start_Points",
                table: "PvpSeasonScores");

            migrationBuilder.DropIndex(
                name: "IX_PvpBattles_AttackerUserId_DefenderUserId_Day",
                table: "PvpBattles");

            // Всё, что было до рейтинговой арены, — бои и очки обычной
            migrationBuilder.AddColumn<string>(
                name: "Mode",
                table: "PvpSeasonScores",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "Casual");

            migrationBuilder.AddColumn<int>(
                name: "AttackerRating",
                table: "PvpBattles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DefenderRating",
                table: "PvpBattles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Mode",
                table: "PvpBattles",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "Casual");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PvpSeasonScores",
                table: "PvpSeasonScores",
                columns: new[] { "UserId", "Mode", "Type", "Start" });

            migrationBuilder.CreateTable(
                name: "RankedSquads",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LockedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HeroesJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankedSquads", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_RankedSquads_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PvpSeasonScores_Mode_Type_Start_Points",
                table: "PvpSeasonScores",
                columns: new[] { "Mode", "Type", "Start", "Points" });

            migrationBuilder.CreateIndex(
                name: "IX_PvpBattles_AttackerUserId_DefenderUserId_Day_Mode",
                table: "PvpBattles",
                columns: new[] { "AttackerUserId", "DefenderUserId", "Day", "Mode" },
                unique: true,
                filter: "\"AttackerWon\" = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RankedSquads");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PvpSeasonScores",
                table: "PvpSeasonScores");

            migrationBuilder.DropIndex(
                name: "IX_PvpSeasonScores_Mode_Type_Start_Points",
                table: "PvpSeasonScores");

            migrationBuilder.DropIndex(
                name: "IX_PvpBattles_AttackerUserId_DefenderUserId_Day_Mode",
                table: "PvpBattles");

            migrationBuilder.DropColumn(
                name: "Mode",
                table: "PvpSeasonScores");

            migrationBuilder.DropColumn(
                name: "AttackerRating",
                table: "PvpBattles");

            migrationBuilder.DropColumn(
                name: "DefenderRating",
                table: "PvpBattles");

            migrationBuilder.DropColumn(
                name: "Mode",
                table: "PvpBattles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PvpSeasonScores",
                table: "PvpSeasonScores",
                columns: new[] { "UserId", "Type", "Start" });

            migrationBuilder.CreateIndex(
                name: "IX_PvpSeasonScores_Type_Start_Points",
                table: "PvpSeasonScores",
                columns: new[] { "Type", "Start", "Points" });

            migrationBuilder.CreateIndex(
                name: "IX_PvpBattles_AttackerUserId_DefenderUserId_Day",
                table: "PvpBattles",
                columns: new[] { "AttackerUserId", "DefenderUserId", "Day" },
                unique: true,
                filter: "\"AttackerWon\" = 1");
        }
    }
}
