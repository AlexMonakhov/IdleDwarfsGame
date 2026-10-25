using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Arenas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Background = table.Column<string>(type: "TEXT", nullable: false),
                    EffectName = table.Column<string>(type: "TEXT", nullable: true),
                    EffectPower = table.Column<int>(type: "INTEGER", nullable: false),
                    EffectDuration = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arenas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CampaignStages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    EnemyLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    ArenaId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignStages_Arenas_ArenaId",
                        column: x => x.ArenaId,
                        principalTable: "Arenas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CampaignStageEnemies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StageId = table.Column<int>(type: "INTEGER", nullable: false),
                    HeroId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    IsBoss = table.Column<bool>(type: "INTEGER", nullable: false),
                    HpMultiplier = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignStageEnemies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignStageEnemies_CampaignStages_StageId",
                        column: x => x.StageId,
                        principalTable: "CampaignStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignStageEnemies_Heroes_HeroId",
                        column: x => x.HeroId,
                        principalTable: "Heroes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerCampaignProgress",
                columns: table => new
                {
                    PlayerId = table.Column<string>(type: "TEXT", nullable: false),
                    StageId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClearedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerCampaignProgress", x => new { x.PlayerId, x.StageId });
                    table.ForeignKey(
                        name: "FK_PlayerCampaignProgress_CampaignStages_StageId",
                        column: x => x.StageId,
                        principalTable: "CampaignStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerCampaignProgress_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Arenas",
                columns: new[] { "Id", "Background", "Description", "EffectDuration", "EffectName", "EffectPower", "Name" },
                values: new object[,]
                {
                    { 1, "lava", "Molten stone burns every fighter at the start of each round.", 1, "Burn", 5, "Lava Arena" },
                    { 2, "ice", "Icy wind wounds every fighter and drains their energy at the start of each round.", 1, "Frost", 3, "Ice Arena" }
                });

            migrationBuilder.InsertData(
                table: "Heroes",
                columns: new[] { "Id", "Attack", "AuraDuration", "AuraEffect", "AuraPower", "AuraTarget", "AuraTrigger", "Color", "EnergyPerAttack", "Immunities", "Level", "MaxEnergy", "MaxHp", "Name", "PlaceInSquad", "PlayerId", "Speed", "SpellIds", "StartEnergy", "UnitType" },
                values: new object[,]
                {
                    { new Guid("33333333-0000-4000-8000-000000000001"), 11, 1, null, 0, 0, 1, "blue", 20, "Frost", 4, 100, 95, "Frost Goblin", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), 8, "", 0, "Goblin" },
                    { new Guid("33333333-0000-4000-8000-000000000002"), 12, 1, null, 0, 0, 1, "blue", 20, "Frost", 4, 100, 120, "Rime Skeleton", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), 5, "4", 20, "Skeleton" },
                    { new Guid("33333333-0000-4000-8000-000000000101"), 16, 1, "Burn", 4, 1, 1, "red", 20, "Burn,Stun", 4, 100, 260, "Gorrak the Molten", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), 5, "2", 20, "Orc" },
                    { new Guid("33333333-0000-4000-8000-000000000102"), 15, 1, "Frost", 3, 1, 1, "blue", 25, "Frost,Stun", 4, 100, 240, "Hrimgar the Frostbound", null, new Guid("c0c0c0c0-0000-4000-8000-000000000003"), 6, "6", 20, "Skeleton" }
                });

            migrationBuilder.InsertData(
                table: "Players",
                columns: new[] { "Id", "Name", "Rating" },
                values: new object[] { "c0c0c0c0-0000-4000-8000-000000000003", "Campaign Monsters", 0 });

            migrationBuilder.InsertData(
                table: "CampaignStages",
                columns: new[] { "Id", "ArenaId", "Description", "EnemyLevel", "Name", "Type" },
                values: new object[,]
                {
                    { 1, 1, "Goblins guard the entrance to the fire caves.", 1, "Ember Gate", 0 },
                    { 2, 1, "An orc leads scouts through the smouldering tunnels.", 1, "Smouldering Tunnels", 0 },
                    { 3, 1, "A skeleton mage has joined the orcs in the hall of ash.", 2, "Cinder Hall", 0 },
                    { 4, 1, "The orc warlord's camp. His aura sets everything ablaze.", 2, "Warlord's Camp", 0 },
                    { 5, 1, "Gorrak the Molten guards the heart of the forge.", 2, "Heart of the Forge", 1 },
                    { 6, 2, "Frost goblins lie in ambush at the pass.", 3, "Frozen Pass", 0 },
                    { 7, 2, "The rime caverns are crawling with undead.", 3, "Rime Caverns", 0 },
                    { 8, 2, "A mixed warband holds the bridge across the glacier.", 3, "Glacier Bridge", 0 },
                    { 9, 2, "The last line of defence before the frost lord's lair.", 3, "Hall of Whispers", 0 },
                    { 10, 2, "Hrimgar the Frostbound awaits on his throne of ice.", 3, "Throne of Frost", 1 }
                });

            migrationBuilder.InsertData(
                table: "CampaignStageEnemies",
                columns: new[] { "Id", "HeroId", "HpMultiplier", "IsBoss", "Position", "StageId" },
                values: new object[,]
                {
                    { 101, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 1, 1 },
                    { 102, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 2, 1 },
                    { 201, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 2 },
                    { 202, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 2 },
                    { 203, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 2 },
                    { 301, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 3 },
                    { 302, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 3 },
                    { 303, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 3 },
                    { 304, new Guid("22222222-0000-4000-8000-000000000005"), 1.0, false, 4, 3 },
                    { 401, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 4 },
                    { 402, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 2, 4 },
                    { 403, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 4 },
                    { 404, new Guid("22222222-0000-4000-8000-000000000004"), 1.0, false, 4, 4 },
                    { 405, new Guid("22222222-0000-4000-8000-000000000005"), 1.0, false, 5, 4 },
                    { 501, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 5 },
                    { 502, new Guid("33333333-0000-4000-8000-000000000101"), 2.0, true, 2, 5 },
                    { 503, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 3, 5 },
                    { 601, new Guid("33333333-0000-4000-8000-000000000001"), 1.0, false, 1, 6 },
                    { 602, new Guid("33333333-0000-4000-8000-000000000002"), 1.0, false, 2, 6 },
                    { 603, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 3, 6 },
                    { 701, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 7 },
                    { 702, new Guid("33333333-0000-4000-8000-000000000001"), 1.0, false, 2, 7 },
                    { 703, new Guid("33333333-0000-4000-8000-000000000002"), 1.0, false, 3, 7 },
                    { 704, new Guid("22222222-0000-4000-8000-000000000005"), 1.0, false, 4, 7 },
                    { 801, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 8 },
                    { 802, new Guid("33333333-0000-4000-8000-000000000001"), 1.0, false, 2, 8 },
                    { 803, new Guid("33333333-0000-4000-8000-000000000002"), 1.0, false, 3, 8 },
                    { 804, new Guid("22222222-0000-4000-8000-000000000005"), 1.0, false, 4, 8 },
                    { 805, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 5, 8 },
                    { 901, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 9 },
                    { 902, new Guid("33333333-0000-4000-8000-000000000001"), 1.0, false, 2, 9 },
                    { 903, new Guid("33333333-0000-4000-8000-000000000002"), 1.0, false, 3, 9 },
                    { 904, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 4, 9 },
                    { 905, new Guid("22222222-0000-4000-8000-000000000005"), 1.0, false, 5, 9 },
                    { 1001, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 1, 10 },
                    { 1002, new Guid("33333333-0000-4000-8000-000000000102"), 1.75, true, 2, 10 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignStageEnemies_HeroId",
                table: "CampaignStageEnemies",
                column: "HeroId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignStageEnemies_StageId_Position",
                table: "CampaignStageEnemies",
                columns: new[] { "StageId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignStages_ArenaId",
                table: "CampaignStages",
                column: "ArenaId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerCampaignProgress_StageId",
                table: "PlayerCampaignProgress",
                column: "StageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignStageEnemies");

            migrationBuilder.DropTable(
                name: "PlayerCampaignProgress");

            migrationBuilder.DropTable(
                name: "CampaignStages");

            migrationBuilder.DropTable(
                name: "Arenas");

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000101"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000102"));

            migrationBuilder.DeleteData(
                table: "Players",
                keyColumn: "Id",
                keyValue: "c0c0c0c0-0000-4000-8000-000000000003");
        }
    }
}
