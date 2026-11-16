using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class Tutorial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerTutorials",
                columns: table => new
                {
                    PlayerId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    DoneAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Step = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerTutorials", x => new { x.PlayerId, x.Key });
                    table.ForeignKey(
                        name: "FK_PlayerTutorials_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CampaignStageEnemies",
                columns: new[] { "Id", "HeroId", "HpMultiplier", "IsBoss", "Position", "StageId" },
                values: new object[] { 103, new Guid("22222222-0000-4000-8000-000000000002"), 1.0, false, 3, 1 });

            migrationBuilder.InsertData(
                table: "ChestDropTables",
                columns: new[] { "Id", "ChestLevel", "ChestType", "Rolls" },
                values: new object[] { "c4e50000-0000-4000-8000-000004000001", 1, 4, 1 });

            migrationBuilder.InsertData(
                table: "DropEntry",
                columns: new[] { "Id", "ChestDropTableId", "ItemLevel", "ItemType", "MaxAmount", "MinAmount", "Probability", "Rarity", "SubType" },
                values: new object[] { new Guid("c4e50001-0000-4000-8000-000400010000"), "c4e50000-0000-4000-8000-000004000001", 1, 3, 25, 25, 1.0, 0, "" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerTutorials");

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000400010000"));

            migrationBuilder.DeleteData(
                table: "ChestDropTables",
                keyColumn: "Id",
                keyValue: "c4e50000-0000-4000-8000-000004000001");
        }
    }
}
