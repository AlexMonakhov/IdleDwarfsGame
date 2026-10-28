using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddElementsSpellsAndArenas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Element",
                table: "Arenas",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "Basic");

            migrationBuilder.UpdateData(
                table: "Arenas",
                keyColumn: "Id",
                keyValue: 1,
                column: "Element",
                value: "Fire");

            migrationBuilder.UpdateData(
                table: "Arenas",
                keyColumn: "Id",
                keyValue: 2,
                column: "Element",
                value: "Frost");

            migrationBuilder.InsertData(
                table: "Arenas",
                columns: new[] { "Id", "Background", "Description", "EffectDuration", "EffectName", "EffectPower", "Element", "Name" },
                values: new object[,]
                {
                    { 3, "storm", "Lightning strikes the summit and shocks every fighter at the start of each round.", 1, "Shock", 4, "Lightning", "Storm Peak" },
                    { 4, "marsh", "Toxic fumes rise from the bog and poison every fighter at the start of each round.", 1, "Poison", 4, "Nature", "Venom Marsh" },
                    { 5, "crypt", "Amethyst shadows curse every fighter at the start of each round, draining life and energy.", 1, "Curse", 4, "Dark", "Shadow Crypt" },
                    { 6, "sanctum", "Diamond light mends the wounds of every fighter — friend and foe — at the start of each round.", 1, "Regen", 6, "Radiance", "Crystal Sanctum" }
                });

            migrationBuilder.InsertData(
                table: "Heroes",
                columns: new[] { "Id", "Attack", "AuraDuration", "AuraEffect", "AuraPower", "AuraTarget", "AuraTrigger", "Element", "EnergyPerAttack", "Immunities", "Level", "MaxEnergy", "MaxHp", "Name", "PlaceInSquad", "PlayerId", "Speed", "SpellIds", "StartEnergy", "UnitType", "UpgradeLevel" },
                values: new object[,]
                {
                    { new Guid("11111111-0000-4000-8000-000000000007"), 19, 1, null, 0, 0, 1, "Dark", 20, "Curse", 5, 100, 150, "Morgra Duskvein", null, new Guid("a1d2f3e4-0000-4000-8000-000000000001"), 6, "39,42,45", 20, "Dwarf", 1 },
                    { new Guid("11111111-0000-4000-8000-000000000008"), 17, 1, null, 0, 0, 1, "Radiance", 20, "Stun", 5, 100, 160, "Elda Brightfacet", null, new Guid("a1d2f3e4-0000-4000-8000-000000000001"), 7, "48,51,54", 20, "Dwarf", 1 },
                    { new Guid("44444444-0000-4000-8000-000000000001"), 13, 1, null, 0, 0, 1, "Nature", 20, "Stun,Poison", 4, 100, 210, "Mossback Troll", null, new Guid("b0b0b0b0-0000-4000-8000-000000000004"), 3, "31", 0, "Troll", 1 },
                    { new Guid("44444444-0000-4000-8000-000000000002"), 13, 1, null, 0, 0, 1, "Frost", 20, "Frost,Freeze", 4, 100, 210, "Rimehorn Yeti", null, new Guid("b0b0b0b0-0000-4000-8000-000000000004"), 3, "16", 0, "Yeti", 1 },
                    { new Guid("44444444-0000-4000-8000-000000000003"), 12, 1, null, 0, 0, 1, "Fire", 20, "Burn", 4, 100, 180, "Ashmaw the Warlord", null, new Guid("b0b0b0b0-0000-4000-8000-000000000004"), 4, "10", 0, "Orc", 1 },
                    { new Guid("44444444-0000-4000-8000-000000000004"), 12, 1, null, 0, 0, 1, "Lightning", 20, "Shock,Stun", 4, 100, 180, "Grukk Stormcaller", null, new Guid("b0b0b0b0-0000-4000-8000-000000000004"), 4, "25", 0, "Ogre", 1 },
                    { new Guid("44444444-0000-4000-8000-000000000005"), 13, 1, null, 0, 0, 1, "Dark", 25, "Curse,Frost", 4, 100, 100, "Nyxara the Hollow Queen", null, new Guid("b0b0b0b0-0000-4000-8000-000000000004"), 6, "37", 0, "Skeleton", 1 },
                    { new Guid("44444444-0000-4000-8000-000000000006"), 13, 1, null, 0, 0, 1, "Radiance", 25, "Stun,Freeze", 4, 100, 100, "Seraphel the Shattered Light", null, new Guid("b0b0b0b0-0000-4000-8000-000000000004"), 6, "47", 0, "Golem", 1 }
                });

            migrationBuilder.InsertData(
                table: "Players",
                columns: new[] { "Id", "Experience", "Gold", "Name", "Rating" },
                values: new object[] { "b0b0b0b0-0000-4000-8000-000000000004", 0L, 0L, "Guild Bosses", 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Arenas",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Arenas",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Arenas",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Arenas",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000007"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000008"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000006"));

            migrationBuilder.DeleteData(
                table: "Players",
                keyColumn: "Id",
                keyValue: "b0b0b0b0-0000-4000-8000-000000000004");

            migrationBuilder.DropColumn(
                name: "Element",
                table: "Arenas");
        }
    }
}
