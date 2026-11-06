using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class MineDwarves : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Сначала новая таблица и перенос нынешних дворфов (на место 1), только потом старая колонка уходит
            migrationBuilder.CreateTable(
                name: "MineDwarves",
                columns: table => new
                {
                    MineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HeroId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Slot = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MineDwarves", x => new { x.MineId, x.HeroId });
                    table.ForeignKey(
                        name: "FK_MineDwarves_Heroes_HeroId",
                        column: x => x.HeroId,
                        principalTable: "Heroes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MineDwarves_Mines_MineId",
                        column: x => x.MineId,
                        principalTable: "Mines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MineDwarves_HeroId",
                table: "MineDwarves",
                column: "HeroId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MineDwarves_MineId_Slot",
                table: "MineDwarves",
                columns: new[] { "MineId", "Slot" },
                unique: true);

            migrationBuilder.Sql("INSERT INTO MineDwarves (MineId, HeroId, Slot) SELECT Id, HeroId, 1 FROM Mines WHERE HeroId IS NOT NULL");

            migrationBuilder.DropForeignKey(
                name: "FK_Mines_Heroes_HeroId",
                table: "Mines");

            migrationBuilder.DropIndex(
                name: "IX_Mines_HeroId",
                table: "Mines");

            migrationBuilder.DropColumn(
                name: "HeroId",
                table: "Mines");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HeroId",
                table: "Mines",
                type: "uniqueidentifier",
                nullable: true);

            // Обратно в одну колонку влезает только дворф с первого занятого места
            migrationBuilder.Sql(@"UPDATE Mines SET HeroId = (
                SELECT TOP 1 d.HeroId FROM MineDwarves d WHERE d.MineId = Mines.Id ORDER BY d.Slot)");

            migrationBuilder.DropTable(
                name: "MineDwarves");

            migrationBuilder.CreateIndex(
                name: "IX_Mines_HeroId",
                table: "Mines",
                column: "HeroId",
                unique: true,
                filter: "[HeroId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Mines_Heroes_HeroId",
                table: "Mines",
                column: "HeroId",
                principalTable: "Heroes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
