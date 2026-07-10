using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DwarfMiningIdleGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddDropEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Players",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "NEWID()",
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PlayerId",
                table: "Heroes",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Heroes",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "NEWID()",
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.CreateTable(
                name: "ChestDropTables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false, defaultValueSql: "NEWID()"),
                    ChestLevel = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChestDropTables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DropEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false, defaultValueSql: "NEWID()"),
                    Probability = table.Column<double>(type: "REAL", nullable: false),
                    MinAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemType = table.Column<int>(type: "INTEGER", nullable: false),
                    Rarity = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    ChestDropTableId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DropEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DropEntry_ChestDropTables_ChestDropTableId",
                        column: x => x.ChestDropTableId,
                        principalTable: "ChestDropTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerChests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false, defaultValueSql: "NEWID()"),
                    PlayerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChestDropTableId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsOpened = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerChests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerChests_ChestDropTables_ChestDropTableId",
                        column: x => x.ChestDropTableId,
                        principalTable: "ChestDropTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerChests_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DropEntry_ChestDropTableId",
                table: "DropEntry",
                column: "ChestDropTableId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerChests_ChestDropTableId",
                table: "PlayerChests",
                column: "ChestDropTableId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerChests_PlayerId",
                table: "PlayerChests",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DropEntry");

            migrationBuilder.DropTable(
                name: "PlayerChests");

            migrationBuilder.DropTable(
                name: "ChestDropTables");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Players",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldDefaultValueSql: "NEWID()")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<int>(
                name: "PlayerId",
                table: "Heroes",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Heroes",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldDefaultValueSql: "NEWID()")
                .Annotation("Sqlite:Autoincrement", true);
        }
    }
}
