using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Passives",
                table: "Heroes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PlayerEquipment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlayerId = table.Column<string>(type: "TEXT", nullable: false),
                    ItemKey = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    HeroId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Slot = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerEquipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerEquipment_Heroes_HeroId",
                        column: x => x.HeroId,
                        principalTable: "Heroes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PlayerEquipment_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ChestDropTables",
                columns: new[] { "Id", "ChestLevel", "ChestType", "Rolls" },
                values: new object[,]
                {
                    { "c4e50000-0000-4000-8000-000003000001", 1, 3, 1 },
                    { "c4e50000-0000-4000-8000-000003000002", 2, 3, 1 },
                    { "c4e50000-0000-4000-8000-000003000003", 3, 3, 1 }
                });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000001"),
                column: "Passives",
                value: "");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000002"),
                column: "Passives",
                value: "");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000003"),
                column: "Passives",
                value: "");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000004"),
                column: "Passives",
                value: "");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000005"),
                column: "Passives",
                value: "");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000006"),
                column: "Passives",
                value: "");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000007"),
                column: "Passives",
                value: "");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000008"),
                column: "Passives",
                value: "");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000001"),
                column: "Passives",
                value: "");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000002"),
                column: "Passives",
                value: "");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000003"),
                column: "Passives",
                value: "");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000004"),
                column: "Passives",
                value: "");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000005"),
                column: "Passives",
                value: "");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000001"),
                column: "Passives",
                value: "");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000002"),
                column: "Passives",
                value: "");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000101"),
                column: "Passives",
                value: "");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000102"),
                column: "Passives",
                value: "");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000001"),
                column: "Passives",
                value: "");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000002"),
                column: "Passives",
                value: "");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000003"),
                column: "Passives",
                value: "");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000004"),
                column: "Passives",
                value: "");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000005"),
                column: "Passives",
                value: "");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000006"),
                column: "Passives",
                value: "");

            migrationBuilder.InsertData(
                table: "DropEntry",
                columns: new[] { "Id", "ChestDropTableId", "ItemLevel", "ItemType", "MaxAmount", "MinAmount", "Probability", "Rarity", "SubType" },
                values: new object[,]
                {
                    { new Guid("c4e50001-0000-4000-8000-000300010000"), "c4e50000-0000-4000-8000-000003000001", 1, 0, 1, 1, 0.14999999999999999, 0, "RustyAxe" },
                    { new Guid("c4e50001-0000-4000-8000-000300010001"), "c4e50000-0000-4000-8000-000003000001", 1, 0, 1, 1, 0.14999999999999999, 0, "IronHelm" },
                    { new Guid("c4e50001-0000-4000-8000-000300010002"), "c4e50000-0000-4000-8000-000003000001", 1, 0, 1, 1, 0.14999999999999999, 0, "BoneNecklace" },
                    { new Guid("c4e50001-0000-4000-8000-000300010003"), "c4e50000-0000-4000-8000-000003000001", 1, 0, 1, 1, 0.14999999999999999, 0, "CopperRing" },
                    { new Guid("c4e50001-0000-4000-8000-000300010004"), "c4e50000-0000-4000-8000-000003000001", 1, 0, 1, 1, 0.049999999999999996, 1, "DwarvenHammer" },
                    { new Guid("c4e50001-0000-4000-8000-000300010005"), "c4e50000-0000-4000-8000-000003000001", 1, 0, 1, 1, 0.049999999999999996, 1, "MinerHelm" },
                    { new Guid("c4e50001-0000-4000-8000-000300010006"), "c4e50000-0000-4000-8000-000003000001", 1, 0, 1, 1, 0.049999999999999996, 1, "MoonstonePendant" },
                    { new Guid("c4e50001-0000-4000-8000-000300010007"), "c4e50000-0000-4000-8000-000003000001", 1, 0, 1, 1, 0.049999999999999996, 1, "RingOfTheUnderdog" },
                    { new Guid("c4e50001-0000-4000-8000-000300010008"), "c4e50000-0000-4000-8000-000003000001", 1, 0, 1, 1, 0.049999999999999996, 1, "SpitefulRing" },
                    { new Guid("c4e50001-0000-4000-8000-000300010009"), "c4e50000-0000-4000-8000-000003000001", 1, 0, 1, 1, 0.049999999999999996, 1, "TwinBand" },
                    { new Guid("c4e50001-0000-4000-8000-000300010010"), "c4e50000-0000-4000-8000-000003000001", 1, 0, 1, 1, 0.012857142857142857, 2, "EmberMace" },
                    { new Guid("c4e50001-0000-4000-8000-000300010011"), "c4e50000-0000-4000-8000-000003000001", 1, 0, 1, 1, 0.012857142857142857, 2, "StormSpear" },
                    { new Guid("c4e50001-0000-4000-8000-000300010012"), "c4e50000-0000-4000-8000-000003000001", 1, 0, 1, 1, 0.012857142857142857, 2, "EmberCrown" },
                    { new Guid("c4e50001-0000-4000-8000-000300010013"), "c4e50000-0000-4000-8000-000003000001", 1, 0, 1, 1, 0.012857142857142857, 2, "FrostCrown" },
                    { new Guid("c4e50001-0000-4000-8000-000300010014"), "c4e50000-0000-4000-8000-000003000001", 1, 0, 1, 1, 0.012857142857142857, 2, "RunicAmulet" },
                    { new Guid("c4e50001-0000-4000-8000-000300010015"), "c4e50000-0000-4000-8000-000003000001", 1, 0, 1, 1, 0.012857142857142857, 2, "RingOfLingeringWords" },
                    { new Guid("c4e50001-0000-4000-8000-000300010016"), "c4e50000-0000-4000-8000-000003000001", 1, 0, 1, 1, 0.012857142857142857, 2, "RingOfTheStoneSkin" },
                    { new Guid("c4e50001-0000-4000-8000-000300010017"), "c4e50000-0000-4000-8000-000003000001", 1, 0, 1, 1, 0.002, 3, "BlackIceSword" },
                    { new Guid("c4e50001-0000-4000-8000-000300010018"), "c4e50000-0000-4000-8000-000003000001", 1, 0, 1, 1, 0.002, 3, "ThornStaff" },
                    { new Guid("c4e50001-0000-4000-8000-000300010019"), "c4e50000-0000-4000-8000-000003000001", 1, 0, 1, 1, 0.002, 3, "GroveCirclet" },
                    { new Guid("c4e50001-0000-4000-8000-000300010020"), "c4e50000-0000-4000-8000-000003000001", 1, 0, 1, 1, 0.002, 3, "StormAmulet" },
                    { new Guid("c4e50001-0000-4000-8000-000300010021"), "c4e50000-0000-4000-8000-000003000001", 1, 0, 1, 1, 0.002, 3, "RingOfCleaving" },
                    { new Guid("c4e50001-0000-4000-8000-000300020000"), "c4e50000-0000-4000-8000-000003000002", 1, 0, 1, 1, 0.074999999999999997, 0, "RustyAxe" },
                    { new Guid("c4e50001-0000-4000-8000-000300020001"), "c4e50000-0000-4000-8000-000003000002", 1, 0, 1, 1, 0.074999999999999997, 0, "IronHelm" },
                    { new Guid("c4e50001-0000-4000-8000-000300020002"), "c4e50000-0000-4000-8000-000003000002", 1, 0, 1, 1, 0.074999999999999997, 0, "BoneNecklace" },
                    { new Guid("c4e50001-0000-4000-8000-000300020003"), "c4e50000-0000-4000-8000-000003000002", 1, 0, 1, 1, 0.074999999999999997, 0, "CopperRing" },
                    { new Guid("c4e50001-0000-4000-8000-000300020004"), "c4e50000-0000-4000-8000-000003000002", 1, 0, 1, 1, 0.066666666666666666, 1, "DwarvenHammer" },
                    { new Guid("c4e50001-0000-4000-8000-000300020005"), "c4e50000-0000-4000-8000-000003000002", 1, 0, 1, 1, 0.066666666666666666, 1, "MinerHelm" },
                    { new Guid("c4e50001-0000-4000-8000-000300020006"), "c4e50000-0000-4000-8000-000003000002", 1, 0, 1, 1, 0.066666666666666666, 1, "MoonstonePendant" },
                    { new Guid("c4e50001-0000-4000-8000-000300020007"), "c4e50000-0000-4000-8000-000003000002", 1, 0, 1, 1, 0.066666666666666666, 1, "RingOfTheUnderdog" },
                    { new Guid("c4e50001-0000-4000-8000-000300020008"), "c4e50000-0000-4000-8000-000003000002", 1, 0, 1, 1, 0.066666666666666666, 1, "SpitefulRing" },
                    { new Guid("c4e50001-0000-4000-8000-000300020009"), "c4e50000-0000-4000-8000-000003000002", 1, 0, 1, 1, 0.066666666666666666, 1, "TwinBand" },
                    { new Guid("c4e50001-0000-4000-8000-000300020010"), "c4e50000-0000-4000-8000-000003000002", 1, 0, 1, 1, 0.031428571428571431, 2, "EmberMace" },
                    { new Guid("c4e50001-0000-4000-8000-000300020011"), "c4e50000-0000-4000-8000-000003000002", 1, 0, 1, 1, 0.031428571428571431, 2, "StormSpear" },
                    { new Guid("c4e50001-0000-4000-8000-000300020012"), "c4e50000-0000-4000-8000-000003000002", 1, 0, 1, 1, 0.031428571428571431, 2, "EmberCrown" },
                    { new Guid("c4e50001-0000-4000-8000-000300020013"), "c4e50000-0000-4000-8000-000003000002", 1, 0, 1, 1, 0.031428571428571431, 2, "FrostCrown" },
                    { new Guid("c4e50001-0000-4000-8000-000300020014"), "c4e50000-0000-4000-8000-000003000002", 1, 0, 1, 1, 0.031428571428571431, 2, "RunicAmulet" },
                    { new Guid("c4e50001-0000-4000-8000-000300020015"), "c4e50000-0000-4000-8000-000003000002", 1, 0, 1, 1, 0.031428571428571431, 2, "RingOfLingeringWords" },
                    { new Guid("c4e50001-0000-4000-8000-000300020016"), "c4e50000-0000-4000-8000-000003000002", 1, 0, 1, 1, 0.031428571428571431, 2, "RingOfTheStoneSkin" },
                    { new Guid("c4e50001-0000-4000-8000-000300020017"), "c4e50000-0000-4000-8000-000003000002", 1, 0, 1, 1, 0.014000000000000002, 3, "BlackIceSword" },
                    { new Guid("c4e50001-0000-4000-8000-000300020018"), "c4e50000-0000-4000-8000-000003000002", 1, 0, 1, 1, 0.014000000000000002, 3, "ThornStaff" },
                    { new Guid("c4e50001-0000-4000-8000-000300020019"), "c4e50000-0000-4000-8000-000003000002", 1, 0, 1, 1, 0.014000000000000002, 3, "GroveCirclet" },
                    { new Guid("c4e50001-0000-4000-8000-000300020020"), "c4e50000-0000-4000-8000-000003000002", 1, 0, 1, 1, 0.014000000000000002, 3, "StormAmulet" },
                    { new Guid("c4e50001-0000-4000-8000-000300020021"), "c4e50000-0000-4000-8000-000003000002", 1, 0, 1, 1, 0.014000000000000002, 3, "RingOfCleaving" },
                    { new Guid("c4e50001-0000-4000-8000-000300020022"), "c4e50000-0000-4000-8000-000003000002", 1, 0, 1, 1, 0.0025000000000000001, 4, "DawnBlade" },
                    { new Guid("c4e50001-0000-4000-8000-000300020023"), "c4e50000-0000-4000-8000-000003000002", 1, 0, 1, 1, 0.0025000000000000001, 4, "WarlordHelm" },
                    { new Guid("c4e50001-0000-4000-8000-000300020024"), "c4e50000-0000-4000-8000-000003000002", 1, 0, 1, 1, 0.0025000000000000001, 4, "HeartOfTheMountain" },
                    { new Guid("c4e50001-0000-4000-8000-000300020025"), "c4e50000-0000-4000-8000-000003000002", 1, 0, 1, 1, 0.0025000000000000001, 4, "RingOfWildfire" },
                    { new Guid("c4e50001-0000-4000-8000-000300030000"), "c4e50000-0000-4000-8000-000003000003", 1, 0, 1, 1, 0.041666666666666664, 1, "DwarvenHammer" },
                    { new Guid("c4e50001-0000-4000-8000-000300030001"), "c4e50000-0000-4000-8000-000003000003", 1, 0, 1, 1, 0.041666666666666664, 1, "MinerHelm" },
                    { new Guid("c4e50001-0000-4000-8000-000300030002"), "c4e50000-0000-4000-8000-000003000003", 1, 0, 1, 1, 0.041666666666666664, 1, "MoonstonePendant" },
                    { new Guid("c4e50001-0000-4000-8000-000300030003"), "c4e50000-0000-4000-8000-000003000003", 1, 0, 1, 1, 0.041666666666666664, 1, "RingOfTheUnderdog" },
                    { new Guid("c4e50001-0000-4000-8000-000300030004"), "c4e50000-0000-4000-8000-000003000003", 1, 0, 1, 1, 0.041666666666666664, 1, "SpitefulRing" },
                    { new Guid("c4e50001-0000-4000-8000-000300030005"), "c4e50000-0000-4000-8000-000003000003", 1, 0, 1, 1, 0.041666666666666664, 1, "TwinBand" },
                    { new Guid("c4e50001-0000-4000-8000-000300030006"), "c4e50000-0000-4000-8000-000003000003", 1, 0, 1, 1, 0.057142857142857148, 2, "EmberMace" },
                    { new Guid("c4e50001-0000-4000-8000-000300030007"), "c4e50000-0000-4000-8000-000003000003", 1, 0, 1, 1, 0.057142857142857148, 2, "StormSpear" },
                    { new Guid("c4e50001-0000-4000-8000-000300030008"), "c4e50000-0000-4000-8000-000003000003", 1, 0, 1, 1, 0.057142857142857148, 2, "EmberCrown" },
                    { new Guid("c4e50001-0000-4000-8000-000300030009"), "c4e50000-0000-4000-8000-000003000003", 1, 0, 1, 1, 0.057142857142857148, 2, "FrostCrown" },
                    { new Guid("c4e50001-0000-4000-8000-000300030010"), "c4e50000-0000-4000-8000-000003000003", 1, 0, 1, 1, 0.057142857142857148, 2, "RunicAmulet" },
                    { new Guid("c4e50001-0000-4000-8000-000300030011"), "c4e50000-0000-4000-8000-000003000003", 1, 0, 1, 1, 0.057142857142857148, 2, "RingOfLingeringWords" },
                    { new Guid("c4e50001-0000-4000-8000-000300030012"), "c4e50000-0000-4000-8000-000003000003", 1, 0, 1, 1, 0.057142857142857148, 2, "RingOfTheStoneSkin" },
                    { new Guid("c4e50001-0000-4000-8000-000300030013"), "c4e50000-0000-4000-8000-000003000003", 1, 0, 1, 1, 0.050000000000000003, 3, "BlackIceSword" },
                    { new Guid("c4e50001-0000-4000-8000-000300030014"), "c4e50000-0000-4000-8000-000003000003", 1, 0, 1, 1, 0.050000000000000003, 3, "ThornStaff" },
                    { new Guid("c4e50001-0000-4000-8000-000300030015"), "c4e50000-0000-4000-8000-000003000003", 1, 0, 1, 1, 0.050000000000000003, 3, "GroveCirclet" },
                    { new Guid("c4e50001-0000-4000-8000-000300030016"), "c4e50000-0000-4000-8000-000003000003", 1, 0, 1, 1, 0.050000000000000003, 3, "StormAmulet" },
                    { new Guid("c4e50001-0000-4000-8000-000300030017"), "c4e50000-0000-4000-8000-000003000003", 1, 0, 1, 1, 0.050000000000000003, 3, "RingOfCleaving" },
                    { new Guid("c4e50001-0000-4000-8000-000300030018"), "c4e50000-0000-4000-8000-000003000003", 1, 0, 1, 1, 0.022499999999999999, 4, "DawnBlade" },
                    { new Guid("c4e50001-0000-4000-8000-000300030019"), "c4e50000-0000-4000-8000-000003000003", 1, 0, 1, 1, 0.022499999999999999, 4, "WarlordHelm" },
                    { new Guid("c4e50001-0000-4000-8000-000300030020"), "c4e50000-0000-4000-8000-000003000003", 1, 0, 1, 1, 0.022499999999999999, 4, "HeartOfTheMountain" },
                    { new Guid("c4e50001-0000-4000-8000-000300030021"), "c4e50000-0000-4000-8000-000003000003", 1, 0, 1, 1, 0.022499999999999999, 4, "RingOfWildfire" },
                    { new Guid("c4e50001-0000-4000-8000-000300030022"), "c4e50000-0000-4000-8000-000003000003", 1, 0, 1, 1, 0.01, 5, "ArmageddonBlade" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerEquipment_HeroId_Slot",
                table: "PlayerEquipment",
                columns: new[] { "HeroId", "Slot" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerEquipment_PlayerId",
                table: "PlayerEquipment",
                column: "PlayerId");

            // Rarity.Epic стала Mythic, а в PlayerItems редкость лежит строкой
            migrationBuilder.Sql("UPDATE PlayerItems SET Rarity = 'Mythic' WHERE Rarity = 'Epic';");

            // Героям настоящих игроков — по случайной пассивке, как у новых (PassiveBook.Random). Сид-герои без аккаунта остаются без неё.
            // upper: в Heroes и Users один и тот же Guid может лежать в разном регистре
            migrationBuilder.Sql(@"
                UPDATE Heroes
                SET Passives = CASE abs(random()) % 3
                    WHEN 0 THEN 'SurvivalOfTheWeakest'
                    WHEN 1 THEN 'SharpTongue'
                    ELSE 'DoubleStrike'
                END
                WHERE upper(PlayerId) IN (SELECT upper(PlayerId) FROM Users);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE PlayerItems SET Rarity = 'Epic' WHERE Rarity = 'Mythic';");

            migrationBuilder.DropTable(
                name: "PlayerEquipment");

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300010000"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300010001"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300010002"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300010003"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300010004"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300010005"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300010006"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300010007"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300010008"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300010009"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300010010"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300010011"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300010012"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300010013"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300010014"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300010015"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300010016"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300010017"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300010018"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300010019"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300010020"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300010021"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300020000"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300020001"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300020002"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300020003"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300020004"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300020005"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300020006"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300020007"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300020008"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300020009"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300020010"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300020011"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300020012"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300020013"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300020014"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300020015"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300020016"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300020017"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300020018"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300020019"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300020020"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300020021"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300020022"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300020023"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300020024"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300020025"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300030000"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300030001"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300030002"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300030003"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300030004"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300030005"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300030006"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300030007"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300030008"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300030009"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300030010"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300030011"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300030012"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300030013"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300030014"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300030015"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300030016"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300030017"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300030018"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300030019"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300030020"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300030021"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300030022"));

            migrationBuilder.DeleteData(
                table: "ChestDropTables",
                keyColumn: "Id",
                keyValue: "c4e50000-0000-4000-8000-000003000001");

            migrationBuilder.DeleteData(
                table: "ChestDropTables",
                keyColumn: "Id",
                keyValue: "c4e50000-0000-4000-8000-000003000002");

            migrationBuilder.DeleteData(
                table: "ChestDropTables",
                keyColumn: "Id",
                keyValue: "c4e50000-0000-4000-8000-000003000003");

            migrationBuilder.DropColumn(
                name: "Passives",
                table: "Heroes");
        }
    }
}
