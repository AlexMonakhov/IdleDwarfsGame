using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddRewardsAndInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rolls",
                table: "ChestDropTables",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            // Таблицы, заведённые в БД вручную до сидов: там, где тип и уровень совпадают с сидом (LootSeedData),
            // главным становится сид, а среди остальных дублей по (тип, уровень) остаётся один — иначе не создать
            // уникальный индекс. Записи и сундуки игроков с удалёнными таблицами уходят каскадом
            migrationBuilder.Sql(
                "DELETE FROM ChestDropTables WHERE Id NOT LIKE 'c4e50000-%' AND ChestType IN (0, 1) AND ChestLevel BETWEEN 1 AND 3;");
            migrationBuilder.Sql(
                "DELETE FROM ChestDropTables WHERE rowid NOT IN (SELECT MIN(rowid) FROM ChestDropTables GROUP BY ChestType, ChestLevel);");

            migrationBuilder.CreateTable(
                name: "PlayerItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlayerId = table.Column<string>(type: "TEXT", nullable: false),
                    ItemType = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    SubType = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Rarity = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerItems_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerRewards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlayerId = table.Column<string>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    SourceId = table.Column<int>(type: "INTEGER", nullable: false),
                    Gold = table.Column<int>(type: "INTEGER", nullable: false),
                    Experience = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ClaimedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerRewards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerRewards_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerRewardChests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RewardId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChestType = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    ChestLevel = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerRewardChests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerRewardChests_PlayerRewards_RewardId",
                        column: x => x.RewardId,
                        principalTable: "PlayerRewards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ChestDropTables",
                columns: new[] { "Id", "ChestLevel", "ChestType", "Rolls" },
                values: new object[,]
                {
                    { "c4e50000-0000-4000-8000-000000000001", 1, 0, 1 },
                    { "c4e50000-0000-4000-8000-000000000002", 2, 0, 1 },
                    { "c4e50000-0000-4000-8000-000000000003", 3, 0, 1 },
                    { "c4e50000-0000-4000-8000-000001000001", 1, 1, 2 },
                    { "c4e50000-0000-4000-8000-000001000002", 2, 1, 3 },
                    { "c4e50000-0000-4000-8000-000001000003", 3, 1, 3 }
                });

            migrationBuilder.InsertData(
                table: "DropEntry",
                columns: new[] { "Id", "ChestDropTableId", "ItemLevel", "ItemType", "MaxAmount", "MinAmount", "Probability", "Rarity", "SubType" },
                values: new object[,]
                {
                    { new Guid("c4e50001-0000-4000-8000-000000010000"), "c4e50000-0000-4000-8000-000000000001", 1, 3, 1, 1, 0.80000000000000004, 0, "" },
                    { new Guid("c4e50001-0000-4000-8000-000000010001"), "c4e50000-0000-4000-8000-000000000001", 2, 3, 1, 1, 0.20000000000000001, 1, "" },
                    { new Guid("c4e50001-0000-4000-8000-000000020000"), "c4e50000-0000-4000-8000-000000000002", 1, 3, 1, 1, 0.5, 0, "" },
                    { new Guid("c4e50001-0000-4000-8000-000000020001"), "c4e50000-0000-4000-8000-000000000002", 2, 3, 1, 1, 0.40000000000000002, 1, "" },
                    { new Guid("c4e50001-0000-4000-8000-000000020002"), "c4e50000-0000-4000-8000-000000000002", 3, 3, 1, 1, 0.10000000000000001, 2, "" },
                    { new Guid("c4e50001-0000-4000-8000-000000030000"), "c4e50000-0000-4000-8000-000000000003", 2, 3, 1, 1, 0.5, 1, "" },
                    { new Guid("c4e50001-0000-4000-8000-000000030001"), "c4e50000-0000-4000-8000-000000000003", 3, 3, 1, 1, 0.40000000000000002, 2, "" },
                    { new Guid("c4e50001-0000-4000-8000-000000030002"), "c4e50000-0000-4000-8000-000000000003", 4, 3, 1, 1, 0.10000000000000001, 3, "" },
                    { new Guid("c4e50001-0000-4000-8000-000100010000"), "c4e50000-0000-4000-8000-000001000001", 1, 2, 4, 2, 0.22, 0, "Ruby" },
                    { new Guid("c4e50001-0000-4000-8000-000100010001"), "c4e50000-0000-4000-8000-000001000001", 1, 2, 4, 2, 0.22, 0, "Sapphire" },
                    { new Guid("c4e50001-0000-4000-8000-000100010002"), "c4e50000-0000-4000-8000-000001000001", 1, 2, 4, 2, 0.22, 0, "Topaz" },
                    { new Guid("c4e50001-0000-4000-8000-000100010003"), "c4e50000-0000-4000-8000-000001000001", 1, 2, 4, 2, 0.22, 0, "Emerald" },
                    { new Guid("c4e50001-0000-4000-8000-000100010004"), "c4e50000-0000-4000-8000-000001000001", 1, 2, 1, 1, 0.050000000000000003, 2, "Diamond" },
                    { new Guid("c4e50001-0000-4000-8000-000100020000"), "c4e50000-0000-4000-8000-000001000002", 1, 2, 6, 3, 0.17999999999999999, 0, "Ruby" },
                    { new Guid("c4e50001-0000-4000-8000-000100020001"), "c4e50000-0000-4000-8000-000001000002", 1, 2, 6, 3, 0.17999999999999999, 0, "Sapphire" },
                    { new Guid("c4e50001-0000-4000-8000-000100020002"), "c4e50000-0000-4000-8000-000001000002", 1, 2, 6, 3, 0.17999999999999999, 0, "Topaz" },
                    { new Guid("c4e50001-0000-4000-8000-000100020003"), "c4e50000-0000-4000-8000-000001000002", 1, 2, 6, 3, 0.17999999999999999, 0, "Emerald" },
                    { new Guid("c4e50001-0000-4000-8000-000100020004"), "c4e50000-0000-4000-8000-000001000002", 1, 2, 2, 1, 0.050000000000000003, 1, "Ruby" },
                    { new Guid("c4e50001-0000-4000-8000-000100020005"), "c4e50000-0000-4000-8000-000001000002", 1, 2, 2, 1, 0.050000000000000003, 1, "Sapphire" },
                    { new Guid("c4e50001-0000-4000-8000-000100020006"), "c4e50000-0000-4000-8000-000001000002", 1, 2, 2, 1, 0.050000000000000003, 1, "Topaz" },
                    { new Guid("c4e50001-0000-4000-8000-000100020007"), "c4e50000-0000-4000-8000-000001000002", 1, 2, 2, 1, 0.050000000000000003, 1, "Emerald" },
                    { new Guid("c4e50001-0000-4000-8000-000100020008"), "c4e50000-0000-4000-8000-000001000002", 1, 2, 1, 1, 0.059999999999999998, 2, "Diamond" },
                    { new Guid("c4e50001-0000-4000-8000-000100030000"), "c4e50000-0000-4000-8000-000001000003", 1, 2, 8, 4, 0.12, 0, "Ruby" },
                    { new Guid("c4e50001-0000-4000-8000-000100030001"), "c4e50000-0000-4000-8000-000001000003", 1, 2, 8, 4, 0.12, 0, "Sapphire" },
                    { new Guid("c4e50001-0000-4000-8000-000100030002"), "c4e50000-0000-4000-8000-000001000003", 1, 2, 8, 4, 0.12, 0, "Topaz" },
                    { new Guid("c4e50001-0000-4000-8000-000100030003"), "c4e50000-0000-4000-8000-000001000003", 1, 2, 8, 4, 0.12, 0, "Emerald" },
                    { new Guid("c4e50001-0000-4000-8000-000100030004"), "c4e50000-0000-4000-8000-000001000003", 1, 2, 3, 2, 0.089999999999999997, 1, "Ruby" },
                    { new Guid("c4e50001-0000-4000-8000-000100030005"), "c4e50000-0000-4000-8000-000001000003", 1, 2, 3, 2, 0.089999999999999997, 1, "Sapphire" },
                    { new Guid("c4e50001-0000-4000-8000-000100030006"), "c4e50000-0000-4000-8000-000001000003", 1, 2, 3, 2, 0.089999999999999997, 1, "Topaz" },
                    { new Guid("c4e50001-0000-4000-8000-000100030007"), "c4e50000-0000-4000-8000-000001000003", 1, 2, 3, 2, 0.089999999999999997, 1, "Emerald" },
                    { new Guid("c4e50001-0000-4000-8000-000100030008"), "c4e50000-0000-4000-8000-000001000003", 1, 2, 2, 1, 0.12, 2, "Diamond" },
                    { new Guid("c4e50001-0000-4000-8000-000100030009"), "c4e50000-0000-4000-8000-000001000003", 1, 2, 1, 1, 0.040000000000000001, 3, "Diamond" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChestDropTables_ChestType_ChestLevel",
                table: "ChestDropTables",
                columns: new[] { "ChestType", "ChestLevel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerItems_PlayerId_ItemType_SubType_Rarity_Level",
                table: "PlayerItems",
                columns: new[] { "PlayerId", "ItemType", "SubType", "Rarity", "Level" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerRewardChests_RewardId",
                table: "PlayerRewardChests",
                column: "RewardId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerRewards_PlayerId_Source_ClaimedAt",
                table: "PlayerRewards",
                columns: new[] { "PlayerId", "Source", "ClaimedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerItems");

            migrationBuilder.DropTable(
                name: "PlayerRewardChests");

            migrationBuilder.DropTable(
                name: "PlayerRewards");

            migrationBuilder.DropIndex(
                name: "IX_ChestDropTables_ChestType_ChestLevel",
                table: "ChestDropTables");

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000010000"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000010001"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000020000"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000020001"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000020002"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000030000"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000030001"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000030002"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100010000"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100010001"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100010002"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100010003"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100010004"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100020000"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100020001"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100020002"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100020003"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100020004"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100020005"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100020006"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100020007"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100020008"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100030000"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100030001"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100030002"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100030003"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100030004"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100030005"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100030006"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100030007"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100030008"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100030009"));

            migrationBuilder.DeleteData(
                table: "ChestDropTables",
                keyColumn: "Id",
                keyValue: "c4e50000-0000-4000-8000-000000000001");

            migrationBuilder.DeleteData(
                table: "ChestDropTables",
                keyColumn: "Id",
                keyValue: "c4e50000-0000-4000-8000-000000000002");

            migrationBuilder.DeleteData(
                table: "ChestDropTables",
                keyColumn: "Id",
                keyValue: "c4e50000-0000-4000-8000-000000000003");

            migrationBuilder.DeleteData(
                table: "ChestDropTables",
                keyColumn: "Id",
                keyValue: "c4e50000-0000-4000-8000-000001000001");

            migrationBuilder.DeleteData(
                table: "ChestDropTables",
                keyColumn: "Id",
                keyValue: "c4e50000-0000-4000-8000-000001000002");

            migrationBuilder.DeleteData(
                table: "ChestDropTables",
                keyColumn: "Id",
                keyValue: "c4e50000-0000-4000-8000-000001000003");

            migrationBuilder.DropColumn(
                name: "Rolls",
                table: "ChestDropTables");
        }
    }
}
