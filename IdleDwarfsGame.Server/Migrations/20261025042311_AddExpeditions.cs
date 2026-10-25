using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddExpeditions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Experience",
                table: "Players",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Gold",
                table: "Players",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "Expeditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlayerId = table.Column<string>(type: "TEXT", nullable: false),
                    BoardStartsAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Slot = table.Column<int>(type: "INTEGER", nullable: false),
                    TemplateKey = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    DurationHours = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndsAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ClaimedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Power = table.Column<int>(type: "INTEGER", nullable: false),
                    Gold = table.Column<int>(type: "INTEGER", nullable: false),
                    Experience = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expeditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expeditions_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpeditionHeroes",
                columns: table => new
                {
                    ExpeditionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    HeroId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Slot = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpeditionHeroes", x => new { x.ExpeditionId, x.HeroId });
                    table.ForeignKey(
                        name: "FK_ExpeditionHeroes_Expeditions_ExpeditionId",
                        column: x => x.ExpeditionId,
                        principalTable: "Expeditions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpeditionHeroes_Heroes_HeroId",
                        column: x => x.HeroId,
                        principalTable: "Heroes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "Id",
                keyValue: "a1d2f3e4-0000-4000-8000-000000000001",
                columns: new[] { "Experience", "Gold" },
                values: new object[] { 0L, 0L });

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "Id",
                keyValue: "c0c0c0c0-0000-4000-8000-000000000003",
                columns: new[] { "Experience", "Gold" },
                values: new object[] { 0L, 0L });

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "Id",
                keyValue: "e0e0e0e0-0000-4000-8000-000000000002",
                columns: new[] { "Experience", "Gold" },
                values: new object[] { 0L, 0L });

            migrationBuilder.CreateIndex(
                name: "IX_ExpeditionHeroes_HeroId",
                table: "ExpeditionHeroes",
                column: "HeroId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expeditions_PlayerId_BoardStartsAt_Slot",
                table: "Expeditions",
                columns: new[] { "PlayerId", "BoardStartsAt", "Slot" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExpeditionHeroes");

            migrationBuilder.DropTable(
                name: "Expeditions");

            migrationBuilder.DropColumn(
                name: "Experience",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Gold",
                table: "Players");
        }
    }
}
