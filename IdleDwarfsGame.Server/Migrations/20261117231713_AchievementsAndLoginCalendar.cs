using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class AchievementsAndLoginCalendar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerAchievements",
                columns: table => new
                {
                    PlayerId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ClaimedTiers = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerAchievements", x => new { x.PlayerId, x.Key });
                    table.ForeignKey(
                        name: "FK_PlayerAchievements_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerLifetimeActivities",
                columns: table => new
                {
                    PlayerId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Count = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerLifetimeActivities", x => new { x.PlayerId, x.Kind });
                    table.ForeignKey(
                        name: "FK_PlayerLifetimeActivities_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerLoginCalendars",
                columns: table => new
                {
                    PlayerId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    Cycle = table.Column<int>(type: "int", nullable: false),
                    Day = table.Column<int>(type: "int", nullable: false),
                    LastClaimDay = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Streak = table.Column<int>(type: "int", nullable: false),
                    BestStreak = table.Column<int>(type: "int", nullable: false),
                    TotalDays = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerLoginCalendars", x => x.PlayerId);
                    table.ForeignKey(
                        name: "FK_PlayerLoginCalendars_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Счётчики за всё время начинаются с того, что ещё хранится за последние сутки (DailyQuests.KeepActivityDays)
            migrationBuilder.Sql(@"
                INSERT INTO PlayerLifetimeActivities (PlayerId, Kind, [Count])
                SELECT PlayerId, Kind, SUM(CAST([Count] AS bigint))
                FROM PlayerDailyActivities
                GROUP BY PlayerId, Kind");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerAchievements");

            migrationBuilder.DropTable(
                name: "PlayerLifetimeActivities");

            migrationBuilder.DropTable(
                name: "PlayerLoginCalendars");
        }
    }
}
