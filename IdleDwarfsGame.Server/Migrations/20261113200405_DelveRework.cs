using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class DelveRework : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Старые забеги (недельная карта, отряд игрока) несовместимы с новыми — удаляются
            migrationBuilder.Sql("DELETE FROM [DelveRuns]");

            migrationBuilder.DropIndex(
                name: "IX_DelveRuns_UserId_Day",
                table: "DelveRuns");

            migrationBuilder.DropColumn(
                name: "Day",
                table: "DelveRuns");

            migrationBuilder.RenameColumn(
                name: "WeekStart",
                table: "DelveRuns",
                newName: "PeriodStart");

            migrationBuilder.RenameIndex(
                name: "IX_DelveRuns_WeekStart",
                table: "DelveRuns",
                newName: "IX_DelveRuns_PeriodStart");

            migrationBuilder.AddColumn<long>(
                name: "ExperienceEarned",
                table: "DelveRuns",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "RainbowCoinsEarned",
                table: "DelveRuns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Revived",
                table: "DelveRuns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RewardsJson",
                table: "DelveRuns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Seed",
                table: "DelveRuns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DelveRuns_UserId_PeriodStart",
                table: "DelveRuns",
                columns: new[] { "UserId", "PeriodStart" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [DelveRuns]");

            migrationBuilder.DropIndex(
                name: "IX_DelveRuns_UserId_PeriodStart",
                table: "DelveRuns");

            migrationBuilder.DropColumn(
                name: "ExperienceEarned",
                table: "DelveRuns");

            migrationBuilder.DropColumn(
                name: "RainbowCoinsEarned",
                table: "DelveRuns");

            migrationBuilder.DropColumn(
                name: "Revived",
                table: "DelveRuns");

            migrationBuilder.DropColumn(
                name: "RewardsJson",
                table: "DelveRuns");

            migrationBuilder.DropColumn(
                name: "Seed",
                table: "DelveRuns");

            migrationBuilder.RenameColumn(
                name: "PeriodStart",
                table: "DelveRuns",
                newName: "WeekStart");

            migrationBuilder.RenameIndex(
                name: "IX_DelveRuns_PeriodStart",
                table: "DelveRuns",
                newName: "IX_DelveRuns_WeekStart");

            migrationBuilder.AddColumn<DateTime>(
                name: "Day",
                table: "DelveRuns",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_DelveRuns_UserId_Day",
                table: "DelveRuns",
                columns: new[] { "UserId", "Day" },
                unique: true);
        }
    }
}
