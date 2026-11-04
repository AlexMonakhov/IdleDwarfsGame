using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class EnemyProgression : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CampaignStageEnemies",
                columns: new[] { "Id", "HeroId", "HpMultiplier", "IsBoss", "Position", "StageId" },
                values: new object[,]
                {
                    { 604, new Guid("22222222-0000-4000-8000-000000000001"), 1.0, false, 4, 6 },
                    { 605, new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false, 5, 6 }
                });

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 2,
                column: "EnemyLevel",
                value: 2);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 3,
                column: "EnemyLevel",
                value: 3);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 4,
                column: "EnemyLevel",
                value: 5);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 5,
                column: "EnemyLevel",
                value: 5);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 6,
                column: "EnemyLevel",
                value: 8);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 7,
                column: "EnemyLevel",
                value: 9);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 8,
                column: "EnemyLevel",
                value: 9);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 9,
                column: "EnemyLevel",
                value: 10);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 10,
                column: "EnemyLevel",
                value: 13);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000001"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 70, 1000 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000002"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 70, 480 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000003"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 65, 440 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000004"),
                columns: new[] { "Attack", "AuraPower", "MaxHp" },
                values: new object[] { 75, 8, 800 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000005"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 75, 480 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000001"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 95, 720 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000002"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 100, 840 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000101"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 110, 1300 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 604);

            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 605);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 2,
                column: "EnemyLevel",
                value: 1);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 3,
                column: "EnemyLevel",
                value: 2);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 4,
                column: "EnemyLevel",
                value: 2);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 5,
                column: "EnemyLevel",
                value: 2);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 6,
                column: "EnemyLevel",
                value: 3);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 7,
                column: "EnemyLevel",
                value: 3);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 8,
                column: "EnemyLevel",
                value: 3);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 9,
                column: "EnemyLevel",
                value: 3);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 10,
                column: "EnemyLevel",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000001"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 95, 1300 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000002"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 85, 560 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000003"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 85, 520 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000004"),
                columns: new[] { "Attack", "AuraPower", "MaxHp" },
                values: new object[] { 95, 10, 1100 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-4000-8000-000000000005"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 100, 620 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000001"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 90, 600 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000002"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 95, 740 });

            migrationBuilder.UpdateData(
                table: "Heroes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-4000-8000-000000000101"),
                columns: new[] { "Attack", "MaxHp" },
                values: new object[] { 130, 1600 });
        }
    }
}
