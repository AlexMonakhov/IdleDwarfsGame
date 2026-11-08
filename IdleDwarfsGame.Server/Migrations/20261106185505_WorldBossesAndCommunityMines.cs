using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class WorldBossesAndCommunityMines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommunityDeposits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    OreTotal = table.Column<double>(type: "float", nullable: false),
                    OreMined = table.Column<double>(type: "float", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SpeedLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    SpawnedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExhaustedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityDeposits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorldBossCooldowns",
                columns: table => new
                {
                    PlayerId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    ReadyAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorldBossCooldowns", x => x.PlayerId);
                    table.ForeignKey(
                        name: "FK_WorldBossCooldowns_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorldBosses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Stage = table.Column<int>(type: "int", nullable: false),
                    MaxHp = table.Column<int>(type: "int", nullable: false),
                    CurrentHp = table.Column<int>(type: "int", nullable: false),
                    SpawnedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DefeatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    KilledByPlayerId = table.Column<string>(type: "nvarchar(36)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorldBosses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DepositDwarves",
                columns: table => new
                {
                    HeroId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepositId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    Rate = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositDwarves", x => x.HeroId);
                    table.ForeignKey(
                        name: "FK_DepositDwarves_CommunityDeposits_DepositId",
                        column: x => x.DepositId,
                        principalTable: "CommunityDeposits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepositDwarves_Heroes_HeroId",
                        column: x => x.HeroId,
                        principalTable: "Heroes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepositRewardClaims",
                columns: table => new
                {
                    DepositId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    ClaimedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositRewardClaims", x => new { x.DepositId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_DepositRewardClaims_CommunityDeposits_DepositId",
                        column: x => x.DepositId,
                        principalTable: "CommunityDeposits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepositRewardClaims_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepositWorkers",
                columns: table => new
                {
                    DepositId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    Mined = table.Column<double>(type: "float", nullable: false),
                    UpgradePoints = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositWorkers", x => new { x.DepositId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_DepositWorkers_CommunityDeposits_DepositId",
                        column: x => x.DepositId,
                        principalTable: "CommunityDeposits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepositWorkers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorldBossContributions",
                columns: table => new
                {
                    BossId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    Damage = table.Column<long>(type: "bigint", nullable: false),
                    Attacks = table.Column<int>(type: "int", nullable: false),
                    LastAttackAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorldBossContributions", x => new { x.BossId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_WorldBossContributions_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorldBossContributions_WorldBosses_BossId",
                        column: x => x.BossId,
                        principalTable: "WorldBosses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorldBossRewardClaims",
                columns: table => new
                {
                    BossId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    ClaimedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorldBossRewardClaims", x => new { x.BossId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_WorldBossRewardClaims_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorldBossRewardClaims_WorldBosses_BossId",
                        column: x => x.BossId,
                        principalTable: "WorldBosses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityDeposits_EventStart_Index",
                table: "CommunityDeposits",
                columns: new[] { "EventStart", "Index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepositDwarves_DepositId",
                table: "DepositDwarves",
                column: "DepositId");

            migrationBuilder.CreateIndex(
                name: "IX_DepositDwarves_PlayerId",
                table: "DepositDwarves",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_DepositRewardClaims_PlayerId",
                table: "DepositRewardClaims",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_DepositWorkers_PlayerId",
                table: "DepositWorkers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_WorldBossContributions_PlayerId",
                table: "WorldBossContributions",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_WorldBosses_EventStart_Stage",
                table: "WorldBosses",
                columns: new[] { "EventStart", "Stage" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorldBossRewardClaims_PlayerId",
                table: "WorldBossRewardClaims",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepositDwarves");

            migrationBuilder.DropTable(
                name: "DepositRewardClaims");

            migrationBuilder.DropTable(
                name: "DepositWorkers");

            migrationBuilder.DropTable(
                name: "WorldBossContributions");

            migrationBuilder.DropTable(
                name: "WorldBossCooldowns");

            migrationBuilder.DropTable(
                name: "WorldBossRewardClaims");

            migrationBuilder.DropTable(
                name: "CommunityDeposits");

            migrationBuilder.DropTable(
                name: "WorldBosses");
        }
    }
}
