using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class SocialDepthDelve : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArenaBetMatches",
                columns: table => new
                {
                    WindowStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AUserId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    BUserId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    AName = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    BName = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    ASquadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BSquadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    APower = table.Column<int>(type: "int", nullable: false),
                    BPower = table.Column<int>(type: "int", nullable: false),
                    Seed = table.Column<int>(type: "int", nullable: false),
                    Winner = table.Column<int>(type: "int", nullable: true),
                    Rounds = table.Column<int>(type: "int", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArenaBetMatches", x => x.WindowStart);
                });

            migrationBuilder.CreateTable(
                name: "DelveRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    PlayerId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    WeekStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Day = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Floor = table.Column<int>(type: "int", nullable: false),
                    Column = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Pending = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    SquadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HpJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RelicsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OfferJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GoldEarned = table.Column<long>(type: "bigint", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DelveRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DelveRuns_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FriendHires",
                columns: table => new
                {
                    BorrowerUserId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    LenderUserId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    Day = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HeroId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HeroName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    StageId = table.Column<int>(type: "int", nullable: false),
                    LenderRewarded = table.Column<bool>(type: "bit", nullable: false),
                    HiredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendHires", x => new { x.BorrowerUserId, x.LenderUserId, x.Day });
                    table.ForeignKey(
                        name: "FK_FriendHires_Users_BorrowerUserId",
                        column: x => x.BorrowerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FriendHires_Users_LenderUserId",
                        column: x => x.LenderUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FriendMineHelps",
                columns: table => new
                {
                    HelperUserId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    TargetUserId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    Day = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    HelperGold = table.Column<long>(type: "bigint", nullable: false),
                    HelpedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendMineHelps", x => new { x.HelperUserId, x.TargetUserId, x.Day });
                    table.ForeignKey(
                        name: "FK_FriendMineHelps_Users_HelperUserId",
                        column: x => x.HelperUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FriendMineHelps_Users_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GuildDepthDwarves",
                columns: table => new
                {
                    WeekStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    GuildId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    PlayerId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    HeroId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BaseRate = table.Column<double>(type: "float", nullable: false),
                    PickLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Mined = table.Column<double>(type: "float", nullable: false),
                    Stored = table.Column<double>(type: "float", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReleaseAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildDepthDwarves", x => new { x.WeekStart, x.UserId });
                    table.ForeignKey(
                        name: "FK_GuildDepthDwarves_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuildDepthRewardClaims",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    WeekStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuildId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    Place = table.Column<int>(type: "int", nullable: false),
                    HonorBadges = table.Column<int>(type: "int", nullable: false),
                    ClaimedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildDepthRewardClaims", x => new { x.UserId, x.WeekStart });
                    table.ForeignKey(
                        name: "FK_GuildDepthRewardClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuildDepthShafts",
                columns: table => new
                {
                    GuildId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    WeekStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildDepthShafts", x => new { x.GuildId, x.WeekStart });
                    table.ForeignKey(
                        name: "FK_GuildDepthShafts_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArenaBets",
                columns: table => new
                {
                    WindowStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    PlayerId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    Side = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    PlacedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Payout = table.Column<long>(type: "bigint", nullable: true),
                    ClaimedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArenaBets", x => new { x.WindowStart, x.UserId });
                    table.ForeignKey(
                        name: "FK_ArenaBets_ArenaBetMatches_WindowStart",
                        column: x => x.WindowStart,
                        principalTable: "ArenaBetMatches",
                        principalColumn: "WindowStart",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArenaBets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArenaBets_UserId",
                table: "ArenaBets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DelveRuns_UserId_Day",
                table: "DelveRuns",
                columns: new[] { "UserId", "Day" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DelveRuns_WeekStart",
                table: "DelveRuns",
                column: "WeekStart");

            migrationBuilder.CreateIndex(
                name: "IX_FriendHires_LenderUserId_Day",
                table: "FriendHires",
                columns: new[] { "LenderUserId", "Day" });

            migrationBuilder.CreateIndex(
                name: "IX_FriendMineHelps_TargetUserId_Day",
                table: "FriendMineHelps",
                columns: new[] { "TargetUserId", "Day" });

            migrationBuilder.CreateIndex(
                name: "IX_GuildDepthDwarves_HeroId",
                table: "GuildDepthDwarves",
                column: "HeroId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildDepthDwarves_PlayerId",
                table: "GuildDepthDwarves",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildDepthDwarves_UserId",
                table: "GuildDepthDwarves",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildDepthDwarves_WeekStart_GuildId",
                table: "GuildDepthDwarves",
                columns: new[] { "WeekStart", "GuildId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArenaBets");

            migrationBuilder.DropTable(
                name: "DelveRuns");

            migrationBuilder.DropTable(
                name: "FriendHires");

            migrationBuilder.DropTable(
                name: "FriendMineHelps");

            migrationBuilder.DropTable(
                name: "GuildDepthDwarves");

            migrationBuilder.DropTable(
                name: "GuildDepthRewardClaims");

            migrationBuilder.DropTable(
                name: "GuildDepthShafts");

            migrationBuilder.DropTable(
                name: "ArenaBetMatches");
        }
    }
}
