using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class GlacialRelic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000201030022"),
                column: "Probability",
                value: 0.0025000000000000001);

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000202030022"),
                column: "Probability",
                value: 0.0025000000000000001);

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000203030022"),
                column: "Probability",
                value: 0.0025000000000000001);

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000204030022"),
                column: "Probability",
                value: 0.0025000000000000001);

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000205030022"),
                column: "Probability",
                value: 0.0025000000000000001);

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000206030022"),
                column: "Probability",
                value: 0.0025000000000000001);

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000207030022"),
                column: "Probability",
                value: 0.0025000000000000001);

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300030022"),
                column: "Probability",
                value: 0.0050000000000000001);

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300040016"),
                column: "Probability",
                value: 0.025000000000000001);

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300050009"),
                column: "Probability",
                value: 0.074999999999999997);

            migrationBuilder.InsertData(
                table: "DropEntry",
                columns: new[] { "Id", "ChestDropTableId", "ItemLevel", "ItemType", "MaxAmount", "MinAmount", "Probability", "Rarity", "SubType" },
                values: new object[,]
                {
                    { new Guid("c4e50001-0000-4000-8000-000201030023"), "c4e50000-0000-4000-8000-000002010003", 1, 0, 1, 1, 0.0025000000000000001, 5, "GlacialRelic" },
                    { new Guid("c4e50001-0000-4000-8000-000202030023"), "c4e50000-0000-4000-8000-000002020003", 1, 0, 1, 1, 0.0025000000000000001, 5, "GlacialRelic" },
                    { new Guid("c4e50001-0000-4000-8000-000203030023"), "c4e50000-0000-4000-8000-000002030003", 1, 0, 1, 1, 0.0025000000000000001, 5, "GlacialRelic" },
                    { new Guid("c4e50001-0000-4000-8000-000204030023"), "c4e50000-0000-4000-8000-000002040003", 1, 0, 1, 1, 0.0025000000000000001, 5, "GlacialRelic" },
                    { new Guid("c4e50001-0000-4000-8000-000205030023"), "c4e50000-0000-4000-8000-000002050003", 1, 0, 1, 1, 0.0025000000000000001, 5, "GlacialRelic" },
                    { new Guid("c4e50001-0000-4000-8000-000206030023"), "c4e50000-0000-4000-8000-000002060003", 1, 0, 1, 1, 0.0025000000000000001, 5, "GlacialRelic" },
                    { new Guid("c4e50001-0000-4000-8000-000207030023"), "c4e50000-0000-4000-8000-000002070003", 1, 0, 1, 1, 0.0025000000000000001, 5, "GlacialRelic" },
                    { new Guid("c4e50001-0000-4000-8000-000300030023"), "c4e50000-0000-4000-8000-000003000003", 1, 0, 1, 1, 0.0050000000000000001, 5, "GlacialRelic" },
                    { new Guid("c4e50001-0000-4000-8000-000300040017"), "c4e50000-0000-4000-8000-000003000004", 1, 0, 1, 1, 0.025000000000000001, 5, "GlacialRelic" },
                    { new Guid("c4e50001-0000-4000-8000-000300050010"), "c4e50000-0000-4000-8000-000003000005", 1, 0, 1, 1, 0.074999999999999997, 5, "GlacialRelic" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000201030023"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000202030023"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000203030023"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000204030023"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000205030023"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000206030023"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000207030023"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300030023"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300040017"));

            migrationBuilder.DeleteData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300050010"));

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000201030022"),
                column: "Probability",
                value: 0.0050000000000000001);

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000202030022"),
                column: "Probability",
                value: 0.0050000000000000001);

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000203030022"),
                column: "Probability",
                value: 0.0050000000000000001);

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000204030022"),
                column: "Probability",
                value: 0.0050000000000000001);

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000205030022"),
                column: "Probability",
                value: 0.0050000000000000001);

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000206030022"),
                column: "Probability",
                value: 0.0050000000000000001);

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000207030022"),
                column: "Probability",
                value: 0.0050000000000000001);

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300030022"),
                column: "Probability",
                value: 0.01);

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300040016"),
                column: "Probability",
                value: 0.050000000000000003);

            migrationBuilder.UpdateData(
                table: "DropEntry",
                keyColumn: "Id",
                keyValue: new Guid("c4e50001-0000-4000-8000-000300050009"),
                column: "Probability",
                value: 0.14999999999999999);
        }
    }
}
