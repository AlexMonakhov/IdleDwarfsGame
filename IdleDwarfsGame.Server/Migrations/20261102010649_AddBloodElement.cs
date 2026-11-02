using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddBloodElement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Новая стихия Blood забирает у тьмы Life Drain. Кровью становятся бывшая внешность Violet и тёмные герои,
            // у которых Life Drain был единственным заклинанием (тестовые аккаунты): их заклинание не меняется.
            // Чёрный лёд кровь носить не может, поэтому снимается. В снимках рейтинговой арены остаётся до перезакрепления отряда.
            migrationBuilder.Sql(
                "UPDATE PlayerEquipment SET HeroId = NULL, Slot = NULL " +
                "WHERE ItemKey = 'BlackIceSword' AND HeroId IN (SELECT Id FROM Heroes " +
                "WHERE Element = 'Dark' AND (Appearance = 'Violet' OR SpellIds IN ('40', '41', '42')));");
            migrationBuilder.Sql(
                "UPDATE Heroes SET Element = 'Blood', SpellIds = CASE WHEN Appearance = 'Violet' " +
                "THEN CASE Level WHEN 2 THEN '40' WHEN 3 THEN '41' ELSE '42' END ELSE SpellIds END " +
                "WHERE Element = 'Dark' AND (Appearance = 'Violet' OR SpellIds IN ('40', '41', '42'));");
            // Остальные тёмные герои: Life Drain заменяется по книге стихии, Hex занимает его место
            migrationBuilder.Sql(
                "UPDATE Heroes SET SpellIds = CASE SpellIds WHEN '38,40' THEN '38,43' WHEN '39,41,43' THEN '39,44' ELSE '39,45' END " +
                "WHERE Element = 'Dark' AND SpellIds IN ('38,40', '39,41,43', '39,42,45');");
            migrationBuilder.Sql(
                "UPDATE RankedSquads SET HeroesJson = (" +
                "SELECT json_group_array(json(json_remove(CASE " +
                "WHEN json_extract(value, '$.Element') <> 'Dark' THEN value " +
                "WHEN json_extract(value, '$.Appearance') = 'Violet' THEN json_set(value, '$.Element', 'Blood', '$.SpellIds', " +
                "json(CASE json_extract(value, '$.Level') WHEN 2 THEN '[40]' WHEN 3 THEN '[41]' ELSE '[42]' END)) " +
                "WHEN json_extract(value, '$.SpellIds') IN ('[40]', '[41]', '[42]') THEN json_set(value, '$.Element', 'Blood') " +
                "WHEN json_extract(value, '$.SpellIds') = '[38,40]' THEN json_set(value, '$.SpellIds', json('[38,43]')) " +
                "WHEN json_extract(value, '$.SpellIds') = '[39,41,43]' THEN json_set(value, '$.SpellIds', json('[39,44]')) " +
                "WHEN json_extract(value, '$.SpellIds') = '[39,42,45]' THEN json_set(value, '$.SpellIds', json('[39,45]')) " +
                "ELSE value END, '$.Appearance'))) FROM json_each(HeroesJson));");

            migrationBuilder.DropColumn(
                name: "Appearance",
                table: "Heroes");

            migrationBuilder.InsertData(
                table: "Arenas",
                columns: new[] { "Id", "Background", "Description", "EffectDuration", "EffectName", "EffectPower", "Element", "Name" },
                values: new object[] { 7, "altar", "Garnet runes open old wounds: every fighter bleeds at the start of each round.", 1, "Bleed", 4, "Blood", "Blood Altar" });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000007"),
                column: "SpellIds",
                value: "39,45");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Arenas",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.AddColumn<string>(
                name: "Appearance",
                table: "Heroes",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "Default");

            // Обратно: кровь снова становится внешностью Violet с полной книгой тьмы своего ранга, Life Drain возвращается тьме
            migrationBuilder.Sql(
                "UPDATE Heroes SET Element = 'Dark', Appearance = 'Violet', " +
                "SpellIds = CASE Level WHEN 2 THEN '37' WHEN 3 THEN '38,40' WHEN 4 THEN '39,41,43' ELSE '39,42,45' END " +
                "WHERE Element = 'Blood';");
            migrationBuilder.Sql(
                "UPDATE Heroes SET SpellIds = CASE SpellIds WHEN '38,43' THEN '38,40' WHEN '39,44' THEN '39,41,43' ELSE '39,42,45' END " +
                "WHERE Element = 'Dark' AND SpellIds IN ('38,43', '39,44', '39,45');");
            migrationBuilder.Sql(
                "UPDATE RankedSquads SET HeroesJson = (" +
                "SELECT json_group_array(json(CASE " +
                "WHEN json_extract(value, '$.Element') = 'Blood' THEN json_set(value, '$.Element', 'Dark', '$.Appearance', 'Violet', '$.SpellIds', " +
                "json(CASE json_extract(value, '$.Level') WHEN 2 THEN '[37]' WHEN 3 THEN '[38,40]' WHEN 4 THEN '[39,41,43]' ELSE '[39,42,45]' END)) " +
                "WHEN json_extract(value, '$.Element') <> 'Dark' THEN value " +
                "WHEN json_extract(value, '$.SpellIds') = '[38,43]' THEN json_set(value, '$.SpellIds', json('[38,40]')) " +
                "WHEN json_extract(value, '$.SpellIds') = '[39,44]' THEN json_set(value, '$.SpellIds', json('[39,41,43]')) " +
                "WHEN json_extract(value, '$.SpellIds') = '[39,45]' THEN json_set(value, '$.SpellIds', json('[39,42,45]')) " +
                "ELSE value END)) FROM json_each(HeroesJson));");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000001"),
                column: "Appearance",
                value: "Default");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000002"),
                column: "Appearance",
                value: "Default");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000003"),
                column: "Appearance",
                value: "Default");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000004"),
                column: "Appearance",
                value: "Default");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000005"),
                column: "Appearance",
                value: "Default");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000006"),
                column: "Appearance",
                value: "Default");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000007"),
                columns: new[] { "Appearance", "SpellIds" },
                values: new object[] { "Default", "39,42,45" });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000008"),
                column: "Appearance",
                value: "Default");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000001"),
                column: "Appearance",
                value: "Default");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000002"),
                column: "Appearance",
                value: "Default");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000003"),
                column: "Appearance",
                value: "Default");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000004"),
                column: "Appearance",
                value: "Default");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000005"),
                column: "Appearance",
                value: "Default");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000001"),
                column: "Appearance",
                value: "Default");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000002"),
                column: "Appearance",
                value: "Default");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000101"),
                column: "Appearance",
                value: "Default");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000102"),
                column: "Appearance",
                value: "Default");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000001"),
                column: "Appearance",
                value: "Default");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000002"),
                column: "Appearance",
                value: "Default");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000003"),
                column: "Appearance",
                value: "Default");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000004"),
                column: "Appearance",
                value: "Default");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000005"),
                column: "Appearance",
                value: "Default");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000006"),
                column: "Appearance",
                value: "Default");
        }
    }
}
