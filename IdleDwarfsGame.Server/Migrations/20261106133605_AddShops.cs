using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddShops : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "BloodCoins",
                table: "Players",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "HonorBadges",
                table: "Players",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "RainbowCoins",
                table: "Players",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "ShopOffers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    Shop = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Slot = table.Column<int>(type: "int", nullable: false),
                    Rarity = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ItemKey = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    HeroJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PriceGold = table.Column<long>(type: "bigint", nullable: false),
                    PriceCurrency = table.Column<long>(type: "bigint", nullable: false),
                    PriceGem = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    PriceGemRarity = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    PriceGems = table.Column<int>(type: "int", nullable: false),
                    Locked = table.Column<bool>(type: "bit", nullable: false),
                    Sold = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopOffers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShopStates",
                columns: table => new
                {
                    PlayerId = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    Shop = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    RefreshedDay = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FreeRerollDay = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopStates", x => new { x.PlayerId, x.Shop });
                    table.ForeignKey(
                        name: "FK_ShopStates_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "Id",
                keyValue: "a1d2f3e4-0000-4000-8000-000000000001",
                columns: new[] { "BloodCoins", "HonorBadges", "RainbowCoins" },
                values: new object[] { 0L, 0L, 0L });

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "Id",
                keyValue: "b0b0b0b0-0000-4000-8000-000000000004",
                columns: new[] { "BloodCoins", "HonorBadges", "RainbowCoins" },
                values: new object[] { 0L, 0L, 0L });

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "Id",
                keyValue: "c0c0c0c0-0000-4000-8000-000000000003",
                columns: new[] { "BloodCoins", "HonorBadges", "RainbowCoins" },
                values: new object[] { 0L, 0L, 0L });

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "Id",
                keyValue: "e0e0e0e0-0000-4000-8000-000000000002",
                columns: new[] { "BloodCoins", "HonorBadges", "RainbowCoins" },
                values: new object[] { 0L, 0L, 0L });

            // Уже существующим игрокам — те же стартовые радужные монеты, что новым (ShopRules.StarterRainbowCoins на момент миграции)
            migrationBuilder.Sql("UPDATE Players SET RainbowCoins = 50");

            migrationBuilder.CreateIndex(
                name: "IX_ShopOffers_PlayerId_Shop_Kind_Slot",
                table: "ShopOffers",
                columns: new[] { "PlayerId", "Shop", "Kind", "Slot" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShopOffers");

            migrationBuilder.DropTable(
                name: "ShopStates");

            migrationBuilder.DropColumn(
                name: "BloodCoins",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "HonorBadges",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "RainbowCoins",
                table: "Players");
        }
    }
}
