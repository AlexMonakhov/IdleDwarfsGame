using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class DailyQuestsAndEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ElderSeals",
                table: "Players",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "DailyQuestStates",
                columns: table => new
                {
                    PlayerId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    Day = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FreeRerollUsed = table.Column<bool>(type: "bit", nullable: false),
                    MilestonesClaimed = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyQuestStates", x => x.PlayerId);
                    table.ForeignKey(
                        name: "FK_DailyQuestStates_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FixedShopPurchases",
                columns: table => new
                {
                    PlayerId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    Shop = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Period = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OfferKey = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixedShopPurchases", x => new { x.PlayerId, x.Shop, x.Period, x.OfferKey });
                    table.ForeignKey(
                        name: "FK_FixedShopPurchases_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerDailyActivities",
                columns: table => new
                {
                    PlayerId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    Day = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerDailyActivities", x => new { x.PlayerId, x.Day, x.Kind });
                    table.ForeignKey(
                        name: "FK_PlayerDailyActivities_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerEventWallets",
                columns: table => new
                {
                    PlayerId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    EventStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Currency = table.Column<long>(type: "bigint", nullable: false),
                    Earned = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerEventWallets", x => new { x.PlayerId, x.EventStart });
                    table.ForeignKey(
                        name: "FK_PlayerEventWallets_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerQuests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    Day = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Slot = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Target = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    ElderSeals = table.Column<int>(type: "int", nullable: false),
                    EventCurrency = table.Column<int>(type: "int", nullable: false),
                    Gold = table.Column<int>(type: "int", nullable: false),
                    Experience = table.Column<int>(type: "int", nullable: false),
                    ClaimedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerQuests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerQuests_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "Id",
                keyValue: "a1d2f3e4-0000-4000-8000-000000000001",
                column: "ElderSeals",
                value: 0L);

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "Id",
                keyValue: "b0b0b0b0-0000-4000-8000-000000000004",
                column: "ElderSeals",
                value: 0L);

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "Id",
                keyValue: "c0c0c0c0-0000-4000-8000-000000000003",
                column: "ElderSeals",
                value: 0L);

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "Id",
                keyValue: "e0e0e0e0-0000-4000-8000-000000000002",
                column: "ElderSeals",
                value: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerQuests_PlayerId_Day_Slot",
                table: "PlayerQuests",
                columns: new[] { "PlayerId", "Day", "Slot" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyQuestStates");

            migrationBuilder.DropTable(
                name: "FixedShopPurchases");

            migrationBuilder.DropTable(
                name: "PlayerDailyActivities");

            migrationBuilder.DropTable(
                name: "PlayerEventWallets");

            migrationBuilder.DropTable(
                name: "PlayerQuests");

            migrationBuilder.DropColumn(
                name: "ElderSeals",
                table: "Players");
        }
    }
}
