using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddAbilities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AuraLevel",
                table: "Heroes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "PassiveLevel",
                table: "Heroes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "PlayerAbilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlayerId = table.Column<string>(type: "TEXT", nullable: false),
                    Kind = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerAbilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerAbilities_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000001"),
                columns: new[] { "AuraLevel", "PassiveLevel" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000002"),
                columns: new[] { "AuraLevel", "PassiveLevel" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000003"),
                columns: new[] { "AuraLevel", "PassiveLevel" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000004"),
                columns: new[] { "AuraLevel", "PassiveLevel" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000005"),
                columns: new[] { "AuraLevel", "PassiveLevel" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000006"),
                columns: new[] { "AuraLevel", "PassiveLevel" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000007"),
                columns: new[] { "AuraLevel", "PassiveLevel" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-4000-8000-000000000008"),
                columns: new[] { "AuraLevel", "PassiveLevel" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000001"),
                columns: new[] { "AuraLevel", "PassiveLevel" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000002"),
                columns: new[] { "AuraLevel", "PassiveLevel" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000003"),
                columns: new[] { "AuraLevel", "PassiveLevel" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000004"),
                columns: new[] { "AuraLevel", "PassiveLevel" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000005"),
                columns: new[] { "AuraLevel", "PassiveLevel" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000001"),
                columns: new[] { "AuraLevel", "PassiveLevel" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000002"),
                columns: new[] { "AuraLevel", "PassiveLevel" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000101"),
                columns: new[] { "AuraLevel", "PassiveLevel" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000102"),
                columns: new[] { "AuraLevel", "PassiveLevel" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000001"),
                columns: new[] { "AuraLevel", "PassiveLevel" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000002"),
                columns: new[] { "AuraLevel", "PassiveLevel" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000003"),
                columns: new[] { "AuraLevel", "PassiveLevel" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000004"),
                columns: new[] { "AuraLevel", "PassiveLevel" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000005"),
                columns: new[] { "AuraLevel", "PassiveLevel" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-0000-4000-8000-000000000006"),
                columns: new[] { "AuraLevel", "PassiveLevel" },
                values: new object[] { 1, 1 });

            // Раньше уровень пассивки шёл от ранга (1–2 ранг — 1, 3–4 — 2, 5 — 3): сохраняем его уже существующим героям.
            // У сид-героев пассивок нет — они остаются как в модели
            migrationBuilder.Sql(
                "UPDATE \"Heroes\" SET \"PassiveLevel\" = CASE WHEN \"Level\" <= 2 THEN 1 WHEN \"Level\" <= 4 THEN 2 ELSE 3 END " +
                "WHERE \"Passives\" <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAbilities_PlayerId",
                table: "PlayerAbilities",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerAbilities");

            migrationBuilder.DropColumn(
                name: "AuraLevel",
                table: "Heroes");

            migrationBuilder.DropColumn(
                name: "PassiveLevel",
                table: "Heroes");
        }
    }
}
