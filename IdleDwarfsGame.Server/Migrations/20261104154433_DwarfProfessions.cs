using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class DwarfProfessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Profession",
                table: "Heroes",
                type: "TEXT",
                maxLength: 16,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000001"),
                columns: new[] { "Attack", "MaxHp", "Profession", "Speed" },
                values: new object[] { 212, 2828, "Defender", 3 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000002"),
                columns: new[] { "Attack", "MaxHp", "Profession", "Speed" },
                values: new object[] { 353, 2036, "Axeman", 4 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000003"),
                columns: new[] { "Attack", "MaxHp", "Profession", "Speed" },
                values: new object[] { 283, 2488, "Warrior", 5 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000004"),
                columns: new[] { "Attack", "MaxHp", "Profession", "Speed" },
                values: new object[] { 269, 2262, "Miner", 6 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000005"),
                columns: new[] { "Attack", "MaxHp", "Profession" },
                values: new object[] { 311, 1810, "Scout" });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000006"),
                columns: new[] { "Attack", "MaxHp", "Profession", "Speed" },
                values: new object[] { 269, 2262, "Miner", 6 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000007"),
                columns: new[] { "Attack", "MaxHp", "Profession", "Speed" },
                values: new object[] { 1219, 7024, "Axeman", 4 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000008"),
                columns: new[] { "Attack", "MaxHp", "Profession", "Speed" },
                values: new object[] { 975, 8584, "Warrior", 5 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000001"),
                columns: new[] { "Attack", "MaxHp", "Profession" },
                values: new object[] { 95, 1300, null });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000002"),
                columns: new[] { "Attack", "MaxHp", "Profession" },
                values: new object[] { 85, 560, null });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000003"),
                columns: new[] { "Attack", "MaxHp", "Profession" },
                values: new object[] { 85, 520, null });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000004"),
                columns: new[] { "Attack", "MaxHp", "Profession" },
                values: new object[] { 95, 1100, null });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000005"),
                columns: new[] { "Attack", "MaxHp", "Profession" },
                values: new object[] { 100, 620, null });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000001"),
                columns: new[] { "Attack", "MaxHp", "Profession" },
                values: new object[] { 90, 600, null });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000002"),
                columns: new[] { "Attack", "MaxHp", "Profession" },
                values: new object[] { 95, 740, null });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000101"),
                columns: new[] { "Attack", "MaxHp", "Profession" },
                values: new object[] { 130, 1600, null });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000102"),
                columns: new[] { "Attack", "MaxHp", "Profession" },
                values: new object[] { 125, 1500, null });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000001"),
                columns: new[] { "Attack", "MaxHp", "Profession" },
                values: new object[] { 100, 1300, null });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000002"),
                columns: new[] { "Attack", "MaxHp", "Profession" },
                values: new object[] { 100, 1300, null });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000003"),
                columns: new[] { "Attack", "MaxHp", "Profession" },
                values: new object[] { 95, 1100, null });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000004"),
                columns: new[] { "Attack", "MaxHp", "Profession" },
                values: new object[] { 95, 1100, null });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000005"),
                columns: new[] { "Attack", "MaxHp", "Profession" },
                values: new object[] { 105, 620, null });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000006"),
                columns: new[] { "Attack", "MaxHp", "Profession" },
                values: new object[] { 105, 620, null });

            // Дворфы игроков: профессия — по скорости старой роли (3 защитник, 4 секироносец, 5 воин, 6 рудокоп, 8 разведчик;
            // у тестовых аккаунтов скорость сдвинута на ±1), статы — заново: профессия × множитель ранга (HeroLevels.RankFactor).
            // Сид-дворфы (11111111-…) уже обновлены выше
            migrationBuilder.Sql(
                "UPDATE \"Heroes\" SET \"Profession\" = CASE " +
                "WHEN \"Speed\" <= 3 THEN 'Defender' WHEN \"Speed\" = 4 THEN 'Axeman' WHEN \"Speed\" = 5 THEN 'Warrior' " +
                "WHEN \"Speed\" = 6 THEN 'Miner' ELSE 'Scout' END " +
                "WHERE \"UnitType\" = 'Dwarf' AND \"Id\" NOT LIKE '11111111-0000-4000-8000-%'");

            const string rankFactor =
                "(CASE \"Level\" WHEN 1 THEN 1.0 WHEN 2 THEN 1.45 WHEN 3 THEN 2.8275 WHEN 4 THEN 9.754875 ELSE 58.04150625 END)";
            migrationBuilder.Sql(
                "UPDATE \"Heroes\" SET " +
                "\"MaxHp\" = CAST(ROUND((CASE \"Profession\" WHEN 'Defender' THEN 1000 WHEN 'Warrior' THEN 880 WHEN 'Axeman' THEN 720 WHEN 'Miner' THEN 800 ELSE 640 END) * " + rankFactor + ") AS INTEGER), " +
                "\"Attack\" = CAST(ROUND((CASE \"Profession\" WHEN 'Defender' THEN 75 WHEN 'Warrior' THEN 100 WHEN 'Axeman' THEN 125 WHEN 'Miner' THEN 95 ELSE 110 END) * " + rankFactor + ") AS INTEGER), " +
                "\"Speed\" = (CASE \"Profession\" WHEN 'Defender' THEN 3 WHEN 'Warrior' THEN 5 WHEN 'Axeman' THEN 4 WHEN 'Miner' THEN 6 ELSE 8 END), " +
                // Потолок прокачки теперь зависит от ранга: 10 / 20 / 50 / 100 / 150
                "\"UpgradeLevel\" = MIN(\"UpgradeLevel\", CASE \"Level\" WHEN 1 THEN 10 WHEN 2 THEN 20 WHEN 3 THEN 50 WHEN 4 THEN 100 ELSE 150 END) " +
                "WHERE \"Profession\" IS NOT NULL AND \"Id\" NOT LIKE '11111111-0000-4000-8000-%'");

            // Снимки отрядов рейтинговой арены хранят старые статы — игроки закрепят отряды заново,
            // тестовые аккаунты получат новые снимки при старте сервера (TestAccountsSeed)
            migrationBuilder.Sql("DELETE FROM \"RankedSquads\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Profession",
                table: "Heroes");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000001"),
                columns: new[] { "Attack", "MaxHp", "Speed" },
                values: new object[] { 14, 260, 4 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000002"),
                columns: new[] { "Attack", "MaxHp", "Speed" },
                values: new object[] { 18, 130, 7 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000003"),
                columns: new[] { "Attack", "MaxHp", "Speed" },
                values: new object[] { 16, 150, 6 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000004"),
                columns: new[] { "Attack", "MaxHp", "Speed" },
                values: new object[] { 20, 180, 5 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000005"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 12, 120 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000006"),
                columns: new[] { "Attack", "MaxHp", "Speed" },
                values: new object[] { 15, 140, 5 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000007"),
                columns: new[] { "Attack", "MaxHp", "Speed" },
                values: new object[] { 19, 150, 6 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000008"),
                columns: new[] { "Attack", "MaxHp", "Speed" },
                values: new object[] { 17, 160, 7 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000001"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 13, 210 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000002"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 10, 90 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000003"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 10, 85 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000004"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 12, 180 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000005"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 13, 100 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000001"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 11, 95 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000002"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 12, 120 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000101"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 16, 260 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000102"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 15, 240 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000001"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 13, 210 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000002"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 13, 210 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000003"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 12, 180 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000004"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 12, 180 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000005"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 13, 100 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000006"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 13, 100 });
        }
    }
}
