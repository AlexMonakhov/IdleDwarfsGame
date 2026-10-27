using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddGuilds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GuildBossCooldowns",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Tier = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    ReadyAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildBossCooldowns", x => new { x.UserId, x.Tier });
                    table.ForeignKey(
                        name: "FK_GuildBossCooldowns_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuildBossRewardClaims",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Tier = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Day = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuildId = table.Column<string>(type: "TEXT", nullable: false),
                    BossId = table.Column<string>(type: "TEXT", nullable: false),
                    RewardId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildBossRewardClaims", x => new { x.UserId, x.Tier, x.Day });
                    table.ForeignKey(
                        name: "FK_GuildBossRewardClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GuildBosses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    GuildId = table.Column<string>(type: "TEXT", nullable: false),
                    Tier = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Day = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MaxHp = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentHp = table.Column<int>(type: "INTEGER", nullable: false),
                    DefeatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    KilledByUserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildBosses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildBosses_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuildJoinRequests",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    GuildId = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildJoinRequests", x => new { x.UserId, x.GuildId });
                    table.ForeignKey(
                        name: "FK_GuildJoinRequests_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GuildJoinRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuildMembers",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    GuildId = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildMembers", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_GuildMembers_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GuildMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuildBossAttacks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    BossId = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Damage = table.Column<int>(type: "INTEGER", nullable: false),
                    AttackedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildBossAttacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildBossAttacks_GuildBosses_BossId",
                        column: x => x.BossId,
                        principalTable: "GuildBosses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GuildBossAttacks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuildBossAttacks_BossId",
                table: "GuildBossAttacks",
                column: "BossId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildBossAttacks_UserId",
                table: "GuildBossAttacks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildBosses_GuildId_Day_Tier",
                table: "GuildBosses",
                columns: new[] { "GuildId", "Day", "Tier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuildJoinRequests_GuildId",
                table: "GuildJoinRequests",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildMembers_GuildId",
                table: "GuildMembers",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Guilds_NormalizedName",
                table: "Guilds",
                column: "NormalizedName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildBossAttacks");

            migrationBuilder.DropTable(
                name: "GuildBossCooldowns");

            migrationBuilder.DropTable(
                name: "GuildBossRewardClaims");

            migrationBuilder.DropTable(
                name: "GuildJoinRequests");

            migrationBuilder.DropTable(
                name: "GuildMembers");

            migrationBuilder.DropTable(
                name: "GuildBosses");

            migrationBuilder.DropTable(
                name: "Guilds");
        }
    }
}
