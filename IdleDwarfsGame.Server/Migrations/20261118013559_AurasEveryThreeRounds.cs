using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdleDwarfsGame.Server.Migrations
{
    /// <inheritdoc />
    public partial class AurasEveryThreeRounds : Migration
    {
        // Ауры дворфов игроков (AuraBook): было — срабатывание каждый раунд наложением на ход,
        // стало — раз в 3 раунда, у каждой стихии своя сила и длительность. Кровотечение не меняется.
        // Сиды (демо-игрок, кампания, боссы гильдий) не трогаем: их ауры — в HasData
        private static readonly (string Effect, int[] OldPower, int[] NewPower, int[] NewDuration)[] Auras =
        [
            ("Burn", [8, 12, 16], [12, 24, 36], [2, 2, 2]),
            ("Frost", [5, 8, 12], [10, 20, 30], [2, 2, 2]),
            ("Shock", [8, 12, 16], [24, 48, 72], [1, 1, 1]),
            ("Poison", [8, 12, 16], [8, 10, 12], [3, 4, 5]),
            ("EnergyBurn", [3, 4, 5], [10, 16, 24], [1, 1, 1]),
            ("Regen", [10, 15, 20], [10, 20, 30], [3, 3, 3])
        ];

        private const string NotSeeded =
            "PlayerId NOT IN ('a1d2f3e4-0000-4000-8000-000000000001', 'e0e0e0e0-0000-4000-8000-000000000002', " +
            "'c0c0c0c0-0000-4000-8000-000000000003', 'b0b0b0b0-0000-4000-8000-000000000004')";

        private static void UpdateAuras(MigrationBuilder migrationBuilder, bool forward)
        {
            foreach (var (effect, oldPower, newPower, newDuration) in Auras)
            {
                for (int level = 1; level <= 3; level++)
                {
                    int power = forward ? newPower[level - 1] : oldPower[level - 1];
                    int duration = forward ? newDuration[level - 1] : 1;
                    int interval = forward ? 3 : 1;
                    migrationBuilder.Sql(
                        $"UPDATE Heroes SET AuraPower = {power}, AuraDuration = {duration}, AuraInterval = {interval} " +
                        $"WHERE AuraEffect = '{effect}' AND AuraLevel = {level} AND {NotSeeded}");
                }
            }
        }

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            UpdateAuras(migrationBuilder, forward: true);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 430,
                column: "EnemyLevel",
                value: 90);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 440,
                column: "EnemyLevel",
                value: 91);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 441,
                column: "EnemyLevel",
                value: 91);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 449,
                column: "EnemyLevel",
                value: 92);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 450,
                column: "EnemyLevel",
                value: 92);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 451,
                column: "EnemyLevel",
                value: 92);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 458,
                column: "EnemyLevel",
                value: 93);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 459,
                column: "EnemyLevel",
                value: 93);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 460,
                column: "EnemyLevel",
                value: 93);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 461,
                column: "EnemyLevel",
                value: 93);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 462,
                column: "EnemyLevel",
                value: 93);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 468,
                column: "EnemyLevel",
                value: 94);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 469,
                column: "EnemyLevel",
                value: 94);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 470,
                column: "EnemyLevel",
                value: 94);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 471,
                column: "EnemyLevel",
                value: 94);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 472,
                column: "EnemyLevel",
                value: 94);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 473,
                column: "EnemyLevel",
                value: 94);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 477,
                column: "EnemyLevel",
                value: 95);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 478,
                column: "EnemyLevel",
                value: 95);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 479,
                column: "EnemyLevel",
                value: 95);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 480,
                column: "EnemyLevel",
                value: 95);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 481,
                column: "EnemyLevel",
                value: 95);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 482,
                column: "EnemyLevel",
                value: 95);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 483,
                column: "EnemyLevel",
                value: 95);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 486,
                column: "EnemyLevel",
                value: 96);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 487,
                column: "EnemyLevel",
                value: 96);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 488,
                column: "EnemyLevel",
                value: 96);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 489,
                column: "EnemyLevel",
                value: 96);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 490,
                column: "EnemyLevel",
                value: 96);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 491,
                column: "EnemyLevel",
                value: 96);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 492,
                column: "EnemyLevel",
                value: 96);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 493,
                column: "EnemyLevel",
                value: 96);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 494,
                column: "EnemyLevel",
                value: 96);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 496,
                column: "EnemyLevel",
                value: 97);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 497,
                column: "EnemyLevel",
                value: 97);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 498,
                column: "EnemyLevel",
                value: 97);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 499,
                column: "EnemyLevel",
                value: 97);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 500,
                column: "EnemyLevel",
                value: 97);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            UpdateAuras(migrationBuilder, forward: false);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 430,
                column: "EnemyLevel",
                value: 91);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 440,
                column: "EnemyLevel",
                value: 92);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 441,
                column: "EnemyLevel",
                value: 92);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 449,
                column: "EnemyLevel",
                value: 93);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 450,
                column: "EnemyLevel",
                value: 93);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 451,
                column: "EnemyLevel",
                value: 93);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 458,
                column: "EnemyLevel",
                value: 94);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 459,
                column: "EnemyLevel",
                value: 94);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 460,
                column: "EnemyLevel",
                value: 94);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 461,
                column: "EnemyLevel",
                value: 94);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 462,
                column: "EnemyLevel",
                value: 94);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 468,
                column: "EnemyLevel",
                value: 95);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 469,
                column: "EnemyLevel",
                value: 95);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 470,
                column: "EnemyLevel",
                value: 95);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 471,
                column: "EnemyLevel",
                value: 95);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 472,
                column: "EnemyLevel",
                value: 95);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 473,
                column: "EnemyLevel",
                value: 95);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 477,
                column: "EnemyLevel",
                value: 96);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 478,
                column: "EnemyLevel",
                value: 96);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 479,
                column: "EnemyLevel",
                value: 96);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 480,
                column: "EnemyLevel",
                value: 96);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 481,
                column: "EnemyLevel",
                value: 96);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 482,
                column: "EnemyLevel",
                value: 96);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 483,
                column: "EnemyLevel",
                value: 96);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 486,
                column: "EnemyLevel",
                value: 97);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 487,
                column: "EnemyLevel",
                value: 97);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 488,
                column: "EnemyLevel",
                value: 97);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 489,
                column: "EnemyLevel",
                value: 97);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 490,
                column: "EnemyLevel",
                value: 97);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 491,
                column: "EnemyLevel",
                value: 97);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 492,
                column: "EnemyLevel",
                value: 97);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 493,
                column: "EnemyLevel",
                value: 97);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 494,
                column: "EnemyLevel",
                value: 97);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 496,
                column: "EnemyLevel",
                value: 98);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 497,
                column: "EnemyLevel",
                value: 98);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 498,
                column: "EnemyLevel",
                value: 98);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 499,
                column: "EnemyLevel",
                value: 98);

            migrationBuilder.UpdateData(
                table: "CampaignStages",
                keyColumn: "Id",
                keyValue: 500,
                column: "EnemyLevel",
                value: 98);
        }
    }
}
