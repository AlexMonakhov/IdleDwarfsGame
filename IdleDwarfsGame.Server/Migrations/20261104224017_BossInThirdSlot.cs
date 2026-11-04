using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class BossInThirdSlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1002);

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000101"), 2.0, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000205"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000215"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000225"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000235"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000245"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000255"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000265"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000205"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000215"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000225"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000235"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000245"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000255"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000265"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000205"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000215"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000225"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000235"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000245"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000255"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000265"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000205"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000215"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000225"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000235"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000245"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000255"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000265"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000205"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000215"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000225"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000235"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000245"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000255"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000265"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000205"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000215"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000225"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000235"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000245"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000255"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000265"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000205"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000215"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000225"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000235"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000245"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000255"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 50002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 50003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000265"), 2.5, true });

            migrationBuilder.InsertData(
                table: "CampaignStageEnemies",
                columns: new[] { "Id", "HeroId", "HpMultiplier", "IsBoss", "Position", "StageId" },
                values: new object[] { 1003, new Guid("33333333-0000-4000-8000-000000000102"), 1.75, true, 3, 10 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 1003);

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000101"), 2.0, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000205"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 2003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000215"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 3003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000225"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 4003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000235"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 5003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000245"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 6003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000255"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 7003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000265"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 8003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000205"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 9003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000215"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 10003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000225"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 11003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000235"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 12003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000245"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 13003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000255"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 14003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000265"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 15003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000205"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 16003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000215"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 17003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000225"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 18003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000235"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 19003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000245"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 20003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000255"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 21003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000265"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 22003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000205"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 23003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000215"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 24003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000225"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 25003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000235"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 26003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000245"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 27003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000255"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 28003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000265"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 29003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000205"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 30003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000215"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 31003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000225"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 32003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000235"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 33003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000245"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 34003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000255"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 35003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000265"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 36003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000205"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 37003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000215"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 38003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000225"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 39003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000235"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 40003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000245"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 41003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000255"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 42003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000265"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 43003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000205"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 44003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000203"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000215"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 45003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000213"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000225"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 46003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000223"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000235"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 47003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000233"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000245"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 48003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000243"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000255"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 49003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("22222222-0000-4000-8000-000000000003"), 1.0, false });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 50002,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000265"), 2.5, true });

            migrationBuilder.UpdateData(
                table: "CampaignStageEnemies",
                keyColumn: "Id",
                keyValue: 50003,
                columns: new[] { "HeroId", "HpMultiplier", "IsBoss" },
                values: new object[] { new Guid("33333333-0000-4000-8000-000000000263"), 1.0, false });

            migrationBuilder.InsertData(
                table: "CampaignStageEnemies",
                columns: new[] { "Id", "HeroId", "HpMultiplier", "IsBoss", "Position", "StageId" },
                values: new object[] { 1002, new Guid("33333333-0000-4000-8000-000000000102"), 1.75, true, 2, 10 });
        }
    }
}
