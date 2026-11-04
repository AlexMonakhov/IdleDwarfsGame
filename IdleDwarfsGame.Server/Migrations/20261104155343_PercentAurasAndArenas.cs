using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class PercentAurasAndArenas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Arenas",
                keyColumn: "Id",
                keyValue: 1,
                column: "EffectPower",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Arenas",
                keyColumn: "Id",
                keyValue: 3,
                column: "EffectPower",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Arenas",
                keyColumn: "Id",
                keyValue: 4,
                column: "EffectPower",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Arenas",
                keyColumn: "Id",
                keyValue: 5,
                column: "EffectPower",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Arenas",
                keyColumn: "Id",
                keyValue: 6,
                column: "EffectPower",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Arenas",
                keyColumn: "Id",
                keyValue: 7,
                column: "EffectPower",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000001"),
                column: "AuraPower",
                value: 10);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000004"),
                column: "AuraPower",
                value: 10);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000101"),
                column: "AuraPower",
                value: 12);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000102"),
                column: "AuraPower",
                value: 10);

            // Ауры дворфов игроков — по книге аур (AuraBook) на их уровне: теперь это процент от атаки носителя
            migrationBuilder.Sql(
                "UPDATE \"Heroes\" SET \"AuraPower\" = CASE " +
                "WHEN \"AuraEffect\" = 'Frost' THEN (CASE \"AuraLevel\" WHEN 1 THEN 5 WHEN 2 THEN 8 ELSE 12 END) " +
                "WHEN \"AuraEffect\" = 'Regen' THEN (CASE \"AuraLevel\" WHEN 1 THEN 10 WHEN 2 THEN 15 ELSE 20 END) " +
                "ELSE (CASE \"AuraLevel\" WHEN 1 THEN 8 WHEN 2 THEN 12 ELSE 16 END) END " +
                "WHERE \"AuraEffect\" IS NOT NULL AND \"Profession\" IS NOT NULL AND \"Id\" NOT LIKE '11111111-0000-4000-8000-%'");

            // Извлечённые ауры хранят только уровень — их сила берётся из книги, менять нечего
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Arenas",
                keyColumn: "Id",
                keyValue: 1,
                column: "EffectPower",
                value: 5);

            migrationBuilder.UpdateData(
                table: "Arenas",
                keyColumn: "Id",
                keyValue: 3,
                column: "EffectPower",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Arenas",
                keyColumn: "Id",
                keyValue: 4,
                column: "EffectPower",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Arenas",
                keyColumn: "Id",
                keyValue: 5,
                column: "EffectPower",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Arenas",
                keyColumn: "Id",
                keyValue: 6,
                column: "EffectPower",
                value: 6);

            migrationBuilder.UpdateData(
                table: "Arenas",
                keyColumn: "Id",
                keyValue: 7,
                column: "EffectPower",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000001"),
                column: "AuraPower",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000004"),
                column: "AuraPower",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000101"),
                column: "AuraPower",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000102"),
                column: "AuraPower",
                value: 3);
        }
    }
}
