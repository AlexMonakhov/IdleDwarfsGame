using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class AuraRework : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000225"),
                columns: new[] { "AuraEffect", "AuraPower" },
                values: new object[] { "EnergyBurn", 3 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000245"),
                column: "AuraPower",
                value: 6);

            // Аура тьмы больше не проклинает, а выжигает энергию: у дворфов и извлечённых аур — новый ключ,
            // сила по уровню ауры 3 / 4 / 5 единиц энергии вместо процента атаки.
            // Аура крови слабее (копится с другими носителями до 5,5×): 6 / 9 / 12% вместо 8 / 12 / 16%
            migrationBuilder.Sql("""
                UPDATE Heroes
                SET AuraEffect = 'EnergyBurn',
                    AuraPower = CASE AuraLevel WHEN 2 THEN 4 WHEN 3 THEN 5 ELSE 3 END
                WHERE AuraEffect = 'Curse';
                UPDATE Heroes
                SET AuraPower = CASE AuraLevel WHEN 2 THEN 9 WHEN 3 THEN 12 ELSE 6 END
                WHERE AuraEffect = 'Bleed';
                UPDATE PlayerAbilities SET [Key] = 'EnergyBurn' WHERE Kind = 'Aura' AND [Key] = 'Curse';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE Heroes
                SET AuraEffect = 'Curse',
                    AuraPower = CASE AuraLevel WHEN 2 THEN 12 WHEN 3 THEN 16 ELSE 8 END
                WHERE AuraEffect = 'EnergyBurn';
                UPDATE Heroes
                SET AuraPower = CASE AuraLevel WHEN 2 THEN 12 WHEN 3 THEN 16 ELSE 8 END
                WHERE AuraEffect = 'Bleed';
                UPDATE PlayerAbilities SET [Key] = 'Curse' WHERE Kind = 'Aura' AND [Key] = 'EnergyBurn';
                """);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000225"),
                columns: new[] { "AuraEffect", "AuraPower" },
                values: new object[] { "Curse", 8 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000245"),
                column: "AuraPower",
                value: 8);
        }
    }
}
