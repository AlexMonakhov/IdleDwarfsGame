using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class EconomyAndChests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ChestDropTables",
                keyColumn: "Id",
                keyValue: "c4e50000-0000-4000-8000-000000000001",
                column: "Rolls",
                value: 2);

            migrationBuilder.UpdateData(
                table: "ChestDropTables",
                keyColumn: "Id",
                keyValue: "c4e50000-0000-4000-8000-000000000002",
                column: "Rolls",
                value: 2);

            migrationBuilder.UpdateData(
                table: "ChestDropTables",
                keyColumn: "Id",
                keyValue: "c4e50000-0000-4000-8000-000000000003",
                column: "Rolls",
                value: 2);

            migrationBuilder.InsertData(
                table: "ChestDropTables",
                columns: new[] { "Id", "ChestLevel", "ChestType", "Rolls" },
                values: new object[,]
                {
                    { "c4e50000-0000-4000-8000-000000000004", 4, 0, 2 },
                    { "c4e50000-0000-4000-8000-000000000005", 5, 0, 2 },
                    { "c4e50000-0000-4000-8000-000001000004", 4, 1, 4 },
                    { "c4e50000-0000-4000-8000-000001000005", 5, 1, 5 },
                    { "c4e50000-0000-4000-8000-000003000004", 4, 3, 1 },
                    { "c4e50000-0000-4000-8000-000003000005", 5, 3, 1 }
                });

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000010000"),
                column: "Probability",
                value: 0.75);

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000010001"),
                column: "Probability",
                value: 0.23000000000000001);

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000020002"),
                column: "Probability",
                value: 0.089999999999999997);

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000030000"),
                columns: new[] { "ItemLevel", "Probability", "Rarity" },
                values: new object[] { 1, 0.29999999999999999, 0 });

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000030001"),
                columns: new[] { "ItemLevel", "Probability", "Rarity" },
                values: new object[] { 2, 0.45000000000000001, 1 });

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000030002"),
                columns: new[] { "ItemLevel", "Probability", "Rarity" },
                values: new object[] { 3, 0.22, 2 });

            migrationBuilder.InsertData(
                table: "DropEntry",
                columns: new[] { "Id", "ChestDropTableId", "ItemLevel", "ItemType", "MaxAmount", "MinAmount", "Probability", "Rarity", "SubType" },
                values: new object[,]
                {
                    { new Guid("c4e50001-0000-4000-8000-000000010002"), "c4e50000-0000-4000-8000-000000000001", 3, 3, 1, 1, 0.02, 2, "" },
                    { new Guid("c4e50001-0000-4000-8000-000000020003"), "c4e50000-0000-4000-8000-000000000002", 4, 3, 1, 1, 0.01, 3, "" },
                    { new Guid("c4e50001-0000-4000-8000-000000030003"), "c4e50000-0000-4000-8000-000000000003", 4, 3, 1, 1, 0.029999999999999999, 3, "" },
                    { new Guid("c4e50001-0000-4000-8000-000000040000"), "c4e50000-0000-4000-8000-000000000004", 1, 3, 1, 1, 0.20000000000000001, 0, "" },
                    { new Guid("c4e50001-0000-4000-8000-000000040001"), "c4e50000-0000-4000-8000-000000000004", 2, 3, 1, 1, 0.40000000000000002, 1, "" },
                    { new Guid("c4e50001-0000-4000-8000-000000040002"), "c4e50000-0000-4000-8000-000000000004", 3, 3, 1, 1, 0.33000000000000002, 2, "" },
                    { new Guid("c4e50001-0000-4000-8000-000000040003"), "c4e50000-0000-4000-8000-000000000004", 4, 3, 1, 1, 0.070000000000000007, 3, "" },
                    { new Guid("c4e50001-0000-4000-8000-000000050000"), "c4e50000-0000-4000-8000-000000000005", 1, 3, 1, 1, 0.10000000000000001, 0, "" },
                    { new Guid("c4e50001-0000-4000-8000-000000050001"), "c4e50000-0000-4000-8000-000000000005", 2, 3, 1, 1, 0.34999999999999998, 1, "" },
                    { new Guid("c4e50001-0000-4000-8000-000000050002"), "c4e50000-0000-4000-8000-000000000005", 3, 3, 1, 1, 0.42999999999999999, 2, "" },
                    { new Guid("c4e50001-0000-4000-8000-000000050003"), "c4e50000-0000-4000-8000-000000000005", 4, 3, 1, 1, 0.12, 3, "" },
                    { new Guid("c4e50001-0000-4000-8000-000100040000"), "c4e50000-0000-4000-8000-000001000004", 1, 2, 12, 6, 0.10000000000000001, 0, "Ruby" },
                    { new Guid("c4e50001-0000-4000-8000-000100040001"), "c4e50000-0000-4000-8000-000001000004", 1, 2, 12, 6, 0.10000000000000001, 0, "Sapphire" },
                    { new Guid("c4e50001-0000-4000-8000-000100040002"), "c4e50000-0000-4000-8000-000001000004", 1, 2, 12, 6, 0.10000000000000001, 0, "Topaz" },
                    { new Guid("c4e50001-0000-4000-8000-000100040003"), "c4e50000-0000-4000-8000-000001000004", 1, 2, 12, 6, 0.10000000000000001, 0, "Emerald" },
                    { new Guid("c4e50001-0000-4000-8000-000100040004"), "c4e50000-0000-4000-8000-000001000004", 1, 2, 5, 3, 0.10000000000000001, 1, "Ruby" },
                    { new Guid("c4e50001-0000-4000-8000-000100040005"), "c4e50000-0000-4000-8000-000001000004", 1, 2, 5, 3, 0.10000000000000001, 1, "Sapphire" },
                    { new Guid("c4e50001-0000-4000-8000-000100040006"), "c4e50000-0000-4000-8000-000001000004", 1, 2, 5, 3, 0.10000000000000001, 1, "Topaz" },
                    { new Guid("c4e50001-0000-4000-8000-000100040007"), "c4e50000-0000-4000-8000-000001000004", 1, 2, 5, 3, 0.10000000000000001, 1, "Emerald" },
                    { new Guid("c4e50001-0000-4000-8000-000100040008"), "c4e50000-0000-4000-8000-000001000004", 1, 2, 3, 1, 0.14000000000000001, 2, "Diamond" },
                    { new Guid("c4e50001-0000-4000-8000-000100040009"), "c4e50000-0000-4000-8000-000001000004", 1, 2, 1, 1, 0.059999999999999998, 3, "Diamond" },
                    { new Guid("c4e50001-0000-4000-8000-000100050000"), "c4e50000-0000-4000-8000-000001000005", 1, 2, 20, 10, 0.080000000000000002, 0, "Ruby" },
                    { new Guid("c4e50001-0000-4000-8000-000100050001"), "c4e50000-0000-4000-8000-000001000005", 1, 2, 20, 10, 0.080000000000000002, 0, "Sapphire" },
                    { new Guid("c4e50001-0000-4000-8000-000100050002"), "c4e50000-0000-4000-8000-000001000005", 1, 2, 20, 10, 0.080000000000000002, 0, "Topaz" },
                    { new Guid("c4e50001-0000-4000-8000-000100050003"), "c4e50000-0000-4000-8000-000001000005", 1, 2, 20, 10, 0.080000000000000002, 0, "Emerald" },
                    { new Guid("c4e50001-0000-4000-8000-000100050004"), "c4e50000-0000-4000-8000-000001000005", 1, 2, 8, 4, 0.11, 1, "Ruby" },
                    { new Guid("c4e50001-0000-4000-8000-000100050005"), "c4e50000-0000-4000-8000-000001000005", 1, 2, 8, 4, 0.11, 1, "Sapphire" },
                    { new Guid("c4e50001-0000-4000-8000-000100050006"), "c4e50000-0000-4000-8000-000001000005", 1, 2, 8, 4, 0.11, 1, "Topaz" },
                    { new Guid("c4e50001-0000-4000-8000-000100050007"), "c4e50000-0000-4000-8000-000001000005", 1, 2, 8, 4, 0.11, 1, "Emerald" },
                    { new Guid("c4e50001-0000-4000-8000-000100050008"), "c4e50000-0000-4000-8000-000001000005", 1, 2, 4, 2, 0.16, 2, "Diamond" },
                    { new Guid("c4e50001-0000-4000-8000-000100050009"), "c4e50000-0000-4000-8000-000001000005", 1, 2, 2, 1, 0.080000000000000002, 3, "Diamond" },
                    { new Guid("c4e50001-0000-4000-8000-000300040000"), "c4e50000-0000-4000-8000-000003000004", 1, 0, 1, 1, 0.049999999999999996, 2, "EmberMace" },
                    { new Guid("c4e50001-0000-4000-8000-000300040001"), "c4e50000-0000-4000-8000-000003000004", 1, 0, 1, 1, 0.049999999999999996, 2, "StormSpear" },
                    { new Guid("c4e50001-0000-4000-8000-000300040002"), "c4e50000-0000-4000-8000-000003000004", 1, 0, 1, 1, 0.049999999999999996, 2, "EmberCrown" },
                    { new Guid("c4e50001-0000-4000-8000-000300040003"), "c4e50000-0000-4000-8000-000003000004", 1, 0, 1, 1, 0.049999999999999996, 2, "FrostCrown" },
                    { new Guid("c4e50001-0000-4000-8000-000300040004"), "c4e50000-0000-4000-8000-000003000004", 1, 0, 1, 1, 0.049999999999999996, 2, "RunicAmulet" },
                    { new Guid("c4e50001-0000-4000-8000-000300040005"), "c4e50000-0000-4000-8000-000003000004", 1, 0, 1, 1, 0.049999999999999996, 2, "RingOfLingeringWords" },
                    { new Guid("c4e50001-0000-4000-8000-000300040006"), "c4e50000-0000-4000-8000-000003000004", 1, 0, 1, 1, 0.049999999999999996, 2, "RingOfTheStoneSkin" },
                    { new Guid("c4e50001-0000-4000-8000-000300040007"), "c4e50000-0000-4000-8000-000003000004", 1, 0, 1, 1, 0.080000000000000002, 3, "BlackIceSword" },
                    { new Guid("c4e50001-0000-4000-8000-000300040008"), "c4e50000-0000-4000-8000-000003000004", 1, 0, 1, 1, 0.080000000000000002, 3, "ThornStaff" },
                    { new Guid("c4e50001-0000-4000-8000-000300040009"), "c4e50000-0000-4000-8000-000003000004", 1, 0, 1, 1, 0.080000000000000002, 3, "GroveCirclet" },
                    { new Guid("c4e50001-0000-4000-8000-000300040010"), "c4e50000-0000-4000-8000-000003000004", 1, 0, 1, 1, 0.080000000000000002, 3, "StormAmulet" },
                    { new Guid("c4e50001-0000-4000-8000-000300040011"), "c4e50000-0000-4000-8000-000003000004", 1, 0, 1, 1, 0.080000000000000002, 3, "RingOfCleaving" },
                    { new Guid("c4e50001-0000-4000-8000-000300040012"), "c4e50000-0000-4000-8000-000003000004", 1, 0, 1, 1, 0.050000000000000003, 4, "DawnBlade" },
                    { new Guid("c4e50001-0000-4000-8000-000300040013"), "c4e50000-0000-4000-8000-000003000004", 1, 0, 1, 1, 0.050000000000000003, 4, "WarlordHelm" },
                    { new Guid("c4e50001-0000-4000-8000-000300040014"), "c4e50000-0000-4000-8000-000003000004", 1, 0, 1, 1, 0.050000000000000003, 4, "HeartOfTheMountain" },
                    { new Guid("c4e50001-0000-4000-8000-000300040015"), "c4e50000-0000-4000-8000-000003000004", 1, 0, 1, 1, 0.050000000000000003, 4, "RingOfWildfire" },
                    { new Guid("c4e50001-0000-4000-8000-000300040016"), "c4e50000-0000-4000-8000-000003000004", 1, 0, 1, 1, 0.050000000000000003, 5, "ArmageddonBlade" },
                    { new Guid("c4e50001-0000-4000-8000-000300050000"), "c4e50000-0000-4000-8000-000003000005", 1, 0, 1, 1, 0.089999999999999997, 3, "BlackIceSword" },
                    { new Guid("c4e50001-0000-4000-8000-000300050001"), "c4e50000-0000-4000-8000-000003000005", 1, 0, 1, 1, 0.089999999999999997, 3, "ThornStaff" },
                    { new Guid("c4e50001-0000-4000-8000-000300050002"), "c4e50000-0000-4000-8000-000003000005", 1, 0, 1, 1, 0.089999999999999997, 3, "GroveCirclet" },
                    { new Guid("c4e50001-0000-4000-8000-000300050003"), "c4e50000-0000-4000-8000-000003000005", 1, 0, 1, 1, 0.089999999999999997, 3, "StormAmulet" },
                    { new Guid("c4e50001-0000-4000-8000-000300050004"), "c4e50000-0000-4000-8000-000003000005", 1, 0, 1, 1, 0.089999999999999997, 3, "RingOfCleaving" },
                    { new Guid("c4e50001-0000-4000-8000-000300050005"), "c4e50000-0000-4000-8000-000003000005", 1, 0, 1, 1, 0.10000000000000001, 4, "DawnBlade" },
                    { new Guid("c4e50001-0000-4000-8000-000300050006"), "c4e50000-0000-4000-8000-000003000005", 1, 0, 1, 1, 0.10000000000000001, 4, "WarlordHelm" },
                    { new Guid("c4e50001-0000-4000-8000-000300050007"), "c4e50000-0000-4000-8000-000003000005", 1, 0, 1, 1, 0.10000000000000001, 4, "HeartOfTheMountain" },
                    { new Guid("c4e50001-0000-4000-8000-000300050008"), "c4e50000-0000-4000-8000-000003000005", 1, 0, 1, 1, 0.10000000000000001, 4, "RingOfWildfire" },
                    { new Guid("c4e50001-0000-4000-8000-000300050009"), "c4e50000-0000-4000-8000-000003000005", 1, 0, 1, 1, 0.14999999999999999, 5, "ArmageddonBlade" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000010002"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000020003"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000030003"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000040000"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000040001"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000040002"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000040003"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000050000"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000050001"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000050002"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000050003"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100040000"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100040001"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100040002"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100040003"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100040004"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100040005"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100040006"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100040007"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100040008"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100040009"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100050000"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100050001"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100050002"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100050003"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100050004"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100050005"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100050006"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100050007"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100050008"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000100050009"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300040000"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300040001"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300040002"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300040003"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300040004"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300040005"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300040006"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300040007"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300040008"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300040009"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300040010"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300040011"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300040012"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300040013"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300040014"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300040015"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300040016"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300050000"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300050001"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300050002"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300050003"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300050004"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300050005"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300050006"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300050007"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300050008"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300050009"));

            migrationBuilder.DeleteData(
                table: "ChestDropTables",
                keyColumn: "Id",
                keyValue: "c4e50000-0000-4000-8000-000000000004");

            migrationBuilder.DeleteData(
                table: "ChestDropTables",
                keyColumn: "Id",
                keyValue: "c4e50000-0000-4000-8000-000000000005");

            migrationBuilder.DeleteData(
                table: "ChestDropTables",
                keyColumn: "Id",
                keyValue: "c4e50000-0000-4000-8000-000001000004");

            migrationBuilder.DeleteData(
                table: "ChestDropTables",
                keyColumn: "Id",
                keyValue: "c4e50000-0000-4000-8000-000001000005");

            migrationBuilder.DeleteData(
                table: "ChestDropTables",
                keyColumn: "Id",
                keyValue: "c4e50000-0000-4000-8000-000003000004");

            migrationBuilder.DeleteData(
                table: "ChestDropTables",
                keyColumn: "Id",
                keyValue: "c4e50000-0000-4000-8000-000003000005");

            migrationBuilder.UpdateData(
                table: "ChestDropTables",
                keyColumn: "Id",
                keyValue: "c4e50000-0000-4000-8000-000000000001",
                column: "Rolls",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ChestDropTables",
                keyColumn: "Id",
                keyValue: "c4e50000-0000-4000-8000-000000000002",
                column: "Rolls",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ChestDropTables",
                keyColumn: "Id",
                keyValue: "c4e50000-0000-4000-8000-000000000003",
                column: "Rolls",
                value: 1);

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000010000"),
                column: "Probability",
                value: 0.80000000000000004);

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000010001"),
                column: "Probability",
                value: 0.20000000000000001);

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000020002"),
                column: "Probability",
                value: 0.10000000000000001);

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000030000"),
                columns: new[] { "ItemLevel", "Probability", "Rarity" },
                values: new object[] { 2, 0.5, 1 });

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000030001"),
                columns: new[] { "ItemLevel", "Probability", "Rarity" },
                values: new object[] { 3, 0.40000000000000002, 2 });

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000000030002"),
                columns: new[] { "ItemLevel", "Probability", "Rarity" },
                values: new object[] { 4, 0.10000000000000001, 3 });
        }
    }
}
