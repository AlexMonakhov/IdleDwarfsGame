using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddHeroElement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Element",
                table: "Heroes",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "Basic");

            // Переносим цвет уже существующих героев в стихию, прежде чем удалить колонку.
            // На 1-м уровне стихии нет — такие герои остаются Basic
            migrationBuilder.Sql(
                "UPDATE Heroes SET Element = CASE Color " +
                "WHEN 'red' THEN 'Fire' WHEN 'blue' THEN 'Frost' " +
                "WHEN 'yellow' THEN 'Lightning' WHEN 'green' THEN 'Nature' ELSE 'Basic' END " +
                "WHERE Level > 1");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Heroes");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000001"),
                column: "Element",
                value: "Fire");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000002"),
                column: "Element",
                value: "Fire");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000003"),
                column: "Element",
                value: "Frost");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000004"),
                column: "Element",
                value: "Fire");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000005"),
                column: "Element",
                value: "Frost");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000006"),
                column: "Element",
                value: "Frost");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000001"),
                column: "Element",
                value: "Frost");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000002"),
                column: "Element",
                value: "Frost");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000003"),
                column: "Element",
                value: "Frost");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000004"),
                column: "Element",
                value: "Frost");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000005"),
                column: "Element",
                value: "Frost");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000001"),
                column: "Element",
                value: "Frost");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000002"),
                column: "Element",
                value: "Frost");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000101"),
                column: "Element",
                value: "Fire");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000102"),
                column: "Element",
                value: "Frost");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Heroes",
                type: "TEXT",
                nullable: false,
                defaultValue: "red");

            migrationBuilder.Sql(
                "UPDATE Heroes SET Color = CASE Element " +
                "WHEN 'Frost' THEN 'blue' WHEN 'Lightning' THEN 'yellow' WHEN 'Nature' THEN 'green' ELSE 'red' END");

            migrationBuilder.DropColumn(
                name: "Element",
                table: "Heroes");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000001"),
                column: "Color",
                value: "red");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000002"),
                column: "Color",
                value: "red");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000003"),
                column: "Color",
                value: "blue");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000004"),
                column: "Color",
                value: "red");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000005"),
                column: "Color",
                value: "blue");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000006"),
                column: "Color",
                value: "blue");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000001"),
                column: "Color",
                value: "blue");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000002"),
                column: "Color",
                value: "blue");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000003"),
                column: "Color",
                value: "blue");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000004"),
                column: "Color",
                value: "blue");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000005"),
                column: "Color",
                value: "blue");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000001"),
                column: "Color",
                value: "blue");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000002"),
                column: "Color",
                value: "blue");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000101"),
                column: "Color",
                value: "red");

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000102"),
                column: "Color",
                value: "blue");
        }
    }
}
