using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddAuraInterval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AuraInterval",
                table: "Heroes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000001"),
                column: "AuraInterval",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000002"),
                column: "AuraInterval",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000003"),
                column: "AuraInterval",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000004"),
                column: "AuraInterval",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000005"),
                column: "AuraInterval",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000006"),
                column: "AuraInterval",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000007"),
                column: "AuraInterval",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000008"),
                column: "AuraInterval",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000001"),
                column: "AuraInterval",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000002"),
                column: "AuraInterval",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000003"),
                column: "AuraInterval",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000004"),
                column: "AuraInterval",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000005"),
                column: "AuraInterval",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000001"),
                column: "AuraInterval",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000002"),
                column: "AuraInterval",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000101"),
                column: "AuraInterval",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000102"),
                column: "AuraInterval",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000001"),
                column: "AuraInterval",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000002"),
                column: "AuraInterval",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000003"),
                column: "AuraInterval",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000004"),
                column: "AuraInterval",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000005"),
                column: "AuraInterval",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000006"),
                column: "AuraInterval",
                value: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuraInterval",
                table: "Heroes");
        }
    }
}
