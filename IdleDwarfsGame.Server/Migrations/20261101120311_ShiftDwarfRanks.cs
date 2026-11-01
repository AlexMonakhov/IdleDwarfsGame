using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class ShiftDwarfRanks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Уровень файла спрайта теперь равен рангу, а 5-го ранга пока нет: дворфы спускаются на ранг ниже, 1-й остаётся.
            // На 1-м ранге нет стихии, поэтому у спустившихся со 2-го сбрасываются стихия, внешность и заклинания.
            // Статы и заклинания остальных не меняются. Снимки отрядов рейтинговой арены хранят героев в JSON — там то же самое.
            migrationBuilder.Sql(
                "UPDATE Heroes SET Element = 'Basic', Appearance = 'Default', SpellIds = '' " +
                "WHERE UnitType = 'Dwarf' AND Level = 2;");
            migrationBuilder.Sql(
                "UPDATE Heroes SET Level = Level - 1 WHERE UnitType = 'Dwarf' AND Level > 1;");
            migrationBuilder.Sql(
                "UPDATE RankedSquads SET HeroesJson = (" +
                "SELECT json_group_array(json(CASE " +
                "WHEN json_extract(value, '$.UnitType') = 'Dwarf' AND json_extract(value, '$.Level') = 2 " +
                "THEN json_set(value, '$.Level', 1, '$.Element', 'Basic', '$.Appearance', 'Default', '$.SpellIds', json('[]')) " +
                "WHEN json_extract(value, '$.UnitType') = 'Dwarf' AND json_extract(value, '$.Level') > 2 " +
                "THEN json_set(value, '$.Level', json_extract(value, '$.Level') - 1) " +
                "ELSE value END)) FROM json_each(HeroesJson));");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000001"),
                column: "Level",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000002"),
                column: "Level",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000003"),
                column: "Level",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000004"),
                column: "Level",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000005"),
                column: "Level",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000006"),
                column: "Level",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000007"),
                column: "Level",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000008"),
                column: "Level",
                value: 4);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Сдвиг рангов не откатывается: сброшенные стихии и заклинания не восстановить. Здесь — только начальные герои
            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000001"),
                column: "Level",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000002"),
                column: "Level",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000003"),
                column: "Level",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000004"),
                column: "Level",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000005"),
                column: "Level",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000006"),
                column: "Level",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000007"),
                column: "Level",
                value: 5);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000008"),
                column: "Level",
                value: 5);
        }
    }
}
