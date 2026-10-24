using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddHeroCombatComponents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AuraDuration",
                table: "Heroes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "AuraEffect",
                table: "Heroes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AuraPower",
                table: "Heroes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AuraTarget",
                table: "Heroes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AuraTrigger",
                table: "Heroes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Heroes",
                type: "TEXT",
                nullable: false,
                defaultValue: "red");

            migrationBuilder.AddColumn<int>(
                name: "EnergyPerAttack",
                table: "Heroes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 20);

            migrationBuilder.AddColumn<string>(
                name: "Immunities",
                table: "Heroes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Heroes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "MaxEnergy",
                table: "Heroes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 100);

            migrationBuilder.AddColumn<string>(
                name: "SpellIds",
                table: "Heroes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "StartEnergy",
                table: "Heroes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UnitType",
                table: "Heroes",
                type: "TEXT",
                nullable: false,
                defaultValue: "Dwarf");

            migrationBuilder.InsertData(
                table: "Heroes",
                columns: new[] { "Id", "Attack", "AuraDuration", "AuraEffect", "AuraPower", "AuraTarget", "AuraTrigger", "Color", "EnergyPerAttack", "Immunities", "Level", "MaxEnergy", "MaxHp", "Name", "PlaceInSquad", "PlayerId", "Speed", "SpellIds", "StartEnergy", "UnitType" },
                values: new object[,]
                {
                    { new Guid("11111111-0000-4000-8000-000000000001"), 14, 1, "Regen", 4, 0, 1, "red", 20, "Stun", 4, 100, 260, "Brakk Ironbeard", 1, new Guid("a1d2f3e4-0000-4000-8000-000000000001"), 4, "", 0, "Dwarf" },
                    { new Guid("11111111-0000-4000-8000-000000000002"), 18, 1, null, 0, 0, 1, "red", 20, "Burn", 4, 100, 130, "Dain Emberforge", 2, new Guid("a1d2f3e4-0000-4000-8000-000000000001"), 7, "2", 20, "Dwarf" },
                    { new Guid("11111111-0000-4000-8000-000000000003"), 16, 1, null, 0, 0, 1, "blue", 25, "", 4, 100, 150, "Thora Stormhammer", 3, new Guid("a1d2f3e4-0000-4000-8000-000000000001"), 6, "4,6", 0, "Dwarf" },
                    { new Guid("11111111-0000-4000-8000-000000000004"), 20, 1, null, 0, 0, 1, "red", 20, "", 4, 100, 180, "Grim Ashaxe", 4, new Guid("a1d2f3e4-0000-4000-8000-000000000001"), 5, "", 0, "Dwarf" },
                    { new Guid("11111111-0000-4000-8000-000000000005"), 12, 1, null, 0, 0, 1, "blue", 20, "", 4, 100, 120, "Runa Frostpick", 5, new Guid("a1d2f3e4-0000-4000-8000-000000000001"), 8, "7", 40, "Dwarf" },
                    { new Guid("11111111-0000-4000-8000-000000000006"), 15, 1, null, 0, 0, 1, "blue", 20, "", 4, 100, 140, "Oin Deepdelver", null, new Guid("a1d2f3e4-0000-4000-8000-000000000001"), 5, "1", 0, "Dwarf" },
                    { new Guid("22222222-0000-4000-8000-000000000001"), 13, 1, null, 0, 0, 1, "blue", 20, "Stun", 4, 100, 210, "Orc Brute", 1, new Guid("e0e0e0e0-0000-4000-8000-000000000002"), 3, "", 0, "Orc" },
                    { new Guid("22222222-0000-4000-8000-000000000002"), 10, 1, null, 0, 0, 1, "blue", 20, "", 4, 100, 90, "Goblin Scout", 2, new Guid("e0e0e0e0-0000-4000-8000-000000000002"), 9, "", 0, "Goblin" },
                    { new Guid("22222222-0000-4000-8000-000000000003"), 10, 1, null, 0, 0, 1, "blue", 20, "", 4, 100, 85, "Goblin Shaman", 3, new Guid("e0e0e0e0-0000-4000-8000-000000000002"), 7, "1", 20, "Goblin" },
                    { new Guid("22222222-0000-4000-8000-000000000004"), 12, 1, "Burn", 3, 1, 1, "blue", 20, "Burn", 4, 100, 180, "Orc Warlord", 4, new Guid("e0e0e0e0-0000-4000-8000-000000000002"), 4, "", 0, "Orc" },
                    { new Guid("22222222-0000-4000-8000-000000000005"), 13, 1, null, 0, 0, 1, "blue", 25, "Frost", 4, 100, 100, "Skeleton Mage", 5, new Guid("e0e0e0e0-0000-4000-8000-000000000002"), 6, "5", 0, "Skeleton" }
                });

            migrationBuilder.InsertData(
                table: "Players",
                columns: new[] { "Id", "Name", "Rating" },
                values: new object[,]
                {
                    { "a1d2f3e4-0000-4000-8000-000000000001", "Demo Player", 0 },
                    { "e0e0e0e0-0000-4000-8000-000000000002", "Tower Monsters", 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Heroes_PlayerId",
                table: "Heroes",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Heroes_PlayerId",
                table: "Heroes");

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000006"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Players",
                keyColumn: "Id",
                keyValue: "a1d2f3e4-0000-4000-8000-000000000001");

            migrationBuilder.DeleteData(
                table: "Players",
                keyColumn: "Id",
                keyValue: "e0e0e0e0-0000-4000-8000-000000000002");

            migrationBuilder.DropColumn(
                name: "AuraDuration",
                table: "Heroes");

            migrationBuilder.DropColumn(
                name: "AuraEffect",
                table: "Heroes");

            migrationBuilder.DropColumn(
                name: "AuraPower",
                table: "Heroes");

            migrationBuilder.DropColumn(
                name: "AuraTarget",
                table: "Heroes");

            migrationBuilder.DropColumn(
                name: "AuraTrigger",
                table: "Heroes");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Heroes");

            migrationBuilder.DropColumn(
                name: "EnergyPerAttack",
                table: "Heroes");

            migrationBuilder.DropColumn(
                name: "Immunities",
                table: "Heroes");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Heroes");

            migrationBuilder.DropColumn(
                name: "MaxEnergy",
                table: "Heroes");

            migrationBuilder.DropColumn(
                name: "SpellIds",
                table: "Heroes");

            migrationBuilder.DropColumn(
                name: "StartEnergy",
                table: "Heroes");

            migrationBuilder.DropColumn(
                name: "UnitType",
                table: "Heroes");
        }
    }
}
